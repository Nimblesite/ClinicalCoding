using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Npgsql;
using Outcome;
using Selecta;

namespace Generated;

/// <summary>
/// Extension methods for 'GetChallengeById'.
/// </summary>
public static partial class GetChallengeByIdExtensions
{
    /// <summary>
    /// Executes 'GetChallengeById.sql' and maps results.
    /// </summary>
    /// <param name="connection">Open NpgsqlConnection connection.</param>
    /// <param name="id">Query parameter.</param>
    /// <param name="now">Query parameter.</param>
    /// <returns>Result of records or SQL error.</returns>
    public static async Task<Result<ImmutableList<GetChallengeById>, SqlError>> GetChallengeByIdAsync(this NpgsqlConnection connection, object id, object now)
    {
        const string sql = @"-- name: GetChallengeById
SELECT id, user_id, challenge, type, created_at, expires_at
FROM gk_challenge
WHERE id = @id AND expires_at > @now;
";

        try
        {
            var results = ImmutableList.CreateBuilder<GetChallengeById>();

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
                        var item = new GetChallengeById(
                            reader.IsDBNull(0) ? null : reader.GetFieldValue<string>(0),
                            reader.IsDBNull(1) ? null : reader.GetFieldValue<string>(1),
                            reader.IsDBNull(2) ? null : reader.GetFieldValue<byte[]>(2),
                            reader.IsDBNull(3) ? null : reader.GetFieldValue<string>(3),
                            reader.IsDBNull(4) ? null : reader.GetFieldValue<string>(4),
                            reader.IsDBNull(5) ? null : reader.GetFieldValue<string>(5)
                        );
                        results.Add(item);
                    }
                }
            }

            return new Result<ImmutableList<GetChallengeById>, SqlError>.Ok<ImmutableList<GetChallengeById>, SqlError>(results.ToImmutable());
        }
        catch (Exception ex)
        {
            return new Result<ImmutableList<GetChallengeById>, SqlError>.Error<ImmutableList<GetChallengeById>, SqlError>(new SqlError("Database error", ex));
        }
    }
}

/// <summary>
/// Result row for 'GetChallengeById' query.
/// </summary>
public record GetChallengeById
{
    /// <summary>Column 'id'.</summary>
    public string id { get; init; }

    /// <summary>Column 'user_id'.</summary>
    public string user_id { get; init; }

    /// <summary>Column 'challenge'.</summary>
    public byte[] challenge { get; init; }

    /// <summary>Column 'type'.</summary>
    public string type { get; init; }

    /// <summary>Column 'created_at'.</summary>
    public string created_at { get; init; }

    /// <summary>Column 'expires_at'.</summary>
    public string expires_at { get; init; }

    /// <summary>Initializes a new instance of GetChallengeById.</summary>
    public GetChallengeById(
        string id,
        string user_id,
        byte[] challenge,
        string type,
        string created_at,
        string expires_at
    )
    {
        this.id = id;
        this.user_id = user_id;
        this.challenge = challenge;
        this.type = type;
        this.created_at = created_at;
        this.expires_at = expires_at;
    }
}
