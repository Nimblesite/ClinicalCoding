// Hand-written replacements for junction-table insert extensions.
//
// The DataProvider Postgres code generator unconditionally appends
// "RETURNING id" to every generated INSERT, but `gk_user_role` and
// `gk_role_permission` use composite primary keys and do not have an
// `id` column. The generated code therefore throws at runtime
// ("column 'id' does not exist"), the error is silently swallowed
// into a Result.Error by the generated try/catch, and dependent
// queries (e.g. /authz/permissions) return empty data.
//
// We disable generateInsert for these tables in DataProvider.json and
// provide drop-in replacements here so existing call sites continue
// to compile against the same `Insertgk_user_roleAsync` /
// `Insertgk_role_permissionAsync` extension method names.

#nullable enable

using System.Data;

namespace Generated;

/// <summary>
/// Hand-written extension methods for inserting into <c>gk_user_role</c>.
/// Replaces the broken DataProvider-generated version.
/// </summary>
public static class gk_user_roleExtensions
{
    private const string Sql =
        @"INSERT INTO public.gk_user_role (user_id, role_id, granted_at, granted_by, expires_at)
          VALUES (@user_id, @role_id, @granted_at, @granted_by, @expires_at)
          ON CONFLICT DO NOTHING";

    /// <summary>Inserts a row into <c>gk_user_role</c>.</summary>
    public static async Task<Result<Guid?, SqlError>> Insertgk_user_roleAsync(
        this NpgsqlConnection conn,
        string user_id,
        string role_id,
        string? granted_at,
        string? granted_by,
        string? expires_at
    )
    {
        try
        {
            await using var cmd = new NpgsqlCommand(Sql, conn);
            BindParameters(cmd, user_id, role_id, granted_at, granted_by, expires_at);
            _ = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            return new Result<Guid?, SqlError>.Ok<Guid?, SqlError>(null);
        }
        catch (Exception ex)
        {
            return new Result<Guid?, SqlError>.Error<Guid?, SqlError>(SqlError.FromException(ex));
        }
    }

    /// <summary>Transaction overload of <see cref="Insertgk_user_roleAsync(NpgsqlConnection, string, string, string?, string?, string?)"/>.</summary>
    public static async Task<Result<Guid?, SqlError>> Insertgk_user_roleAsync(
        this IDbTransaction transaction,
        string user_id,
        string role_id,
        string? granted_at,
        string? granted_by,
        string? expires_at
    )
    {
        if (transaction.Connection is not NpgsqlConnection conn)
        {
            return new Result<Guid?, SqlError>.Error<Guid?, SqlError>(
                new SqlError("Transaction.Connection must be NpgsqlConnection")
            );
        }

        try
        {
            await using var cmd = new NpgsqlCommand(Sql, conn, (NpgsqlTransaction)transaction);
            BindParameters(cmd, user_id, role_id, granted_at, granted_by, expires_at);
            _ = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            return new Result<Guid?, SqlError>.Ok<Guid?, SqlError>(null);
        }
        catch (Exception ex)
        {
            return new Result<Guid?, SqlError>.Error<Guid?, SqlError>(SqlError.FromException(ex));
        }
    }

    private static void BindParameters(
        NpgsqlCommand cmd,
        string user_id,
        string role_id,
        string? granted_at,
        string? granted_by,
        string? expires_at
    )
    {
        cmd.Parameters.AddWithValue("user_id", user_id);
        cmd.Parameters.AddWithValue("role_id", role_id);
        cmd.Parameters.AddWithValue("granted_at", (object?)granted_at ?? DBNull.Value);
        cmd.Parameters.AddWithValue("granted_by", (object?)granted_by ?? DBNull.Value);
        cmd.Parameters.AddWithValue("expires_at", (object?)expires_at ?? DBNull.Value);
    }
}

/// <summary>
/// Hand-written extension methods for inserting into <c>gk_role_permission</c>.
/// Replaces the broken DataProvider-generated version.
/// </summary>
public static class gk_role_permissionExtensions
{
    private const string Sql =
        @"INSERT INTO public.gk_role_permission (role_id, permission_id, granted_at)
          VALUES (@role_id, @permission_id, @granted_at)
          ON CONFLICT DO NOTHING";

    /// <summary>Inserts a row into <c>gk_role_permission</c>.</summary>
    public static async Task<Result<Guid?, SqlError>> Insertgk_role_permissionAsync(
        this NpgsqlConnection conn,
        string role_id,
        string permission_id,
        string? granted_at
    )
    {
        try
        {
            await using var cmd = new NpgsqlCommand(Sql, conn);
            BindParameters(cmd, role_id, permission_id, granted_at);
            _ = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            return new Result<Guid?, SqlError>.Ok<Guid?, SqlError>(null);
        }
        catch (Exception ex)
        {
            return new Result<Guid?, SqlError>.Error<Guid?, SqlError>(SqlError.FromException(ex));
        }
    }

    /// <summary>Transaction overload of <see cref="Insertgk_role_permissionAsync(NpgsqlConnection, string, string, string?)"/>.</summary>
    public static async Task<Result<Guid?, SqlError>> Insertgk_role_permissionAsync(
        this IDbTransaction transaction,
        string role_id,
        string permission_id,
        string? granted_at
    )
    {
        if (transaction.Connection is not NpgsqlConnection conn)
        {
            return new Result<Guid?, SqlError>.Error<Guid?, SqlError>(
                new SqlError("Transaction.Connection must be NpgsqlConnection")
            );
        }

        try
        {
            await using var cmd = new NpgsqlCommand(Sql, conn, (NpgsqlTransaction)transaction);
            BindParameters(cmd, role_id, permission_id, granted_at);
            _ = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            return new Result<Guid?, SqlError>.Ok<Guid?, SqlError>(null);
        }
        catch (Exception ex)
        {
            return new Result<Guid?, SqlError>.Error<Guid?, SqlError>(SqlError.FromException(ex));
        }
    }

    private static void BindParameters(
        NpgsqlCommand cmd,
        string role_id,
        string permission_id,
        string? granted_at
    )
    {
        cmd.Parameters.AddWithValue("role_id", role_id);
        cmd.Parameters.AddWithValue("permission_id", permission_id);
        cmd.Parameters.AddWithValue("granted_at", (object?)granted_at ?? DBNull.Value);
    }
}
