using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Npgsql;
using Outcome;
using Selecta;

namespace Generated;

/// <summary>
/// Extension methods for 'GetAppointmentsByPatient'.
/// </summary>
public static partial class GetAppointmentsByPatientExtensions
{
    /// <summary>
    /// Executes 'GetAppointmentsByPatient.sql' and maps results.
    /// </summary>
    /// <param name="connection">Open NpgsqlConnection connection.</param>
    /// <param name="patientReference">Query parameter.</param>
    /// <returns>Result of records or SQL error.</returns>
    public static async Task<Result<ImmutableList<GetAppointmentsByPatient>, SqlError>> GetAppointmentsByPatientAsync(this NpgsqlConnection connection, object patientReference)
    {
        const string sql = @"SELECT fhir_Appointment.Id, fhir_Appointment.Status, fhir_Appointment.ServiceCategory, fhir_Appointment.ServiceType, fhir_Appointment.ReasonCode, fhir_Appointment.Priority, fhir_Appointment.Description, fhir_Appointment.StartTime, fhir_Appointment.EndTime, fhir_Appointment.MinutesDuration, fhir_Appointment.PatientReference, fhir_Appointment.PractitionerReference, fhir_Appointment.Created, fhir_Appointment.Comment FROM fhir_Appointment WHERE fhir_Appointment.PatientReference = @patientReference ORDER BY fhir_Appointment.StartTime DESC";

        try
        {
            var results = ImmutableList.CreateBuilder<GetAppointmentsByPatient>();

            using (var command = new NpgsqlCommand(sql, connection))
            {
                if (patientReference is not null and not DBNull)
                    command.Parameters.AddWithValue("@patientReference", patientReference);
                else
                    command.Parameters.Add(new NpgsqlParameter("@patientReference", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var item = new GetAppointmentsByPatient(
                            reader.IsDBNull(0) ? null : reader.GetFieldValue<string>(0),
                            reader.IsDBNull(1) ? null : reader.GetFieldValue<string>(1),
                            reader.IsDBNull(2) ? null : reader.GetFieldValue<string>(2),
                            reader.IsDBNull(3) ? null : reader.GetFieldValue<string>(3),
                            reader.IsDBNull(4) ? null : reader.GetFieldValue<string>(4),
                            reader.IsDBNull(5) ? null : reader.GetFieldValue<string>(5),
                            reader.IsDBNull(6) ? null : reader.GetFieldValue<string>(6),
                            reader.IsDBNull(7) ? null : reader.GetFieldValue<string>(7),
                            reader.IsDBNull(8) ? null : reader.GetFieldValue<string>(8),
                            reader.IsDBNull(9) ? default(long) : reader.GetFieldValue<long>(9),
                            reader.IsDBNull(10) ? null : reader.GetFieldValue<string>(10),
                            reader.IsDBNull(11) ? null : reader.GetFieldValue<string>(11),
                            reader.IsDBNull(12) ? null : reader.GetFieldValue<string>(12),
                            reader.IsDBNull(13) ? null : reader.GetFieldValue<string>(13)
                        );
                        results.Add(item);
                    }
                }
            }

            return new Result<ImmutableList<GetAppointmentsByPatient>, SqlError>.Ok<ImmutableList<GetAppointmentsByPatient>, SqlError>(results.ToImmutable());
        }
        catch (Exception ex)
        {
            return new Result<ImmutableList<GetAppointmentsByPatient>, SqlError>.Error<ImmutableList<GetAppointmentsByPatient>, SqlError>(new SqlError("Database error", ex));
        }
    }
}

/// <summary>
/// Result row for 'GetAppointmentsByPatient' query.
/// </summary>
public record GetAppointmentsByPatient
{
    /// <summary>Column 'Id'.</summary>
    public string Id { get; init; }

    /// <summary>Column 'Status'.</summary>
    public string Status { get; init; }

    /// <summary>Column 'ServiceCategory'.</summary>
    public string ServiceCategory { get; init; }

    /// <summary>Column 'ServiceType'.</summary>
    public string ServiceType { get; init; }

    /// <summary>Column 'ReasonCode'.</summary>
    public string ReasonCode { get; init; }

    /// <summary>Column 'Priority'.</summary>
    public string Priority { get; init; }

    /// <summary>Column 'Description'.</summary>
    public string Description { get; init; }

    /// <summary>Column 'StartTime'.</summary>
    public string StartTime { get; init; }

    /// <summary>Column 'EndTime'.</summary>
    public string EndTime { get; init; }

    /// <summary>Column 'MinutesDuration'.</summary>
    public long MinutesDuration { get; init; }

    /// <summary>Column 'PatientReference'.</summary>
    public string PatientReference { get; init; }

    /// <summary>Column 'PractitionerReference'.</summary>
    public string PractitionerReference { get; init; }

    /// <summary>Column 'Created'.</summary>
    public string Created { get; init; }

    /// <summary>Column 'Comment'.</summary>
    public string Comment { get; init; }

    /// <summary>Initializes a new instance of GetAppointmentsByPatient.</summary>
    public GetAppointmentsByPatient(
        string Id,
        string Status,
        string ServiceCategory,
        string ServiceType,
        string ReasonCode,
        string Priority,
        string Description,
        string StartTime,
        string EndTime,
        long MinutesDuration,
        string PatientReference,
        string PractitionerReference,
        string Created,
        string Comment
    )
    {
        this.Id = Id;
        this.Status = Status;
        this.ServiceCategory = ServiceCategory;
        this.ServiceType = ServiceType;
        this.ReasonCode = ReasonCode;
        this.Priority = Priority;
        this.Description = Description;
        this.StartTime = StartTime;
        this.EndTime = EndTime;
        this.MinutesDuration = MinutesDuration;
        this.PatientReference = PatientReference;
        this.PractitionerReference = PractitionerReference;
        this.Created = Created;
        this.Comment = Comment;
    }
}
