using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Npgsql;
using Outcome;
using Nimblesite.Sql.Model;

namespace Generated;

/// <summary>
/// Extension methods for 'GetConditionsByPatient'.
/// </summary>
public static partial class GetConditionsByPatientExtensions
{
    /// <summary>
    /// Executes 'GetConditionsByPatient.sql' and maps results.
    /// </summary>
    /// <param name="connection">Open NpgsqlConnection connection.</param>
    /// <param name="patientId">Query parameter.</param>
    /// <returns>Result of records or SQL error.</returns>
    public static async Task<Result<ImmutableList<GetConditionsByPatient>, SqlError>> GetConditionsByPatientAsync(this NpgsqlConnection connection, object patientId)
    {
        const string sql = @"SELECT fhir_Condition.Id, fhir_Condition.ClinicalStatus, fhir_Condition.VerificationStatus, fhir_Condition.Category, fhir_Condition.Severity, fhir_Condition.CodeSystem, fhir_Condition.CodeValue, fhir_Condition.CodeDisplay, fhir_Condition.SubjectReference, fhir_Condition.EncounterReference, fhir_Condition.OnsetDateTime, fhir_Condition.RecordedDate, fhir_Condition.RecorderReference, fhir_Condition.NoteText, fhir_Condition.LastUpdated, fhir_Condition.VersionId FROM fhir_Condition WHERE fhir_Condition.SubjectReference = @patientId ORDER BY fhir_Condition.RecordedDate DESC";

        try
        {
            var results = ImmutableList.CreateBuilder<GetConditionsByPatient>();

            using (var command = new NpgsqlCommand(sql, connection))
            {
                if (patientId is not null and not DBNull)
                    command.Parameters.AddWithValue("@patientId", patientId);
                else
                    command.Parameters.Add(new NpgsqlParameter("@patientId", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var item = new GetConditionsByPatient(
                            reader.IsDBNull(0) ? null : reader.GetFieldValue<string>(0),
                            reader.IsDBNull(1) ? null : reader.GetFieldValue<string>(1),
                            reader.IsDBNull(2) ? null : reader.GetFieldValue<string>(2),
                            reader.IsDBNull(3) ? null : reader.GetFieldValue<string>(3),
                            reader.IsDBNull(4) ? null : reader.GetFieldValue<string>(4),
                            reader.IsDBNull(5) ? null : reader.GetFieldValue<string>(5),
                            reader.IsDBNull(6) ? null : reader.GetFieldValue<string>(6),
                            reader.IsDBNull(7) ? null : reader.GetFieldValue<string>(7),
                            reader.IsDBNull(8) ? null : reader.GetFieldValue<string>(8),
                            reader.IsDBNull(9) ? null : reader.GetFieldValue<string>(9),
                            reader.IsDBNull(10) ? null : reader.GetFieldValue<string>(10),
                            reader.IsDBNull(11) ? null : reader.GetFieldValue<string>(11),
                            reader.IsDBNull(12) ? null : reader.GetFieldValue<string>(12),
                            reader.IsDBNull(13) ? null : reader.GetFieldValue<string>(13),
                            reader.IsDBNull(14) ? null : reader.GetFieldValue<string>(14),
                            reader.IsDBNull(15) ? default(long) : reader.GetFieldValue<long>(15)
                        );
                        results.Add(item);
                    }
                }
            }

            return new Result<ImmutableList<GetConditionsByPatient>, SqlError>.Ok<ImmutableList<GetConditionsByPatient>, SqlError>(results.ToImmutable());
        }
        catch (Exception ex)
        {
            return new Result<ImmutableList<GetConditionsByPatient>, SqlError>.Error<ImmutableList<GetConditionsByPatient>, SqlError>(new SqlError("Database error", ex));
        }
    }
}

/// <summary>
/// Result row for 'GetConditionsByPatient' query.
/// </summary>
public record GetConditionsByPatient
{
    /// <summary>Column 'Id'.</summary>
    public string Id { get; init; }

    /// <summary>Column 'ClinicalStatus'.</summary>
    public string ClinicalStatus { get; init; }

    /// <summary>Column 'VerificationStatus'.</summary>
    public string VerificationStatus { get; init; }

    /// <summary>Column 'Category'.</summary>
    public string Category { get; init; }

    /// <summary>Column 'Severity'.</summary>
    public string Severity { get; init; }

    /// <summary>Column 'CodeSystem'.</summary>
    public string CodeSystem { get; init; }

    /// <summary>Column 'CodeValue'.</summary>
    public string CodeValue { get; init; }

    /// <summary>Column 'CodeDisplay'.</summary>
    public string CodeDisplay { get; init; }

    /// <summary>Column 'SubjectReference'.</summary>
    public string SubjectReference { get; init; }

    /// <summary>Column 'EncounterReference'.</summary>
    public string EncounterReference { get; init; }

    /// <summary>Column 'OnsetDateTime'.</summary>
    public string OnsetDateTime { get; init; }

    /// <summary>Column 'RecordedDate'.</summary>
    public string RecordedDate { get; init; }

    /// <summary>Column 'RecorderReference'.</summary>
    public string RecorderReference { get; init; }

    /// <summary>Column 'NoteText'.</summary>
    public string NoteText { get; init; }

    /// <summary>Column 'LastUpdated'.</summary>
    public string LastUpdated { get; init; }

    /// <summary>Column 'VersionId'.</summary>
    public long VersionId { get; init; }

    /// <summary>Initializes a new instance of GetConditionsByPatient.</summary>
    public GetConditionsByPatient(
        string Id,
        string ClinicalStatus,
        string VerificationStatus,
        string Category,
        string Severity,
        string CodeSystem,
        string CodeValue,
        string CodeDisplay,
        string SubjectReference,
        string EncounterReference,
        string OnsetDateTime,
        string RecordedDate,
        string RecorderReference,
        string NoteText,
        string LastUpdated,
        long VersionId
    )
    {
        this.Id = Id;
        this.ClinicalStatus = ClinicalStatus;
        this.VerificationStatus = VerificationStatus;
        this.Category = Category;
        this.Severity = Severity;
        this.CodeSystem = CodeSystem;
        this.CodeValue = CodeValue;
        this.CodeDisplay = CodeDisplay;
        this.SubjectReference = SubjectReference;
        this.EncounterReference = EncounterReference;
        this.OnsetDateTime = OnsetDateTime;
        this.RecordedDate = RecordedDate;
        this.RecorderReference = RecorderReference;
        this.NoteText = NoteText;
        this.LastUpdated = LastUpdated;
        this.VersionId = VersionId;
    }
}
