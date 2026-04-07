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
    /// Extension methods for table operations on fhir_MedicationRequest
    /// </summary>
    public static partial class fhir_MedicationRequestExtensions
    {

    /// <summary>
    /// Inserts a new row into the fhir_MedicationRequest table.
    /// </summary>
    public static async Task<Result<int, SqlError>> Insertfhir_MedicationRequestAsync(this IDbTransaction transaction, string? id, string? status, string? intent, string? patientid, string? practitionerid, string? encounterid, string? medicationcode, string? medicationdisplay, string? dosageinstruction, double? quantity, string? unit, long? refills, string? authoredon, string? lastupdated, long? versionid)
    {
        const string sql = "INSERT INTO fhir_MedicationRequest (Id, Status, Intent, PatientId, PractitionerId, EncounterId, MedicationCode, MedicationDisplay, DosageInstruction, Quantity, Unit, Refills, AuthoredOn, LastUpdated, VersionId) VALUES (@Id, @Status, @Intent, @PatientId, @PractitionerId, @EncounterId, @MedicationCode, @MedicationDisplay, @DosageInstruction, @Quantity, @Unit, @Refills, @AuthoredOn, @LastUpdated, @VersionId)";

        if (transaction.Connection is null)
            return new Result<int, SqlError>.Error<int, SqlError>(new SqlError("Transaction has no connection"));

        try
        {
            using (var command = new NpgsqlCommand(sql, (NpgsqlConnection)transaction.Connection!, (NpgsqlTransaction)transaction))
            {
                command.Parameters.AddWithValue("@Id", id ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Status", status ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Intent", intent ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PatientId", patientid ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PractitionerId", practitionerid ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@EncounterId", encounterid ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@MedicationCode", medicationcode ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@MedicationDisplay", medicationdisplay ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@DosageInstruction", dosageinstruction ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Quantity", quantity ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Unit", unit ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Refills", refills ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@AuthoredOn", authoredon ?? (object)DBNull.Value);
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
