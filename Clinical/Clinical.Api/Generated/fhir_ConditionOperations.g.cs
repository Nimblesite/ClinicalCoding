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
    /// Extension methods for table operations on fhir_Condition
    /// </summary>
    public static partial class fhir_ConditionExtensions
    {

    /// <summary>
    /// Inserts a new row into the fhir_Condition table.
    /// </summary>
    public static async Task<Result<int, SqlError>> Insertfhir_ConditionAsync(this IDbTransaction transaction, string? id, string? clinicalstatus, string? verificationstatus, string? category, string? severity, string? codesystem, string? codevalue, string? codedisplay, string? subjectreference, string? encounterreference, string? onsetdatetime, string? recordeddate, string? recorderreference, string? notetext, string? lastupdated, long? versionid)
    {
        const string sql = "INSERT INTO fhir_Condition (Id, ClinicalStatus, VerificationStatus, Category, Severity, CodeSystem, CodeValue, CodeDisplay, SubjectReference, EncounterReference, OnsetDateTime, RecordedDate, RecorderReference, NoteText, LastUpdated, VersionId) VALUES (@Id, @ClinicalStatus, @VerificationStatus, @Category, @Severity, @CodeSystem, @CodeValue, @CodeDisplay, @SubjectReference, @EncounterReference, @OnsetDateTime, @RecordedDate, @RecorderReference, @NoteText, @LastUpdated, @VersionId)";

        if (transaction.Connection is null)
            return new Result<int, SqlError>.Error<int, SqlError>(new SqlError("Transaction has no connection"));

        try
        {
            using (var command = new NpgsqlCommand(sql, (NpgsqlConnection)transaction.Connection!, (NpgsqlTransaction)transaction))
            {
                command.Parameters.AddWithValue("@Id", id ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ClinicalStatus", clinicalstatus ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@VerificationStatus", verificationstatus ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Category", category ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Severity", severity ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CodeSystem", codesystem ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CodeValue", codevalue ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CodeDisplay", codedisplay ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@SubjectReference", subjectreference ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@EncounterReference", encounterreference ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@OnsetDateTime", onsetdatetime ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@RecordedDate", recordeddate ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@RecorderReference", recorderreference ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@NoteText", notetext ?? (object)DBNull.Value);
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
