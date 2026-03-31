using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Npgsql;
using Outcome;
using Selecta;

namespace Generated;

/// <summary>
/// Extension methods for 'GetPractitionerById'.
/// </summary>
public static partial class GetPractitionerByIdExtensions
{
    /// <summary>
    /// Executes 'GetPractitionerById.sql' and maps results.
    /// </summary>
    /// <param name="connection">Open NpgsqlConnection connection.</param>
    /// <param name="id">Query parameter.</param>
    /// <returns>Result of records or SQL error.</returns>
    public static async Task<Result<ImmutableList<GetPractitionerById>, SqlError>> GetPractitionerByIdAsync(this NpgsqlConnection connection, object id)
    {
        const string sql = @"SELECT fhir_Practitioner.Id, fhir_Practitioner.Identifier, fhir_Practitioner.Active, fhir_Practitioner.NameFamily, fhir_Practitioner.NameGiven, fhir_Practitioner.Qualification, fhir_Practitioner.Specialty, fhir_Practitioner.TelecomEmail, fhir_Practitioner.TelecomPhone FROM fhir_Practitioner WHERE fhir_Practitioner.Id = @id";

        try
        {
            var results = ImmutableList.CreateBuilder<GetPractitionerById>();

            using (var command = new NpgsqlCommand(sql, connection))
            {
                if (id is not null and not DBNull)
                    command.Parameters.AddWithValue("@id", id);
                else
                    command.Parameters.Add(new NpgsqlParameter("@id", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var item = new GetPractitionerById(
                            reader.IsDBNull(0) ? null : reader.GetFieldValue<string>(0),
                            reader.IsDBNull(1) ? null : reader.GetFieldValue<string>(1),
                            reader.IsDBNull(2) ? default(long) : reader.GetFieldValue<long>(2),
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

            return new Result<ImmutableList<GetPractitionerById>, SqlError>.Ok<ImmutableList<GetPractitionerById>, SqlError>(results.ToImmutable());
        }
        catch (Exception ex)
        {
            return new Result<ImmutableList<GetPractitionerById>, SqlError>.Error<ImmutableList<GetPractitionerById>, SqlError>(new SqlError("Database error", ex));
        }
    }
}

/// <summary>
/// Result row for 'GetPractitionerById' query.
/// </summary>
public record GetPractitionerById
{
    /// <summary>Column 'Id'.</summary>
    public string Id { get; init; }

    /// <summary>Column 'Identifier'.</summary>
    public string Identifier { get; init; }

    /// <summary>Column 'Active'.</summary>
    public long Active { get; init; }

    /// <summary>Column 'NameFamily'.</summary>
    public string NameFamily { get; init; }

    /// <summary>Column 'NameGiven'.</summary>
    public string NameGiven { get; init; }

    /// <summary>Column 'Qualification'.</summary>
    public string Qualification { get; init; }

    /// <summary>Column 'Specialty'.</summary>
    public string Specialty { get; init; }

    /// <summary>Column 'TelecomEmail'.</summary>
    public string TelecomEmail { get; init; }

    /// <summary>Column 'TelecomPhone'.</summary>
    public string TelecomPhone { get; init; }

    /// <summary>Initializes a new instance of GetPractitionerById.</summary>
    public GetPractitionerById(
        string Id,
        string Identifier,
        long Active,
        string NameFamily,
        string NameGiven,
        string Qualification,
        string Specialty,
        string TelecomEmail,
        string TelecomPhone
    )
    {
        this.Id = Id;
        this.Identifier = Identifier;
        this.Active = Active;
        this.NameFamily = NameFamily;
        this.NameGiven = NameGiven;
        this.Qualification = Qualification;
        this.Specialty = Specialty;
        this.TelecomEmail = TelecomEmail;
        this.TelecomPhone = TelecomPhone;
    }
}
