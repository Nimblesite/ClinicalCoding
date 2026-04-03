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
    /// Extension methods for table operations on gk_user_role
    /// </summary>
    public static partial class gk_user_roleExtensions
    {

    /// <summary>
    /// Inserts a new row into the gk_user_role table.
    /// </summary>
    public static async Task<Result<int, SqlError>> Insertgk_user_roleAsync(this IDbTransaction transaction, string? user_id, string? role_id, string? granted_at, string? granted_by, string? expires_at)
    {
        const string sql = "INSERT INTO gk_user_role (user_id, role_id, granted_at, granted_by, expires_at) VALUES (@user_id, @role_id, @granted_at, @granted_by, @expires_at)";

        if (transaction.Connection is null)
            return new Result<int, SqlError>.Error<int, SqlError>(new SqlError("Transaction has no connection"));

        try
        {
            using (var command = new NpgsqlCommand(sql, (NpgsqlConnection)transaction.Connection!, (NpgsqlTransaction)transaction))
            {
                command.Parameters.AddWithValue("@user_id", user_id ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@role_id", role_id ?? (object)DBNull.Value);
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
