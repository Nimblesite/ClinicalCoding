using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Npgsql;
using Outcome;
using Nimblesite.Sql.Model;

namespace Generated;

/// <summary>
/// Extension methods for 'CheckResourceGrant'.
/// </summary>
public static partial class CheckResourceGrantExtensions
{
    /// <summary>
    /// Executes 'CheckResourceGrant.sql' and maps results.
    /// </summary>
    /// <param name="connection">Open NpgsqlConnection connection.</param>
    /// <param name="user_id">Query parameter.</param>
    /// <param name="resource_type">Query parameter.</param>
    /// <param name="permission_code">Query parameter.</param>
    /// <param name="now">Query parameter.</param>
    /// <param name="resource_id">Query parameter.</param>
    /// <returns>Result of records or SQL error.</returns>
    public static async Task<Result<ImmutableList<CheckResourceGrant>, SqlError>> CheckResourceGrantAsync(this NpgsqlConnection connection, object user_id, object resource_type, object permission_code, object now, object resource_id)
    {
        const string sql = @"-- name: CheckResourceGrant
SELECT rg.id, rg.user_id, rg.resource_type, rg.resource_id, rg.permission_id,
       rg.granted_at, rg.granted_by, rg.expires_at, p.code as permission_code
FROM gk_resource_grant rg
JOIN gk_permission p ON rg.permission_id = p.id
WHERE rg.user_id = @user_id
  AND rg.resource_type = @resource_type
  AND rg.resource_id = @resource_id
  AND p.code = @permission_code
  AND (rg.expires_at IS NULL OR rg.expires_at > @now);
";

        try
        {
            var results = ImmutableList.CreateBuilder<CheckResourceGrant>();

            using (var command = new NpgsqlCommand(sql, connection))
            {
                if (user_id is not null and not DBNull)
                    command.Parameters.AddWithValue("@user_id", user_id);
                else
                    command.Parameters.Add(new NpgsqlParameter("@user_id", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });
                if (resource_type is not null and not DBNull)
                    command.Parameters.AddWithValue("@resource_type", resource_type);
                else
                    command.Parameters.Add(new NpgsqlParameter("@resource_type", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });
                if (permission_code is not null and not DBNull)
                    command.Parameters.AddWithValue("@permission_code", permission_code);
                else
                    command.Parameters.Add(new NpgsqlParameter("@permission_code", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });
                if (now is not null and not DBNull)
                    command.Parameters.AddWithValue("@now", now);
                else
                    command.Parameters.Add(new NpgsqlParameter("@now", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });
                if (resource_id is not null and not DBNull)
                    command.Parameters.AddWithValue("@resource_id", resource_id);
                else
                    command.Parameters.Add(new NpgsqlParameter("@resource_id", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var item = new CheckResourceGrant(
                            reader.IsDBNull(0) ? null : reader.GetFieldValue<string>(0),
                            reader.IsDBNull(1) ? null : reader.GetFieldValue<string>(1),
                            reader.IsDBNull(2) ? null : reader.GetFieldValue<string>(2),
                            reader.IsDBNull(3) ? null : reader.GetFieldValue<string>(3),
                            reader.IsDBNull(4) ? null : reader.GetFieldValue<string>(4),
                            reader.IsDBNull(5) ? null : reader.GetFieldValue<string>(5),
                            reader.IsDBNull(6) ? null : reader.GetFieldValue<string>(6),
                            reader.IsDBNull(7) ? null : reader.GetFieldValue<string>(7),
                            reader.IsDBNull(8) ? null : reader.GetFieldValue<string>(8)
                        );
                        results.Add(item);
                    }
                }
            }

            return new Result<ImmutableList<CheckResourceGrant>, SqlError>.Ok<ImmutableList<CheckResourceGrant>, SqlError>(results.ToImmutable());
        }
        catch (Exception ex)
        {
            return new Result<ImmutableList<CheckResourceGrant>, SqlError>.Error<ImmutableList<CheckResourceGrant>, SqlError>(new SqlError("Database error", ex));
        }
    }
}

/// <summary>
/// Result row for 'CheckResourceGrant' query.
/// </summary>
public record CheckResourceGrant
{
    /// <summary>Column 'id'.</summary>
    public string id { get; init; }

    /// <summary>Column 'user_id'.</summary>
    public string user_id { get; init; }

    /// <summary>Column 'resource_type'.</summary>
    public string resource_type { get; init; }

    /// <summary>Column 'resource_id'.</summary>
    public string resource_id { get; init; }

    /// <summary>Column 'permission_id'.</summary>
    public string permission_id { get; init; }

    /// <summary>Column 'granted_at'.</summary>
    public string granted_at { get; init; }

    /// <summary>Column 'granted_by'.</summary>
    public string granted_by { get; init; }

    /// <summary>Column 'expires_at'.</summary>
    public string expires_at { get; init; }

    /// <summary>Column 'permission_code'.</summary>
    public string permission_code { get; init; }

    /// <summary>Initializes a new instance of CheckResourceGrant.</summary>
    public CheckResourceGrant(
        string id,
        string user_id,
        string resource_type,
        string resource_id,
        string permission_id,
        string granted_at,
        string granted_by,
        string expires_at,
        string permission_code
    )
    {
        this.id = id;
        this.user_id = user_id;
        this.resource_type = resource_type;
        this.resource_id = resource_id;
        this.permission_id = permission_id;
        this.granted_at = granted_at;
        this.granted_by = granted_by;
        this.expires_at = expires_at;
        this.permission_code = permission_code;
    }
}
