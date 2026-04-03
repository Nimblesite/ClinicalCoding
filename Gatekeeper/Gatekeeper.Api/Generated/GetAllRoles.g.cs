using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Npgsql;
using Outcome;
using Selecta;

namespace Generated;

/// <summary>
/// Extension methods for 'GetAllRoles'.
/// </summary>
public static partial class GetAllRolesExtensions
{
    /// <summary>
    /// Executes 'GetAllRoles.sql' and maps results.
    /// </summary>
    /// <param name="connection">Open NpgsqlConnection connection.</param>
    /// <returns>Result of records or SQL error.</returns>
    public static async Task<Result<ImmutableList<GetAllRoles>, SqlError>> GetAllRolesAsync(this NpgsqlConnection connection)
    {
        const string sql = @"-- name: GetAllRoles
SELECT id, name, description, is_system, created_at, parent_role_id
FROM gk_role
ORDER BY name;
";

        try
        {
            var results = ImmutableList.CreateBuilder<GetAllRoles>();

            using (var command = new NpgsqlCommand(sql, connection))
            {

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var item = new GetAllRoles(
                            reader.IsDBNull(0) ? null : reader.GetFieldValue<string>(0),
                            reader.IsDBNull(1) ? null : reader.GetFieldValue<string>(1),
                            reader.IsDBNull(2) ? null : reader.GetFieldValue<string>(2),
                            reader.IsDBNull(3) ? null : reader.GetFieldValue<bool?>(3),
                            reader.IsDBNull(4) ? null : reader.GetFieldValue<string>(4),
                            reader.IsDBNull(5) ? null : reader.GetFieldValue<string>(5)
                        );
                        results.Add(item);
                    }
                }
            }

            return new Result<ImmutableList<GetAllRoles>, SqlError>.Ok<ImmutableList<GetAllRoles>, SqlError>(results.ToImmutable());
        }
        catch (Exception ex)
        {
            return new Result<ImmutableList<GetAllRoles>, SqlError>.Error<ImmutableList<GetAllRoles>, SqlError>(new SqlError("Database error", ex));
        }
    }
}

/// <summary>
/// Result row for 'GetAllRoles' query.
/// </summary>
public record GetAllRoles
{
    /// <summary>Column 'id'.</summary>
    public string id { get; init; }

    /// <summary>Column 'name'.</summary>
    public string name { get; init; }

    /// <summary>Column 'description'.</summary>
    public string description { get; init; }

    /// <summary>Column 'is_system'.</summary>
    public bool? is_system { get; init; }

    /// <summary>Column 'created_at'.</summary>
    public string created_at { get; init; }

    /// <summary>Column 'parent_role_id'.</summary>
    public string parent_role_id { get; init; }

    /// <summary>Initializes a new instance of GetAllRoles.</summary>
    public GetAllRoles(
        string id,
        string name,
        string description,
        bool? is_system,
        string created_at,
        string parent_role_id
    )
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.is_system = is_system;
        this.created_at = created_at;
        this.parent_role_id = parent_role_id;
    }
}
