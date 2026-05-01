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

    private static NpgsqlConnection AsNpgsql(IDbConnection conn) =>
        conn as NpgsqlConnection
            ?? throw new InvalidOperationException(
                $"Expected NpgsqlConnection but got {conn.GetType().Name}."
            );
}
