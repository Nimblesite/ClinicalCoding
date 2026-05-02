using System.Security.Cryptography;

namespace Gatekeeper.Api;

/// <summary>
/// Supabase JWT authentication provider.
/// Validates Supabase-issued RS256/EdDSA tokens via JWKS, then issues an internal
/// Gatekeeper token. Has zero knowledge of passkey or any other provider.
/// </summary>
public sealed class SupabaseAuthProvider : ITokenExchangeProvider, IDisposable
{
    private readonly HttpClient _http;
    private readonly Uri _supabaseUrl;
    private readonly string _expectedAudience;
    private readonly ILogger<SupabaseAuthProvider> _logger;
    private readonly SemaphoreSlim _jwksLock = new(1, 1);

    private Dictionary<string, ECDsa> _ecKeys = [];
    private Dictionary<string, RSA> _rsaKeys = [];
    private DateTime _jwksFetchedAt = DateTime.MinValue;
    private static readonly TimeSpan JwksCacheTtl = TimeSpan.FromHours(1);

    /// <inheritdoc/>
    public string ProviderName => "supabase";

    public SupabaseAuthProvider(
        HttpClient http,
        Uri supabaseUrl,
        string expectedAudience,
        ILogger<SupabaseAuthProvider> logger
    )
    {
        _http = http;
        _supabaseUrl = supabaseUrl;
        _expectedAudience = expectedAudience;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AuthCompleteResult, AuthError>> ExchangeAsync(
        IDbConnection conn,
        string externalToken,
        JwtConfig jwt
    )
    {
        var npgsql = conn as NpgsqlConnection
            ?? throw new InvalidOperationException("SupabaseAuthProvider requires NpgsqlConnection.");

        var validateResult = await ValidateSupabaseTokenAsync(externalToken).ConfigureAwait(false);
        if (validateResult is null)
            return Err("Invalid or untrusted Supabase token");

        var (sub, email, displayName) = validateResult.Value;
        var now = Now();

        var userId = await ResolveUserAsync(npgsql, sub, email, displayName, now).ConfigureAwait(false);

        var rolesResult = await npgsql.GetUserRolesAsync(userId, now).ConfigureAwait(false);
        var roles = rolesResult is GetUserRolesOk rOk
            ? rOk.Value.Where(r => r.name is not null).Select(r => r.name!).ToList()
            : new List<string>();

        var token = TokenService.CreateToken(userId, displayName, email, roles, jwt.SigningKey, jwt.TokenLifetime);
        return new Result<AuthCompleteResult, AuthError>.Ok<AuthCompleteResult, AuthError>(
            new AuthCompleteResult(token, userId, displayName, email, roles)
        );
    }

    private async Task<(string Sub, string? Email, string? DisplayName)?> ValidateSupabaseTokenAsync(string token)
    {
        var parts = token.Split('.');
        if (parts.Length != 3)
            return null;

        JsonElement header, payload;
        try
        {
            header = JsonDocument.Parse(TokenService.Base64UrlDecode(parts[0])).RootElement;
            payload = JsonDocument.Parse(TokenService.Base64UrlDecode(parts[1])).RootElement;
        }
        catch (JsonException)
        {
            return null;
        }

        var alg = header.TryGetProperty("alg", out var algEl) ? algEl.GetString() : null;
        if (alg is not ("RS256" or "ES256"))
        {
            _logger.LogWarning("Supabase token rejected: unsupported algorithm {Alg}", alg);
            return null;
        }

        if (!payload.TryGetProperty("exp", out var expEl) || DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expEl.GetInt64())
            return null;

        if (!payload.TryGetProperty("aud", out var audEl))
            return null;
        var aud = audEl.ValueKind == JsonValueKind.Array
            ? audEl.EnumerateArray().Select(e => e.GetString()).ToList()
            : [audEl.GetString()];
        if (!aud.Contains(_expectedAudience))
            return null;

        var expectedIss = new Uri(_supabaseUrl, "auth/v1").ToString().TrimEnd('/');
        if (!payload.TryGetProperty("iss", out var issEl) || issEl.GetString()?.TrimEnd('/') != expectedIss)
            return null;

        var kid = header.TryGetProperty("kid", out var kidEl) ? kidEl.GetString() : null;
        var verified = alg == "RS256"
            ? await VerifyRsa256Async(parts[0], parts[1], parts[2], kid).ConfigureAwait(false)
            : await VerifyEs256Async(parts[0], parts[1], parts[2], kid).ConfigureAwait(false);

        if (!verified)
            return null;

        var sub = payload.TryGetProperty("sub", out var subEl) ? subEl.GetString() : null;
        if (string.IsNullOrEmpty(sub))
            return null;

        var email = payload.TryGetProperty("email", out var emailEl) ? emailEl.GetString() : null;
        var name = payload.TryGetProperty("user_metadata", out var metaEl)
            && metaEl.TryGetProperty("full_name", out var fnEl)
            ? fnEl.GetString()
            : email;

        return (sub, email, name);
    }

    private async Task<bool> VerifyRsa256Async(string header, string payload, string sig, string? kid)
    {
        var key = await GetRsaKeyAsync(kid).ConfigureAwait(false);
        if (key is null) return false;
        var data = Encoding.UTF8.GetBytes($"{header}.{payload}");
        return key.VerifyData(data, TokenService.Base64UrlDecode(sig), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }

    private async Task<bool> VerifyEs256Async(string header, string payload, string sig, string? kid)
    {
        var key = await GetEcKeyAsync(kid).ConfigureAwait(false);
        if (key is null) return false;
        var data = Encoding.UTF8.GetBytes($"{header}.{payload}");
        return key.VerifyData(data, TokenService.Base64UrlDecode(sig), HashAlgorithmName.SHA256);
    }

    private async Task<RSA?> GetRsaKeyAsync(string? kid)
    {
        await EnsureJwksLoadedAsync(kid).ConfigureAwait(false);
        return (kid is not null && _rsaKeys.TryGetValue(kid, out var k)) ? k : _rsaKeys.Values.FirstOrDefault();
    }

    private async Task<ECDsa?> GetEcKeyAsync(string? kid)
    {
        await EnsureJwksLoadedAsync(kid).ConfigureAwait(false);
        return (kid is not null && _ecKeys.TryGetValue(kid, out var k)) ? k : _ecKeys.Values.FirstOrDefault();
    }

    private async Task EnsureJwksLoadedAsync(string? requiredKid)
    {
        var stale = DateTime.UtcNow - _jwksFetchedAt > JwksCacheTtl;
        var kidMissing = requiredKid is not null && !_rsaKeys.ContainsKey(requiredKid) && !_ecKeys.ContainsKey(requiredKid);
        if (!stale && !kidMissing) return;

        await _jwksLock.WaitAsync().ConfigureAwait(false);
        try
        {
            stale = DateTime.UtcNow - _jwksFetchedAt > JwksCacheTtl;
            kidMissing = requiredKid is not null && !_rsaKeys.ContainsKey(requiredKid) && !_ecKeys.ContainsKey(requiredKid);
            if (!stale && !kidMissing) return;
            await FetchJwksAsync().ConfigureAwait(false);
        }
        finally
        {
            _jwksLock.Release();
        }
    }

    private async Task FetchJwksAsync()
    {
        var url = new Uri(_supabaseUrl, "auth/v1/.well-known/jwks.json");
        _logger.LogInformation("Fetching JWKS from {Url}", url);
        try
        {
            var json = await _http.GetStringAsync(url).ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);
            var newRsa = new Dictionary<string, RSA>();
            var newEc = new Dictionary<string, ECDsa>();

            foreach (var jwk in doc.RootElement.GetProperty("keys").EnumerateArray())
            {
                var kty = jwk.TryGetProperty("kty", out var ktyEl) ? ktyEl.GetString() : null;
                var kid = jwk.TryGetProperty("kid", out var kidEl) ? kidEl.GetString() ?? "default" : "default";

                if (kty == "RSA")
                {
                    var rsa = RSA.Create();
                    rsa.ImportParameters(new RSAParameters
                    {
                        Modulus = B64Bytes(jwk, "n"),
                        Exponent = B64Bytes(jwk, "e"),
                    });
                    newRsa[kid] = rsa;
                }
                else if (kty == "EC")
                {
                    var crv = jwk.TryGetProperty("crv", out var crvEl) ? crvEl.GetString() : null;
                    var curve = crv == "P-384" ? ECCurve.NamedCurves.nistP384 : ECCurve.NamedCurves.nistP256;
                    newEc[kid] = ECDsa.Create(new ECParameters
                    {
                        Curve = curve,
                        Q = new ECPoint { X = B64Bytes(jwk, "x"), Y = B64Bytes(jwk, "y") },
                    });
                }
            }

            _rsaKeys = newRsa;
            _ecKeys = newEc;
            _jwksFetchedAt = DateTime.UtcNow;
            _logger.LogInformation("JWKS loaded: {R} RSA, {E} EC", newRsa.Count, newEc.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "JWKS fetch failed from {Url}", url);
        }
    }

    private static byte[]? B64Bytes(JsonElement jwk, string prop) =>
        jwk.TryGetProperty(prop, out var el) && el.GetString() is { } s ? TokenService.Base64UrlDecode(s) : null;

    private static async Task<string> ResolveUserAsync(
        NpgsqlConnection conn,
        string supabaseSub,
        string? email,
        string? displayName,
        string now
    )
    {
        using var checkCmd = conn.CreateCommand();
        checkCmd.CommandText =
            "SELECT user_id FROM gk_user_identity WHERE provider = 'supabase' AND external_id = @sub LIMIT 1";
        checkCmd.Parameters.AddWithValue("@sub", supabaseSub);
        var existing = await checkCmd.ExecuteScalarAsync().ConfigureAwait(false);
        if (existing is string userId && !string.IsNullOrEmpty(userId))
            return userId;

        var newUserId = Guid.NewGuid().ToString();
        if (!string.IsNullOrEmpty(email))
        {
            var byEmail = await conn.GetUserByEmailAsync(email).ConfigureAwait(false);
            if (byEmail is GetUserByEmailOk { Value.Count: > 0 } emailOk)
                newUserId = emailOk.Value[0].id ?? newUserId;
            else
            {
                await using var tx = await conn.BeginTransactionAsync().ConfigureAwait(false);
                _ = await tx.Insertgk_userAsync(newUserId, displayName ?? email, email, now, null, true, 0, 0, null, null).ConfigureAwait(false);
                _ = await tx.Insertgk_user_roleAsync(newUserId, "role-user", now, null, null).ConfigureAwait(false);
                await tx.CommitAsync().ConfigureAwait(false);
            }
        }

        using var linkCmd = conn.CreateCommand();
        linkCmd.CommandText =
            @"INSERT INTO gk_user_identity (id, user_id, provider, external_id, linked_at)
              VALUES (@id, @uid, 'supabase', @sub, @at) ON CONFLICT DO NOTHING";
        linkCmd.Parameters.AddWithValue("@id", Guid.NewGuid().ToString());
        linkCmd.Parameters.AddWithValue("@uid", newUserId);
        linkCmd.Parameters.AddWithValue("@sub", supabaseSub);
        linkCmd.Parameters.AddWithValue("@at", now);
        await linkCmd.ExecuteNonQueryAsync().ConfigureAwait(false);

        return newUserId;
    }

    private static string Now() => DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);

    private static Result<AuthCompleteResult, AuthError> Err(string reason) =>
        new Result<AuthCompleteResult, AuthError>.Error<AuthCompleteResult, AuthError>(new AuthError(reason));

    /// <inheritdoc/>
    public void Dispose()
    {
        _jwksLock.Dispose();
        foreach (var k in _rsaKeys.Values) k.Dispose();
        foreach (var k in _ecKeys.Values) k.Dispose();
    }
}
