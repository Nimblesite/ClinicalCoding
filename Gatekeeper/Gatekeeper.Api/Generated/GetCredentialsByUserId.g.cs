using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Npgsql;
using Outcome;
using Selecta;

namespace Generated;

/// <summary>
/// Extension methods for 'GetCredentialsByUserId'.
/// </summary>
public static partial class GetCredentialsByUserIdExtensions
{
    /// <summary>
    /// Executes 'GetCredentialsByUserId.sql' and maps results.
    /// </summary>
    /// <param name="connection">Open NpgsqlConnection connection.</param>
    /// <param name="userId">Query parameter.</param>
    /// <returns>Result of records or SQL error.</returns>
    public static async Task<Result<ImmutableList<GetCredentialsByUserId>, SqlError>> GetCredentialsByUserIdAsync(this NpgsqlConnection connection, object userId)
    {
        const string sql = @"-- name: GetCredentialsByUserId
SELECT id, user_id, public_key, sign_count, aaguid, credential_type, transports,
       attestation_format, created_at, last_used_at, device_name,
       is_backup_eligible, is_backed_up
FROM gk_credential
WHERE user_id = @userId;
";

        try
        {
            var results = ImmutableList.CreateBuilder<GetCredentialsByUserId>();

            using (var command = new NpgsqlCommand(sql, connection))
            {
                if (userId is not null and not DBNull)
                    command.Parameters.AddWithValue("@userId", userId);
                else
                    command.Parameters.Add(new NpgsqlParameter("@userId", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var item = new GetCredentialsByUserId(
                            reader.IsDBNull(0) ? null : reader.GetFieldValue<string>(0),
                            reader.IsDBNull(1) ? null : reader.GetFieldValue<string>(1),
                            reader.IsDBNull(2) ? null : reader.GetFieldValue<byte[]>(2),
                            reader.IsDBNull(3) ? default(long) : reader.GetFieldValue<long>(3),
                            reader.IsDBNull(4) ? null : reader.GetFieldValue<string>(4),
                            reader.IsDBNull(5) ? null : reader.GetFieldValue<string>(5),
                            reader.IsDBNull(6) ? null : reader.GetFieldValue<string>(6),
                            reader.IsDBNull(7) ? null : reader.GetFieldValue<string>(7),
                            reader.IsDBNull(8) ? null : reader.GetFieldValue<string>(8),
                            reader.IsDBNull(9) ? null : reader.GetFieldValue<string>(9),
                            reader.IsDBNull(10) ? null : reader.GetFieldValue<string>(10),
                            reader.IsDBNull(11) ? null : reader.GetFieldValue<bool?>(11),
                            reader.IsDBNull(12) ? null : reader.GetFieldValue<bool?>(12)
                        );
                        results.Add(item);
                    }
                }
            }

            return new Result<ImmutableList<GetCredentialsByUserId>, SqlError>.Ok<ImmutableList<GetCredentialsByUserId>, SqlError>(results.ToImmutable());
        }
        catch (Exception ex)
        {
            return new Result<ImmutableList<GetCredentialsByUserId>, SqlError>.Error<ImmutableList<GetCredentialsByUserId>, SqlError>(new SqlError("Database error", ex));
        }
    }
}

/// <summary>
/// Result row for 'GetCredentialsByUserId' query.
/// </summary>
public record GetCredentialsByUserId
{
    /// <summary>Column 'id'.</summary>
    public string id { get; init; }

    /// <summary>Column 'user_id'.</summary>
    public string user_id { get; init; }

    /// <summary>Column 'public_key'.</summary>
    public byte[] public_key { get; init; }

    /// <summary>Column 'sign_count'.</summary>
    public long sign_count { get; init; }

    /// <summary>Column 'aaguid'.</summary>
    public string aaguid { get; init; }

    /// <summary>Column 'credential_type'.</summary>
    public string credential_type { get; init; }

    /// <summary>Column 'transports'.</summary>
    public string transports { get; init; }

    /// <summary>Column 'attestation_format'.</summary>
    public string attestation_format { get; init; }

    /// <summary>Column 'created_at'.</summary>
    public string created_at { get; init; }

    /// <summary>Column 'last_used_at'.</summary>
    public string last_used_at { get; init; }

    /// <summary>Column 'device_name'.</summary>
    public string device_name { get; init; }

    /// <summary>Column 'is_backup_eligible'.</summary>
    public bool? is_backup_eligible { get; init; }

    /// <summary>Column 'is_backed_up'.</summary>
    public bool? is_backed_up { get; init; }

    /// <summary>Initializes a new instance of GetCredentialsByUserId.</summary>
    public GetCredentialsByUserId(
        string id,
        string user_id,
        byte[] public_key,
        long sign_count,
        string aaguid,
        string credential_type,
        string transports,
        string attestation_format,
        string created_at,
        string last_used_at,
        string device_name,
        bool? is_backup_eligible,
        bool? is_backed_up
    )
    {
        this.id = id;
        this.user_id = user_id;
        this.public_key = public_key;
        this.sign_count = sign_count;
        this.aaguid = aaguid;
        this.credential_type = credential_type;
        this.transports = transports;
        this.attestation_format = attestation_format;
        this.created_at = created_at;
        this.last_used_at = last_used_at;
        this.device_name = device_name;
        this.is_backup_eligible = is_backup_eligible;
        this.is_backed_up = is_backed_up;
    }
}
