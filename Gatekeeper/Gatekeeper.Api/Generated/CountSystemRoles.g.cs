using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Npgsql;
using Outcome;
using Nimblesite.Sql.Model;

namespace Generated;

/// <summary>
/// Extension methods for 'CountSystemRoles'.
/// </summary>
public static partial class CountSystemRolesExtensions
{
    /// <summary>
    /// Executes 'CountSystemRoles.sql' and maps results.
    /// </summary>
    /// <param name="connection">Open NpgsqlConnection connection.</param>
    /// <returns>Result of records or SQL error.</returns>
    public static async Task<Result<ImmutableList<CountSystemRoles>, SqlError>> CountSystemRolesAsync(this NpgsqlConnection connection)
    {
        const string sql = @"-- name: CountSystemRoles
SELECT COUNT(*) as cnt FROM gk_role WHERE is_system = true;
";

        try
        {
            var results = ImmutableList.CreateBuilder<CountSystemRoles>();

            using (var command = new NpgsqlCommand(sql, connection))
            {

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var item = new CountSystemRoles(
                            reader.IsDBNull(0) ? default(long) : reader.GetFieldValue<long>(0)
                        );
                        results.Add(item);
                    }
                }
            }

            return new Result<ImmutableList<CountSystemRoles>, SqlError>.Ok<ImmutableList<CountSystemRoles>, SqlError>(results.ToImmutable());
        }
        catch (Exception ex)
        {
            return new Result<ImmutableList<CountSystemRoles>, SqlError>.Error<ImmutableList<CountSystemRoles>, SqlError>(new SqlError("Database error", ex));
        }
    }
}

/// <summary>
/// Result row for 'CountSystemRoles' query.
/// </summary>
public record CountSystemRoles
{
    /// <summary>Column 'cnt'.</summary>
    public long cnt { get; init; }

    /// <summary>Initializes a new instance of CountSystemRoles.</summary>
    public CountSystemRoles(
        long cnt
    )
    {
        this.cnt = cnt;
    }
}
