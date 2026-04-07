using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Npgsql;
using Outcome;
using Nimblesite.Sql.Model;

namespace Generated;

/// <summary>
/// Extension methods for 'CheckPermission'.
/// </summary>
public static partial class CheckPermissionExtensions
{
    /// <summary>
    /// Executes 'CheckPermission.sql' and maps results.
    /// </summary>
    /// <param name="connection">Open NpgsqlConnection connection.</param>
    /// <param name="permissionCode">Query parameter.</param>
    /// <param name="userId">Query parameter.</param>
    /// <param name="now">Query parameter.</param>
    /// <returns>Result of records or SQL error.</returns>
    public static async Task<Result<ImmutableList<CheckPermission>, SqlError>> CheckPermissionAsync(this NpgsqlConnection connection, object permissionCode, object userId, object now)
    {
        const string sql = @"-- name: CheckPermission
-- Checks if user has a specific permission code (via roles or direct grant)
SELECT 1 AS has_permission
FROM gk_permission p
WHERE p.code = @permissionCode
  AND (
    -- Check role permissions
    EXISTS (
      SELECT 1 FROM gk_role_permission rp
      JOIN gk_user_role ur ON rp.role_id = ur.role_id
      WHERE rp.permission_id = p.id
        AND ur.user_id = @userId
        AND (ur.expires_at IS NULL OR ur.expires_at > @now)
    )
    OR
    -- Check direct permissions
    EXISTS (
      SELECT 1 FROM gk_user_permission up
      WHERE up.permission_id = p.id
        AND up.user_id = @userId
        AND (up.expires_at IS NULL OR up.expires_at > @now)
    )
  )
LIMIT 1;
";

        try
        {
            var results = ImmutableList.CreateBuilder<CheckPermission>();

            using (var command = new NpgsqlCommand(sql, connection))
            {
                if (permissionCode is not null and not DBNull)
                    command.Parameters.AddWithValue("@permissionCode", permissionCode);
                else
                    command.Parameters.Add(new NpgsqlParameter("@permissionCode", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });
                if (userId is not null and not DBNull)
                    command.Parameters.AddWithValue("@userId", userId);
                else
                    command.Parameters.Add(new NpgsqlParameter("@userId", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });
                if (now is not null and not DBNull)
                    command.Parameters.AddWithValue("@now", now);
                else
                    command.Parameters.Add(new NpgsqlParameter("@now", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var item = new CheckPermission(
                            reader.IsDBNull(0) ? null : reader.GetFieldValue<byte[]>(0)
                        );
                        results.Add(item);
                    }
                }
            }

            return new Result<ImmutableList<CheckPermission>, SqlError>.Ok<ImmutableList<CheckPermission>, SqlError>(results.ToImmutable());
        }
        catch (Exception ex)
        {
            return new Result<ImmutableList<CheckPermission>, SqlError>.Error<ImmutableList<CheckPermission>, SqlError>(new SqlError("Database error", ex));
        }
    }
}

/// <summary>
/// Result row for 'CheckPermission' query.
/// </summary>
public record CheckPermission
{
    /// <summary>Column 'has_permission'.</summary>
    public byte[] has_permission { get; init; }

    /// <summary>Initializes a new instance of CheckPermission.</summary>
    public CheckPermission(
        byte[] has_permission
    )
    {
        this.has_permission = has_permission;
    }
}
