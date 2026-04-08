// PostgreSQL DDL emitter that produces lowercase identifiers, matching the
// case-folding behaviour of unquoted identifiers in dataprovider-postgres
// generated INSERT/SELECT SQL.
//
// This intentionally does NOT depend on
// Nimblesite.DataProvider.Migration.Postgres.PostgresDdlGenerator: that
// type's public surface has shifted across recent beta releases and was
// causing compile failures in some build environments. Owning the DDL
// here keeps DatabaseSetup.cs files in every API project free of that
// version risk while preserving the lowercase identifier convention the
// generated SQL relies on.

using System.Text;
using Nimblesite.DataProvider.Migration.Core;

namespace Samples.Authorization;

/// <summary>
/// Generates <c>CREATE TABLE</c> / <c>CREATE INDEX</c> statements with
/// lowercase identifiers from a <see cref="TableDefinition"/>.
/// </summary>
public static class LowercaseDdl
{
    /// <summary>Returns the DDL statements needed to create the table and its indexes.</summary>
    public static IEnumerable<string> GenerateStatements(TableDefinition table)
    {
        yield return BuildCreateTable(table);
        if (table.Indexes is { } indexes)
        {
            foreach (var idx in indexes)
            {
                yield return BuildCreateIndex(table, idx);
            }
        }
    }

    private static string BuildCreateTable(TableDefinition table)
    {
        var sb = new StringBuilder();
        sb.Append("CREATE TABLE IF NOT EXISTS ");
        sb.Append(QualifiedName(table));
        sb.Append(" (");
        var clauses = new List<string>();
        foreach (var col in table.Columns)
        {
            clauses.Add(BuildColumnClause(col));
        }
        if (table.PrimaryKey is { } pk)
        {
            clauses.Add(BuildPrimaryKey(pk));
        }
        if (table.UniqueConstraints is { Count: > 0 } uniques)
        {
            clauses.AddRange(uniques.Select(BuildUnique));
        }
        if (table.ForeignKeys is { Count: > 0 } fks)
        {
            clauses.AddRange(fks.Select(BuildForeignKey));
        }
        sb.Append(string.Join(", ", clauses));
        sb.Append(')');
        return sb.ToString();
    }

    private static string BuildColumnClause(ColumnDefinition col)
    {
        var sb = new StringBuilder();
        sb.Append(Quote(col.Name));
        sb.Append(' ');
        sb.Append(MapType(col.Type));
        if (col.IsNullable == false)
        {
            sb.Append(" NOT NULL");
        }
        if (!string.IsNullOrEmpty(col.DefaultValue))
        {
            sb.Append(" DEFAULT ");
            sb.Append(col.DefaultValue);
        }
        if (!string.IsNullOrEmpty(col.CheckConstraint))
        {
            sb.Append(" CHECK (");
            sb.Append(col.CheckConstraint);
            sb.Append(')');
        }
        return sb.ToString();
    }

    private static string BuildPrimaryKey(PrimaryKeyDefinition pk)
    {
        var name = string.IsNullOrEmpty(pk.Name) ? "pk" : pk.Name;
        var cols = string.Join(", ", pk.Columns.Select(Quote));
        return $"CONSTRAINT {Quote(name)} PRIMARY KEY ({cols})";
    }

    private static string BuildUnique(UniqueConstraintDefinition uq)
    {
        var name = string.IsNullOrEmpty(uq.Name) ? "uq" : uq.Name;
        var cols = string.Join(", ", uq.Columns.Select(Quote));
        return $"CONSTRAINT {Quote(name)} UNIQUE ({cols})";
    }

    private static string BuildForeignKey(ForeignKeyDefinition fk)
    {
        var name = string.IsNullOrEmpty(fk.Name) ? "fk" : fk.Name;
        var cols = string.Join(", ", fk.Columns.Select(Quote));
        var refSchema = string.IsNullOrEmpty(fk.ReferencedSchema) ? "public" : fk.ReferencedSchema;
        var refTable = $"{Quote(refSchema)}.{Quote(fk.ReferencedTable)}";
        var refCols = string.Join(", ", fk.ReferencedColumns.Select(Quote));
        var sb = new StringBuilder();
        sb.Append(
            $"CONSTRAINT {Quote(name)} FOREIGN KEY ({cols}) REFERENCES {refTable} ({refCols})"
        );
        if (fk.OnDelete != ForeignKeyAction.NoAction)
        {
            sb.Append(" ON DELETE ");
            sb.Append(MapAction(fk.OnDelete));
        }
        return sb.ToString();
    }

    private static string BuildCreateIndex(TableDefinition table, IndexDefinition idx)
    {
        var unique = idx.IsUnique ? "UNIQUE " : string.Empty;
        var cols = string.Join(", ", idx.Columns.Select(Quote));
        return $"CREATE {unique}INDEX IF NOT EXISTS {Quote(idx.Name)} ON {QualifiedName(table)} ({cols})";
    }

    private static string QualifiedName(TableDefinition table)
    {
        var schema = string.IsNullOrEmpty(table.Schema) ? "public" : table.Schema;
        return $"{Quote(schema)}.{Quote(table.Name)}";
    }

    private static string Quote(string identifier) =>
        "\"" + identifier.ToLowerInvariant().Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";

    private static string MapType(PortableType type) =>
        type switch
        {
            TextType => "TEXT",
            IntType => "INTEGER",
            BigIntType => "BIGINT",
            SmallIntType => "SMALLINT",
            TinyIntType => "SMALLINT",
            DoubleType => "DOUBLE PRECISION",
            FloatType => "REAL",
            BooleanType => "BOOLEAN",
            DateTimeType => "TIMESTAMP",
            DateType => "DATE",
            DateTimeOffsetType => "TIMESTAMPTZ",
            UuidType => "UUID",
            BlobType => "BYTEA",
            VarBinaryType => "BYTEA",
            JsonType => "JSONB",
            _ => throw new NotSupportedException(
                $"Column type {type.GetType().Name} is not supported"
            ),
        };

    private static string MapAction(ForeignKeyAction action) =>
        action switch
        {
            ForeignKeyAction.Cascade => "CASCADE",
            ForeignKeyAction.SetNull => "SET NULL",
            ForeignKeyAction.SetDefault => "SET DEFAULT",
            ForeignKeyAction.Restrict => "RESTRICT",
            _ => "NO ACTION",
        };
}
