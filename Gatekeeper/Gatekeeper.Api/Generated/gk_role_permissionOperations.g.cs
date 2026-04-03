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
    /// Extension methods for table operations on gk_role_permission
    /// </summary>
    public static partial class gk_role_permissionExtensions
    {

    /// <summary>
    /// Inserts a new row into the gk_role_permission table.
    /// </summary>
    public static async Task<Result<int, SqlError>> Insertgk_role_permissionAsync(this IDbTransaction transaction, string? role_id, string? permission_id, string? granted_at)
    {
        const string sql = "INSERT INTO gk_role_permission (role_id, permission_id, granted_at) VALUES (@role_id, @permission_id, @granted_at)";

        if (transaction.Connection is null)
            return new Result<int, SqlError>.Error<int, SqlError>(new SqlError("Transaction has no connection"));

        try
        {
            using (var command = new NpgsqlCommand(sql, (NpgsqlConnection)transaction.Connection!, (NpgsqlTransaction)transaction))
            {
                command.Parameters.AddWithValue("@role_id", role_id ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@permission_id", permission_id ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@granted_at", granted_at ?? (object)DBNull.Value);

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
