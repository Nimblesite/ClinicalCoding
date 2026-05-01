#pragma warning disable IDE0037

using Microsoft.AspNetCore.Http.Json;
using InitError = Outcome.Result<bool, string>.Error<bool, string>;

var builder = WebApplication.CreateBuilder(args);

var logPath =
    Environment.GetEnvironmentVariable("LOG_PATH")
    ?? (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"
        ? "/tmp/gatekeeper.log"
        : Path.Combine(AppContext.BaseDirectory, "gatekeeper.log"));
builder.Logging.AddFileLogging(logPath);

builder.Services.Configure<JsonOptions>(o => o.SerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddCors(o =>
    o.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// FIDO2
var serverDomain = builder.Configuration["Fido2:ServerDomain"] ?? "localhost";
var serverName   = builder.Configuration["Fido2:ServerName"]   ?? "Gatekeeper";
var origin       = builder.Configuration["Fido2:Origin"]       ?? "http://localhost:5173";
builder.Services.AddFido2(o =>
{
    o.ServerDomain = serverDomain;
    o.ServerName   = serverName;
    o.Origins      = new HashSet<string> { origin };
    o.TimestampDriftTolerance = 300000;
});

// Database
var connectionString =
    builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("Connection string 'Postgres' is required");
builder.Services.AddSingleton(new DbConfig(connectionString));

// JWT
var signingKeyBase64 = builder.Configuration["Jwt:SigningKey"];
var signingKey = string.IsNullOrEmpty(signingKeyBase64)
    ? new byte[32]
    : Convert.FromBase64String(signingKeyBase64);
builder.Services.AddSingleton(new JwtConfig(signingKey, TimeSpan.FromHours(24)));

// Auth providers
builder.Services.AddScoped<PasskeyAuthProvider>();
builder.Services.AddHttpClient<SupabaseAuthProvider>(c => c.Timeout = TimeSpan.FromSeconds(10));
var supabaseUrl = builder.Configuration["Supabase:Url"] ?? "https://placeholder.supabase.co";
var supabaseAud = builder.Configuration["Supabase:Audience"] ?? "authenticated";
builder.Services.AddSingleton(sp =>
    new SupabaseAuthProvider(
        sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(SupabaseAuthProvider)),
        new Uri(supabaseUrl),
        supabaseAud,
        sp.GetRequiredService<ILogger<SupabaseAuthProvider>>()
    ));

var app = builder.Build();

// Initialise DB
using (var conn = OpenNpgsqlConnection(connectionString))
{
    if (DatabaseSetup.Initialize(conn, app.Logger) is InitError err)
        Environment.FailFast(err.Value);
}

app.UseCors("AllowAll");

// ── /auth ────────────────────────────────────────────────────────────────────

var auth = app.MapGroup("/auth").WithTags("Authentication");

auth.MapPost("/register/begin",
    async (RegisterBeginRequest req, PasskeyAuthProvider passkey, DbConfig db) =>
    {
        using var conn = OpenConnection(db);
        var result = await passkey.BeginAsync(conn, new AuthBeginRequest(req.Email, req.DisplayName))
            .ConfigureAwait(false);
        return result switch
        {
            Result<AuthBeginResult, AuthError>.Ok<AuthBeginResult, AuthError> ok =>
                Results.Ok(new { ok.Value.ChallengeId, OptionsJson = ok.Value.OptionsJson }),
            Result<AuthBeginResult, AuthError>.Error<AuthBeginResult, AuthError> err =>
                Results.BadRequest(new { Error = err.Value.Reason }),
        };
    });

auth.MapPost("/register/complete",
    async (RegisterCompleteRequest req, PasskeyAuthProvider passkey, DbConfig db, JwtConfig jwt) =>
    {
        using var conn = OpenConnection(db);
        var result = await passkey.CompleteAsync(
                conn,
                new AuthCompleteRequest(req.ChallengeId, req.OptionsJson, req.AttestationResponse, req.DeviceName),
                jwt)
            .ConfigureAwait(false);
        return ToResult(result);
    });

auth.MapPost("/login/begin",
    async (PasskeyAuthProvider passkey, DbConfig db) =>
    {
        using var conn = OpenConnection(db);
        var result = await passkey.BeginLoginAsync(conn).ConfigureAwait(false);
        return result switch
        {
            Result<AuthBeginResult, AuthError>.Ok<AuthBeginResult, AuthError> ok =>
                Results.Ok(new { ok.Value.ChallengeId, OptionsJson = ok.Value.OptionsJson }),
            Result<AuthBeginResult, AuthError>.Error<AuthBeginResult, AuthError> err =>
                Results.BadRequest(new { Error = err.Value.Reason }),
        };
    });

auth.MapPost("/login/complete",
    async (LoginCompleteRequest req, PasskeyAuthProvider passkey, DbConfig db, JwtConfig jwt) =>
    {
        using var conn = OpenConnection(db);
        var result = await passkey.CompleteLoginAsync(
                conn,
                new AuthCompleteRequest(req.ChallengeId, req.OptionsJson, req.AssertionResponse),
                jwt)
            .ConfigureAwait(false);
        return ToResult(result);
    });

// Supabase token exchange — fully independent of passkey
auth.MapPost("/supabase/exchange",
    async (SupabaseExchangeRequest req, SupabaseAuthProvider supabase, DbConfig db, JwtConfig jwt) =>
    {
        if (string.IsNullOrEmpty(supabase.ProviderName) || string.IsNullOrEmpty(req.SupabaseToken))
            return Results.BadRequest(new { Error = "Missing token" });
        using var conn = OpenConnection(db);
        var result = await supabase.ExchangeAsync(conn, req.SupabaseToken, jwt).ConfigureAwait(false);
        return ToResult(result);
    });

// Link a Supabase identity to an existing passkey account (or vice-versa).
// Requires a valid Gatekeeper token (the user must already be authenticated).
auth.MapPost("/identity/link/supabase",
    async (LinkSupabaseRequest req, HttpContext ctx, SupabaseAuthProvider supabase, DbConfig db, JwtConfig jwt) =>
    {
        var token = TokenService.ExtractBearerToken(ctx.Request.Headers.Authorization);
        if (string.IsNullOrEmpty(token))
            return Results.Unauthorized();

        using var conn = OpenConnection(db);
        var validateResult = await TokenService
            .ValidateTokenAsync(conn, token, jwt.SigningKey, checkRevocation: true)
            .ConfigureAwait(false);
        if (validateResult is not TokenService.TokenValidationOk ok)
            return Results.Unauthorized();

        // Validate the Supabase token and extract the sub
        var exchangeResult = await supabase.ExchangeAsync(conn, req.SupabaseToken, jwt).ConfigureAwait(false);
        if (exchangeResult is not Result<AuthCompleteResult, AuthError>.Ok<AuthCompleteResult, AuthError>)
            return Results.BadRequest(new { Error = "Invalid Supabase token" });

        // The exchange already handled identity linking via ResolveUserAsync.
        // If the Supabase sub resolved to a *different* user we do not merge accounts —
        // providers are decoupled and account merging is an admin operation.
        return Results.Ok(new { Linked = true });
    });

auth.MapGet("/session",
    async (HttpContext ctx, DbConfig db, JwtConfig jwt) =>
    {
        var token = TokenService.ExtractBearerToken(ctx.Request.Headers.Authorization);
        if (string.IsNullOrEmpty(token))
            return Results.Unauthorized();

        using var conn = OpenConnection(db);
        var result = await TokenService.ValidateTokenAsync(conn, token, jwt.SigningKey, checkRevocation: true)
            .ConfigureAwait(false);
        if (result is not TokenService.TokenValidationOk ok)
            return Results.Unauthorized();

        return Results.Ok(new
        {
            ok.Claims.UserId,
            ok.Claims.DisplayName,
            ok.Claims.Email,
            ok.Claims.Roles,
            ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(ok.Claims.Exp)
                .ToString("o", CultureInfo.InvariantCulture),
        });
    });

auth.MapPost("/logout",
    async (HttpContext ctx, DbConfig db, JwtConfig jwt) =>
    {
        var token = TokenService.ExtractBearerToken(ctx.Request.Headers.Authorization);
        if (string.IsNullOrEmpty(token))
            return Results.Unauthorized();

        using var conn = OpenConnection(db);
        var result = await TokenService.ValidateTokenAsync(conn, token, jwt.SigningKey, checkRevocation: false)
            .ConfigureAwait(false);
        if (result is TokenService.TokenValidationOk ok)
            await TokenService.RevokeTokenAsync(conn, ok.Claims.Jti).ConfigureAwait(false);

        return Results.NoContent();
    });

// Dev-only token (returns 404 when a real signing key is configured)
auth.MapGet("/dev-token",
    (JwtConfig jwt) =>
    {
        if (!IsDevKey(jwt.SigningKey))
            return Results.NotFound();

        var token = TokenService.CreateToken(
            "e2e-test-user", "E2E Test User", "e2etest@example.com",
            ["admin", "user"], jwt.SigningKey, TimeSpan.FromHours(1));
        return Results.Ok(new { Token = token });
    });

// ── /authz ───────────────────────────────────────────────────────────────────

var authz = app.MapGroup("/authz").WithTags("Authorization");

authz.MapGet("/check",
    async (string permission, string? resourceType, string? resourceId, HttpContext ctx, DbConfig db, JwtConfig jwt) =>
    {
        var claims = await ValidateRequestAsync(ctx, db, jwt).ConfigureAwait(false);
        if (claims is null)
            return Results.Unauthorized();

        using var conn = OpenConnection(db);
        var (allowed, reason) = await AuthorizationService
            .CheckPermissionAsync(conn, claims.UserId, permission, resourceType, resourceId, Now())
            .ConfigureAwait(false);
        return Results.Ok(new { Allowed = allowed, Reason = reason });
    });

authz.MapGet("/permissions",
    async (HttpContext ctx, DbConfig db, JwtConfig jwt) =>
    {
        var claims = await ValidateRequestAsync(ctx, db, jwt).ConfigureAwait(false);
        if (claims is null)
            return Results.Unauthorized();

        using var conn = OpenConnection(db);
        var npgsql = (NpgsqlConnection)conn;
        var result = await npgsql.GetUserPermissionsAsync(claims.UserId, Now()).ConfigureAwait(false);
        var perms = result is GetUserPermissionsOk ok
            ? ok.Value.Select(p => new { p.code, p.source_name, p.source_type, p.scope_type, p.scope_value }).ToList()
            : [];
        return Results.Ok(new { Permissions = perms });
    });

authz.MapPost("/evaluate",
    async (EvaluateRequest request, HttpContext ctx, DbConfig db, JwtConfig jwt) =>
    {
        var claims = await ValidateRequestAsync(ctx, db, jwt).ConfigureAwait(false);
        if (claims is null)
            return Results.Unauthorized();

        if (request.Checks.Count > 50)
            return Results.BadRequest(new { Error = "Maximum 50 checks per request" });

        using var conn = OpenConnection(db);
        var now = Now();
        var results = new List<object>();
        foreach (var check in request.Checks)
        {
            var (allowed, _) = await AuthorizationService
                .CheckPermissionAsync(conn, claims.UserId, check.Permission, check.ResourceType, check.ResourceId, now)
                .ConfigureAwait(false);
            results.Add(new { check.Permission, check.ResourceId, Allowed = allowed });
        }
        return Results.Ok(new { Results = results });
    });

// ── /health ──────────────────────────────────────────────────────────────────

app.MapGet("/health", () => Results.Ok(new { Status = "healthy", Service = "Gatekeeper.Api" }));

app.Run();

// ── Helpers ──────────────────────────────────────────────────────────────────

static string Now() => DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);

static NpgsqlConnection OpenNpgsqlConnection(string cs)
{
    var conn = new NpgsqlConnection(cs);
    conn.Open();
    return conn;
}

static IDbConnection OpenConnection(DbConfig db)
{
    var conn = new NpgsqlConnection(db.ConnectionString);
    conn.Open();
    return conn;
}

static bool IsDevKey(byte[] key) => key.Length == 32 && key.All(b => b == 0);

static IResult ToResult(Result<AuthCompleteResult, AuthError> result) =>
    result switch
    {
        Result<AuthCompleteResult, AuthError>.Ok<AuthCompleteResult, AuthError> ok => Results.Ok(new
        {
            ok.Value.Token,
            ok.Value.UserId,
            ok.Value.DisplayName,
            ok.Value.Email,
            ok.Value.Roles,
        }),
        Result<AuthCompleteResult, AuthError>.Error<AuthCompleteResult, AuthError> err =>
            Results.BadRequest(new { Error = err.Value.Reason }),
    };

static async Task<TokenService.TokenClaims?> ValidateRequestAsync(HttpContext ctx, DbConfig db, JwtConfig jwt)
{
    var token = TokenService.ExtractBearerToken(ctx.Request.Headers.Authorization);
    if (string.IsNullOrEmpty(token))
        return null;

    using var conn = OpenConnection(db);
    var result = await TokenService.ValidateTokenAsync(conn, token, jwt.SigningKey, checkRevocation: true)
        .ConfigureAwait(false);
    return result is TokenService.TokenValidationOk ok ? ok.Claims : null;
}

// ── Types ─────────────────────────────────────────────────────────────────────

namespace Gatekeeper.Api
{
    /// <summary>Program entry point marker for WebApplicationFactory.</summary>
    public partial class Program { }

    /// <summary>Database connection configuration.</summary>
    public sealed record DbConfig(string ConnectionString);

    /// <summary>JWT signing configuration.</summary>
    public sealed record JwtConfig(byte[] SigningKey, TimeSpan TokenLifetime);

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
}
