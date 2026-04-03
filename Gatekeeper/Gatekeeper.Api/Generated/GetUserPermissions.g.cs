using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Npgsql;
using Outcome;
using Selecta;

namespace Generated;

/// <summary>
/// Extension methods for 'GetUserPermissions'.
/// </summary>
public static partial class GetUserPermissionsExtensions
{
    /// <summary>
    /// Executes 'GetUserPermissions.sql' and maps results.
    /// </summary>
    /// <param name="connection">Open NpgsqlConnection connection.</param>
    /// <param name="user_id">Query parameter.</param>
    /// <param name="now">Query parameter.</param>
    /// <returns>Result of records or SQL error.</returns>
    public static async Task<Result<ImmutableList<GetUserPermissions>, SqlError>> GetUserPermissionsAsync(this NpgsqlConnection connection, object user_id, object now)
    {
        const string sql = @"-- name: GetUserPermissions
-- Returns all permissions for a user: from roles + direct grants
-- Note: source_type column uses role name prefix to indicate source (role-based vs direct)
SELECT DISTINCT p.id, p.code, p.resource_type, p.action, p.description,
       r.name as source_name,
       ur.role_id as source_type,
       NULL as scope_type,
       NULL as scope_value
FROM gk_user_role ur
JOIN gk_role r ON ur.role_id = r.id
JOIN gk_role_permission rp ON r.id = rp.role_id
JOIN gk_permission p ON rp.permission_id = p.id
WHERE ur.user_id = @user_id
  AND (ur.expires_at IS NULL OR ur.expires_at > @now)

UNION ALL

SELECT p.id, p.code, p.resource_type, p.action, p.description,
       p.code as source_name,
       up.permission_id as source_type,
       COALESCE(up.scope_type, p.resource_type) as scope_type,
       COALESCE(up.scope_value, p.action) as scope_value
FROM gk_user_permission up
JOIN gk_permission p ON up.permission_id = p.id
WHERE up.user_id = @user_id
  AND (up.expires_at IS NULL OR up.expires_at > @now);
";

        try
        {
            var results = ImmutableList.CreateBuilder<GetUserPermissions>();

            using (var command = new NpgsqlCommand(sql, connection))
            {
                if (user_id is not null and not DBNull)
                    command.Parameters.AddWithValue("@user_id", user_id);
                else
                    command.Parameters.Add(new NpgsqlParameter("@user_id", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });
                if (now is not null and not DBNull)
                    command.Parameters.AddWithValue("@now", now);
                else
                    command.Parameters.Add(new NpgsqlParameter("@now", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var item = new GetUserPermissions(
                            reader.IsDBNull(0) ? null : reader.GetFieldValue<string>(0),
                            reader.IsDBNull(1) ? null : reader.GetFieldValue<string>(1),
                            reader.IsDBNull(2) ? null : reader.GetFieldValue<string>(2),
                            reader.IsDBNull(3) ? null : reader.GetFieldValue<string>(3),
                            reader.IsDBNull(4) ? null : reader.GetFieldValue<string>(4),
                            reader.IsDBNull(5) ? null : reader.GetFieldValue<string>(5),
                            reader.IsDBNull(6) ? null : reader.GetFieldValue<string>(6),
                            reader.IsDBNull(7) ? null : reader.GetFieldValue<byte[]>(7),
                            reader.IsDBNull(8) ? null : reader.GetFieldValue<byte[]>(8)
                        );
                        results.Add(item);
                    }
                }
            }

            return new Result<ImmutableList<GetUserPermissions>, SqlError>.Ok<ImmutableList<GetUserPermissions>, SqlError>(results.ToImmutable());
        }
        catch (Exception ex)
        {
            return new Result<ImmutableList<GetUserPermissions>, SqlError>.Error<ImmutableList<GetUserPermissions>, SqlError>(new SqlError("Database error", ex));
        }
    }
}

/// <summary>
/// Result row for 'GetUserPermissions' query.
/// </summary>
public record GetUserPermissions
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

    /// <summary>Column 'source_name'.</summary>
    public string source_name { get; init; }

    /// <summary>Column 'source_type'.</summary>
    public string source_type { get; init; }

    /// <summary>Column 'scope_type'.</summary>
    public byte[] scope_type { get; init; }

    /// <summary>Column 'scope_value'.</summary>
    public byte[] scope_value { get; init; }

    /// <summary>Initializes a new instance of GetUserPermissions.</summary>
    public GetUserPermissions(
        string id,
        string code,
        string resource_type,
        string action,
        string description,
        string source_name,
        string source_type,
        byte[] scope_type,
        byte[] scope_value
    )
    {
        this.id = id;
        this.code = code;
        this.resource_type = resource_type;
        this.action = action;
        this.description = description;
        this.source_name = source_name;
        this.source_type = source_type;
        this.scope_type = scope_type;
        this.scope_value = scope_value;
    }
}
