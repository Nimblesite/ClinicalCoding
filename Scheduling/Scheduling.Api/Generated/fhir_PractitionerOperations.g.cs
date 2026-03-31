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
    /// Extension methods for table operations on fhir_Practitioner
    /// </summary>
    public static partial class fhir_PractitionerExtensions
    {

    /// <summary>
    /// Inserts a new row into the fhir_Practitioner table.
    /// </summary>
    public static async Task<Result<int, SqlError>> Insertfhir_PractitionerAsync(this IDbTransaction transaction, string? id, string? identifier, long? active, string? namefamily, string? namegiven, string? qualification, string? specialty, string? telecomemail, string? telecomphone)
    {
        const string sql = "INSERT INTO fhir_Practitioner (Id, Identifier, Active, NameFamily, NameGiven, Qualification, Specialty, TelecomEmail, TelecomPhone) VALUES (@Id, @Identifier, @Active, @NameFamily, @NameGiven, @Qualification, @Specialty, @TelecomEmail, @TelecomPhone)";

        if (transaction.Connection is null)
            return new Result<int, SqlError>.Error<int, SqlError>(new SqlError("Transaction has no connection"));

        try
        {
            using (var command = new NpgsqlCommand(sql, (NpgsqlConnection)transaction.Connection!, (NpgsqlTransaction)transaction))
            {
                command.Parameters.AddWithValue("@Id", id ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Identifier", identifier ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Active", active ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@NameFamily", namefamily ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@NameGiven", namegiven ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Qualification", qualification ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Specialty", specialty ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@TelecomEmail", telecomemail ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@TelecomPhone", telecomphone ?? (object)DBNull.Value);

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
