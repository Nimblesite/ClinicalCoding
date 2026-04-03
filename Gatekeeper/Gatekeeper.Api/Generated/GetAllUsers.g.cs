using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Npgsql;
using Outcome;
using Selecta;

namespace Generated;

/// <summary>
/// Extension methods for 'GetAllUsers'.
/// </summary>
public static partial class GetAllUsersExtensions
{
    /// <summary>
    /// Executes 'GetAllUsers.sql' and maps results.
    /// </summary>
    /// <param name="connection">Open NpgsqlConnection connection.</param>
    /// <returns>Result of records or SQL error.</returns>
    public static async Task<Result<ImmutableList<GetAllUsers>, SqlError>> GetAllUsersAsync(this NpgsqlConnection connection)
    {
        const string sql = @"-- name: GetAllUsers
SELECT id, display_name, email, created_at, last_login_at, is_active
FROM gk_user
ORDER BY display_name;
";

        try
        {
            var results = ImmutableList.CreateBuilder<GetAllUsers>();

            using (var command = new NpgsqlCommand(sql, connection))
            {

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var item = new GetAllUsers(
                            reader.IsDBNull(0) ? null : reader.GetFieldValue<string>(0),
                            reader.IsDBNull(1) ? null : reader.GetFieldValue<string>(1),
                            reader.IsDBNull(2) ? null : reader.GetFieldValue<string>(2),
                            reader.IsDBNull(3) ? null : reader.GetFieldValue<string>(3),
                            reader.IsDBNull(4) ? null : reader.GetFieldValue<string>(4),
                            reader.IsDBNull(5) ? null : reader.GetFieldValue<bool?>(5)
                        );
                        results.Add(item);
                    }
                }
            }

            return new Result<ImmutableList<GetAllUsers>, SqlError>.Ok<ImmutableList<GetAllUsers>, SqlError>(results.ToImmutable());
        }
        catch (Exception ex)
        {
            return new Result<ImmutableList<GetAllUsers>, SqlError>.Error<ImmutableList<GetAllUsers>, SqlError>(new SqlError("Database error", ex));
        }
    }
}

/// <summary>
/// Result row for 'GetAllUsers' query.
/// </summary>
public record GetAllUsers
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

    /// <summary>Initializes a new instance of GetAllUsers.</summary>
    public GetAllUsers(
        string id,
        string display_name,
        string email,
        string created_at,
        string last_login_at,
        bool? is_active
    )
    {
        this.id = id;
        this.display_name = display_name;
        this.email = email;
        this.created_at = created_at;
        this.last_login_at = last_login_at;
        this.is_active = is_active;
    }
}
