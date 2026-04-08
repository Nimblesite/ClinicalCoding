using Nimblesite.DataProvider.Migration.Core;
using Npgsql;
using Samples.Authorization;

namespace ICD10.TestSupport;

/// <summary>
/// Helpers to provision an ICD-10 test database (schema + seed data) without
/// running the heavyweight Python CDC import (~3 minutes for 44k embeddings).
/// </summary>
public static class Icd10TestDatabase
{
    /// <summary>
    /// Enables pgvector, applies the icd10-schema.yaml schema via the Migration
    /// library, then seeds reference data and (if the embedding service at
    /// http://localhost:8000 is available) embeddings.
    /// </summary>
    /// <param name="connectionString">Connection string to a fresh database.</param>
    /// <param name="schemaYamlPath">Absolute path to icd10-schema.yaml.</param>
    public static void Initialize(string connectionString, string schemaYamlPath)
    {
        if (!File.Exists(schemaYamlPath))
        {
            throw new FileNotFoundException(
                $"icd10-schema.yaml not found at '{schemaYamlPath}'",
                schemaYamlPath
            );
        }

        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "CREATE EXTENSION IF NOT EXISTS vector";
            cmd.ExecuteNonQuery();
        }

        var schema = SchemaYamlSerializer.FromYamlFile(schemaYamlPath);
        foreach (var table in schema.Tables)
        {
            foreach (var statement in LowercaseDdl.GenerateStatements(table))
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = statement;
                cmd.ExecuteNonQuery();
            }
        }

        TestDataSeeder.Seed(conn);
        TestDataSeeder.SeedEmbeddings(conn);
    }
}
