namespace Gatekeeper.Api;

/// <summary>
/// WebAuthn/FIDO2 passkey authentication provider.
/// Implements the two-step begin/complete ceremony.
/// Has no knowledge of any other auth provider.
/// </summary>
public sealed class PasskeyAuthProvider : IChallengeAuthProvider
{
    private readonly IFido2 _fido2;
    private readonly ILogger<PasskeyAuthProvider> _logger;

    /// <inheritdoc/>
    public string ProviderName => "passkey";

    public PasskeyAuthProvider(IFido2 fido2, ILogger<PasskeyAuthProvider> logger)
    {
        _fido2 = fido2;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AuthBeginResult, AuthError>> BeginAsync(
        IDbConnection conn,
        AuthBeginRequest request
    )
    {
        var npgsql = Npgsql(conn);
        var now = Now();

        var existingUser = await npgsql.GetUserByEmailAsync(request.Email ?? string.Empty).ConfigureAwait(false);
        var isNew = existingUser is not GetUserByEmailOk { Value.Count: > 0 };
        var userId = isNew
            ? Guid.NewGuid().ToString()
            : ((GetUserByEmailOk)existingUser).Value[0].id ?? Guid.NewGuid().ToString();

        if (isNew)
        {
            await using var tx = await npgsql.BeginTransactionAsync().ConfigureAwait(false);
            _ = await tx.Insertgk_userAsync(userId, request.DisplayName ?? request.Email ?? userId, request.Email ?? string.Empty, now, null, true, null, null, null, null).ConfigureAwait(false);
            await tx.CommitAsync().ConfigureAwait(false);
        }

        var existingCreds = await npgsql.GetUserCredentialsAsync(userId).ConfigureAwait(false);
        var excludeCredentials = existingCreds switch
        {
            GetUserCredentialsOk ok => ok.Value
                .Where(c => c.id is not null)
                .Select(c => new PublicKeyCredentialDescriptor(Base64Url.Decode(c.id!)))
                .ToList(),
            GetUserCredentialsError => new List<PublicKeyCredentialDescriptor>(),
        };

        var fido2User = new Fido2User
        {
            Id = Encoding.UTF8.GetBytes(userId),
            Name = request.Email ?? userId,
            DisplayName = request.DisplayName ?? request.Email ?? userId,
        };
        var options = _fido2.RequestNewCredential(new RequestNewCredentialParams
        {
            User = fido2User,
            ExcludeCredentials = excludeCredentials,
            AuthenticatorSelection = new AuthenticatorSelection
            {
                ResidentKey = ResidentKeyRequirement.Required,
                UserVerification = UserVerificationRequirement.Required,
            },
            AttestationPreference = ResolveAttestation(),
        });

        var challengeId = Guid.NewGuid().ToString();
        var expiry = DateTime.UtcNow.AddSeconds(120).ToString("o", System.Globalization.CultureInfo.InvariantCulture);

        await using var tx2 = await npgsql.BeginTransactionAsync().ConfigureAwait(false);
        // Invalidate any prior pending registration challenge for this user
        await DeletePendingChallengeAsync(npgsql, userId, "registration").ConfigureAwait(false);
        _ = await tx2.Insertgk_challengeAsync(challengeId, userId, options.Challenge, "registration", now, expiry, null).ConfigureAwait(false);
        await tx2.CommitAsync().ConfigureAwait(false);

        return new Result<AuthBeginResult, AuthError>.Ok<AuthBeginResult, AuthError>(
            new AuthBeginResult(challengeId, options.ToJson())
        );
    }

    /// <inheritdoc/>
    public async Task<Result<AuthCompleteResult, AuthError>> CompleteAsync(
        IDbConnection conn,
        AuthCompleteRequest request,
        JwtConfig jwt
    )
    {
        var npgsql = Npgsql(conn);
        var now = Now();

        if (request.ProviderResponse is not AuthenticatorAttestationRawResponse attestation)
            return Err("Invalid provider response type for passkey registration");

        var challengeResult = await npgsql.GetChallengeByIdAsync(request.ChallengeId, now).ConfigureAwait(false);
        if (challengeResult is not GetChallengeByIdOk { Value.Count: > 0 } challengeOk)
            return Err("Challenge not found or expired");

        var stored = challengeOk.Value[0];
        if (string.IsNullOrEmpty(stored.user_id))
            return Err("Invalid challenge: no user");

        var credResult = await _fido2.MakeNewCredentialAsync(new MakeNewCredentialParams
        {
            AttestationResponse = attestation,
            OriginalOptions = CredentialCreateOptions.FromJson(request.OptionsJson),
            IsCredentialIdUniqueToUserCallback = async (args, _) =>
            {
                var existing = await npgsql.GetCredentialByIdAsync(Base64Url.Encode(args.CredentialId)).ConfigureAwait(false);
                return existing is not GetCredentialByIdOk { Value.Count: > 0 };
            },
        }).ConfigureAwait(false);

        await using var tx = await npgsql.BeginTransactionAsync().ConfigureAwait(false);
        _ = await tx.Insertgk_credentialAsync(
            Base64Url.Encode(credResult.Id),
            stored.user_id,
            credResult.PublicKey,
            (int?)credResult.SignCount,
            credResult.AaGuid.ToString(),
            credResult.Type.ToString(),
            credResult.Transports != null ? string.Join(",", credResult.Transports) : null,
            credResult.AttestationFormat,
            now, null,
            request.DeviceName,
            credResult.IsBackupEligible,
            credResult.IsBackedUp
        ).ConfigureAwait(false);
        _ = await tx.Insertgk_user_roleAsync(stored.user_id, "role-user", now, null, null).ConfigureAwait(false);

        // Link identity — idempotent via ON CONFLICT DO NOTHING in schema unique index
        await InsertUserIdentityAsync(npgsql, stored.user_id, ProviderName, stored.user_id).ConfigureAwait(false);

        await tx.CommitAsync().ConfigureAwait(false);

        // Consume challenge atomically (delete after use)
        await DeleteChallengeAsync(npgsql, request.ChallengeId).ConfigureAwait(false);

        return await BuildResultAsync(npgsql, stored.user_id, jwt, now).ConfigureAwait(false);
    }

    /// <summary>
    /// Begins a passkey login ceremony. Returns challenge options for the client.
    /// </summary>
    public async Task<Result<AuthBeginResult, AuthError>> BeginLoginAsync(IDbConnection conn)
    {
        var npgsql = Npgsql(conn);
        var now = Now();
        var options = _fido2.GetAssertionOptions(new GetAssertionOptionsParams
        {
            AllowedCredentials = [],
            UserVerification = UserVerificationRequirement.Required,
        });

        var challengeId = Guid.NewGuid().ToString();
        var expiry = DateTime.UtcNow.AddSeconds(120).ToString("o", System.Globalization.CultureInfo.InvariantCulture);

        await using var tx = await npgsql.BeginTransactionAsync().ConfigureAwait(false);
        _ = await tx.Insertgk_challengeAsync(challengeId, null, options.Challenge, "authentication", now, expiry, null).ConfigureAwait(false);
        await tx.CommitAsync().ConfigureAwait(false);

        return new Result<AuthBeginResult, AuthError>.Ok<AuthBeginResult, AuthError>(
            new AuthBeginResult(challengeId, options.ToJson())
        );
    }

    /// <summary>
    /// Completes a passkey login ceremony with sign-count validation.
    /// </summary>
    public async Task<Result<AuthCompleteResult, AuthError>> CompleteLoginAsync(
        IDbConnection conn,
        AuthCompleteRequest request,
        JwtConfig jwt
    )
    {
        var npgsql = Npgsql(conn);
        var now = Now();

        if (request.ProviderResponse is not AuthenticatorAssertionRawResponse assertion)
            return Err("Invalid provider response type for passkey login");

        var challengeResult = await npgsql.GetChallengeByIdAsync(request.ChallengeId, now).ConfigureAwait(false);
        if (challengeResult is not GetChallengeByIdOk { Value.Count: > 0 } challengeOk)
            return Err("Challenge not found or expired");

        var credResult = await npgsql.GetCredentialByIdAsync(assertion.Id).ConfigureAwait(false);
        if (credResult is not GetCredentialByIdOk { Value.Count: > 0 } credOk)
            return Err("Credential not found");

        var storedCred = credOk.Value[0];

        // [AUTH-LOCKOUT-CHECK] Check lockout before processing assertion
        var userCheckResult = await npgsql.GetUserByIdAsync(storedCred.user_id ?? string.Empty).ConfigureAwait(false);
        if (userCheckResult is GetUserByIdOk { Value.Count: > 0 } userCheckOk)
        {
            var userCheck = userCheckOk.Value[0];
            if (userCheck.locked_until is not null &&
                DateTime.TryParse(userCheck.locked_until, null, System.Globalization.DateTimeStyles.RoundtripKind, out var lockedUntil) &&
                lockedUntil > DateTime.UtcNow)
            {
                _logger.LogWarning("Login blocked: user {UserId} is locked until {LockedUntil}", storedCred.user_id, userCheck.locked_until);
                return Err("Account locked");
            }
        }

        var storedSignCount = (uint)(storedCred.last_sign_count ?? 0);

        Fido2NetLib.Objects.VerifyAssertionResult assertionResult;
        try
        {
            assertionResult = await _fido2.MakeAssertionAsync(new MakeAssertionParams
            {
                AssertionResponse = assertion,
                OriginalOptions = AssertionOptions.FromJson(request.OptionsJson),
                StoredPublicKey = storedCred.public_key ?? Array.Empty<byte>(),
                StoredSignatureCounter = storedSignCount,
                IsUserHandleOwnerOfCredentialIdCallback = (args, _) =>
                    Task.FromResult(storedCred.user_id == Encoding.UTF8.GetString(args.UserHandle)),
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "FIDO2 assertion failed for user {UserId}", storedCred.user_id);
            // [AUTH-LOCKOUT-INCREMENT] Increment failed login count on assertion failure
            await IncrementFailedLoginAsync(npgsql, storedCred.user_id ?? string.Empty).ConfigureAwait(false);
            return Err("Assertion failed");
        }

        // Sign count validation — detect cloned authenticators
        var newSignCount = assertionResult.SignCount;
        if (newSignCount != 0 && newSignCount <= storedSignCount)
        {
            _logger.LogWarning(
                "Credential clone detected: credentialId={CredentialId} stored={Stored} received={Received}",
                assertion.Id, storedSignCount, newSignCount
            );
            // [AUTH-LOCKOUT-INCREMENT] Increment failed login count; lock at 5 failures for 15 min
            await IncrementFailedLoginAsync(npgsql, storedCred.user_id ?? string.Empty).ConfigureAwait(false);
            return Err("Credential clone detected");
        }

        // Update sign count and last used
        using var updateCmd = npgsql.CreateCommand();
        updateCmd.CommandText = "UPDATE gk_credential SET last_sign_count = @c, last_used_at = @n WHERE id = @id";
        updateCmd.Parameters.AddWithValue("@c", (long)newSignCount);
        updateCmd.Parameters.AddWithValue("@n", now);
        updateCmd.Parameters.AddWithValue("@id", assertion.Id);
        await updateCmd.ExecuteNonQueryAsync().ConfigureAwait(false);

        // [AUTH-LOCKOUT-RESET] Reset failed login count on success
        using var resetCmd = npgsql.CreateCommand();
        resetCmd.CommandText = "UPDATE gk_user SET failed_login_count = 0, locked_until = NULL, last_login_at = @n WHERE id = @id";
        resetCmd.Parameters.AddWithValue("@n", now);
        resetCmd.Parameters.AddWithValue("@id", (object?)storedCred.user_id ?? DBNull.Value);
        await resetCmd.ExecuteNonQueryAsync().ConfigureAwait(false);

        // Consume challenge
        await DeleteChallengeAsync(npgsql, request.ChallengeId).ConfigureAwait(false);

        return await BuildResultAsync(npgsql, storedCred.user_id ?? string.Empty, jwt, now).ConfigureAwait(false);
    }

    private static async Task<Result<AuthCompleteResult, AuthError>> BuildResultAsync(
        NpgsqlConnection conn,
        string userId,
        JwtConfig jwt,
        string now
    )
    {
        var userResult = await conn.GetUserByIdAsync(userId).ConfigureAwait(false);
        var user = userResult is GetUserByIdOk { Value.Count: > 0 } uOk ? uOk.Value[0] : null;

        var rolesResult = await conn.GetUserRolesAsync(userId, now).ConfigureAwait(false);
        var roles = rolesResult is GetUserRolesOk rOk
            ? rOk.Value.Where(r => r.name is not null).Select(r => r.name!).ToList()
            : new List<string>();

        var token = TokenService.CreateToken(userId, user?.display_name, user?.email, roles, jwt.SigningKey, jwt.TokenLifetime);
        return new Result<AuthCompleteResult, AuthError>.Ok<AuthCompleteResult, AuthError>(
            new AuthCompleteResult(token, userId, user?.display_name, user?.email, roles)
        );
    }

    private static async Task DeletePendingChallengeAsync(NpgsqlConnection conn, string userId, string type)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM gk_challenge WHERE user_id = @u AND type = @t";
        cmd.Parameters.AddWithValue("@u", userId);
        cmd.Parameters.AddWithValue("@t", type);
        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    private static async Task DeleteChallengeAsync(NpgsqlConnection conn, string challengeId)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM gk_challenge WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", challengeId);
        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    private static async Task InsertUserIdentityAsync(NpgsqlConnection conn, string userId, string provider, string externalId)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText =
            @"INSERT INTO gk_user_identity (id, user_id, provider, external_id, linked_at)
              VALUES (@id, @uid, @prov, @ext, @at)
              ON CONFLICT DO NOTHING";
        cmd.Parameters.AddWithValue("@id", Guid.NewGuid().ToString());
        cmd.Parameters.AddWithValue("@uid", userId);
        cmd.Parameters.AddWithValue("@prov", provider);
        cmd.Parameters.AddWithValue("@ext", externalId);
        cmd.Parameters.AddWithValue("@at", Now());
        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    private static AttestationConveyancePreference ResolveAttestation()
    {
        var pref = Environment.GetEnvironmentVariable("ATTESTATION_PREFERENCE");
        return pref?.ToLowerInvariant() switch
        {
            "direct" => AttestationConveyancePreference.Direct,
            "indirect" => AttestationConveyancePreference.Indirect,
            _ => AttestationConveyancePreference.None,
        };
    }

    // [AUTH-LOCKOUT-INCREMENT] Atomically increments failed_login_count; sets locked_until at 5 failures.
    private static async Task IncrementFailedLoginAsync(NpgsqlConnection conn, string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return;
        var lockUntil = DateTime.UtcNow.AddMinutes(15).ToString("o", System.Globalization.CultureInfo.InvariantCulture);
        using var cmd = conn.CreateCommand();
        cmd.CommandText =
            "UPDATE gk_user SET failed_login_count = failed_login_count + 1, " +
            "locked_until = CASE WHEN failed_login_count + 1 >= 5 THEN @lockUntil ELSE locked_until END " +
            "WHERE id = @id";
        cmd.Parameters.AddWithValue("@lockUntil", lockUntil);
        cmd.Parameters.AddWithValue("@id", userId);
        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    private static NpgsqlConnection Npgsql(IDbConnection conn) =>
        conn as NpgsqlConnection ?? throw new InvalidOperationException("PasskeyAuthProvider requires NpgsqlConnection.");

    private static string Now() =>
        DateTime.UtcNow.ToString("o", System.Globalization.CultureInfo.InvariantCulture);

    private static Result<AuthCompleteResult, AuthError> Err(string reason) =>
        new Result<AuthCompleteResult, AuthError>.Error<AuthCompleteResult, AuthError>(new AuthError(reason));
}
