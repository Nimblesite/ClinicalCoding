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
    /// Extension methods for table operations on gk_user
    /// </summary>
    public static partial class gk_userExtensions
    {

    /// <summary>
    /// Inserts a new row into the gk_user table.
    /// </summary>
    public static async Task<Result<int, SqlError>> Insertgk_userAsync(this IDbTransaction transaction, string? id, string? display_name, string? email, string? created_at, string? last_login_at, bool? is_active, string? metadata)
    {
        const string sql = "INSERT INTO gk_user (id, display_name, email, created_at, last_login_at, is_active, metadata) VALUES (@id, @display_name, @email, @created_at, @last_login_at, @is_active, @metadata)";

        if (transaction.Connection is null)
            return new Result<int, SqlError>.Error<int, SqlError>(new SqlError("Transaction has no connection"));

        try
        {
            using (var command = new NpgsqlCommand(sql, (NpgsqlConnection)transaction.Connection!, (NpgsqlTransaction)transaction))
            {
                command.Parameters.AddWithValue("@id", id ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@display_name", display_name ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@email", email ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@created_at", created_at ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@last_login_at", last_login_at ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@is_active", is_active ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@metadata", metadata ?? (object)DBNull.Value);

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
