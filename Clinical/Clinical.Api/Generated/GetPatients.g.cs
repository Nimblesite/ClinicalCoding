using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Npgsql;
using Outcome;
using Selecta;

namespace Generated;

/// <summary>
/// Extension methods for 'GetPatients'.
/// </summary>
public static partial class GetPatientsExtensions
{
    /// <summary>
    /// Executes 'GetPatients.sql' and maps results.
    /// </summary>
    /// <param name="connection">Open NpgsqlConnection connection.</param>
    /// <param name="active">Query parameter.</param>
    /// <param name="familyName">Query parameter.</param>
    /// <param name="givenName">Query parameter.</param>
    /// <param name="gender">Query parameter.</param>
    /// <returns>Result of records or SQL error.</returns>
    public static async Task<Result<ImmutableList<GetPatients>, SqlError>> GetPatientsAsync(this NpgsqlConnection connection, object active, object familyName, object givenName, object gender)
    {
        const string sql = @"SELECT fhir_Patient.Id, fhir_Patient.Active, fhir_Patient.GivenName, fhir_Patient.FamilyName, fhir_Patient.BirthDate, fhir_Patient.Gender, fhir_Patient.Phone, fhir_Patient.Email, fhir_Patient.AddressLine, fhir_Patient.City, fhir_Patient.State, fhir_Patient.PostalCode, fhir_Patient.Country, fhir_Patient.LastUpdated, fhir_Patient.VersionId FROM fhir_Patient WHERE (@active IS NULL OR fhir_Patient.Active = @active) AND (@familyName IS NULL OR fhir_Patient.FamilyName LIKE '%' || @familyName || '%') AND (@givenName IS NULL OR fhir_Patient.GivenName LIKE '%' || @givenName || '%') AND (@gender IS NULL OR fhir_Patient.Gender = @gender) ORDER BY fhir_Patient.FamilyName , fhir_Patient.GivenName ";

        try
        {
            var results = ImmutableList.CreateBuilder<GetPatients>();

            using (var command = new NpgsqlCommand(sql, connection))
            {
                if (active is not null and not DBNull)
                    command.Parameters.AddWithValue("@active", active);
                else
                    command.Parameters.Add(new NpgsqlParameter("@active", NpgsqlTypes.NpgsqlDbType.Bigint) { Value = DBNull.Value });
                if (familyName is not null and not DBNull)
                    command.Parameters.AddWithValue("@familyName", familyName);
                else
                    command.Parameters.Add(new NpgsqlParameter("@familyName", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });
                if (givenName is not null and not DBNull)
                    command.Parameters.AddWithValue("@givenName", givenName);
                else
                    command.Parameters.Add(new NpgsqlParameter("@givenName", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });
                if (gender is not null and not DBNull)
                    command.Parameters.AddWithValue("@gender", gender);
                else
                    command.Parameters.Add(new NpgsqlParameter("@gender", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var item = new GetPatients(
                            reader.IsDBNull(0) ? null : reader.GetFieldValue<string>(0),
                            reader.IsDBNull(1) ? default(long) : reader.GetFieldValue<long>(1),
                            reader.IsDBNull(2) ? null : reader.GetFieldValue<string>(2),
                            reader.IsDBNull(3) ? null : reader.GetFieldValue<string>(3),
                            reader.IsDBNull(4) ? null : reader.GetFieldValue<string>(4),
                            reader.IsDBNull(5) ? null : reader.GetFieldValue<string>(5),
                            reader.IsDBNull(6) ? null : reader.GetFieldValue<string>(6),
                            reader.IsDBNull(7) ? null : reader.GetFieldValue<string>(7),
                            reader.IsDBNull(8) ? null : reader.GetFieldValue<string>(8),
                            reader.IsDBNull(9) ? null : reader.GetFieldValue<string>(9),
                            reader.IsDBNull(10) ? null : reader.GetFieldValue<string>(10),
                            reader.IsDBNull(11) ? null : reader.GetFieldValue<string>(11),
                            reader.IsDBNull(12) ? null : reader.GetFieldValue<string>(12),
                            reader.IsDBNull(13) ? null : reader.GetFieldValue<string>(13),
                            reader.IsDBNull(14) ? default(long) : reader.GetFieldValue<long>(14)
                        );
                        results.Add(item);
                    }
                }
            }

            return new Result<ImmutableList<GetPatients>, SqlError>.Ok<ImmutableList<GetPatients>, SqlError>(results.ToImmutable());
        }
        catch (Exception ex)
        {
            return new Result<ImmutableList<GetPatients>, SqlError>.Error<ImmutableList<GetPatients>, SqlError>(new SqlError("Database error", ex));
        }
    }
}

/// <summary>
/// Result row for 'GetPatients' query.
/// </summary>
public record GetPatients
{
    /// <summary>Column 'Id'.</summary>
    public string Id { get; init; }

    /// <summary>Column 'Active'.</summary>
    public long Active { get; init; }

    /// <summary>Column 'GivenName'.</summary>
    public string GivenName { get; init; }

    /// <summary>Column 'FamilyName'.</summary>
    public string FamilyName { get; init; }

    /// <summary>Column 'BirthDate'.</summary>
    public string BirthDate { get; init; }

    /// <summary>Column 'Gender'.</summary>
    public string Gender { get; init; }

    /// <summary>Column 'Phone'.</summary>
    public string Phone { get; init; }

    /// <summary>Column 'Email'.</summary>
    public string Email { get; init; }

    /// <summary>Column 'AddressLine'.</summary>
    public string AddressLine { get; init; }

    /// <summary>Column 'City'.</summary>
    public string City { get; init; }

    /// <summary>Column 'State'.</summary>
    public string State { get; init; }

    /// <summary>Column 'PostalCode'.</summary>
    public string PostalCode { get; init; }

    /// <summary>Column 'Country'.</summary>
    public string Country { get; init; }

    /// <summary>Column 'LastUpdated'.</summary>
    public string LastUpdated { get; init; }

    /// <summary>Column 'VersionId'.</summary>
    public long VersionId { get; init; }

    /// <summary>Initializes a new instance of GetPatients.</summary>
    public GetPatients(
        string Id,
        long Active,
        string GivenName,
        string FamilyName,
        string BirthDate,
        string Gender,
        string Phone,
        string Email,
        string AddressLine,
        string City,
        string State,
        string PostalCode,
        string Country,
        string LastUpdated,
        long VersionId
    )
    {
        this.Id = Id;
        this.Active = Active;
        this.GivenName = GivenName;
        this.FamilyName = FamilyName;
        this.BirthDate = BirthDate;
        this.Gender = Gender;
        this.Phone = Phone;
        this.Email = Email;
        this.AddressLine = AddressLine;
        this.City = City;
        this.State = State;
        this.PostalCode = PostalCode;
        this.Country = Country;
        this.LastUpdated = LastUpdated;
        this.VersionId = VersionId;
    }
}
