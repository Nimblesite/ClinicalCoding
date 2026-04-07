using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Npgsql;
using Outcome;
using Nimblesite.Sql.Model;

namespace Generated;

/// <summary>
/// Extension methods for 'RevokeSession'.
/// </summary>
public static partial class RevokeSessionExtensions
{
    /// <summary>
    /// Executes 'RevokeSession.sql' and maps results.
    /// </summary>
    /// <param name="connection">Open NpgsqlConnection connection.</param>
    /// <param name="jti">Query parameter.</param>
    /// <returns>Result of records or SQL error.</returns>
    public static async Task<Result<ImmutableList<RevokeSession>, SqlError>> RevokeSessionAsync(this NpgsqlConnection connection, object jti)
    {
        const string sql = @"-- Revokes a session by setting is_revoked = true
-- @jti: The session ID (JWT ID) to revoke
UPDATE gk_session SET is_revoked = true WHERE id = @jti RETURNING id, is_revoked;
";

        try
        {
            var results = ImmutableList.CreateBuilder<RevokeSession>();

            using (var command = new NpgsqlCommand(sql, connection))
            {
                if (jti is not null and not DBNull)
                    command.Parameters.AddWithValue("@jti", jti);
                else
                    command.Parameters.Add(new NpgsqlParameter("@jti", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var item = new RevokeSession(
                            reader.IsDBNull(0) ? null : reader.GetFieldValue<string>(0),
                            reader.IsDBNull(1) ? null : reader.GetFieldValue<bool?>(1)
                        );
                        results.Add(item);
                    }
                }
            }

            return new Result<ImmutableList<RevokeSession>, SqlError>.Ok<ImmutableList<RevokeSession>, SqlError>(results.ToImmutable());
        }
        catch (Exception ex)
        {
            return new Result<ImmutableList<RevokeSession>, SqlError>.Error<ImmutableList<RevokeSession>, SqlError>(new SqlError("Database error", ex));
        }
    }
}

/// <summary>
/// Result row for 'RevokeSession' query.
/// </summary>
public record RevokeSession
{
    /// <summary>Column 'id'.</summary>
    public string id { get; init; }

    /// <summary>Column 'is_revoked'.</summary>
    public bool? is_revoked { get; init; }

    /// <summary>Initializes a new instance of RevokeSession.</summary>
    public RevokeSession(
        string id,
        bool? is_revoked
    )
    {
        this.id = id;
        this.is_revoked = is_revoked;
    }
}
