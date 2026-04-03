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
    /// Extension methods for table operations on gk_session
    /// </summary>
    public static partial class gk_sessionExtensions
    {

    /// <summary>
    /// Inserts a new row into the gk_session table.
    /// </summary>
    public static async Task<Result<int, SqlError>> Insertgk_sessionAsync(this IDbTransaction transaction, string? id, string? user_id, string? credential_id, string? created_at, string? expires_at, string? last_activity_at, string? ip_address, string? user_agent, bool? is_revoked)
    {
        const string sql = "INSERT INTO gk_session (id, user_id, credential_id, created_at, expires_at, last_activity_at, ip_address, user_agent, is_revoked) VALUES (@id, @user_id, @credential_id, @created_at, @expires_at, @last_activity_at, @ip_address, @user_agent, @is_revoked)";

        if (transaction.Connection is null)
            return new Result<int, SqlError>.Error<int, SqlError>(new SqlError("Transaction has no connection"));

        try
        {
            using (var command = new NpgsqlCommand(sql, (NpgsqlConnection)transaction.Connection!, (NpgsqlTransaction)transaction))
            {
                command.Parameters.AddWithValue("@id", id ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@user_id", user_id ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@credential_id", credential_id ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@created_at", created_at ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@expires_at", expires_at ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@last_activity_at", last_activity_at ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ip_address", ip_address ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@user_agent", user_agent ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@is_revoked", is_revoked ?? (object)DBNull.Value);

                var rowsAffected = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                return new Result<int, SqlError>.Ok<int, SqlError>(rowsAffected);
            }
        }
        catch (Exception ex)
        {
            return new Result<int, SqlError>.Error<int, SqlError>(new SqlError("Insert failed", ex));
        }
    }


    /// <summary>
    /// Updates a row in the gk_session table.
    /// </summary>
    public static async Task<Result<int, SqlError>> Updategk_sessionAsync(this IDbTransaction transaction, string id, string user_id, string credential_id, string created_at, string expires_at, string last_activity_at, string ip_address, string user_agent, bool? is_revoked)
    {
        const string sql = "UPDATE gk_session SET user_id = @user_id, credential_id = @credential_id, created_at = @created_at, expires_at = @expires_at, last_activity_at = @last_activity_at, ip_address = @ip_address, user_agent = @user_agent, is_revoked = @is_revoked WHERE id = @id";

        if (transaction.Connection is null)
            return new Result<int, SqlError>.Error<int, SqlError>(new SqlError("Transaction has no connection"));

        try
        {
            using (var command = new NpgsqlCommand(sql, (NpgsqlConnection)transaction.Connection!, (NpgsqlTransaction)transaction))
            {
                command.Parameters.AddWithValue("@id", id ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@user_id", user_id ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@credential_id", credential_id ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@created_at", created_at ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@expires_at", expires_at ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@last_activity_at", last_activity_at ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ip_address", ip_address ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@user_agent", user_agent ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@is_revoked", is_revoked ?? (object)DBNull.Value);

                var rowsAffected = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                return new Result<int, SqlError>.Ok<int, SqlError>(rowsAffected);
            }
        }
        catch (Exception ex)
        {
            return new Result<int, SqlError>.Error<int, SqlError>(new SqlError("Update failed", ex));
        }
    }

    }
}
