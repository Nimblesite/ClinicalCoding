using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Npgsql;
using Outcome;
using Selecta;

namespace Generated;

/// <summary>
/// Extension methods for 'GetSessionForRevoke'.
/// </summary>
public static partial class GetSessionForRevokeExtensions
{
    /// <summary>
    /// Executes 'GetSessionForRevoke.sql' and maps results.
    /// </summary>
    /// <param name="connection">Open NpgsqlConnection connection.</param>
    /// <param name="jti">Query parameter.</param>
    /// <returns>Result of records or SQL error.</returns>
    public static async Task<Result<ImmutableList<GetSessionForRevoke>, SqlError>> GetSessionForRevokeAsync(this NpgsqlConnection connection, object jti)
    {
        const string sql = @"-- Gets a session for revocation (no filters)
-- @jti: The session ID (JWT ID) to get
SELECT id, user_id, credential_id, created_at, expires_at, last_activity_at,
       ip_address, user_agent, is_revoked
FROM gk_session
WHERE id = @jti;
";

        try
        {
            var results = ImmutableList.CreateBuilder<GetSessionForRevoke>();

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
                        var item = new GetSessionForRevoke(
                            reader.IsDBNull(0) ? null : reader.GetFieldValue<string>(0),
                            reader.IsDBNull(1) ? null : reader.GetFieldValue<string>(1),
                            reader.IsDBNull(2) ? null : reader.GetFieldValue<string>(2),
                            reader.IsDBNull(3) ? null : reader.GetFieldValue<string>(3),
                            reader.IsDBNull(4) ? null : reader.GetFieldValue<string>(4),
                            reader.IsDBNull(5) ? null : reader.GetFieldValue<string>(5),
                            reader.IsDBNull(6) ? null : reader.GetFieldValue<string>(6),
                            reader.IsDBNull(7) ? null : reader.GetFieldValue<string>(7),
                            reader.IsDBNull(8) ? null : reader.GetFieldValue<bool?>(8)
                        );
                        results.Add(item);
                    }
                }
            }

            return new Result<ImmutableList<GetSessionForRevoke>, SqlError>.Ok<ImmutableList<GetSessionForRevoke>, SqlError>(results.ToImmutable());
        }
        catch (Exception ex)
        {
            return new Result<ImmutableList<GetSessionForRevoke>, SqlError>.Error<ImmutableList<GetSessionForRevoke>, SqlError>(new SqlError("Database error", ex));
        }
    }
}

/// <summary>
/// Result row for 'GetSessionForRevoke' query.
/// </summary>
public record GetSessionForRevoke
{
    /// <summary>Column 'id'.</summary>
    public string id { get; init; }

    /// <summary>Column 'user_id'.</summary>
    public string user_id { get; init; }

    /// <summary>Column 'credential_id'.</summary>
    public string credential_id { get; init; }

    /// <summary>Column 'created_at'.</summary>
    public string created_at { get; init; }

    /// <summary>Column 'expires_at'.</summary>
    public string expires_at { get; init; }

    /// <summary>Column 'last_activity_at'.</summary>
    public string last_activity_at { get; init; }

    /// <summary>Column 'ip_address'.</summary>
    public string ip_address { get; init; }

    /// <summary>Column 'user_agent'.</summary>
    public string user_agent { get; init; }

    /// <summary>Column 'is_revoked'.</summary>
    public bool? is_revoked { get; init; }

    /// <summary>Initializes a new instance of GetSessionForRevoke.</summary>
    public GetSessionForRevoke(
        string id,
        string user_id,
        string credential_id,
        string created_at,
        string expires_at,
        string last_activity_at,
        string ip_address,
        string user_agent,
        bool? is_revoked
    )
    {
        this.id = id;
        this.user_id = user_id;
        this.credential_id = credential_id;
        this.created_at = created_at;
        this.expires_at = expires_at;
        this.last_activity_at = last_activity_at;
        this.ip_address = ip_address;
        this.user_agent = user_agent;
        this.is_revoked = is_revoked;
    }
}
