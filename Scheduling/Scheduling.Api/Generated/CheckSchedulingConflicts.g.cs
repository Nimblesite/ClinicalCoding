using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Npgsql;
using Outcome;
using Selecta;

namespace Generated;

/// <summary>
/// Extension methods for 'CheckSchedulingConflicts'.
/// </summary>
public static partial class CheckSchedulingConflictsExtensions
{
    /// <summary>
    /// Executes 'CheckSchedulingConflicts.sql' and maps results.
    /// </summary>
    /// <param name="connection">Open NpgsqlConnection connection.</param>
    /// <param name="practitionerRef">Query parameter.</param>
    /// <param name="proposedEnd">Query parameter.</param>
    /// <param name="proposedStart">Query parameter.</param>
    /// <returns>Result of records or SQL error.</returns>
    public static async Task<Result<ImmutableList<CheckSchedulingConflicts>, SqlError>> CheckSchedulingConflictsAsync(this NpgsqlConnection connection, object practitionerRef, object proposedEnd, object proposedStart)
    {
        const string sql = @"SELECT fhir_Appointment.Id, fhir_Appointment.StartTime, fhir_Appointment.EndTime, fhir_Appointment.Status FROM fhir_Appointment WHERE fhir_Appointment.PractitionerReference = @practitionerRef AND fhir_Appointment.Status != 'cancelled' AND fhir_Appointment.StartTime < @proposedEnd AND fhir_Appointment.EndTime > @proposedStart";

        try
        {
            var results = ImmutableList.CreateBuilder<CheckSchedulingConflicts>();

            using (var command = new NpgsqlCommand(sql, connection))
            {
                if (practitionerRef is not null and not DBNull)
                    command.Parameters.AddWithValue("@practitionerRef", practitionerRef);
                else
                    command.Parameters.Add(new NpgsqlParameter("@practitionerRef", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });
                if (proposedEnd is not null and not DBNull)
                    command.Parameters.AddWithValue("@proposedEnd", proposedEnd);
                else
                    command.Parameters.Add(new NpgsqlParameter("@proposedEnd", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });
                if (proposedStart is not null and not DBNull)
                    command.Parameters.AddWithValue("@proposedStart", proposedStart);
                else
                    command.Parameters.Add(new NpgsqlParameter("@proposedStart", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var item = new CheckSchedulingConflicts(
                            reader.IsDBNull(0) ? null : reader.GetFieldValue<string>(0),
                            reader.IsDBNull(1) ? null : reader.GetFieldValue<string>(1),
                            reader.IsDBNull(2) ? null : reader.GetFieldValue<string>(2),
                            reader.IsDBNull(3) ? null : reader.GetFieldValue<string>(3)
                        );
                        results.Add(item);
                    }
                }
            }

            return new Result<ImmutableList<CheckSchedulingConflicts>, SqlError>.Ok<ImmutableList<CheckSchedulingConflicts>, SqlError>(results.ToImmutable());
        }
        catch (Exception ex)
        {
            return new Result<ImmutableList<CheckSchedulingConflicts>, SqlError>.Error<ImmutableList<CheckSchedulingConflicts>, SqlError>(new SqlError("Database error", ex));
        }
    }
}

/// <summary>
/// Result row for 'CheckSchedulingConflicts' query.
/// </summary>
public record CheckSchedulingConflicts
{
    /// <summary>Column 'Id'.</summary>
    public string Id { get; init; }

    /// <summary>Column 'StartTime'.</summary>
    public string StartTime { get; init; }

    /// <summary>Column 'EndTime'.</summary>
    public string EndTime { get; init; }

    /// <summary>Column 'Status'.</summary>
    public string Status { get; init; }

    /// <summary>Initializes a new instance of CheckSchedulingConflicts.</summary>
    public CheckSchedulingConflicts(
        string Id,
        string StartTime,
        string EndTime,
        string Status
    )
    {
        this.Id = Id;
        this.StartTime = StartTime;
        this.EndTime = EndTime;
        this.Status = Status;
    }
}
