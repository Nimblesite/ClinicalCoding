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
    /// Extension methods for table operations on gk_credential
    /// </summary>
    public static partial class gk_credentialExtensions
    {

    /// <summary>
    /// Inserts a new row into the gk_credential table.
    /// </summary>
    public static async Task<Result<int, SqlError>> Insertgk_credentialAsync(this IDbTransaction transaction, string? id, string? user_id, byte[] public_key, long? sign_count, string? aaguid, string? credential_type, string? transports, string? attestation_format, string? created_at, string? last_used_at, string? device_name, bool? is_backup_eligible, bool? is_backed_up)
    {
        const string sql = "INSERT INTO gk_credential (id, user_id, public_key, sign_count, aaguid, credential_type, transports, attestation_format, created_at, last_used_at, device_name, is_backup_eligible, is_backed_up) VALUES (@id, @user_id, @public_key, @sign_count, @aaguid, @credential_type, @transports, @attestation_format, @created_at, @last_used_at, @device_name, @is_backup_eligible, @is_backed_up)";

        if (transaction.Connection is null)
            return new Result<int, SqlError>.Error<int, SqlError>(new SqlError("Transaction has no connection"));

        try
        {
            using (var command = new NpgsqlCommand(sql, (NpgsqlConnection)transaction.Connection!, (NpgsqlTransaction)transaction))
            {
                command.Parameters.AddWithValue("@id", id ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@user_id", user_id ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@public_key", public_key ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@sign_count", sign_count ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@aaguid", aaguid ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@credential_type", credential_type ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@transports", transports ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@attestation_format", attestation_format ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@created_at", created_at ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@last_used_at", last_used_at ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@device_name", device_name ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@is_backup_eligible", is_backup_eligible ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@is_backed_up", is_backed_up ?? (object)DBNull.Value);

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
