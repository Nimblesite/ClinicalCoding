using Nimblesite.DataProvider.Migration.Core;
using Nimblesite.DataProvider.Migration.Postgres;
using InitError = Outcome.Result<bool, string>.Error<bool, string>;
using InitOk = Outcome.Result<bool, string>.Ok<bool, string>;
using InitResult = Outcome.Result<bool, string>;

namespace Gatekeeper.Api;

/// <summary>
/// Database initialization and seeding.
/// Accepts IDbConnection; casts to NpgsqlConnection at the migration boundary only.
/// </summary>
internal static class DatabaseSetup
{
    /// <summary>Initializes the database schema and seeds default data.</summary>
    public static InitResult Initialize(IDbConnection conn, ILogger logger)
    {
        var npgsql = conn as NpgsqlConnection
            ?? throw new InvalidOperationException("DatabaseSetup requires NpgsqlConnection.");

        var schemaResult = CreateSchema(npgsql, logger);
        if (schemaResult is InitError)
            return schemaResult;

        return SeedDefaultData(npgsql, logger);
    }

    private static InitResult CreateSchema(NpgsqlConnection conn, ILogger logger)
    {
        logger.LogInformation("Applying gatekeeper-schema.yaml");
        try
        {
            var yamlPath = Path.Combine(AppContext.BaseDirectory, "gatekeeper-schema.yaml");
            var schema = SchemaYamlSerializer.FromYamlFile(yamlPath);
            PostgresDdlGenerator.MigrateSchema(conn, schema);
            logger.LogInformation("Gatekeeper schema applied");
            return new InitOk(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Schema migration failed");
            return new InitError($"Schema migration failed: {ex.Message}");
        }
    }

    private static InitResult SeedDefaultData(NpgsqlConnection conn, ILogger logger)
    {
        try
        {
            var now = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);

            using var checkCmd = conn.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM gk_role WHERE is_system = true";
            if (Convert.ToInt64(checkCmd.ExecuteScalar(), CultureInfo.InvariantCulture) > 0)
            {
                logger.LogInformation("Database already seeded");
                return new InitOk(true);
            }

            logger.LogInformation("Seeding default roles and permissions");

            Exec(conn, """
                INSERT INTO gk_role (id, name, description, is_system, created_at)
                VALUES ('role-admin', 'admin', 'Full system access', true, @now),
                       ('role-user',  'user',  'Basic authenticated user', true, @now)
                """, ("@now", now));

            Exec(conn, """
                INSERT INTO gk_permission (id, code, resource_type, action, description, created_at)
                VALUES ('perm-admin-all',        'admin:*',           'admin',  '*',      'Full admin access',       @now),
                       ('perm-user-profile',     'user:profile',      'user',   'read',   'View own profile',        @now),
                       ('perm-user-credentials', 'user:credentials',  'user',   'manage', 'Manage own passkeys',     @now),
                       ('perm-sync-read',        'sync:read',         'sync',   'read',   'Read sync data',          @now),
                       ('perm-sync-write',       'sync:write',        'sync',   'write',  'Write sync data',         @now),
                       ('perm-record-read',      'record:read',       'record', 'read',   'Read any record',         @now),
                       ('perm-order-read',       'order:read',        'order',  'read',   'Read order records',      @now),
                       ('perm-patient-read',     'patient:read',      'patient','read',   'Read patient records',    @now)
                """, ("@now", now));

            Exec(conn, """
                INSERT INTO gk_role_permission (role_id, permission_id, granted_at)
                VALUES ('role-admin', 'perm-admin-all',        @now),
                       ('role-admin', 'perm-sync-read',        @now),
                       ('role-admin', 'perm-sync-write',       @now),
                       ('role-user',  'perm-user-profile',     @now),
                       ('role-user',  'perm-user-credentials', @now)
                """, ("@now", now));

            logger.LogInformation("Seed complete");
            return new InitOk(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Seed failed");
            return new InitError($"Seed failed: {ex.Message}");
        }
    }

    private static void Exec(NpgsqlConnection conn, string sql, params (string Name, object Value)[] parameters)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        foreach (var (name, value) in parameters)
            cmd.Parameters.AddWithValue(name, value);
        cmd.ExecuteNonQuery();
    }
}
