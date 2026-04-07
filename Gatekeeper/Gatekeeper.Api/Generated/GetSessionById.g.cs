using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Npgsql;
using Outcome;
using Nimblesite.Sql.Model;

namespace Generated;

/// <summary>
/// Extension methods for 'GetSessionById'.
/// </summary>
public static partial class GetSessionByIdExtensions
{
    /// <summary>
    /// Executes 'GetSessionById.sql' and maps results.
    /// </summary>
    /// <param name="connection">Open NpgsqlConnection connection.</param>
    /// <param name="id">Query parameter.</param>
    /// <param name="now">Query parameter.</param>
    /// <returns>Result of records or SQL error.</returns>
    public static async Task<Result<ImmutableList<GetSessionById>, SqlError>> GetSessionByIdAsync(this NpgsqlConnection connection, object id, object now)
    {
        const string sql = @"-- name: GetSessionById
SELECT s.id, s.user_id, s.credential_id, s.created_at, s.expires_at, s.last_activity_at,
       s.ip_address, s.user_agent, s.is_revoked,
       u.display_name, u.email
FROM gk_session s
JOIN gk_user u ON s.user_id = u.id
WHERE s.id = @id AND s.is_revoked = false AND s.expires_at > @now AND u.is_active = true;
";

        try
        {
            var results = ImmutableList.CreateBuilder<GetSessionById>();

            using (var command = new NpgsqlCommand(sql, connection))
            {
                if (id is not null and not DBNull)
                    command.Parameters.AddWithValue("@id", id);
                else
                    command.Parameters.Add(new NpgsqlParameter("@id", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });
                if (now is not null and not DBNull)
                    command.Parameters.AddWithValue("@now", now);
                else
                    command.Parameters.Add(new NpgsqlParameter("@now", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var item = new GetSessionById(
                            reader.IsDBNull(0) ? null : reader.GetFieldValue<string>(0),
                            reader.IsDBNull(1) ? null : reader.GetFieldValue<string>(1),
                            reader.IsDBNull(2) ? null : reader.GetFieldValue<string>(2),
                            reader.IsDBNull(3) ? null : reader.GetFieldValue<string>(3),
                            reader.IsDBNull(4) ? null : reader.GetFieldValue<string>(4),
                            reader.IsDBNull(5) ? null : reader.GetFieldValue<string>(5),
                            reader.IsDBNull(6) ? null : reader.GetFieldValue<string>(6),
                            reader.IsDBNull(7) ? null : reader.GetFieldValue<string>(7),
                            reader.IsDBNull(8) ? null : reader.GetFieldValue<bool?>(8),
                            reader.IsDBNull(9) ? null : reader.GetFieldValue<string>(9),
                            reader.IsDBNull(10) ? null : reader.GetFieldValue<string>(10)
                        );
                        results.Add(item);
                    }
                }
            }

            return new Result<ImmutableList<GetSessionById>, SqlError>.Ok<ImmutableList<GetSessionById>, SqlError>(results.ToImmutable());
        }
        catch (Exception ex)
        {
            return new Result<ImmutableList<GetSessionById>, SqlError>.Error<ImmutableList<GetSessionById>, SqlError>(new SqlError("Database error", ex));
        }
    }
}

/// <summary>
/// Result row for 'GetSessionById' query.
/// </summary>
public record GetSessionById
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

    /// <summary>Column 'display_name'.</summary>
    public string display_name { get; init; }

    /// <summary>Column 'email'.</summary>
    public string email { get; init; }

    /// <summary>Initializes a new instance of GetSessionById.</summary>
    public GetSessionById(
        string id,
        string user_id,
        string credential_id,
        string created_at,
        string expires_at,
        string last_activity_at,
        string ip_address,
        string user_agent,
        bool? is_revoked,
        string display_name,
        string email
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
        this.display_name = display_name;
        this.email = email;
    }
}
