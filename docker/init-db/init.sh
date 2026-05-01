#!/bin/bash
set -e

# Create databases and users using environment variable for password
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    CREATE DATABASE gatekeeper;
    CREATE DATABASE clinical;
    CREATE DATABASE scheduling;
    CREATE DATABASE icd10;

    CREATE USER gatekeeper WITH PASSWORD '$POSTGRES_PASSWORD';
    CREATE USER clinical WITH PASSWORD '$POSTGRES_PASSWORD';
    CREATE USER scheduling WITH PASSWORD '$POSTGRES_PASSWORD';
    CREATE USER icd10 WITH PASSWORD '$POSTGRES_PASSWORD';

    GRANT ALL PRIVILEGES ON DATABASE gatekeeper TO gatekeeper;
    GRANT ALL PRIVILEGES ON DATABASE clinical TO clinical;
    GRANT ALL PRIVILEGES ON DATABASE scheduling TO scheduling;
    GRANT ALL PRIVILEGES ON DATABASE icd10 TO icd10;
EOSQL

# Grant schema privileges and default privileges so tables created by superuser are accessible
for db in gatekeeper clinical scheduling icd10; do
    psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$db" <<-EOSQL
        GRANT ALL ON SCHEMA public TO $db;
        -- Tables created by the postgres superuser (e.g. via db-migrate) are reassigned to $db
        ALTER DEFAULT PRIVILEGES FOR ROLE postgres IN SCHEMA public GRANT ALL ON TABLES TO $db;
        ALTER DEFAULT PRIVILEGES FOR ROLE postgres IN SCHEMA public GRANT ALL ON SEQUENCES TO $db;
EOSQL
done

# Enable pgvector extension for ICD10 database (vector similarity search)
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "icd10" <<-EOSQL
    CREATE EXTENSION IF NOT EXISTS vector;
EOSQL
