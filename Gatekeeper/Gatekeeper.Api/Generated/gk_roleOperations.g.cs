#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using Npgsql;
using Outcome;
using Nimblesite.Sql.Model;

namespace Generated
{
    /// <summary>
    /// Extension methods for table operations on gk_role
    /// </summary>
    public static partial class gk_roleExtensions
    {

    /// <summary>
    /// Inserts a new row into the gk_role table.
    /// </summary>
    public static async Task<Result<int, SqlError>> Insertgk_roleAsync(this IDbTransaction transaction, string? id, string? name, string? description, bool? is_system, string? created_at, string? parent_role_id)
    {
        const string sql = "INSERT INTO gk_role (id, name, description, is_system, created_at, parent_role_id) VALUES (@id, @name, @description, @is_system, @created_at, @parent_role_id)";

        if (transaction.Connection is null)
            return new Result<int, SqlError>.Error<int, SqlError>(new SqlError("Transaction has no connection"));

        try
        {
            using (var command = new NpgsqlCommand(sql, (NpgsqlConnection)transaction.Connection!, (NpgsqlTransaction)transaction))
            {
                command.Parameters.AddWithValue("@id", id ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@name", name ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@description", description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@is_system", is_system ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@created_at", created_at ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@parent_role_id", parent_role_id ?? (object)DBNull.Value);

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
