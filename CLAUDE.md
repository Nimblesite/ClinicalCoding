# Nimblesite Clinical Coding Platform -- Agent Instructions

⚠️ CRITICAL: **Reduce token usage.** Check file size before loading. Write less. Delete fluff and dead code. ⚠️

> Read this entire file before writing any code.
> These rules are NON-NEGOTIABLE. Violations will be rejected in review.

<!-- agent-pmo:6647c8e -->

## Project Overview

Agentic clinical coding platform built on Nimblesite DataProvider. Four FHIR R5-compliant microservices (Clinical, Scheduling, ICD-10, Gatekeeper) with bidirectional sync, semantic search via pgvector embeddings, passkey auth, and a React dashboard (H5 transpiler). The ICD-10 RAG service is the foundation for AI-assisted clinical coding from patient encounters and notes.

**Primary language:** C# (.NET 10.0)
**Build:** `make ci` | **Test:** `make test` | **Lint:** `make lint`

NuGet packages use the `Nimblesite.` prefix (e.g., Nimblesite.DataProvider.Core, Nimblesite.Sync.Postgres, Nimblesite.Lql.Postgres).

## Hard Rules

- **DO NOT use git commands.** CI and GitHub Actions handle git.
- **TOTALL CSS MUST < 1K LOC** - CSS is cancer and you are the cure for it
- **MIGRATING ANY DB WITH ANYTHING OTHER THAN DataProvider Migrations is COMPLETELY ILLEGAL**
- **ZERO DUPLICATION.** Search the codebase before writing. Move code, don't copy it.
- **NO THROWING EXCEPTIONS.** Return `Result<T,E>` or `Option<T>`. Exceptions are only for unrecoverable bugs.
- **NO REGEX on structured data.** Use proper parsers.
- **NO PLACEHOLDERS.** Unimplemented code must leave a compilation error with TODO.
- **Functions < 20 lines. Files < 500 lines.** Refactor aggressively.
- **100% test coverage is the goal.** Never delete or skip tests. Never remove assertions.
- **Prefer E2E/integration tests.** Unit tests only for isolating problems.
- **No suppressing linter warnings.** Fix the code, not the linter.
- **Pure functions** over statements.
- **Spec section IDs** must be hierarchical and non-numeric: `[GROUP-TOPIC-DETAIL]` (e.g., `[AUTH-TOKEN-VERIFY]`). Code and tests reference these via comments.

## C# Rules

- No `!` null-forgiving, no `as` casts (use pattern matching), no `dynamic`
- Nullable reference types enabled everywhere
- Records for immutable data. Avoid classes. Use static methods as pure functions.
- `ImmutableList`, `FrozenSet`, or `ImmutableArray` instead of `List<T>`
- All tables: single UUID primary key
- No in-memory DBs. No raw SQL inserts/updates -- use generated extensions.
- DB schemas via DataProvider Migrations only. SQL for schema creation = ILLEGAL.
- All public members require XMLDOC (except test projects)
- One type per file (except small records). No commented-out code.
- Medical data must follow [FHIR R5 spec](https://build.fhir.org/resourcelist.html)
- Common packages go in Directory.Build.props

## Multi-Agent Coordination (TMC)

If the TMC server is available:
1. Register immediately with descriptive name, intent, and files you will touch
2. Lock files via TMC before editing
3. Broadcast your plan before starting work
4. Release locks immediately when done
5. Never edit a locked file

## Logging

- `Microsoft.Extensions.Logging` only. Never `Console.WriteLine`.
- Log entry/exit of significant operations. Silent failures are forbidden.
- Structured fields over string interpolation.
- Never log PII or secrets.

## Testing

- Never delete a failing test. Fix the code or the expectation.
- Never skip a test without a ticket number and expiry date.
- Assertions must be specific. No try/catch that swallows exceptions.
- Tests must be deterministic. No sleep(), no timing, no random state.
- E2E tests: black-box only via public APIs. Never use Fluent Assertions.

## Make Targets

```bash
make start-docker   # spin up the full stack via docker compose
make start-local    # run APIs locally against docker Postgres
make ci             # lint + test + build (test includes coverage enforcement)
make build          # compile everything
make test           # run tests with coverage (fails if below threshold)
make lint           # check formatting + run all linters
make fmt            # format all code
make clean          # remove build artifacts
make setup          # restore tools + packages (first time)
make db-up          # start Postgres container
make db-down        # stop Postgres container
make db-migrate     # apply YAML schemas to all databases
make db-reset       # destroy DB volume and recreate
```

## Repo Structure

```
Clinical/
  Clinical.Api/             # FHIR Patient, Encounter, Condition, MedicationRequest
  Clinical.Api.Tests/
  Clinical.Sync/            # Pulls Practitioner data from Scheduling
Scheduling/
  Scheduling.Api/           # FHIR Practitioner, Appointment, Schedule, Slot
  Scheduling.Api.Tests/
  Scheduling.Sync/          # Pulls Patient data from Clinical
ICD10/
  ICD10.Api/                # ICD-10/ACHI codes, pgvector embeddings, RAG search
  ICD10.Api.Tests/
  ICD10.Cli/                # Interactive TUI client
  ICD10.Cli.Tests/
  embedding-service/        # Python FastAPI embedding service
  scripts/                  # DB import + embedding generation
Gatekeeper/
  Gatekeeper.Api/           # Passkey auth, RBAC authorization
  Gatekeeper.Api.Tests/
Dashboard/
  Dashboard.Web/            # React UI (H5 transpiler C# -> JS)
  Dashboard.Integration.Tests/
Shared/
  Authorization/            # Shared authorization library
docker/                     # Docker compose and configuration
docs/
  specs/                    # Specification documents
  plans/                    # Implementation plans
```

## Data Ownership

| Domain | Owns | Receives via Sync |
|--------|------|-------------------|
| Clinical | fhir_Patient, fhir_Encounter, fhir_Condition, fhir_MedicationRequest | sync_Provider |
| Scheduling | fhir_Practitioner, fhir_Appointment, fhir_Schedule, fhir_Slot | sync_ScheduledPatient |
| ICD10 | icd10_chapter, icd10_block, icd10_category, icd10_code, achi_block, achi_code | N/A (read-only) |
