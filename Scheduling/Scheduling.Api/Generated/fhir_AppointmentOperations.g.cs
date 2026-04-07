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
    /// Extension methods for table operations on fhir_Appointment
    /// </summary>
    public static partial class fhir_AppointmentExtensions
    {

    /// <summary>
    /// Inserts a new row into the fhir_Appointment table.
    /// </summary>
    public static async Task<Result<int, SqlError>> Insertfhir_AppointmentAsync(this IDbTransaction transaction, string? id, string? status, string? servicecategory, string? servicetype, string? reasoncode, string? priority, string? description, string? starttime, string? endtime, long? minutesduration, string? patientreference, string? practitionerreference, string? created, string? comment)
    {
        const string sql = "INSERT INTO fhir_Appointment (Id, Status, ServiceCategory, ServiceType, ReasonCode, Priority, Description, StartTime, EndTime, MinutesDuration, PatientReference, PractitionerReference, Created, Comment) VALUES (@Id, @Status, @ServiceCategory, @ServiceType, @ReasonCode, @Priority, @Description, @StartTime, @EndTime, @MinutesDuration, @PatientReference, @PractitionerReference, @Created, @Comment)";

        if (transaction.Connection is null)
            return new Result<int, SqlError>.Error<int, SqlError>(new SqlError("Transaction has no connection"));

        try
        {
            using (var command = new NpgsqlCommand(sql, (NpgsqlConnection)transaction.Connection!, (NpgsqlTransaction)transaction))
            {
                command.Parameters.AddWithValue("@Id", id ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Status", status ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ServiceCategory", servicecategory ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ServiceType", servicetype ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ReasonCode", reasoncode ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Priority", priority ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Description", description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@StartTime", starttime ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@EndTime", endtime ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@MinutesDuration", minutesduration ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PatientReference", patientreference ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PractitionerReference", practitionerreference ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Created", created ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Comment", comment ?? (object)DBNull.Value);

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
