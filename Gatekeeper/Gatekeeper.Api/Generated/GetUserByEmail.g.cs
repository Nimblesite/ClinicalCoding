using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Npgsql;
using Outcome;
using Selecta;

namespace Generated;

/// <summary>
/// Extension methods for 'GetUserByEmail'.
/// </summary>
public static partial class GetUserByEmailExtensions
{
    /// <summary>
    /// Executes 'GetUserByEmail.sql' and maps results.
    /// </summary>
    /// <param name="connection">Open NpgsqlConnection connection.</param>
    /// <param name="email">Query parameter.</param>
    /// <returns>Result of records or SQL error.</returns>
    public static async Task<Result<ImmutableList<GetUserByEmail>, SqlError>> GetUserByEmailAsync(this NpgsqlConnection connection, object email)
    {
        const string sql = @"-- name: GetUserByEmail
SELECT id, display_name, email, created_at, last_login_at, is_active, metadata
FROM gk_user
WHERE email = @email AND is_active = true;
";

        try
        {
            var results = ImmutableList.CreateBuilder<GetUserByEmail>();

            using (var command = new NpgsqlCommand(sql, connection))
            {
                if (email is not null and not DBNull)
                    command.Parameters.AddWithValue("@email", email);
                else
                    command.Parameters.Add(new NpgsqlParameter("@email", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var item = new GetUserByEmail(
                            reader.IsDBNull(0) ? null : reader.GetFieldValue<string>(0),
                            reader.IsDBNull(1) ? null : reader.GetFieldValue<string>(1),
                            reader.IsDBNull(2) ? null : reader.GetFieldValue<string>(2),
                            reader.IsDBNull(3) ? null : reader.GetFieldValue<string>(3),
                            reader.IsDBNull(4) ? null : reader.GetFieldValue<string>(4),
                            reader.IsDBNull(5) ? null : reader.GetFieldValue<bool?>(5),
                            reader.IsDBNull(6) ? null : reader.GetFieldValue<string>(6)
                        );
                        results.Add(item);
                    }
                }
            }

            return new Result<ImmutableList<GetUserByEmail>, SqlError>.Ok<ImmutableList<GetUserByEmail>, SqlError>(results.ToImmutable());
        }
        catch (Exception ex)
        {
            return new Result<ImmutableList<GetUserByEmail>, SqlError>.Error<ImmutableList<GetUserByEmail>, SqlError>(new SqlError("Database error", ex));
        }
    }
}

/// <summary>
/// Result row for 'GetUserByEmail' query.
/// </summary>
public record GetUserByEmail
{
    /// <summary>Column 'id'.</summary>
    public string id { get; init; }

    /// <summary>Column 'display_name'.</summary>
    public string display_name { get; init; }

    /// <summary>Column 'email'.</summary>
    public string email { get; init; }

    /// <summary>Column 'created_at'.</summary>
    public string created_at { get; init; }

    /// <summary>Column 'last_login_at'.</summary>
    public string last_login_at { get; init; }

    /// <summary>Column 'is_active'.</summary>
    public bool? is_active { get; init; }

    /// <summary>Column 'metadata'.</summary>
    public string metadata { get; init; }

    /// <summary>Initializes a new instance of GetUserByEmail.</summary>
    public GetUserByEmail(
        string id,
        string display_name,
        string email,
        string created_at,
        string last_login_at,
        bool? is_active,
        string metadata
    )
    {
        this.id = id;
        this.display_name = display_name;
        this.email = email;
        this.created_at = created_at;
        this.last_login_at = last_login_at;
        this.is_active = is_active;
        this.metadata = metadata;
    }
}
