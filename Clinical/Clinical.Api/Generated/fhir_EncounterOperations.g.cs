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
    /// Extension methods for table operations on fhir_Encounter
    /// </summary>
    public static partial class fhir_EncounterExtensions
    {

    /// <summary>
    /// Inserts a new row into the fhir_Encounter table.
    /// </summary>
    public static async Task<Result<int, SqlError>> Insertfhir_EncounterAsync(this IDbTransaction transaction, string? id, string? status, string? @class, string? patientid, string? practitionerid, string? servicetype, string? reasoncode, string? periodstart, string? periodend, string? notes, string? lastupdated, long? versionid)
    {
        const string sql = "INSERT INTO fhir_Encounter (Id, Status, Class, PatientId, PractitionerId, ServiceType, ReasonCode, PeriodStart, PeriodEnd, Notes, LastUpdated, VersionId) VALUES (@Id, @Status, @Class, @PatientId, @PractitionerId, @ServiceType, @ReasonCode, @PeriodStart, @PeriodEnd, @Notes, @LastUpdated, @VersionId)";

        if (transaction.Connection is null)
            return new Result<int, SqlError>.Error<int, SqlError>(new SqlError("Transaction has no connection"));

        try
        {
            using (var command = new NpgsqlCommand(sql, (NpgsqlConnection)transaction.Connection!, (NpgsqlTransaction)transaction))
            {
                command.Parameters.AddWithValue("@Id", id ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Status", status ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Class", @class ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PatientId", patientid ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PractitionerId", practitionerid ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ServiceType", servicetype ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ReasonCode", reasoncode ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PeriodStart", periodstart ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PeriodEnd", periodend ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Notes", notes ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@LastUpdated", lastupdated ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@VersionId", versionid ?? (object)DBNull.Value);

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
