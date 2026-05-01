// Hand-written replacements for junction-table insert extensions.
//
// The DataProvider Postgres code generator unconditionally appends
// "RETURNING id" to every generated INSERT, but gk_user_role and
// gk_role_permission use composite primary keys and have no id column.
// We disable generateInsert for these tables in DataProvider.json and
// provide drop-in replacements using IDbConnection/IDbCommand/IDbTransaction
// so no Npgsql types leak above the generated layer.

#nullable enable

namespace Generated;

/// <summary>
/// Hand-written extension methods for inserting into <c>gk_user_role</c>.
/// </summary>
public static class gk_user_roleExtensions
{
    private const string Sql =
        @"INSERT INTO public.gk_user_role (user_id, role_id, granted_at, granted_by, expires_at)
          VALUES (@user_id, @role_id, @granted_at, @granted_by, @expires_at)
          ON CONFLICT DO NOTHING";

    /// <summary>Inserts a row into <c>gk_user_role</c> using a plain connection.</summary>
    public static async Task<Result<Guid?, SqlError>> Insertgk_user_roleAsync(
        this IDbConnection conn,
        string user_id,
        string role_id,
        string? granted_at,
        string? granted_by,
        string? expires_at
    )
    {
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = Sql;
            BindParameters(cmd, user_id, role_id, granted_at, granted_by, expires_at);
            _ = await ((System.Data.Common.DbCommand)cmd).ExecuteNonQueryAsync().ConfigureAwait(false);
            return new Result<Guid?, SqlError>.Ok<Guid?, SqlError>(null);
        }
        catch (Exception ex)
        {
            return new Result<Guid?, SqlError>.Error<Guid?, SqlError>(SqlError.FromException(ex));
        }
    }

    /// <summary>Transaction overload.</summary>
    public static async Task<Result<Guid?, SqlError>> Insertgk_user_roleAsync(
        this IDbTransaction transaction,
        string user_id,
        string role_id,
        string? granted_at,
        string? granted_by,
        string? expires_at
    )
    {
        if (transaction.Connection is not IDbConnection conn)
            return new Result<Guid?, SqlError>.Error<Guid?, SqlError>(
                new SqlError("Transaction has no connection")
            );

        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = Sql;
            cmd.Transaction = transaction;
            BindParameters(cmd, user_id, role_id, granted_at, granted_by, expires_at);
            _ = await ((System.Data.Common.DbCommand)cmd).ExecuteNonQueryAsync().ConfigureAwait(false);
            return new Result<Guid?, SqlError>.Ok<Guid?, SqlError>(null);
        }
        catch (Exception ex)
        {
            return new Result<Guid?, SqlError>.Error<Guid?, SqlError>(SqlError.FromException(ex));
        }
    }

    private static void BindParameters(
        IDbCommand cmd,
        string user_id,
        string role_id,
        string? granted_at,
        string? granted_by,
        string? expires_at
    )
    {
        AddParam(cmd, "user_id", user_id);
        AddParam(cmd, "role_id", role_id);
        AddParam(cmd, "granted_at", (object?)granted_at ?? DBNull.Value);
        AddParam(cmd, "granted_by", (object?)granted_by ?? DBNull.Value);
        AddParam(cmd, "expires_at", (object?)expires_at ?? DBNull.Value);
    }

    private static void AddParam(IDbCommand cmd, string name, object value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        cmd.Parameters.Add(p);
    }
}

/// <summary>
/// Hand-written extension methods for inserting into <c>gk_role_permission</c>.
/// </summary>
public static class gk_role_permissionExtensions
{
    private const string Sql =
        @"INSERT INTO public.gk_role_permission (role_id, permission_id, granted_at)
          VALUES (@role_id, @permission_id, @granted_at)
          ON CONFLICT DO NOTHING";

    /// <summary>Inserts a row into <c>gk_role_permission</c> using a plain connection.</summary>
    public static async Task<Result<Guid?, SqlError>> Insertgk_role_permissionAsync(
        this IDbConnection conn,
        string role_id,
        string permission_id,
        string? granted_at
    )
    {
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = Sql;
            BindParameters(cmd, role_id, permission_id, granted_at);
            _ = await ((System.Data.Common.DbCommand)cmd).ExecuteNonQueryAsync().ConfigureAwait(false);
            return new Result<Guid?, SqlError>.Ok<Guid?, SqlError>(null);
        }
        catch (Exception ex)
        {
            return new Result<Guid?, SqlError>.Error<Guid?, SqlError>(SqlError.FromException(ex));
        }
    }

    /// <summary>Transaction overload.</summary>
    public static async Task<Result<Guid?, SqlError>> Insertgk_role_permissionAsync(
        this IDbTransaction transaction,
        string role_id,
        string permission_id,
        string? granted_at
    )
    {
        if (transaction.Connection is not IDbConnection conn)
            return new Result<Guid?, SqlError>.Error<Guid?, SqlError>(
                new SqlError("Transaction has no connection")
            );

        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = Sql;
            cmd.Transaction = transaction;
            BindParameters(cmd, role_id, permission_id, granted_at);
            _ = await ((System.Data.Common.DbCommand)cmd).ExecuteNonQueryAsync().ConfigureAwait(false);
            return new Result<Guid?, SqlError>.Ok<Guid?, SqlError>(null);
        }
        catch (Exception ex)
        {
            return new Result<Guid?, SqlError>.Error<Guid?, SqlError>(SqlError.FromException(ex));
        }
    }

    private static void BindParameters(IDbCommand cmd, string role_id, string permission_id, string? granted_at)
    {
        AddParam(cmd, "role_id", role_id);
        AddParam(cmd, "permission_id", permission_id);
        AddParam(cmd, "granted_at", (object?)granted_at ?? DBNull.Value);
    }

    private static void AddParam(IDbCommand cmd, string name, object value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        cmd.Parameters.Add(p);
    }
}
