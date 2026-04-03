using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Npgsql;
using Outcome;
using Selecta;

namespace Generated;

/// <summary>
/// Extension methods for 'GetUserRoles'.
/// </summary>
public static partial class GetUserRolesExtensions
{
    /// <summary>
    /// Executes 'GetUserRoles.sql' and maps results.
    /// </summary>
    /// <param name="connection">Open NpgsqlConnection connection.</param>
    /// <param name="user_id">Query parameter.</param>
    /// <param name="now">Query parameter.</param>
    /// <returns>Result of records or SQL error.</returns>
    public static async Task<Result<ImmutableList<GetUserRoles>, SqlError>> GetUserRolesAsync(this NpgsqlConnection connection, object user_id, object now)
    {
        const string sql = @"-- name: GetUserRoles
SELECT r.id, r.name, r.description, r.is_system, ur.granted_at, ur.expires_at
FROM gk_user_role ur
JOIN gk_role r ON ur.role_id = r.id
WHERE ur.user_id = @user_id
  AND (ur.expires_at IS NULL OR ur.expires_at > @now);
";

        try
        {
            var results = ImmutableList.CreateBuilder<GetUserRoles>();

            using (var command = new NpgsqlCommand(sql, connection))
            {
                if (user_id is not null and not DBNull)
                    command.Parameters.AddWithValue("@user_id", user_id);
                else
                    command.Parameters.Add(new NpgsqlParameter("@user_id", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });
                if (now is not null and not DBNull)
                    command.Parameters.AddWithValue("@now", now);
                else
                    command.Parameters.Add(new NpgsqlParameter("@now", NpgsqlTypes.NpgsqlDbType.Text) { Value = DBNull.Value });

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var item = new GetUserRoles(
                            reader.IsDBNull(0) ? null : reader.GetFieldValue<string>(0),
                            reader.IsDBNull(1) ? null : reader.GetFieldValue<string>(1),
                            reader.IsDBNull(2) ? null : reader.GetFieldValue<string>(2),
                            reader.IsDBNull(3) ? null : reader.GetFieldValue<bool?>(3),
                            reader.IsDBNull(4) ? null : reader.GetFieldValue<string>(4),
                            reader.IsDBNull(5) ? null : reader.GetFieldValue<string>(5)
                        );
                        results.Add(item);
                    }
                }
            }

            return new Result<ImmutableList<GetUserRoles>, SqlError>.Ok<ImmutableList<GetUserRoles>, SqlError>(results.ToImmutable());
        }
        catch (Exception ex)
        {
            return new Result<ImmutableList<GetUserRoles>, SqlError>.Error<ImmutableList<GetUserRoles>, SqlError>(new SqlError("Database error", ex));
        }
    }
}

/// <summary>
/// Result row for 'GetUserRoles' query.
/// </summary>
public record GetUserRoles
{
    /// <summary>Column 'id'.</summary>
    public string id { get; init; }

    /// <summary>Column 'name'.</summary>
    public string name { get; init; }

    /// <summary>Column 'description'.</summary>
    public string description { get; init; }

    /// <summary>Column 'is_system'.</summary>
    public bool? is_system { get; init; }

    /// <summary>Column 'granted_at'.</summary>
    public string granted_at { get; init; }

    /// <summary>Column 'expires_at'.</summary>
    public string expires_at { get; init; }

    /// <summary>Initializes a new instance of GetUserRoles.</summary>
    public GetUserRoles(
        string id,
        string name,
        string description,
        bool? is_system,
        string granted_at,
        string expires_at
    )
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.is_system = is_system;
        this.granted_at = granted_at;
        this.expires_at = expires_at;
    }
}
