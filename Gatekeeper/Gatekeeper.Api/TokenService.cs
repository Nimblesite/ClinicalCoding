using System.Security.Cryptography;

namespace Gatekeeper.Api;

/// <summary>
/// JWT token generation and validation.
/// </summary>
public static class TokenService
{
    /// <summary>Token claims data.</summary>
    public sealed record TokenClaims(
        string UserId,
        string? DisplayName,
        string? Email,
        IReadOnlyList<string> Roles,
        string Jti,
        long Exp,
        int TokenVersion,
        string? Issuer,
        string? Audience
    );

    /// <summary>Successful token validation result.</summary>
    public sealed record TokenValidationOk(TokenClaims Claims);

    /// <summary>Failed token validation result.</summary>
    public sealed record TokenValidationError(string Reason);

    /// <summary>Extracts the token from a Bearer authorization header.</summary>
    public static string? ExtractBearerToken(string? authHeader) =>
        authHeader?.StartsWith("Bearer ", StringComparison.Ordinal) == true
            ? authHeader["Bearer ".Length..]
            : null;

    /// <summary>Creates a JWT for the given user.</summary>
    public static string CreateToken(
        string userId,
        string? displayName,
        string? email,
        IReadOnlyList<string> roles,
        byte[] signingKey,
        TimeSpan lifetime,
        string? issuer = null,
        string? audience = null,
        int tokenVersion = 0
    )
    {
        var now = DateTimeOffset.UtcNow;
        var exp = now.Add(lifetime);
        var jti = Guid.NewGuid().ToString();

        var header = Base64UrlEncode(
            JsonSerializer.SerializeToUtf8Bytes(new { alg = "HS256", typ = "JWT" })
        );
        var payload = Base64UrlEncode(
            JsonSerializer.SerializeToUtf8Bytes(
                new
                {
                    sub = userId,
                    name = displayName,
                    email,
                    roles,
                    jti,
                    iat = now.ToUnixTimeSeconds(),
                    exp = exp.ToUnixTimeSeconds(),
                    iss = issuer,
                    aud = audience,
                    ver = tokenVersion,
                }
            )
        );
        var signature = ComputeSignature(header, payload, signingKey);
        return $"{header}.{payload}.{signature}";
    }

    /// <summary>Validates a JWT token, optionally checking revocation and user state.</summary>
    public static async Task<object> ValidateTokenAsync(
        IDbConnection conn,
        string token,
        byte[] signingKey,
        bool checkRevocation,
        string? expectedIssuer = null,
        string? expectedAudience = null,
        ILogger? logger = null
    )
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3)
                return new TokenValidationError("Invalid token format");

            // Enforce algorithm server-side — never trust the header alg claim
            var headerBytes = Base64UrlDecode(parts[0]);
            using var headerDoc = JsonDocument.Parse(headerBytes);
            var alg = headerDoc.RootElement.TryGetProperty("alg", out var algEl)
                ? algEl.GetString()
                : null;
            if (!string.Equals(alg, "HS256", StringComparison.Ordinal))
                return new TokenValidationError("Unsupported signing algorithm");

            var expectedSignature = ComputeSignature(parts[0], parts[1], signingKey);
            if (!CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(expectedSignature),
                    Encoding.UTF8.GetBytes(parts[2])))
                return new TokenValidationError("Invalid signature");

            var payloadBytes = Base64UrlDecode(parts[1]);
            using var doc = JsonDocument.Parse(payloadBytes);
            var root = doc.RootElement;

            var exp = root.GetProperty("exp").GetInt64();
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > exp)
                return new TokenValidationError("Token expired");

            if (expectedIssuer is not null)
            {
                var iss = root.TryGetProperty("iss", out var issEl) ? issEl.GetString() : null;
                if (!string.Equals(iss, expectedIssuer, StringComparison.Ordinal))
                    return new TokenValidationError("Invalid issuer");
            }

            if (expectedAudience is not null)
            {
                var aud = root.TryGetProperty("aud", out var audEl) ? audEl.GetString() : null;
                if (!string.Equals(aud, expectedAudience, StringComparison.Ordinal))
                    return new TokenValidationError("Invalid audience");
            }

            var jti = root.GetProperty("jti").GetString() ?? string.Empty;
            var ver = root.TryGetProperty("ver", out var verEl) ? verEl.GetInt32() : 0;

            if (checkRevocation && await IsTokenRevokedAsync(conn, jti).ConfigureAwait(false))
                return new TokenValidationError("Token revoked");

            var roles = root.TryGetProperty("roles", out var rolesEl)
                ? rolesEl.EnumerateArray().Select(e => e.GetString() ?? string.Empty).ToList()
                : (IReadOnlyList<string>)[];

            return new TokenValidationOk(new TokenClaims(
                UserId: root.GetProperty("sub").GetString() ?? string.Empty,
                DisplayName: root.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : null,
                Email: root.TryGetProperty("email", out var emailEl) ? emailEl.GetString() : null,
                Roles: roles,
                Jti: jti,
                Exp: exp,
                TokenVersion: ver,
                Issuer: root.TryGetProperty("iss", out var issEl2) ? issEl2.GetString() : null,
                Audience: root.TryGetProperty("aud", out var audEl2) ? audEl2.GetString() : null
            ));
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Token validation failed");
            return new TokenValidationError("Token validation failed");
        }
    }

    /// <summary>Revokes a token by JTI.</summary>
    public static async Task RevokeTokenAsync(IDbConnection conn, string jti) =>
        _ = await DbExtensions.RevokeSessionAdapterAsync(conn, jti).ConfigureAwait(false);

    private static async Task<bool> IsTokenRevokedAsync(IDbConnection conn, string jti)
    {
        var result = await DbExtensions.GetSessionRevokedAdapterAsync(conn, jti).ConfigureAwait(false);
        return result switch
        {
            GetSessionRevokedOk ok => ok.Value.FirstOrDefault()?.is_revoked == true,
            GetSessionRevokedError => false,
        };
    }

    internal static string Base64UrlEncode(byte[] input) =>
        Convert.ToBase64String(input)
            .Replace("+", "-", StringComparison.Ordinal)
            .Replace("/", "_", StringComparison.Ordinal)
            .TrimEnd('=');

    internal static byte[] Base64UrlDecode(string input)
    {
        var padded = input
            .Replace("-", "+", StringComparison.Ordinal)
            .Replace("_", "/", StringComparison.Ordinal);
        var padding = (4 - (padded.Length % 4)) % 4;
        padded += new string('=', padding);
        return Convert.FromBase64String(padded);
    }

    private static string ComputeSignature(string header, string payload, byte[] key)
    {
        var data = Encoding.UTF8.GetBytes($"{header}.{payload}");
        using var hmac = new HMACSHA256(key);
        return Base64UrlEncode(hmac.ComputeHash(data));
    }
}
