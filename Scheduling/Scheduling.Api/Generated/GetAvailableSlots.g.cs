using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Npgsql;
using Outcome;
using Selecta;

namespace Generated;

/// <summary>
/// Extension methods for 'GetAvailableSlots'.
/// </summary>
public static partial class GetAvailableSlotsExtensions
{
    /// <summary>
    /// Executes 'GetAvailableSlots.sql' and maps results.
    /// </summary>
    /// <param name="connection">Open NpgsqlConnection connection.</param>
    /// <param name="practitionerRef">Query parameter.</param>
    /// <param name="fromDate">Query parameter.</param>
    /// <param name="toDate">Query parameter.</param>
    /// <returns>Result of records or SQL error.</returns>
    public static async Task<Result<ImmutableList<GetAvailableSlots>, SqlError>> GetAvailableSlotsAsync(this NpgsqlConnection connection, object practitionerRef, object fromDate, object toDate)
    {
        const string sql = @"SELECT fhir_Slot.Id, fhir_Slot.Status, fhir_Slot.StartTime, fhir_Slot.EndTime, fhir_Schedule.PractitionerReference FROM fhir_Slot INNER JOIN fhir_Schedule ON fhir_Slot.ScheduleReference = fhir_Schedule.Id WHERE fhir_Schedule.PractitionerReference = @practitionerRef AND fhir_Slot.Status = 'free' AND fhir_Slot.StartTime >= @fromDate AND fhir_Slot.StartTime < @toDate ORDER BY fhir_Slot.StartTime ";

        try
        {
            var results = ImmutableList.CreateBuilder<GetAvailableSlots>();

            using (var command = new NpgsqlCommand(sql, connection))
            {
                if (practitionerRef is not null and not DBNull)
                    command.Parameters.AddWithValue("@practitionerRef", practitionerRef);
                else
                    command.Parameters.Add(new NpgsqlParameter("@practitionerRef", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });
                if (fromDate is not null and not DBNull)
                    command.Parameters.AddWithValue("@fromDate", fromDate);
                else
                    command.Parameters.Add(new NpgsqlParameter("@fromDate", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });
                if (toDate is not null and not DBNull)
                    command.Parameters.AddWithValue("@toDate", toDate);
                else
                    command.Parameters.Add(new NpgsqlParameter("@toDate", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var item = new GetAvailableSlots(
                            reader.IsDBNull(0) ? null : reader.GetFieldValue<string>(0),
                            reader.IsDBNull(1) ? null : reader.GetFieldValue<string>(1),
                            reader.IsDBNull(2) ? null : reader.GetFieldValue<string>(2),
                            reader.IsDBNull(3) ? null : reader.GetFieldValue<string>(3),
                            reader.IsDBNull(4) ? null : reader.GetFieldValue<string>(4)
                        );
                        results.Add(item);
                    }
                }
            }

            return new Result<ImmutableList<GetAvailableSlots>, SqlError>.Ok<ImmutableList<GetAvailableSlots>, SqlError>(results.ToImmutable());
        }
        catch (Exception ex)
        {
            return new Result<ImmutableList<GetAvailableSlots>, SqlError>.Error<ImmutableList<GetAvailableSlots>, SqlError>(new SqlError("Database error", ex));
        }
    }
}

/// <summary>
/// Result row for 'GetAvailableSlots' query.
/// </summary>
public record GetAvailableSlots
{
    /// <summary>Column 'Id'.</summary>
    public string Id { get; init; }

    /// <summary>Column 'Status'.</summary>
    public string Status { get; init; }

    /// <summary>Column 'StartTime'.</summary>
    public string StartTime { get; init; }

    /// <summary>Column 'EndTime'.</summary>
    public string EndTime { get; init; }

    /// <summary>Column 'PractitionerReference'.</summary>
    public string PractitionerReference { get; init; }

    /// <summary>Initializes a new instance of GetAvailableSlots.</summary>
    public GetAvailableSlots(
        string Id,
        string Status,
        string StartTime,
        string EndTime,
        string PractitionerReference
    )
    {
        this.Id = Id;
        this.Status = Status;
        this.StartTime = StartTime;
        this.EndTime = EndTime;
        this.PractitionerReference = PractitionerReference;
    }
}
