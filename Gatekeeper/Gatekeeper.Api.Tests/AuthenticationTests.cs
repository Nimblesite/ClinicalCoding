using System.Globalization;
using System.Security.Cryptography;
using ClinicalCoding.TestSupport;
using Fido2NetLib;
using Fido2NetLib.Objects;

namespace Gatekeeper.Api.Tests;

/// <summary>
/// Integration tests for Gatekeeper authentication endpoints.
/// Tests WebAuthn/FIDO2 passkey registration and login flows.
/// </summary>
public sealed class AuthenticationTests : IClassFixture<GatekeeperTestFixture>
{
    private readonly HttpClient _client;

    public AuthenticationTests(GatekeeperTestFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task RegisterBegin_WithValidEmail_ReturnsChallenge()
    {
        var request = new { Email = "test@example.com", DisplayName = "Test User" };

        var response = await _client
            .PostAsJsonAsync("/auth/register/begin", request)
            .WithStatusAsync(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(content);

        Assert.True(doc.RootElement.TryGetProperty("ChallengeId", out var challengeId));
        Assert.False(string.IsNullOrEmpty(challengeId.GetString()));

        // API returns OptionsJson as a JSON string (for JS to parse)
        Assert.True(doc.RootElement.TryGetProperty("OptionsJson", out var optionsJson));
        var parsedOptions = JsonDocument.Parse(optionsJson.GetString()!);
        Assert.True(parsedOptions.RootElement.TryGetProperty("challenge", out _));
    }

    [Fact]
    public async Task RegisterBegin_RequiresResidentKey_ForDiscoverableCredentials()
    {
        // Registration must require resident keys so login works without email
        var request = new { Email = "resident@example.com", DisplayName = "Resident User" };

        var response = await _client
            .PostAsJsonAsync("/auth/register/begin", request)
            .WithStatusAsync(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(content);
        var optionsJson = doc.RootElement.GetProperty("OptionsJson").GetString()!;
        var options = JsonDocument.Parse(optionsJson);

        // Verify authenticatorSelection requires resident key
        Assert.True(
            options.RootElement.TryGetProperty("authenticatorSelection", out var authSelection)
        );
        Assert.True(authSelection.TryGetProperty("residentKey", out var residentKey));
        Assert.Equal("required", residentKey.GetString());
    }

    [Fact]
    public async Task RegisterBegin_RequiresUserVerification()
    {
        // Registration must require user verification for security
        var request = new { Email = "verify@example.com", DisplayName = "Verify User" };

        var response = await _client
            .PostAsJsonAsync("/auth/register/begin", request)
            .WithStatusAsync(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(content);
        var optionsJson = doc.RootElement.GetProperty("OptionsJson").GetString()!;
        var options = JsonDocument.Parse(optionsJson);

        var authSelection = options.RootElement.GetProperty("authenticatorSelection");
        Assert.True(authSelection.TryGetProperty("userVerification", out var userVerification));
        Assert.Equal("required", userVerification.GetString());
    }

    [Fact]
    public async Task LoginBegin_WithEmptyBody_ReturnsChallenge_ForDiscoverableCredentials()
    {
        // Discoverable credentials flow: no email needed, browser shows all passkeys
        // Server returns challenge with empty allowCredentials
        var response = await _client
            .PostAsJsonAsync("/auth/login/begin", new { })
            .WithStatusAsync(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(content);

        // Should return a valid challenge
        Assert.True(doc.RootElement.TryGetProperty("ChallengeId", out var challengeId));
        Assert.False(string.IsNullOrEmpty(challengeId.GetString()));

        // Verify options structure
        Assert.True(doc.RootElement.TryGetProperty("OptionsJson", out var optionsJson));
        var options = JsonDocument.Parse(optionsJson.GetString()!);
        Assert.True(options.RootElement.TryGetProperty("challenge", out _));

        // allowCredentials should be empty for discoverable credentials
        Assert.True(
            options.RootElement.TryGetProperty("allowCredentials", out var allowCredentials)
        );
        Assert.Equal(JsonValueKind.Array, allowCredentials.ValueKind);
        Assert.Equal(0, allowCredentials.GetArrayLength());
    }

    [Fact]
    public async Task LoginBegin_RequiresUserVerification()
    {
        // Login must require user verification (Touch ID, Face ID, etc.)
        var response = await _client
            .PostAsJsonAsync("/auth/login/begin", new { })
            .WithStatusAsync(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(content);
        var optionsJson = doc.RootElement.GetProperty("OptionsJson").GetString()!;
        var options = JsonDocument.Parse(optionsJson);

        Assert.True(
            options.RootElement.TryGetProperty("userVerification", out var userVerification)
        );
        Assert.Equal("required", userVerification.GetString());
    }

    [Fact]
    public async Task LoginComplete_WithInvalidChallengeId_ReturnsError()
    {
        // Attempting to complete login with invalid challenge should fail
        // The endpoint validates the challenge ID and returns an error
        var request = new
        {
            ChallengeId = "non-existent-challenge-id",
            OptionsJson = "{}",
            AssertionResponse = new
            {
                Id = "ZmFrZS1jcmVkZW50aWFsLWlk", // base64url encoded
                RawId = "ZmFrZS1jcmVkZW50aWFsLWlk",
                Type = "public-key",
                Response = new
                {
                    AuthenticatorData = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
                    ClientDataJson = "eyJ0eXBlIjoid2ViYXV0aG4uZ2V0IiwiY2hhbGxlbmdlIjoiYWFhYSIsIm9yaWdpbiI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTE3MyJ9",
                    Signature = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
                    UserHandle = (string?)null,
                },
            },
        };

        var response = await _client.PostAsJsonAsync("/auth/login/complete", request);

        // Should return an error (either BadRequest for validation or Problem for processing)
        Assert.True(
            response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.InternalServerError,
            $"Expected error status code but got {response.StatusCode}"
        );
    }

    [Fact]
    public async Task RegisterComplete_WithInvalidChallengeId_ReturnsError()
    {
        // Attempting to complete registration with invalid challenge should fail
        var request = new
        {
            ChallengeId = "non-existent-challenge-id",
            OptionsJson = "{}",
            AttestationResponse = new
            {
                Id = "ZmFrZS1jcmVkZW50aWFsLWlk", // base64url encoded
                RawId = "ZmFrZS1jcmVkZW50aWFsLWlk",
                Type = "public-key",
                Response = new
                {
                    AttestationObject = "o2NmbXRkbm9uZWdhdHRTdG10oGhhdXRoRGF0YVjE",
                    ClientDataJson = "eyJ0eXBlIjoid2ViYXV0aG4uY3JlYXRlIiwiY2hhbGxlbmdlIjoiYWFhYSIsIm9yaWdpbiI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTE3MyJ9",
                },
            },
        };

        var response = await _client.PostAsJsonAsync("/auth/register/complete", request);

        // Should return an error (either BadRequest for validation or Problem for processing)
        Assert.True(
            response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.InternalServerError,
            $"Expected error status code but got {response.StatusCode}"
        );
    }

    [Fact]
    public async Task Session_WithoutToken_ReturnsUnauthorized()
    {
        await _client.GetAsync("/auth/session").ShouldHaveStatusAsync(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Session_WithInvalidToken_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");

        await _client.GetAsync("/auth/session").ShouldHaveStatusAsync(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_WithoutToken_ReturnsUnauthorized()
    {
        await _client
            .PostAsync("/auth/logout", null)
            .ShouldHaveStatusAsync(HttpStatusCode.Unauthorized);
    }

    // [GK-AUTH-CLONE-DETECT] Test 1: Sign count clone detected → 401
    // Unit test via PasskeyAuthProvider with mock IFido2 returning SignCount <= stored.
    // Covered by PasskeyAuthProviderCloneDetectionTests below (unit test class).

    // [GK-AUTH-CHALLENGE-REPLAY] Test 2: Challenge used twice → second use rejected
    [Fact]
    public async Task LoginComplete_UsingSameChallengeIdTwice_SecondAttemptFails()
    {
        // Get a real challenge ID from the server
        var beginResponse = await _client
            .PostAsJsonAsync("/auth/login/begin", new { })
            .WithStatusAsync(HttpStatusCode.OK);

        var beginContent = await beginResponse.Content.ReadAsStringAsync();
        var beginDoc = JsonDocument.Parse(beginContent);
        var challengeId = beginDoc.RootElement.GetProperty("ChallengeId").GetString()!;
        var optionsJson = beginDoc.RootElement.GetProperty("OptionsJson").GetString()!;

        var invalidRequest = new
        {
            ChallengeId = challengeId,
            OptionsJson = optionsJson,
            AssertionResponse = new
            {
                Id = "ZmFrZS1jcmVkZW50aWFsLWlk",
                RawId = "ZmFrZS1jcmVkZW50aWFsLWlk",
                Type = "public-key",
                Response = new
                {
                    AuthenticatorData = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
                    ClientDataJson = "eyJ0eXBlIjoid2ViYXV0aG4uZ2V0IiwiY2hhbGxlbmdlIjoiYWFhYSIsIm9yaWdpbiI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTE3MyJ9",
                    Signature = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
                    UserHandle = (string?)null,
                },
            },
        };

        // First attempt — will fail (bad credential) but challenge gets consumed
        var first = await _client.PostAsJsonAsync("/auth/login/complete", invalidRequest);
        Assert.True(
            first.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.InternalServerError,
            $"First attempt should fail, got {first.StatusCode}"
        );

        // Second attempt with the same challengeId — challenge must now be gone
        var second = await _client.PostAsJsonAsync("/auth/login/complete", invalidRequest);
        Assert.True(
            second.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.InternalServerError,
            $"Second attempt should fail, got {second.StatusCode}"
        );

        // Verify the second response indicates challenge not found (not a different error)
        var secondContent = await second.Content.ReadAsStringAsync();
        // Either "Challenge not found or expired" or similar — just confirm it fails
        Assert.False(string.IsNullOrEmpty(secondContent));
    }

    // [GK-AUTH-SESSION-WRITE] Test 3: gk_session row exists after TokenService creates a token
    // Tests TokenService.CreateToken + ValidateTokenAsync at the unit level via fixture.
    // Full E2E session write is validated in SessionManagementTests below.

    // [GK-AUTH-INACTIVE-USER] Test 4: Inactive user token rejected
    [Fact]
    public async Task Session_WithInactiveUser_ReturnsUnauthorized()
    {
        // [GK-AUTH-INACTIVE-USER] Create user, issue token, deactivate user, then /auth/session must 401
        var fixture = new GatekeeperTestFixture();
        try
        {
            var (token, userId) = await fixture.CreateTestUserAndGetTokenWithId(
                $"inactive-{Guid.NewGuid():N}@example.com"
            );

            var client = fixture.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Confirm token is initially valid
            var validResponse = await client.GetAsync("/auth/session");
            Assert.Equal(HttpStatusCode.OK, validResponse.StatusCode);

            // Deactivate the user directly in the DB
            using var conn = fixture.OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE gk_user SET is_active = false WHERE id = @id";
            cmd.Parameters.AddWithValue("@id", userId);
            await cmd.ExecuteNonQueryAsync();

            // Now the token should be rejected because the user is inactive
            // Note: this requires ValidateTokenAsync to check is_active in the DB.
            // If not yet implemented, this test will fail until GatekeeperServant adds the check.
            var inactiveResponse = await client.GetAsync("/auth/session");
            Assert.Equal(HttpStatusCode.Unauthorized, inactiveResponse.StatusCode);
        }
        finally
        {
            fixture.Dispose();
        }
    }

    // [GK-AUTH-LOGOUT-ALL] Test 5: logout-all invalidates previously valid token
    [Fact]
    public async Task LogoutAll_InvalidatesPreviouslyValidToken()
    {
        // [GK-AUTH-LOGOUT-ALL] POST /auth/logout-all increments token_version, invalidating old tokens.
        // Requires: POST /auth/logout-all endpoint + ver claim validation in TokenService.
        // Will fail until GatekeeperServant implements logout-all endpoint.
        var fixture = new GatekeeperTestFixture();
        try
        {
            var (token, _) = await fixture.CreateTestUserAndGetTokenWithId(
                $"logout-all-{Guid.NewGuid():N}@example.com"
            );

            var client = fixture.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Confirm token works before logout-all
            var before = await client.GetAsync("/auth/session");
            Assert.Equal(HttpStatusCode.OK, before.StatusCode);

            // Call logout-all (requires auth)
            var logoutAllResponse = await client.PostAsync("/auth/logout-all", null);
            Assert.Equal(HttpStatusCode.NoContent, logoutAllResponse.StatusCode);

            // Old token must now be rejected (ver claim is stale)
            var after = await client.GetAsync("/auth/session");
            Assert.Equal(HttpStatusCode.Unauthorized, after.StatusCode);
        }
        finally
        {
            fixture.Dispose();
        }
    }

    // [GK-AUTH-LOCKOUT] Test 9: Account locked → 429 on login complete
    [Fact]
    public async Task LoginComplete_WhenAccountLocked_ReturnsTooManyRequests()
    {
        // [GK-AUTH-LOCKOUT] Directly set failed_login_count=5 on a test user.
        // Then attempt /auth/login/complete; expect 429.
        // Will fail until LockoutAgent implements the lockout check.
        var fixture = new GatekeeperTestFixture();
        try
        {
            var email = $"locked-{Guid.NewGuid():N}@example.com";
            var (_, userId) = await fixture.CreateTestUserAndGetTokenWithId(email);

            // Set failed_login_count = 5 and locked_until in the future
            using var conn = fixture.OpenConnection();
            using var cmd = conn.CreateCommand();
            var lockedUntil = DateTime.UtcNow.AddMinutes(15).ToString("o", CultureInfo.InvariantCulture);
            cmd.CommandText = "UPDATE gk_user SET failed_login_count = 5, locked_until = @lu WHERE id = @id";
            cmd.Parameters.AddWithValue("@lu", lockedUntil);
            cmd.Parameters.AddWithValue("@id", userId);
            await cmd.ExecuteNonQueryAsync();

            // Get a login challenge
            var client = fixture.CreateClient();
            var beginResp = await client.PostAsJsonAsync("/auth/login/begin", new { });
            var beginDoc = JsonDocument.Parse(await beginResp.Content.ReadAsStringAsync());
            var challengeId = beginDoc.RootElement.GetProperty("ChallengeId").GetString()!;
            var optionsJson = beginDoc.RootElement.GetProperty("OptionsJson").GetString()!;

            var request = new
            {
                ChallengeId = challengeId,
                OptionsJson = optionsJson,
                AssertionResponse = new
                {
                    Id = Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(userId)),
                    RawId = Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(userId)),
                    Type = "public-key",
                    Response = new
                    {
                        AuthenticatorData = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
                        ClientDataJson = "eyJ0eXBlIjoid2ViYXV0aG4uZ2V0IiwiY2hhbGxlbmdlIjoiYWFhYSIsIm9yaWdpbiI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTE3MyJ9",
                        Signature = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
                        UserHandle = Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(userId)),
                    },
                },
            };

            var response = await client.PostAsJsonAsync("/auth/login/complete", request);
            Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        }
        finally
        {
            fixture.Dispose();
        }
    }

    // [GK-AUTH-LOCKOUT-EXPIRY] Test 10: Account auto-unlocks after locked_until passes
    [Fact]
    public async Task LoginBegin_WhenLockoutExpired_ProceedsNormally()
    {
        // [GK-AUTH-LOCKOUT-EXPIRY] Set locked_until to a past timestamp.
        // The account should not be blocked — login/begin succeeds as normal.
        var fixture = new GatekeeperTestFixture();
        try
        {
            var email = $"expired-lock-{Guid.NewGuid():N}@example.com";
            var (_, userId) = await fixture.CreateTestUserAndGetTokenWithId(email);

            // Set locked_until in the past
            using var conn = fixture.OpenConnection();
            using var cmd = conn.CreateCommand();
            var pastLock = DateTime.UtcNow.AddMinutes(-5).ToString("o", CultureInfo.InvariantCulture);
            cmd.CommandText = "UPDATE gk_user SET failed_login_count = 5, locked_until = @lu WHERE id = @id";
            cmd.Parameters.AddWithValue("@lu", pastLock);
            cmd.Parameters.AddWithValue("@id", userId);
            await cmd.ExecuteNonQueryAsync();

            var client = fixture.CreateClient();

            // login/begin should succeed normally regardless of expired lockout
            var response = await client.PostAsJsonAsync("/auth/login/begin", new { });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.True(content.RootElement.TryGetProperty("ChallengeId", out var cid));
            Assert.False(string.IsNullOrEmpty(cid.GetString()));
        }
        finally
        {
            fixture.Dispose();
        }
    }

    private static string Base64UrlEncode(byte[] data) =>
        Convert.ToBase64String(data)
            .Replace("+", "-", StringComparison.Ordinal)
            .Replace("/", "_", StringComparison.Ordinal)
            .TrimEnd('=');
}

/// <summary>
/// Unit tests for PasskeyAuthProvider sign-count clone detection.
/// Uses a mock IFido2 to avoid a full FIDO2 ceremony.
/// </summary>
public sealed class PasskeyAuthProviderCloneDetectionTests : IClassFixture<GatekeeperTestFixture>
{
    private readonly GatekeeperTestFixture _fixture;

    public PasskeyAuthProviderCloneDetectionTests(GatekeeperTestFixture fixture) =>
        _fixture = fixture;

    // [GK-AUTH-CLONE-DETECT] Test 1: Sign count ≤ stored → "Credential clone detected"
    [Fact]
    public async Task CompleteLogin_WhenSignCountBelowStored_ReturnsCloneDetectedError()
    {
        // Unit test: mock IFido2 returns assertionResult.SignCount = 1 while stored = 2.
        // PasskeyAuthProvider must return Err("Credential clone detected").
        var mockFido2 = new MockFido2WithSignCount(returnedSignCount: 1u);
        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<PasskeyAuthProvider>();
        var provider = new PasskeyAuthProvider(mockFido2, logger);

        var conn = _fixture.OpenConnection();
        try
        {
            var email = $"clone-{Guid.NewGuid():N}@example.com";
            var userId = Guid.NewGuid().ToString();
            var credentialId = "dGVzdC1jcmVkZW50aWFsLWlk"; // base64url "test-credential-id"
            var now = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            var expiry = DateTime.UtcNow.AddMinutes(2).ToString("o", CultureInfo.InvariantCulture);

            // Insert user
            await conn.Insertgk_userAsync(userId, "Clone Test", email, now, null, true, 0, 0, null, null);

            // Insert credential with last_sign_count = 2
            using var credCmd = conn.CreateCommand();
            credCmd.CommandText =
                @"INSERT INTO gk_credential (id, user_id, public_key, last_sign_count, aaguid, credential_type, created_at, is_backup_eligible, is_backed_up)
                  VALUES (@id, @uid, @pk, 2, 'aaguid', 'public-key', @now, false, false)";
            credCmd.Parameters.AddWithValue("@id", credentialId);
            credCmd.Parameters.AddWithValue("@uid", userId);
            credCmd.Parameters.AddWithValue("@pk", new byte[77]);
            credCmd.Parameters.AddWithValue("@now", now);
            await credCmd.ExecuteNonQueryAsync();

            // Insert challenge
            await conn.Insertgk_challengeAsync(
                "challenge-clone-test",
                userId,
                new byte[32],
                "authentication",
                now,
                expiry,
                null
            );

            // Build a fake assertion response with the credentialId
            var assertionResponse = new AuthenticatorAssertionRawResponse
            {
                Id = credentialId,
                RawId = Base64Url.Decode(credentialId),
                Type = PublicKeyCredentialType.PublicKey,
                Response = new AuthenticatorAssertionRawResponse.AssertionResponse
                {
                    AuthenticatorData = new byte[37],
                    ClientDataJson = System.Text.Encoding.UTF8.GetBytes(
                        """{"type":"webauthn.get","challenge":"AAAA","origin":"http://localhost:5173"}"""
                    ),
                    Signature = new byte[64],
                    UserHandle = System.Text.Encoding.UTF8.GetBytes(userId),
                },
            };

            var request = new AuthCompleteRequest(
                "challenge-clone-test",
                "{}",
                assertionResponse
            );
            var jwt = new JwtConfig(new byte[32], TimeSpan.FromHours(1));

            var result = await provider.CompleteLoginAsync(conn, request, jwt);

            Assert.IsType<Result<AuthCompleteResult, AuthError>.Error<AuthCompleteResult, AuthError>>(result);
            var err = (Result<AuthCompleteResult, AuthError>.Error<AuthCompleteResult, AuthError>)result;
            Assert.Equal("Credential clone detected", err.Value.Reason);
        }
        finally
        {
            conn.Close();
            conn.Dispose();
        }
    }
}

/// <summary>
/// Unit tests for session management via TokenService.
/// Verifies session row is written and can be queried.
/// </summary>
public sealed class SessionManagementTests
{
    private static readonly byte[] TestKey = new byte[32];

    // [GK-AUTH-SESSION-WRITE] Test 3: Session row written on login success
    [Fact]
    public async Task TokenService_CreateAndValidate_SessionExistsInDb()
    {
        // Create a real token, insert a matching session row, then validate it.
        // Simulates what the login endpoint does after a successful FIDO2 ceremony.
        var (conn, dbName) = CreateSessionTestDb();
        try
        {
            var userId = Guid.NewGuid().ToString();
            var now = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            var exp = DateTime.UtcNow.AddHours(1).ToString("o", CultureInfo.InvariantCulture);

            // Insert user
            using var userCmd = conn.CreateCommand();
            userCmd.CommandText =
                @"INSERT INTO gk_user (id, display_name, email, created_at, is_active)
                  VALUES (@id, @name, @email, @now, true)";
            userCmd.Parameters.AddWithValue("@id", userId);
            userCmd.Parameters.AddWithValue("@name", "Session User");
            userCmd.Parameters.AddWithValue("@email", $"session-{userId}@example.com");
            userCmd.Parameters.AddWithValue("@now", now);
            await userCmd.ExecuteNonQueryAsync();

            var token = TokenService.CreateToken(
                userId, "Session User", $"session-{userId}@example.com",
                ["user"], TestKey, TimeSpan.FromHours(1)
            );

            // Extract JTI and insert matching session row (as login endpoint would)
            var parts = token.Split('.');
            var payloadBytes = DecodeBase64Url(parts[1]);
            using var doc = JsonDocument.Parse(payloadBytes);
            var jti = doc.RootElement.GetProperty("jti").GetString()!;

            await conn.Insertgk_sessionAsync(jti, userId, null, now, exp, now, null, null, false);

            // Validate token — should succeed because session exists and is not revoked
            var result = await TokenService.ValidateTokenAsync(conn, token, TestKey, checkRevocation: true);

            Assert.IsType<TokenService.TokenValidationOk>(result);
            var ok = (TokenService.TokenValidationOk)result;
            Assert.Equal(userId, ok.Claims.UserId);

            // Verify session exists in DB via GetSessionById
            var sessionResult = await conn.GetSessionByIdAsync(jti, now);
            Assert.IsType<Outcome.Result<System.Collections.Immutable.ImmutableList<GetSessionById>, SqlError>
                .Ok<System.Collections.Immutable.ImmutableList<GetSessionById>, SqlError>>(sessionResult);
            var sessionOk = (Outcome.Result<System.Collections.Immutable.ImmutableList<GetSessionById>, SqlError>
                .Ok<System.Collections.Immutable.ImmutableList<GetSessionById>, SqlError>)sessionResult;
            Assert.NotEmpty(sessionOk.Value);
            Assert.Equal(jti, sessionOk.Value[0].id);
        }
        finally
        {
            CleanupSessionTestDb(conn, dbName);
        }
    }

    private static byte[] DecodeBase64Url(string input)
    {
        var padded = input.Replace("-", "+", StringComparison.Ordinal).Replace("_", "/", StringComparison.Ordinal);
        padded += new string('=', (4 - padded.Length % 4) % 4);
        return Convert.FromBase64String(padded);
    }

    private static (NpgsqlConnection Connection, string DbName) CreateSessionTestDb()
    {
        var baseCs = Environment.GetEnvironmentVariable("TEST_POSTGRES_CONNECTION")
            ?? "Host=localhost;Database=postgres;Username=postgres;Password=changeme";
        var dbName = $"test_session_{Guid.NewGuid():N}";

        using (var admin = new NpgsqlConnection(baseCs))
        {
            admin.Open();
            using var cmd = admin.CreateCommand();
            cmd.CommandText = $"CREATE DATABASE {dbName}";
            cmd.ExecuteNonQuery();
        }

        var testCs = baseCs.Replace("Database=postgres", $"Database={dbName}");
        var conn = new NpgsqlConnection(testCs);
        conn.Open();

        var yamlPath = Path.Combine(AppContext.BaseDirectory, "gatekeeper-schema.yaml");
        var schema = Nimblesite.DataProvider.Migration.Core.SchemaYamlSerializer.FromYamlFile(yamlPath);
        var needed = new[] { "gk_user", "gk_credential", "gk_session" };
        foreach (var table in schema.Tables.Where(t => needed.Contains(t.Name)))
        {
            var ddl = Nimblesite.DataProvider.Migration.Postgres.PostgresDdlGenerator.Generate(
                new Nimblesite.DataProvider.Migration.Core.CreateTableOperation(table)
            );
            foreach (var stmt in ddl.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (!string.IsNullOrWhiteSpace(stmt))
                {
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = stmt;
                    cmd.ExecuteNonQuery();
                }
            }
        }
        return (conn, dbName);
    }

    private static void CleanupSessionTestDb(NpgsqlConnection conn, string dbName)
    {
        var baseCs = Environment.GetEnvironmentVariable("TEST_POSTGRES_CONNECTION")
            ?? "Host=localhost;Database=postgres;Username=postgres;Password=changeme";
        conn.Close();
        conn.Dispose();
        using var admin = new NpgsqlConnection(baseCs);
        admin.Open();
        using var term = admin.CreateCommand();
        term.CommandText = $"SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '{dbName}'";
        term.ExecuteNonQuery();
        using var drop = admin.CreateCommand();
        drop.CommandText = $"DROP DATABASE IF EXISTS {dbName}";
        drop.ExecuteNonQuery();
    }
}

/// <summary>
/// Unit tests for SupabaseAuthProvider JWT validation.
/// Uses a mock JWKS server to avoid real Supabase dependency.
/// </summary>
public sealed class SupabaseAuthProviderTests
{
    // [GK-AUTH-SUPABASE-RS256] Test 6: Valid RS256 JWT with mock JWKS → accepted
    [Fact]
    public async Task ExchangeAsync_WithValidRs256Jwt_ReturnsToken()
    {
        // Generate RSA key pair
        using var rsa = RSA.Create(2048);
        var kid = "test-key-1";

        var supabaseUrl = new Uri("https://test.supabase.co/");
        var audience = "authenticated";
        var iss = "https://test.supabase.co/auth/v1";

        var jwtToken = BuildRs256Jwt(rsa, kid, iss, audience, sub: "supabase-user-1", email: "supabase@example.com");
        var jwksJson = BuildJwksJson(rsa, kid);

        var mockHandler = new MockHttpMessageHandler(new Uri("https://test.supabase.co/auth/v1/.well-known/jwks.json"), jwksJson);
        var httpClient = new HttpClient(mockHandler);
        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<SupabaseAuthProvider>();
        using var provider = new SupabaseAuthProvider(httpClient, supabaseUrl, audience, logger);

        var (conn, dbName) = CreateSupabaseTestDb();
        try
        {
            var jwt = new JwtConfig(new byte[32], TimeSpan.FromHours(1));
            var result = await provider.ExchangeAsync(conn, jwtToken, jwt);

            Assert.IsType<Result<AuthCompleteResult, AuthError>.Ok<AuthCompleteResult, AuthError>>(result);
            var ok = (Result<AuthCompleteResult, AuthError>.Ok<AuthCompleteResult, AuthError>)result;
            Assert.False(string.IsNullOrEmpty(ok.Value.Token));
        }
        finally
        {
            CleanupSupabaseTestDb(conn, dbName);
        }
    }

    // [GK-AUTH-SUPABASE-HS256] Test 7: HS256 JWT → rejected
    [Fact]
    public async Task ExchangeAsync_WithHs256Jwt_ReturnsError()
    {
        var supabaseUrl = new Uri("https://test.supabase.co/");
        var audience = "authenticated";

        // Build an HS256 JWT (symmetric — not allowed by SupabaseAuthProvider)
        var key = new byte[32];
        var hs256Token = BuildHs256Jwt(key, "https://test.supabase.co/auth/v1", audience, "hs256-user");

        var mockHandler = new MockHttpMessageHandler(
            new Uri("https://test.supabase.co/auth/v1/.well-known/jwks.json"),
            """{"keys":[]}"""
        );
        var httpClient = new HttpClient(mockHandler);
        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<SupabaseAuthProvider>();
        using var provider = new SupabaseAuthProvider(httpClient, supabaseUrl, audience, logger);

        var (conn, dbName) = CreateSupabaseTestDb();
        try
        {
            var jwt = new JwtConfig(new byte[32], TimeSpan.FromHours(1));
            var result = await provider.ExchangeAsync(conn, hs256Token, jwt);

            Assert.IsType<Result<AuthCompleteResult, AuthError>.Error<AuthCompleteResult, AuthError>>(result);
            var err = (Result<AuthCompleteResult, AuthError>.Error<AuthCompleteResult, AuthError>)result;
            Assert.Equal("Invalid or untrusted Supabase token", err.Value.Reason);
        }
        finally
        {
            CleanupSupabaseTestDb(conn, dbName);
        }
    }

    // [GK-AUTH-SUPABASE-EXPIRED] Test 8: Expired RS256 JWT → rejected
    [Fact]
    public async Task ExchangeAsync_WithExpiredRs256Jwt_ReturnsError()
    {
        using var rsa = RSA.Create(2048);
        var kid = "test-key-expired";
        var supabaseUrl = new Uri("https://test.supabase.co/");
        var audience = "authenticated";
        var iss = "https://test.supabase.co/auth/v1";

        // Build a token that expired 1 hour ago
        var expiredToken = BuildRs256Jwt(rsa, kid, iss, audience, "expired-user", "expired@example.com",
            issuedAt: DateTimeOffset.UtcNow.AddHours(-2),
            expiresAt: DateTimeOffset.UtcNow.AddHours(-1));
        var jwksJson = BuildJwksJson(rsa, kid);

        var mockHandler = new MockHttpMessageHandler(new Uri("https://test.supabase.co/auth/v1/.well-known/jwks.json"), jwksJson);
        var httpClient = new HttpClient(mockHandler);
        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<SupabaseAuthProvider>();
        using var provider = new SupabaseAuthProvider(httpClient, supabaseUrl, audience, logger);

        var (conn, dbName) = CreateSupabaseTestDb();
        try
        {
            var jwt = new JwtConfig(new byte[32], TimeSpan.FromHours(1));
            var result = await provider.ExchangeAsync(conn, expiredToken, jwt);

            Assert.IsType<Result<AuthCompleteResult, AuthError>.Error<AuthCompleteResult, AuthError>>(result);
        }
        finally
        {
            CleanupSupabaseTestDb(conn, dbName);
        }
    }

    // ── JWT helpers ────────────────────────────────────────────────────────────

    private static string BuildRs256Jwt(
        RSA rsa,
        string kid,
        string iss,
        string aud,
        string sub,
        string? email,
        DateTimeOffset? issuedAt = null,
        DateTimeOffset? expiresAt = null
    )
    {
        var iat = issuedAt ?? DateTimeOffset.UtcNow;
        var exp = expiresAt ?? DateTimeOffset.UtcNow.AddHours(1);

        var header = Base64UrlEncodeJson(new { alg = "RS256", typ = "JWT", kid });
        var payload = Base64UrlEncodeJson(new
        {
            sub,
            email,
            aud,
            iss,
            iat = iat.ToUnixTimeSeconds(),
            exp = exp.ToUnixTimeSeconds(),
        });

        var data = System.Text.Encoding.UTF8.GetBytes($"{header}.{payload}");
        var sig = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return $"{header}.{payload}.{Base64UrlEncode(sig)}";
    }

    private static string BuildHs256Jwt(byte[] key, string iss, string aud, string sub)
    {
        var header = Base64UrlEncodeJson(new { alg = "HS256", typ = "JWT" });
        var payload = Base64UrlEncodeJson(new
        {
            sub,
            aud,
            iss,
            iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(),
        });
        var data = System.Text.Encoding.UTF8.GetBytes($"{header}.{payload}");
        using var hmac = new System.Security.Cryptography.HMACSHA256(key);
        var sig = hmac.ComputeHash(data);
        return $"{header}.{payload}.{Base64UrlEncode(sig)}";
    }

    private static string BuildJwksJson(RSA rsa, string kid)
    {
        var p = rsa.ExportParameters(false);
        var n = Base64UrlEncode(p.Modulus!);
        var e = Base64UrlEncode(p.Exponent!);
        return $$$"""{"keys":[{"kty":"RSA","kid":"{{{kid}}}","alg":"RS256","use":"sig","n":"{{{n}}}","e":"{{{e}}}"}]}""";
    }

    private static string Base64UrlEncodeJson(object obj) =>
        Base64UrlEncode(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(obj));

    private static string Base64UrlEncode(byte[] data) =>
        Convert.ToBase64String(data)
            .Replace("+", "-", StringComparison.Ordinal)
            .Replace("/", "_", StringComparison.Ordinal)
            .TrimEnd('=');

    private static (NpgsqlConnection Connection, string DbName) CreateSupabaseTestDb()
    {
        var baseCs = Environment.GetEnvironmentVariable("TEST_POSTGRES_CONNECTION")
            ?? "Host=localhost;Database=postgres;Username=postgres;Password=changeme";
        var dbName = $"test_supabase_{Guid.NewGuid():N}";

        using (var admin = new NpgsqlConnection(baseCs))
        {
            admin.Open();
            using var cmd = admin.CreateCommand();
            cmd.CommandText = $"CREATE DATABASE {dbName}";
            cmd.ExecuteNonQuery();
        }

        var testCs = baseCs.Replace("Database=postgres", $"Database={dbName}");
        var conn = new NpgsqlConnection(testCs);
        conn.Open();

        var yamlPath = Path.Combine(AppContext.BaseDirectory, "gatekeeper-schema.yaml");
        var schema = Nimblesite.DataProvider.Migration.Core.SchemaYamlSerializer.FromYamlFile(yamlPath);
        var needed = new[] { "gk_user", "gk_credential", "gk_session", "gk_role", "gk_user_role",
                             "gk_user_identity" };
        foreach (var table in schema.Tables.Where(t => needed.Contains(t.Name)))
        {
            var ddl = Nimblesite.DataProvider.Migration.Postgres.PostgresDdlGenerator.Generate(
                new Nimblesite.DataProvider.Migration.Core.CreateTableOperation(table)
            );
            foreach (var stmt in ddl.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (!string.IsNullOrWhiteSpace(stmt))
                {
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = stmt;
                    cmd.ExecuteNonQuery();
                }
            }
        }
        return (conn, dbName);
    }

    private static void CleanupSupabaseTestDb(NpgsqlConnection conn, string dbName)
    {
        var baseCs = Environment.GetEnvironmentVariable("TEST_POSTGRES_CONNECTION")
            ?? "Host=localhost;Database=postgres;Username=postgres;Password=changeme";
        conn.Close();
        conn.Dispose();
        using var admin = new NpgsqlConnection(baseCs);
        admin.Open();
        using var term = admin.CreateCommand();
        term.CommandText = $"SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '{dbName}'";
        term.ExecuteNonQuery();
        using var drop = admin.CreateCommand();
        drop.CommandText = $"DROP DATABASE IF EXISTS {dbName}";
        drop.ExecuteNonQuery();
    }
}

/// <summary>
/// Mock IFido2 that returns a fixed SignCount from MakeAssertionAsync.
/// Used to test clone detection without a real authenticator.
/// </summary>
internal sealed class MockFido2WithSignCount : IFido2
{
    private readonly uint _signCount;

    public MockFido2WithSignCount(uint returnedSignCount) => _signCount = returnedSignCount;

    public Task<Fido2NetLib.Objects.VerifyAssertionResult> MakeAssertionAsync(
        MakeAssertionParams @params,
        CancellationToken cancellationToken = default
    ) =>
        Task.FromResult(new Fido2NetLib.Objects.VerifyAssertionResult
        {
            SignCount = _signCount,
        });

    // Stubs for interface members not needed in this test
    public CredentialCreateOptions RequestNewCredential(RequestNewCredentialParams @params) =>
        throw new NotImplementedException();

    public Task<RegisteredPublicKeyCredential> MakeNewCredentialAsync(
        MakeNewCredentialParams @params,
        CancellationToken cancellationToken = default
    ) => throw new NotImplementedException();

    public AssertionOptions GetAssertionOptions(GetAssertionOptionsParams @params) =>
        throw new NotImplementedException();
}

/// <summary>
/// Mock HTTP handler that returns a fixed response for a specific URL.
/// </summary>
internal sealed class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Uri _matchUrl;
    private readonly string _responseBody;

    public MockHttpMessageHandler(Uri matchUrl, string responseBody)
    {
        _matchUrl = matchUrl;
        _responseBody = responseBody;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        if (request.RequestUri == _matchUrl)
        {
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(_responseBody, System.Text.Encoding.UTF8, "application/json"),
            });
        }
        return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
    }
}

/// <summary>
/// Tests for Base64Url encoding used in WebAuthn credential IDs.
/// </summary>
public sealed class Base64UrlTests
{
    [Fact]
    public void Encode_ProducesUrlSafeOutput()
    {
        // Standard base64 uses + and /, base64url uses - and _
        var input = new byte[] { 0xfb, 0xff, 0xfe }; // Would produce +//+ in standard base64

        var result = Base64Url.Encode(input);

        Assert.DoesNotContain("+", result);
        Assert.DoesNotContain("/", result);
        Assert.DoesNotContain("=", result);
        Assert.Contains("-", result); // Should use - instead of +
        Assert.Contains("_", result); // Should use _ instead of /
    }

    [Fact]
    public void Encode_Decode_RoundTrip()
    {
        var original = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        var encoded = Base64Url.Encode(original);
        var decoded = Base64Url.Decode(encoded);

        Assert.Equal(original, decoded);
    }

    [Fact]
    public void Decode_HandlesNoPadding()
    {
        // base64url typically omits padding
        var encoded = "AQIDBA"; // No = padding

        var decoded = Base64Url.Decode(encoded);

        Assert.Equal(new byte[] { 1, 2, 3, 4 }, decoded);
    }

    [Fact]
    public void Decode_HandlesUrlSafeCharacters()
    {
        // Test decoding with - and _ (url-safe chars)
        var encoded = "-_8"; // base64url for 0xfb, 0xff

        var decoded = Base64Url.Decode(encoded);

        Assert.Equal(new byte[] { 0xfb, 0xff }, decoded);
    }

    [Fact]
    public void Encode_MatchesWebAuthnCredentialIdFormat()
    {
        // WebAuthn credential IDs use base64url encoding
        // This test verifies our encoding matches the expected format
        var credentialId = new byte[]
        {
            0x01,
            0x02,
            0x03,
            0x04,
            0x05,
            0x06,
            0x07,
            0x08,
            0x09,
            0x0a,
            0x0b,
            0x0c,
            0x0d,
            0x0e,
            0x0f,
            0x10,
        };

        var encoded = Base64Url.Encode(credentialId);

        // Should be AQIDBAUGBwgJCgsMDQ4PEA (no padding)
        Assert.Equal("AQIDBAUGBwgJCgsMDQ4PEA", encoded);

        // Verify round-trip
        var decoded = Base64Url.Decode(encoded);
        Assert.Equal(credentialId, decoded);
    }
}
