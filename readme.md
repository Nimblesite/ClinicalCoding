# HealthcareSamples

FHIR R5-compliant healthcare microservices built with the [DataProvider](https://github.com/MelbourneDeveloper/DataProvider) .NET toolkit.

Four APIs (Clinical, Scheduling, ICD-10, Gatekeeper), bidirectional sync workers, semantic search via pgvector embeddings, and a React dashboard.

## Quick Start

Prerequisites: [Docker](https://docs.docker.com/get-docker/), [.NET 10 SDK](https://dotnet.microsoft.com/download), [GNU Make](https://www.gnu.org/software/make/)

```bash
make start-docker
```

That's it. Builds the dashboard, starts Postgres, migrates schemas, boots all APIs, serves the dashboard. Open http://localhost:5173.

Force-rebuild containers:

```bash
make start-docker BUILD=1
```

Run APIs locally (faster rebuild cycle, Postgres still in Docker):

```bash
make start-local
```

Ctrl+C stops everything.

## Services

| Service | Port | Description |
|---------|------|-------------|
| Dashboard | http://localhost:5173 | React UI (H5 transpiler C# to JS) |
| Clinical API | http://localhost:5080 | Patient, Encounter, Condition, MedicationRequest |
| Scheduling API | http://localhost:5001 | Practitioner, Appointment, Schedule, Slot |
| ICD-10 API | http://localhost:5090 | ICD-10/ACHI codes, semantic search via pgvector |
| Gatekeeper API | http://localhost:5002 | Passkey authentication, RBAC authorization |
| Postgres | localhost:5432 | pgvector-enabled, 4 databases |

## Development

```bash
make ci             # full CI: lint + test + coverage-check + build
make test           # run all tests with coverage
make lint           # run all linters
make fmt            # format all code
make build          # compile everything (Release)
make clean          # remove build artifacts
make setup          # restore tools + packages (run once after clone)
```

### Database

```bash
make db-up          # start Postgres container
make db-down        # stop Postgres container
make db-migrate     # apply schemas to all databases
make db-reset       # wipe and recreate databases from scratch
```

## Architecture

```
Dashboard.Web (React/H5)
       |
       +--> Gatekeeper.Api     (Passkey auth, RBAC)
       |
       +--> Clinical.Api <---- Clinical.Sync <-+
       |    (PostgreSQL)                       |
       |    fhir_Patient, fhir_Encounter       | Practitioner -> Provider
       |                                       |
       +--> Scheduling.Api <-- Scheduling.Sync <+
       |    (PostgreSQL)       Patient -> ScheduledPatient
       |    fhir_Practitioner, fhir_Appointment
       |
       +--> ICD10.Api
            (PostgreSQL + pgvector)
            icd10_code, achi_code, embeddings
```

Clinical and Scheduling sync data bidirectionally. ICD-10 is a read-only reference database with semantic search powered by pgvector embeddings.

## Data Ownership

| Domain | Owns | Receives via Sync |
|--------|------|-------------------|
| Clinical | fhir_Patient, fhir_Encounter, fhir_Condition, fhir_MedicationRequest | sync_Provider |
| Scheduling | fhir_Practitioner, fhir_Appointment, fhir_Schedule, fhir_Slot | sync_ScheduledPatient |
| ICD10 | icd10_chapter, icd10_block, icd10_category, icd10_code, achi_block, achi_code | N/A (read-only) |

## API Endpoints

### Clinical (`:5080`)
- `GET/POST /fhir/Patient` - Patients
- `GET /fhir/Patient/_search?q=smith` - Search
- `GET/POST /fhir/Patient/{id}/Encounter` - Encounters
- `GET/POST /fhir/Patient/{id}/Condition` - Conditions
- `GET/POST /fhir/Patient/{id}/MedicationRequest` - Medications
- `GET /sync/changes?fromVersion=0` - Sync feed

### Scheduling (`:5001`)
- `GET/POST /Practitioner` - Practitioners
- `GET /Practitioner/_search?specialty=cardiology` - Search
- `GET/POST /Appointment` - Appointments
- `PATCH /Appointment/{id}/status` - Update status
- `GET /sync/changes?fromVersion=0` - Sync feed

### ICD10 (`:5090`)
- `GET /api/icd10/chapters` - ICD-10 chapters
- `GET /api/icd10/chapters/{id}/blocks` - Blocks within chapter
- `GET /api/icd10/codes/{code}` - Direct code lookup (`?format=fhir`)
- `GET /api/icd10/codes?q={query}&limit=20` - Text search
- `GET /api/achi/blocks` - ACHI procedure blocks
- `GET /api/achi/codes?q={query}&limit=20` - ACHI text search
- `POST /api/search` - RAG semantic search (requires embedding service)

### Gatekeeper (`:5002`)
- `POST /auth/register/begin` - Start passkey registration
- `POST /auth/register/complete` - Complete passkey registration
- `POST /auth/login/begin` - Start passkey login
- `POST /auth/login/complete` - Complete passkey login
- `GET /auth/session` - Current session info
- `GET /authz/check` - Permission check
- `POST /authz/evaluate` - Bulk permission evaluation

## Tech Stack

- .NET 10, ASP.NET Core Minimal API
- PostgreSQL with pgvector (semantic search)
- DataProvider (compile-time safe SQL)
- Sync Framework (bidirectional sync)
- LQL (Lambda Query Language)
- H5 transpiler + React 18
- Docker Compose

## License

MIT
