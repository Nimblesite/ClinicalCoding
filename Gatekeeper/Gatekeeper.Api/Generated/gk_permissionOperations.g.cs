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
    /// Extension methods for table operations on gk_permission
    /// </summary>
    public static partial class gk_permissionExtensions
    {

    /// <summary>
    /// Inserts a new row into the gk_permission table.
    /// </summary>
    public static async Task<Result<int, SqlError>> Insertgk_permissionAsync(this IDbTransaction transaction, string? id, string? code, string? resource_type, string? action, string? description, string? created_at)
    {
        const string sql = "INSERT INTO gk_permission (id, code, resource_type, action, description, created_at) VALUES (@id, @code, @resource_type, @action, @description, @created_at)";

        if (transaction.Connection is null)
            return new Result<int, SqlError>.Error<int, SqlError>(new SqlError("Transaction has no connection"));

        try
        {
            using (var command = new NpgsqlCommand(sql, (NpgsqlConnection)transaction.Connection!, (NpgsqlTransaction)transaction))
            {
                command.Parameters.AddWithValue("@id", id ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@code", code ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@resource_type", resource_type ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@action", action ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@description", description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@created_at", created_at ?? (object)DBNull.Value);

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
