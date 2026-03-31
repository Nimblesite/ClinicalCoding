using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Npgsql;
using Outcome;
using Selecta;

namespace Generated;

/// <summary>
/// Extension methods for 'GetAppointmentsByStatus'.
/// </summary>
public static partial class GetAppointmentsByStatusExtensions
{
    /// <summary>
    /// Executes 'GetAppointmentsByStatus.sql' and maps results.
    /// </summary>
    /// <param name="connection">Open NpgsqlConnection connection.</param>
    /// <param name="status">Query parameter.</param>
    /// <param name="dateStart">Query parameter.</param>
    /// <param name="dateEnd">Query parameter.</param>
    /// <returns>Result of records or SQL error.</returns>
    public static async Task<Result<ImmutableList<GetAppointmentsByStatus>, SqlError>> GetAppointmentsByStatusAsync(this NpgsqlConnection connection, object status, object dateStart, object dateEnd)
    {
        const string sql = @"SELECT fhir_Appointment.Id, fhir_Appointment.StartTime, fhir_Appointment.EndTime, fhir_Appointment.Status, sync_ScheduledPatient.DisplayName, fhir_Practitioner.NameFamily, fhir_Practitioner.NameGiven, fhir_Appointment.ServiceType, fhir_Appointment.ReasonCode FROM fhir_Appointment INNER JOIN sync_ScheduledPatient ON fhir_Appointment.PatientReference = sync_ScheduledPatient.PatientId INNER JOIN fhir_Practitioner ON fhir_Appointment.PractitionerReference = fhir_Practitioner.Id WHERE fhir_Appointment.Status = @status AND fhir_Appointment.StartTime >= @dateStart AND fhir_Appointment.StartTime < @dateEnd ORDER BY fhir_Appointment.StartTime ";

        try
        {
            var results = ImmutableList.CreateBuilder<GetAppointmentsByStatus>();

            using (var command = new NpgsqlCommand(sql, connection))
            {
                if (status is not null and not DBNull)
                    command.Parameters.AddWithValue("@status", status);
                else
                    command.Parameters.Add(new NpgsqlParameter("@status", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });
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
                        var item = new GetAppointmentsByStatus(
                            reader.IsDBNull(0) ? null : reader.GetFieldValue<string>(0),
                            reader.IsDBNull(1) ? null : reader.GetFieldValue<string>(1),
                            reader.IsDBNull(2) ? null : reader.GetFieldValue<string>(2),
                            reader.IsDBNull(3) ? null : reader.GetFieldValue<string>(3),
                            reader.IsDBNull(4) ? null : reader.GetFieldValue<string>(4),
                            reader.IsDBNull(5) ? null : reader.GetFieldValue<string>(5),
                            reader.IsDBNull(6) ? null : reader.GetFieldValue<string>(6),
                            reader.IsDBNull(7) ? null : reader.GetFieldValue<string>(7),
                            reader.IsDBNull(8) ? null : reader.GetFieldValue<string>(8)
                        );
                        results.Add(item);
                    }
                }
            }

            return new Result<ImmutableList<GetAppointmentsByStatus>, SqlError>.Ok<ImmutableList<GetAppointmentsByStatus>, SqlError>(results.ToImmutable());
        }
        catch (Exception ex)
        {
            return new Result<ImmutableList<GetAppointmentsByStatus>, SqlError>.Error<ImmutableList<GetAppointmentsByStatus>, SqlError>(new SqlError("Database error", ex));
        }
    }
}

/// <summary>
/// Result row for 'GetAppointmentsByStatus' query.
/// </summary>
public record GetAppointmentsByStatus
{
    /// <summary>Column 'Id'.</summary>
    public string Id { get; init; }

    /// <summary>Column 'StartTime'.</summary>
    public string StartTime { get; init; }

    /// <summary>Column 'EndTime'.</summary>
    public string EndTime { get; init; }

    /// <summary>Column 'Status'.</summary>
    public string Status { get; init; }

    /// <summary>Column 'DisplayName'.</summary>
    public string DisplayName { get; init; }

    /// <summary>Column 'NameFamily'.</summary>
    public string NameFamily { get; init; }

    /// <summary>Column 'NameGiven'.</summary>
    public string NameGiven { get; init; }

    /// <summary>Column 'ServiceType'.</summary>
    public string ServiceType { get; init; }

    /// <summary>Column 'ReasonCode'.</summary>
    public string ReasonCode { get; init; }

    /// <summary>Initializes a new instance of GetAppointmentsByStatus.</summary>
    public GetAppointmentsByStatus(
        string Id,
        string StartTime,
        string EndTime,
        string Status,
        string DisplayName,
        string NameFamily,
        string NameGiven,
        string ServiceType,
        string ReasonCode
    )
    {
        this.Id = Id;
        this.StartTime = StartTime;
        this.EndTime = EndTime;
        this.Status = Status;
        this.DisplayName = DisplayName;
        this.NameFamily = NameFamily;
        this.NameGiven = NameGiven;
        this.ServiceType = ServiceType;
        this.ReasonCode = ReasonCode;
    }
}
