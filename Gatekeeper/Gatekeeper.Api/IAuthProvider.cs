namespace Gatekeeper.Api;

/// <summary>
/// Abstracts an authentication provider. Each provider proves identity independently
/// and hands off resolved user data to the shared token pipeline. Providers have
/// zero knowledge of each other.
/// </summary>
public interface IAuthProvider
{
    /// <summary>Unique provider identifier stored in gk_user_identity.provider.</summary>
    string ProviderName { get; }
}

/// <summary>
/// A provider that supports a two-step begin/complete ceremony (e.g. passkey).
/// </summary>
public interface IChallengeAuthProvider : IAuthProvider
{
    Task<Result<AuthBeginResult, AuthError>> BeginAsync(IDbConnection conn, AuthBeginRequest request);
    Task<Result<AuthCompleteResult, AuthError>> CompleteAsync(IDbConnection conn, AuthCompleteRequest request, JwtConfig jwt);
}

/// <summary>
/// A provider that validates an external token in a single step (e.g. Supabase JWT).
/// </summary>
public interface ITokenExchangeProvider : IAuthProvider
{
    Task<Result<AuthCompleteResult, AuthError>> ExchangeAsync(IDbConnection conn, string externalToken, JwtConfig jwt);
}

/// <summary>Request to begin a challenge-based auth flow.</summary>
public sealed record AuthBeginRequest(string? Email, string? DisplayName);

/// <summary>Result of beginning a challenge-based auth flow.</summary>
public sealed record AuthBeginResult(string ChallengeId, string OptionsJson);

/// <summary>Request to complete a challenge-based auth flow.</summary>
public sealed record AuthCompleteRequest(string ChallengeId, string OptionsJson, object ProviderResponse, string? DeviceName = null);

/// <summary>Successful authentication result — provider-agnostic.</summary>
public sealed record AuthCompleteResult(
    string Token,
    string UserId,
    string? DisplayName,
    string? Email,
    IReadOnlyList<string> Roles,
    string? CredentialId = null
);

/// <summary>Authentication failure.</summary>
public sealed record AuthError(string Reason);
