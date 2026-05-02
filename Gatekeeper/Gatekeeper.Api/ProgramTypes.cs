namespace Gatekeeper.Api;

/// <summary>Program entry point marker for WebApplicationFactory.</summary>
public partial class Program { }

/// <summary>Database connection configuration.</summary>
public sealed record DbConfig(string ConnectionString);

/// <summary>JWT signing configuration.</summary>
public sealed record JwtConfig(byte[] SigningKey, TimeSpan TokenLifetime, string Issuer = "gatekeeper", string Audience = "gatekeeper");

/// <summary>Request to begin passkey registration.</summary>
public sealed record RegisterBeginRequest(string Email, string DisplayName);

/// <summary>Request to complete passkey registration.</summary>
public sealed record RegisterCompleteRequest(
    string ChallengeId,
    string OptionsJson,
    AuthenticatorAttestationRawResponse AttestationResponse,
    string? DeviceName
);

/// <summary>Request to complete passkey login.</summary>
public sealed record LoginCompleteRequest(
    string ChallengeId,
    string OptionsJson,
    AuthenticatorAssertionRawResponse AssertionResponse
);

/// <summary>Request to exchange a Supabase JWT for a Gatekeeper token.</summary>
public sealed record SupabaseExchangeRequest(string SupabaseToken);

/// <summary>Request to link a Supabase identity to the current authenticated user.</summary>
public sealed record LinkSupabaseRequest(string SupabaseToken);

/// <summary>Bulk permission evaluation request.</summary>
public sealed record EvaluateRequest(List<PermissionCheck> Checks);

/// <summary>Single permission check item.</summary>
public sealed record PermissionCheck(string Permission, string? ResourceType, string? ResourceId);

/// <summary>
/// HTTP 429 response with Retry-After header for locked accounts. [AUTH-LOCKOUT-429]
/// </summary>
public sealed class AccountLockedResult : IResult
{
    /// <inheritdoc/>
    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = 429;
        httpContext.Response.Headers["Retry-After"] = "900";
        httpContext.Response.ContentType = "application/json";
        return httpContext.Response.WriteAsync("{\"Error\":\"Account locked\"}");
    }
}

/// <summary>Base64URL encoding utilities for WebAuthn credential IDs.</summary>
public static class Base64Url
{
    /// <summary>Encodes bytes to base64url string.</summary>
    public static string Encode(byte[] input) =>
        Convert.ToBase64String(input)
            .Replace("+", "-", StringComparison.Ordinal)
            .Replace("/", "_", StringComparison.Ordinal)
            .TrimEnd('=');

    /// <summary>Decodes base64url string to bytes.</summary>
    public static byte[] Decode(string input)
    {
        var padded = input
            .Replace("-", "+", StringComparison.Ordinal)
            .Replace("_", "/", StringComparison.Ordinal);
        var padding = (4 - (padded.Length % 4)) % 4;
        padded += new string('=', padding);
        return Convert.FromBase64String(padded);
    }
}
