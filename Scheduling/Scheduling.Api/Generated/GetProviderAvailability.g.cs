using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Npgsql;
using Outcome;
using Selecta;

namespace Generated;

/// <summary>
/// Extension methods for 'GetProviderAvailability'.
/// </summary>
public static partial class GetProviderAvailabilityExtensions
{
    /// <summary>
    /// Executes 'GetProviderAvailability.sql' and maps results.
    /// </summary>
    /// <param name="connection">Open NpgsqlConnection connection.</param>
    /// <param name="practitionerRef">Query parameter.</param>
    /// <returns>Result of records or SQL error.</returns>
    public static async Task<Result<ImmutableList<GetProviderAvailability>, SqlError>> GetProviderAvailabilityAsync(this NpgsqlConnection connection, object practitionerRef)
    {
        const string sql = @"SELECT fhir_Schedule.Id, fhir_Schedule.PractitionerReference, fhir_Practitioner.NameFamily, fhir_Practitioner.NameGiven, fhir_Schedule.PlanningHorizon, fhir_Schedule.Active FROM fhir_Schedule INNER JOIN fhir_Practitioner ON fhir_Schedule.PractitionerReference = fhir_Practitioner.Id WHERE fhir_Schedule.PractitionerReference = @practitionerRef AND fhir_Schedule.Active = 1";

        try
        {
            var results = ImmutableList.CreateBuilder<GetProviderAvailability>();

            using (var command = new NpgsqlCommand(sql, connection))
            {
                if (practitionerRef is not null and not DBNull)
                    command.Parameters.AddWithValue("@practitionerRef", practitionerRef);
                else
                    command.Parameters.Add(new NpgsqlParameter("@practitionerRef", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var item = new GetProviderAvailability(
                            reader.IsDBNull(0) ? null : reader.GetFieldValue<string>(0),
                            reader.IsDBNull(1) ? null : reader.GetFieldValue<string>(1),
                            reader.IsDBNull(2) ? null : reader.GetFieldValue<string>(2),
                            reader.IsDBNull(3) ? null : reader.GetFieldValue<string>(3),
                            reader.IsDBNull(4) ? default(long) : reader.GetFieldValue<long>(4),
                            reader.IsDBNull(5) ? default(long) : reader.GetFieldValue<long>(5)
                        );
                        results.Add(item);
                    }
                }
            }

            return new Result<ImmutableList<GetProviderAvailability>, SqlError>.Ok<ImmutableList<GetProviderAvailability>, SqlError>(results.ToImmutable());
        }
        catch (Exception ex)
        {
            return new Result<ImmutableList<GetProviderAvailability>, SqlError>.Error<ImmutableList<GetProviderAvailability>, SqlError>(new SqlError("Database error", ex));
        }
    }
}

/// <summary>
/// Result row for 'GetProviderAvailability' query.
/// </summary>
public record GetProviderAvailability
{
    /// <summary>Column 'Id'.</summary>
    public string Id { get; init; }

    /// <summary>Column 'PractitionerReference'.</summary>
    public string PractitionerReference { get; init; }

    /// <summary>Column 'NameFamily'.</summary>
    public string NameFamily { get; init; }

    /// <summary>Column 'NameGiven'.</summary>
    public string NameGiven { get; init; }

    /// <summary>Column 'PlanningHorizon'.</summary>
    public long PlanningHorizon { get; init; }

    /// <summary>Column 'Active'.</summary>
    public long Active { get; init; }

    /// <summary>Initializes a new instance of GetProviderAvailability.</summary>
    public GetProviderAvailability(
        string Id,
        string PractitionerReference,
        string NameFamily,
        string NameGiven,
        long PlanningHorizon,
        long Active
    )
    {
        this.Id = Id;
        this.PractitionerReference = PractitionerReference;
        this.NameFamily = NameFamily;
        this.NameGiven = NameGiven;
        this.PlanningHorizon = PlanningHorizon;
        this.Active = Active;
    }
}
