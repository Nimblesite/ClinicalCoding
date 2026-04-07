using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Npgsql;
using Outcome;
using Nimblesite.Sql.Model;

namespace Generated;

/// <summary>
/// Extension methods for 'GetMedicationsByPatient'.
/// </summary>
public static partial class GetMedicationsByPatientExtensions
{
    /// <summary>
    /// Executes 'GetMedicationsByPatient.sql' and maps results.
    /// </summary>
    /// <param name="connection">Open NpgsqlConnection connection.</param>
    /// <param name="patientId">Query parameter.</param>
    /// <returns>Result of records or SQL error.</returns>
    public static async Task<Result<ImmutableList<GetMedicationsByPatient>, SqlError>> GetMedicationsByPatientAsync(this NpgsqlConnection connection, object patientId)
    {
        const string sql = @"SELECT fhir_MedicationRequest.Id, fhir_MedicationRequest.Status, fhir_MedicationRequest.Intent, fhir_MedicationRequest.PatientId, fhir_MedicationRequest.PractitionerId, fhir_MedicationRequest.EncounterId, fhir_MedicationRequest.MedicationCode, fhir_MedicationRequest.MedicationDisplay, fhir_MedicationRequest.DosageInstruction, fhir_MedicationRequest.Quantity, fhir_MedicationRequest.Unit, fhir_MedicationRequest.Refills, fhir_MedicationRequest.AuthoredOn, fhir_MedicationRequest.LastUpdated, fhir_MedicationRequest.VersionId FROM fhir_MedicationRequest WHERE fhir_MedicationRequest.PatientId = @patientId ORDER BY fhir_MedicationRequest.AuthoredOn DESC";

        try
        {
            var results = ImmutableList.CreateBuilder<GetMedicationsByPatient>();

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
                        var item = new GetMedicationsByPatient(
                            reader.IsDBNull(0) ? null : reader.GetFieldValue<string>(0),
                            reader.IsDBNull(1) ? null : reader.GetFieldValue<string>(1),
                            reader.IsDBNull(2) ? null : reader.GetFieldValue<string>(2),
                            reader.IsDBNull(3) ? null : reader.GetFieldValue<string>(3),
                            reader.IsDBNull(4) ? null : reader.GetFieldValue<string>(4),
                            reader.IsDBNull(5) ? null : reader.GetFieldValue<string>(5),
                            reader.IsDBNull(6) ? null : reader.GetFieldValue<string>(6),
                            reader.IsDBNull(7) ? null : reader.GetFieldValue<string>(7),
                            reader.IsDBNull(8) ? null : reader.GetFieldValue<string>(8),
                            reader.IsDBNull(9) ? default(double) : reader.GetFieldValue<double>(9),
                            reader.IsDBNull(10) ? null : reader.GetFieldValue<string>(10),
                            reader.IsDBNull(11) ? default(long) : reader.GetFieldValue<long>(11),
                            reader.IsDBNull(12) ? null : reader.GetFieldValue<string>(12),
                            reader.IsDBNull(13) ? null : reader.GetFieldValue<string>(13),
                            reader.IsDBNull(14) ? default(long) : reader.GetFieldValue<long>(14)
                        );
                        results.Add(item);
                    }
                }
            }

            return new Result<ImmutableList<GetMedicationsByPatient>, SqlError>.Ok<ImmutableList<GetMedicationsByPatient>, SqlError>(results.ToImmutable());
        }
        catch (Exception ex)
        {
            return new Result<ImmutableList<GetMedicationsByPatient>, SqlError>.Error<ImmutableList<GetMedicationsByPatient>, SqlError>(new SqlError("Database error", ex));
        }
    }
}

/// <summary>
/// Result row for 'GetMedicationsByPatient' query.
/// </summary>
public record GetMedicationsByPatient
{
    /// <summary>Column 'Id'.</summary>
    public string Id { get; init; }

    /// <summary>Column 'Status'.</summary>
    public string Status { get; init; }

    /// <summary>Column 'Intent'.</summary>
    public string Intent { get; init; }

    /// <summary>Column 'PatientId'.</summary>
    public string PatientId { get; init; }

    /// <summary>Column 'PractitionerId'.</summary>
    public string PractitionerId { get; init; }

    /// <summary>Column 'EncounterId'.</summary>
    public string EncounterId { get; init; }

    /// <summary>Column 'MedicationCode'.</summary>
    public string MedicationCode { get; init; }

    /// <summary>Column 'MedicationDisplay'.</summary>
    public string MedicationDisplay { get; init; }

    /// <summary>Column 'DosageInstruction'.</summary>
    public string DosageInstruction { get; init; }

    /// <summary>Column 'Quantity'.</summary>
    public double Quantity { get; init; }

    /// <summary>Column 'Unit'.</summary>
    public string Unit { get; init; }

    /// <summary>Column 'Refills'.</summary>
    public long Refills { get; init; }

    /// <summary>Column 'AuthoredOn'.</summary>
    public string AuthoredOn { get; init; }

    /// <summary>Column 'LastUpdated'.</summary>
    public string LastUpdated { get; init; }

    /// <summary>Column 'VersionId'.</summary>
    public long VersionId { get; init; }

    /// <summary>Initializes a new instance of GetMedicationsByPatient.</summary>
    public GetMedicationsByPatient(
        string Id,
        string Status,
        string Intent,
        string PatientId,
        string PractitionerId,
        string EncounterId,
        string MedicationCode,
        string MedicationDisplay,
        string DosageInstruction,
        double Quantity,
        string Unit,
        long Refills,
        string AuthoredOn,
        string LastUpdated,
        long VersionId
    )
    {
        this.Id = Id;
        this.Status = Status;
        this.Intent = Intent;
        this.PatientId = PatientId;
        this.PractitionerId = PractitionerId;
        this.EncounterId = EncounterId;
        this.MedicationCode = MedicationCode;
        this.MedicationDisplay = MedicationDisplay;
        this.DosageInstruction = DosageInstruction;
        this.Quantity = Quantity;
        this.Unit = Unit;
        this.Refills = Refills;
        this.AuthoredOn = AuthoredOn;
        this.LastUpdated = LastUpdated;
        this.VersionId = VersionId;
    }
}
