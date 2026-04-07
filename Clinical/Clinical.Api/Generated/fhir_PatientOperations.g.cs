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
    /// Extension methods for table operations on fhir_Patient
    /// </summary>
    public static partial class fhir_PatientExtensions
    {

    /// <summary>
    /// Inserts a new row into the fhir_Patient table.
    /// </summary>
    public static async Task<Result<int, SqlError>> Insertfhir_PatientAsync(this IDbTransaction transaction, string? id, long? active, string? givenname, string? familyname, string? birthdate, string? gender, string? phone, string? email, string? addressline, string? city, string? state, string? postalcode, string? country, string? lastupdated, long? versionid)
    {
        const string sql = "INSERT INTO fhir_Patient (Id, Active, GivenName, FamilyName, BirthDate, Gender, Phone, Email, AddressLine, City, State, PostalCode, Country, LastUpdated, VersionId) VALUES (@Id, @Active, @GivenName, @FamilyName, @BirthDate, @Gender, @Phone, @Email, @AddressLine, @City, @State, @PostalCode, @Country, @LastUpdated, @VersionId)";

        if (transaction.Connection is null)
            return new Result<int, SqlError>.Error<int, SqlError>(new SqlError("Transaction has no connection"));

        try
        {
            using (var command = new NpgsqlCommand(sql, (NpgsqlConnection)transaction.Connection!, (NpgsqlTransaction)transaction))
            {
                command.Parameters.AddWithValue("@Id", id ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Active", active ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@GivenName", givenname ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@FamilyName", familyname ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@BirthDate", birthdate ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Gender", gender ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Phone", phone ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Email", email ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@AddressLine", addressline ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@City", city ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@State", state ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PostalCode", postalcode ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Country", country ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@LastUpdated", lastupdated ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@VersionId", versionid ?? (object)DBNull.Value);

                var rowsAffected = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                return new Result<int, SqlError>.Ok<int, SqlError>(rowsAffected);
            }
        }
        catch (Exception ex)
        {
            return new Result<int, SqlError>.Error<int, SqlError>(new SqlError("Insert failed", ex));
        }
    }


    /// <summary>
    /// Updates a row in the fhir_Patient table.
    /// </summary>
    public static async Task<Result<int, SqlError>> Updatefhir_PatientAsync(this IDbTransaction transaction, string id, long? active, string givenname, string familyname, string birthdate, string gender, string phone, string email, string addressline, string city, string state, string postalcode, string country, string lastupdated, long? versionid)
    {
        const string sql = "UPDATE fhir_Patient SET Active = @Active, GivenName = @GivenName, FamilyName = @FamilyName, BirthDate = @BirthDate, Gender = @Gender, Phone = @Phone, Email = @Email, AddressLine = @AddressLine, City = @City, State = @State, PostalCode = @PostalCode, Country = @Country, LastUpdated = @LastUpdated, VersionId = @VersionId WHERE Id = @Id";

        if (transaction.Connection is null)
            return new Result<int, SqlError>.Error<int, SqlError>(new SqlError("Transaction has no connection"));

        try
        {
            using (var command = new NpgsqlCommand(sql, (NpgsqlConnection)transaction.Connection!, (NpgsqlTransaction)transaction))
            {
                command.Parameters.AddWithValue("@Id", id ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Active", active ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@GivenName", givenname ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@FamilyName", familyname ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@BirthDate", birthdate ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Gender", gender ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Phone", phone ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Email", email ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@AddressLine", addressline ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@City", city ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@State", state ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PostalCode", postalcode ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Country", country ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@LastUpdated", lastupdated ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@VersionId", versionid ?? (object)DBNull.Value);

                var rowsAffected = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                return new Result<int, SqlError>.Ok<int, SqlError>(rowsAffected);
            }
        }
        catch (Exception ex)
        {
            return new Result<int, SqlError>.Error<int, SqlError>(new SqlError("Update failed", ex));
        }
    }

    }
}
