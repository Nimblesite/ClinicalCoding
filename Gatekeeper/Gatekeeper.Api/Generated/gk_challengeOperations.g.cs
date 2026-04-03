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
    /// Extension methods for table operations on gk_challenge
    /// </summary>
    public static partial class gk_challengeExtensions
    {

    /// <summary>
    /// Inserts a new row into the gk_challenge table.
    /// </summary>
    public static async Task<Result<int, SqlError>> Insertgk_challengeAsync(this IDbTransaction transaction, string? id, string? user_id, byte[] challenge, string? type, string? created_at, string? expires_at)
    {
        const string sql = "INSERT INTO gk_challenge (id, user_id, challenge, type, created_at, expires_at) VALUES (@id, @user_id, @challenge, @type, @created_at, @expires_at)";

        if (transaction.Connection is null)
            return new Result<int, SqlError>.Error<int, SqlError>(new SqlError("Transaction has no connection"));

        try
        {
            using (var command = new NpgsqlCommand(sql, (NpgsqlConnection)transaction.Connection!, (NpgsqlTransaction)transaction))
            {
                command.Parameters.AddWithValue("@id", id ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@user_id", user_id ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@challenge", challenge ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@type", type ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@created_at", created_at ?? (object)DBNull.Value);
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
