#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using Npgsql;
using Outcome;
using Selecta;

namespace Generated
{
    /// <summary>
    /// Extension methods for table operations on gk_resource_grant
    /// </summary>
    public static partial class gk_resource_grantExtensions
    {

    /// <summary>
    /// Inserts a new row into the gk_resource_grant table.
    /// </summary>
    public static async Task<Result<int, SqlError>> Insertgk_resource_grantAsync(this IDbTransaction transaction, string? id, string? user_id, string? resource_type, string? resource_id, string? permission_id, string? granted_at, string? granted_by, string? expires_at)
    {
        const string sql = "INSERT INTO gk_resource_grant (id, user_id, resource_type, resource_id, permission_id, granted_at, granted_by, expires_at) VALUES (@id, @user_id, @resource_type, @resource_id, @permission_id, @granted_at, @granted_by, @expires_at)";

        if (transaction.Connection is null)
            return new Result<int, SqlError>.Error<int, SqlError>(new SqlError("Transaction has no connection"));

        try
        {
            using (var command = new NpgsqlCommand(sql, (NpgsqlConnection)transaction.Connection!, (NpgsqlTransaction)transaction))
            {
                command.Parameters.AddWithValue("@id", id ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@user_id", user_id ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@resource_type", resource_type ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@resource_id", resource_id ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@permission_id", permission_id ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@granted_at", granted_at ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@granted_by", granted_by ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@expires_at", expires_at ?? (object)DBNull.Value);

                var rowsAffected = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                return new Result<int, SqlError>.Ok<int, SqlError>(rowsAffected);
            }
        }
        catch (Exception ex)
        {
            return new Result<int, SqlError>.Error<int, SqlError>(new SqlError("Insert failed", ex));
        }
    }

    }
}
