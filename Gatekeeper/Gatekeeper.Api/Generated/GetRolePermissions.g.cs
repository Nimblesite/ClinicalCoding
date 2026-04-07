using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Npgsql;
using Outcome;
using Nimblesite.Sql.Model;

namespace Generated;

/// <summary>
/// Extension methods for 'GetRolePermissions'.
/// </summary>
public static partial class GetRolePermissionsExtensions
{
    /// <summary>
    /// Executes 'GetRolePermissions.sql' and maps results.
    /// </summary>
    /// <param name="connection">Open NpgsqlConnection connection.</param>
    /// <param name="roleId">Query parameter.</param>
    /// <returns>Result of records or SQL error.</returns>
    public static async Task<Result<ImmutableList<GetRolePermissions>, SqlError>> GetRolePermissionsAsync(this NpgsqlConnection connection, object roleId)
    {
        const string sql = @"-- name: GetRolePermissions
SELECT p.id, p.code, p.resource_type, p.action, p.description, p.created_at,
       rp.granted_at
FROM gk_permission p
JOIN gk_role_permission rp ON p.id = rp.permission_id
WHERE rp.role_id = @roleId;
";

        try
        {
            var results = ImmutableList.CreateBuilder<GetRolePermissions>();

            using (var command = new NpgsqlCommand(sql, connection))
            {
                if (roleId is not null and not DBNull)
                    command.Parameters.AddWithValue("@roleId", roleId);
                else
                    command.Parameters.Add(new NpgsqlParameter("@roleId", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var item = new GetRolePermissions(
                            reader.IsDBNull(0) ? null : reader.GetFieldValue<string>(0),
                            reader.IsDBNull(1) ? null : reader.GetFieldValue<string>(1),
                            reader.IsDBNull(2) ? null : reader.GetFieldValue<string>(2),
                            reader.IsDBNull(3) ? null : reader.GetFieldValue<string>(3),
                            reader.IsDBNull(4) ? null : reader.GetFieldValue<string>(4),
                            reader.IsDBNull(5) ? null : reader.GetFieldValue<string>(5),
                            reader.IsDBNull(6) ? null : reader.GetFieldValue<string>(6)
                        );
                        results.Add(item);
                    }
                }
            }

            return new Result<ImmutableList<GetRolePermissions>, SqlError>.Ok<ImmutableList<GetRolePermissions>, SqlError>(results.ToImmutable());
        }
        catch (Exception ex)
        {
            return new Result<ImmutableList<GetRolePermissions>, SqlError>.Error<ImmutableList<GetRolePermissions>, SqlError>(new SqlError("Database error", ex));
        }
    }
}

/// <summary>
/// Result row for 'GetRolePermissions' query.
/// </summary>
public record GetRolePermissions
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

    /// <summary>Column 'granted_at'.</summary>
    public string granted_at { get; init; }

    /// <summary>Initializes a new instance of GetRolePermissions.</summary>
    public GetRolePermissions(
        string id,
        string code,
        string resource_type,
        string action,
        string description,
        string created_at,
        string granted_at
    )
    {
        this.id = id;
        this.code = code;
        this.resource_type = resource_type;
        this.action = action;
        this.description = description;
        this.created_at = created_at;
        this.granted_at = granted_at;
    }
}
