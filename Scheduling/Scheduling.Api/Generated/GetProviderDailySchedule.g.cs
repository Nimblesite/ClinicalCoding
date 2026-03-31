using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Npgsql;
using Outcome;
using Selecta;

namespace Generated;

/// <summary>
/// Extension methods for 'GetProviderDailySchedule'.
/// </summary>
public static partial class GetProviderDailyScheduleExtensions
{
    /// <summary>
    /// Executes 'GetProviderDailySchedule.sql' and maps results.
    /// </summary>
    /// <param name="connection">Open NpgsqlConnection connection.</param>
    /// <param name="practitionerRef">Query parameter.</param>
    /// <param name="dateStart">Query parameter.</param>
    /// <param name="dateEnd">Query parameter.</param>
    /// <returns>Result of records or SQL error.</returns>
    public static async Task<Result<ImmutableList<GetProviderDailySchedule>, SqlError>> GetProviderDailyScheduleAsync(this NpgsqlConnection connection, object practitionerRef, object dateStart, object dateEnd)
    {
        const string sql = @"SELECT fhir_Appointment.Id, fhir_Appointment.StartTime, fhir_Appointment.EndTime, fhir_Appointment.MinutesDuration, fhir_Appointment.Status, fhir_Appointment.ServiceCategory, fhir_Appointment.ServiceType, fhir_Appointment.ReasonCode, fhir_Appointment.Description, fhir_Appointment.PatientReference, sync_ScheduledPatient.PatientId, sync_ScheduledPatient.DisplayName, sync_ScheduledPatient.ContactPhone, fhir_Appointment.PractitionerReference FROM fhir_Appointment INNER JOIN sync_ScheduledPatient ON fhir_Appointment.PatientReference = sync_ScheduledPatient.PatientId WHERE fhir_Appointment.PractitionerReference = @practitionerRef AND fhir_Appointment.StartTime >= @dateStart AND fhir_Appointment.StartTime < @dateEnd ORDER BY fhir_Appointment.StartTime ";

        try
        {
            var results = ImmutableList.CreateBuilder<GetProviderDailySchedule>();

            using (var command = new NpgsqlCommand(sql, connection))
            {
                if (practitionerRef is not null and not DBNull)
                    command.Parameters.AddWithValue("@practitionerRef", practitionerRef);
                else
                    command.Parameters.Add(new NpgsqlParameter("@practitionerRef", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });
                if (dateStart is not null and not DBNull)
                    command.Parameters.AddWithValue("@dateStart", dateStart);
                else
                    command.Parameters.Add(new NpgsqlParameter("@dateStart", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });
                if (dateEnd is not null and not DBNull)
                    command.Parameters.AddWithValue("@dateEnd", dateEnd);
                else
                    command.Parameters.Add(new NpgsqlParameter("@dateEnd", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var item = new GetProviderDailySchedule(
                            reader.IsDBNull(0) ? null : reader.GetFieldValue<string>(0),
                            reader.IsDBNull(1) ? null : reader.GetFieldValue<string>(1),
                            reader.IsDBNull(2) ? null : reader.GetFieldValue<string>(2),
                            reader.IsDBNull(3) ? default(long) : reader.GetFieldValue<long>(3),
                            reader.IsDBNull(4) ? null : reader.GetFieldValue<string>(4),
                            reader.IsDBNull(5) ? null : reader.GetFieldValue<string>(5),
                            reader.IsDBNull(6) ? null : reader.GetFieldValue<string>(6),
                            reader.IsDBNull(7) ? null : reader.GetFieldValue<string>(7),
                            reader.IsDBNull(8) ? null : reader.GetFieldValue<string>(8),
                            reader.IsDBNull(9) ? null : reader.GetFieldValue<string>(9),
                            reader.IsDBNull(10) ? null : reader.GetFieldValue<string>(10),
                            reader.IsDBNull(11) ? null : reader.GetFieldValue<string>(11),
                            reader.IsDBNull(12) ? null : reader.GetFieldValue<string>(12),
                            reader.IsDBNull(13) ? null : reader.GetFieldValue<string>(13)
                        );
                        results.Add(item);
                    }
                }
            }

            return new Result<ImmutableList<GetProviderDailySchedule>, SqlError>.Ok<ImmutableList<GetProviderDailySchedule>, SqlError>(results.ToImmutable());
        }
        catch (Exception ex)
        {
            return new Result<ImmutableList<GetProviderDailySchedule>, SqlError>.Error<ImmutableList<GetProviderDailySchedule>, SqlError>(new SqlError("Database error", ex));
        }
    }
}

/// <summary>
/// Result row for 'GetProviderDailySchedule' query.
/// </summary>
public record GetProviderDailySchedule
{
    /// <summary>Column 'Id'.</summary>
    public string Id { get; init; }

    /// <summary>Column 'StartTime'.</summary>
    public string StartTime { get; init; }

    /// <summary>Column 'EndTime'.</summary>
    public string EndTime { get; init; }

    /// <summary>Column 'MinutesDuration'.</summary>
    public long MinutesDuration { get; init; }

    /// <summary>Column 'Status'.</summary>
    public string Status { get; init; }

    /// <summary>Column 'ServiceCategory'.</summary>
    public string ServiceCategory { get; init; }

    /// <summary>Column 'ServiceType'.</summary>
    public string ServiceType { get; init; }

    /// <summary>Column 'ReasonCode'.</summary>
    public string ReasonCode { get; init; }

    /// <summary>Column 'Description'.</summary>
    public string Description { get; init; }

    /// <summary>Column 'PatientReference'.</summary>
    public string PatientReference { get; init; }

    /// <summary>Column 'PatientId'.</summary>
    public string PatientId { get; init; }

    /// <summary>Column 'DisplayName'.</summary>
    public string DisplayName { get; init; }

    /// <summary>Column 'ContactPhone'.</summary>
    public string ContactPhone { get; init; }

    /// <summary>Column 'PractitionerReference'.</summary>
    public string PractitionerReference { get; init; }

    /// <summary>Initializes a new instance of GetProviderDailySchedule.</summary>
    public GetProviderDailySchedule(
        string Id,
        string StartTime,
        string EndTime,
        long MinutesDuration,
        string Status,
        string ServiceCategory,
        string ServiceType,
        string ReasonCode,
        string Description,
        string PatientReference,
        string PatientId,
        string DisplayName,
        string ContactPhone,
        string PractitionerReference
    )
    {
        this.Id = Id;
        this.StartTime = StartTime;
        this.EndTime = EndTime;
        this.MinutesDuration = MinutesDuration;
        this.Status = Status;
        this.ServiceCategory = ServiceCategory;
        this.ServiceType = ServiceType;
        this.ReasonCode = ReasonCode;
        this.Description = Description;
        this.PatientReference = PatientReference;
        this.PatientId = PatientId;
        this.DisplayName = DisplayName;
        this.ContactPhone = ContactPhone;
        this.PractitionerReference = PractitionerReference;
    }
}
