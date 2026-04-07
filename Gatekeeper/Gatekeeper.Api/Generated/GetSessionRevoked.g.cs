using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Npgsql;
using Outcome;
using Nimblesite.Sql.Model;

namespace Generated;

/// <summary>
/// Extension methods for 'GetSessionRevoked'.
/// </summary>
public static partial class GetSessionRevokedExtensions
{
    /// <summary>
    /// Executes 'GetSessionRevoked.sql' and maps results.
    /// </summary>
    /// <param name="connection">Open NpgsqlConnection connection.</param>
    /// <param name="jti">Query parameter.</param>
    /// <returns>Result of records or SQL error.</returns>
    public static async Task<Result<ImmutableList<GetSessionRevoked>, SqlError>> GetSessionRevokedAsync(this NpgsqlConnection connection, object jti)
    {
        const string sql = @"-- Gets the revocation status of a session
-- @jti: The session ID (JWT ID) to check
SELECT is_revoked FROM gk_session WHERE id = @jti;
";

        try
        {
            var results = ImmutableList.CreateBuilder<GetSessionRevoked>();

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
                        var item = new GetSessionRevoked(
                            reader.IsDBNull(0) ? null : reader.GetFieldValue<bool?>(0)
                        );
                        results.Add(item);
                    }
                }
            }

            return new Result<ImmutableList<GetSessionRevoked>, SqlError>.Ok<ImmutableList<GetSessionRevoked>, SqlError>(results.ToImmutable());
        }
        catch (Exception ex)
        {
            return new Result<ImmutableList<GetSessionRevoked>, SqlError>.Error<ImmutableList<GetSessionRevoked>, SqlError>(new SqlError("Database error", ex));
        }
    }
}

/// <summary>
/// Result row for 'GetSessionRevoked' query.
/// </summary>
public record GetSessionRevoked
{
    /// <summary>Column 'is_revoked'.</summary>
    public bool? is_revoked { get; init; }

    /// <summary>Initializes a new instance of GetSessionRevoked.</summary>
    public GetSessionRevoked(
        bool? is_revoked
    )
    {
        this.is_revoked = is_revoked;
    }
}
