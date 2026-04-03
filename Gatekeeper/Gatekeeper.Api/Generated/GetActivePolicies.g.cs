using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Npgsql;
using Outcome;
using Selecta;

namespace Generated;

/// <summary>
/// Extension methods for 'GetActivePolicies'.
/// </summary>
public static partial class GetActivePoliciesExtensions
{
    /// <summary>
    /// Executes 'GetActivePolicies.sql' and maps results.
    /// </summary>
    /// <param name="connection">Open NpgsqlConnection connection.</param>
    /// <param name="resource_type">Query parameter.</param>
    /// <param name="action">Query parameter.</param>
    /// <returns>Result of records or SQL error.</returns>
    public static async Task<Result<ImmutableList<GetActivePolicies>, SqlError>> GetActivePoliciesAsync(this NpgsqlConnection connection, object resource_type, object action)
    {
        const string sql = @"-- name: GetActivePolicies
SELECT id, name, description, resource_type, action, condition, effect, priority
FROM gk_policy
WHERE is_active = true
  AND (resource_type = @resource_type OR resource_type = '*')
  AND (action = @action OR action = '*')
ORDER BY priority DESC;
";

        try
        {
            var results = ImmutableList.CreateBuilder<GetActivePolicies>();

            using (var command = new NpgsqlCommand(sql, connection))
            {
                if (resource_type is not null and not DBNull)
                    command.Parameters.AddWithValue("@resource_type", resource_type);
                else
                    command.Parameters.Add(new NpgsqlParameter("@resource_type", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });
                if (action is not null and not DBNull)
                    command.Parameters.AddWithValue("@action", action);
                else
                    command.Parameters.Add(new NpgsqlParameter("@action", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var item = new GetActivePolicies(
                            reader.IsDBNull(0) ? null : reader.GetFieldValue<string>(0),
                            reader.IsDBNull(1) ? null : reader.GetFieldValue<string>(1),
                            reader.IsDBNull(2) ? null : reader.GetFieldValue<string>(2),
                            reader.IsDBNull(3) ? null : reader.GetFieldValue<string>(3),
                            reader.IsDBNull(4) ? null : reader.GetFieldValue<string>(4),
                            reader.IsDBNull(5) ? null : reader.GetFieldValue<string>(5),
                            reader.IsDBNull(6) ? null : reader.GetFieldValue<string>(6),
                            reader.IsDBNull(7) ? default(long) : reader.GetFieldValue<long>(7)
                        );
                        results.Add(item);
                    }
                }
            }

            return new Result<ImmutableList<GetActivePolicies>, SqlError>.Ok<ImmutableList<GetActivePolicies>, SqlError>(results.ToImmutable());
        }
        catch (Exception ex)
        {
            return new Result<ImmutableList<GetActivePolicies>, SqlError>.Error<ImmutableList<GetActivePolicies>, SqlError>(new SqlError("Database error", ex));
        }
    }
}

/// <summary>
/// Result row for 'GetActivePolicies' query.
/// </summary>
public record GetActivePolicies
{
    /// <summary>Column 'id'.</summary>
    public string id { get; init; }

    /// <summary>Column 'name'.</summary>
    public string name { get; init; }

    /// <summary>Column 'description'.</summary>
    public string description { get; init; }

    /// <summary>Column 'resource_type'.</summary>
    public string resource_type { get; init; }

    /// <summary>Column 'action'.</summary>
    public string action { get; init; }

    /// <summary>Column 'condition'.</summary>
    public string condition { get; init; }

    /// <summary>Column 'effect'.</summary>
    public string effect { get; init; }

    /// <summary>Column 'priority'.</summary>
    public long priority { get; init; }

    /// <summary>Initializes a new instance of GetActivePolicies.</summary>
    public GetActivePolicies(
        string id,
        string name,
        string description,
        string resource_type,
        string action,
        string condition,
        string effect,
        long priority
    )
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.resource_type = resource_type;
        this.action = action;
        this.condition = condition;
        this.effect = effect;
        this.priority = priority;
    }
}
