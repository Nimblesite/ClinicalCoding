using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Npgsql;
using Outcome;
using Nimblesite.Sql.Model;

namespace Generated;

/// <summary>
/// Extension methods for 'GetEncountersByPatient'.
/// </summary>
public static partial class GetEncountersByPatientExtensions
{
    /// <summary>
    /// Executes 'GetEncountersByPatient.sql' and maps results.
    /// </summary>
    /// <param name="connection">Open NpgsqlConnection connection.</param>
    /// <param name="patientId">Query parameter.</param>
    /// <returns>Result of records or SQL error.</returns>
    public static async Task<Result<ImmutableList<GetEncountersByPatient>, SqlError>> GetEncountersByPatientAsync(this NpgsqlConnection connection, object patientId)
    {
        const string sql = @"SELECT fhir_Encounter.Id, fhir_Encounter.Status, fhir_Encounter.Class, fhir_Encounter.PatientId, fhir_Encounter.PractitionerId, fhir_Encounter.ServiceType, fhir_Encounter.ReasonCode, fhir_Encounter.PeriodStart, fhir_Encounter.PeriodEnd, fhir_Encounter.Notes, fhir_Encounter.LastUpdated, fhir_Encounter.VersionId FROM fhir_Encounter WHERE fhir_Encounter.PatientId = @patientId ORDER BY fhir_Encounter.PeriodStart DESC";

        try
        {
            var results = ImmutableList.CreateBuilder<GetEncountersByPatient>();

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
                        var item = new GetEncountersByPatient(
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
                            reader.IsDBNull(11) ? default(long) : reader.GetFieldValue<long>(11)
                        );
                        results.Add(item);
                    }
                }
            }

            return new Result<ImmutableList<GetEncountersByPatient>, SqlError>.Ok<ImmutableList<GetEncountersByPatient>, SqlError>(results.ToImmutable());
        }
        catch (Exception ex)
        {
            return new Result<ImmutableList<GetEncountersByPatient>, SqlError>.Error<ImmutableList<GetEncountersByPatient>, SqlError>(new SqlError("Database error", ex));
        }
    }
}

/// <summary>
/// Result row for 'GetEncountersByPatient' query.
/// </summary>
public record GetEncountersByPatient
{
    /// <summary>Column 'Id'.</summary>
    public string Id { get; init; }

    /// <summary>Column 'Status'.</summary>
    public string Status { get; init; }

    /// <summary>Column 'Class'.</summary>
    public string Class { get; init; }

    /// <summary>Column 'PatientId'.</summary>
    public string PatientId { get; init; }

    /// <summary>Column 'PractitionerId'.</summary>
    public string PractitionerId { get; init; }

    /// <summary>Column 'ServiceType'.</summary>
    public string ServiceType { get; init; }

    /// <summary>Column 'ReasonCode'.</summary>
    public string ReasonCode { get; init; }

    /// <summary>Column 'PeriodStart'.</summary>
    public string PeriodStart { get; init; }

    /// <summary>Column 'PeriodEnd'.</summary>
    public string PeriodEnd { get; init; }

    /// <summary>Column 'Notes'.</summary>
    public string Notes { get; init; }

    /// <summary>Column 'LastUpdated'.</summary>
    public string LastUpdated { get; init; }

    /// <summary>Column 'VersionId'.</summary>
    public long VersionId { get; init; }

    /// <summary>Initializes a new instance of GetEncountersByPatient.</summary>
    public GetEncountersByPatient(
        string Id,
        string Status,
        string Class,
        string PatientId,
        string PractitionerId,
        string ServiceType,
        string ReasonCode,
        string PeriodStart,
        string PeriodEnd,
        string Notes,
        string LastUpdated,
        long VersionId
    )
    {
        this.Id = Id;
        this.Status = Status;
        this.Class = Class;
        this.PatientId = PatientId;
        this.PractitionerId = PractitionerId;
        this.ServiceType = ServiceType;
        this.ReasonCode = ReasonCode;
        this.PeriodStart = PeriodStart;
        this.PeriodEnd = PeriodEnd;
        this.Notes = Notes;
        this.LastUpdated = LastUpdated;
        this.VersionId = VersionId;
    }
}
