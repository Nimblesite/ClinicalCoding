// Thin wrappers over DataProvider-generated NpgsqlConnection extensions.
// The generated layer is Npgsql-typed by design; these adapters are the
// only place outside Program.cs where NpgsqlConnection is referenced.
// All application code above this file uses IDbConnection.

namespace Gatekeeper.Api;

/// <summary>
/// Database operation adapters that bridge IDbConnection to the generated Npgsql extensions.
/// </summary>
internal static class DbExtensions
{
    /// <summary>Revokes the session identified by <paramref name="jti"/>.</summary>
    internal static Task<Result<
        System.Collections.Immutable.ImmutableList<Generated.RevokeSession>,
        SqlError>> RevokeSessionAdapterAsync(IDbConnection conn, string jti) =>
        AsNpgsql(conn).RevokeSessionAsync(jti);

    /// <summary>Checks whether the session identified by <paramref name="jti"/> is revoked.</summary>
    internal static Task<Result<
        System.Collections.Immutable.ImmutableList<Generated.GetSessionRevoked>,
        SqlError>> GetSessionRevokedAdapterAsync(IDbConnection conn, string jti) =>
        AsNpgsql(conn).GetSessionRevokedAsync(jti);

    /// <summary>Gets a user by ID, including is_active and token_version for token validation.</summary>
    internal static Task<Result<
        System.Collections.Immutable.ImmutableList<Generated.GetUserById>,
        SqlError>> GetUserByIdAdapterAsync(IDbConnection conn, string userId) =>
        AsNpgsql(conn).GetUserByIdAsync(userId);

    /// <summary>Inserts a session row. Called after successful login or registration.</summary>
    internal static Task<Result<Guid?, SqlError>> InsertSessionAdapterAsync(
        IDbConnection conn,
        string id, string? userId, string? credentialId, string? createdAt, string? expiresAt,
        string? lastActivityAt, string? ipAddress, string? userAgent, bool? isRevoked) =>
        AsNpgsql(conn).Insertgk_sessionAsync(id, userId, credentialId, createdAt, expiresAt, lastActivityAt, ipAddress, userAgent, isRevoked);

    /// <summary>Increments the token_version for a user, invalidating all outstanding tokens.</summary>
    internal static async Task<Result<int, SqlError>> IncrementTokenVersionAsync(IDbConnection conn, string userId)
    {
        try
        {
            var npgsql = AsNpgsql(conn);
            await using var cmd = npgsql.CreateCommand();
            cmd.CommandText = "UPDATE gk_user SET token_version = token_version + 1 WHERE id = @id";
            cmd.Parameters.AddWithValue("@id", userId);
            var rows = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            return new Result<int, SqlError>.Ok<int, SqlError>(rows);
        }
        catch (Exception ex)
        {
            return new Result<int, SqlError>.Error<int, SqlError>(SqlError.FromException(ex));
        }
    }

    private static NpgsqlConnection AsNpgsql(IDbConnection conn) =>
        conn as NpgsqlConnection
            ?? throw new InvalidOperationException(
                $"Expected NpgsqlConnection but got {conn.GetType().Name}."
            );
}
