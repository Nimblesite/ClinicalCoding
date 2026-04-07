using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Npgsql;
using Outcome;
using Nimblesite.Sql.Model;

namespace Generated;

/// <summary>
/// Extension methods for 'GetAllPermissions'.
/// </summary>
public static partial class GetAllPermissionsExtensions
{
    /// <summary>
    /// Executes 'GetAllPermissions.sql' and maps results.
    /// </summary>
    /// <param name="connection">Open NpgsqlConnection connection.</param>
    /// <returns>Result of records or SQL error.</returns>
    public static async Task<Result<ImmutableList<GetAllPermissions>, SqlError>> GetAllPermissionsAsync(this NpgsqlConnection connection)
    {
        const string sql = @"-- name: GetAllPermissions
SELECT id, code, resource_type, action, description, created_at
FROM gk_permission
ORDER BY resource_type, action;
";

        try
        {
            var results = ImmutableList.CreateBuilder<GetAllPermissions>();

            using (var command = new NpgsqlCommand(sql, connection))
            {

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var item = new GetAllPermissions(
                            reader.IsDBNull(0) ? null : reader.GetFieldValue<string>(0),
                            reader.IsDBNull(1) ? null : reader.GetFieldValue<string>(1),
                            reader.IsDBNull(2) ? null : reader.GetFieldValue<string>(2),
                            reader.IsDBNull(3) ? null : reader.GetFieldValue<string>(3),
                            reader.IsDBNull(4) ? null : reader.GetFieldValue<string>(4),
                            reader.IsDBNull(5) ? null : reader.GetFieldValue<string>(5)
                        );
                        results.Add(item);
                    }
                }
            }

            return new Result<ImmutableList<GetAllPermissions>, SqlError>.Ok<ImmutableList<GetAllPermissions>, SqlError>(results.ToImmutable());
        }
        catch (Exception ex)
        {
            return new Result<ImmutableList<GetAllPermissions>, SqlError>.Error<ImmutableList<GetAllPermissions>, SqlError>(new SqlError("Database error", ex));
        }
    }
}

/// <summary>
/// Result row for 'GetAllPermissions' query.
/// </summary>
public record GetAllPermissions
{
    /// <summary>Column 'id'.</summary>
    public string id { get; init; }

    /// <summary>Column 'code'.</summary>
    public string code { get; init; }

    /// <summary>Column 'resource_type'.</summary>
    public string resource_type { get; init; }

    /// <summary>Column 'action'.</summary>
    public string action { get; init; }

    /// <summary>Column 'description'.</summary>
    public string description { get; init; }

    /// <summary>Column 'created_at'.</summary>
    public string created_at { get; init; }

    /// <summary>Initializes a new instance of GetAllPermissions.</summary>
    public GetAllPermissions(
        string id,
        string code,
        string resource_type,
        string action,
        string description,
        string created_at
    )
    {
        this.id = id;
        this.code = code;
        this.resource_type = resource_type;
        this.action = action;
        this.description = description;
        this.created_at = created_at;
    }
}
