# HealthcareSamples -- Agent Instructions

⚠️ CRITICAL: **Reduce token usage.** Check file size before loading. Write less. Delete fluff and dead code. Alert user when context is loaded with pointless files. ⚠️ 

⚠️ MIGRATING ANY DB WITH ANYTHING OTHER THAN Data Provider Migrations is COMPLETELY ILLEGAL ⚠️ 

> Read this entire file before writing any code.
> These rules are NON-NEGOTIABLE. Violations will be rejected in review.

<!-- agent-pmo:29b9dcf -->

## Project Overview

HealthcareSamples is a comprehensive demonstration of the DataProvider .NET toolkit. It contains three FHIR-compliant microservices (Clinical API, Scheduling API, ICD-10 API) with bidirectional sync workers, semantic search via pgvector embeddings, a React dashboard (H5 transpiler), and Docker configuration. All medical data follows the FHIR R5 specification.

**Primary language(s):** C# (.NET 10.0)
**Build command:** `make ci`
**Test command:** `make test`
**Lint command:** `make lint`

This repo depends on NuGet packages from MelbourneDeveloper/DataProvider with the `MelbourneDev.` prefix (e.g., MelbourneDev.DataProvider, MelbourneDev.Migration, MelbourneDev.Sync.Postgres, MelbourneDev.Lql.Postgres, MelbourneDev.Selecta).

## Too Many Cooks (Multi-Agent Coordination)

If the TMC server is available:
1. Register immediately: descriptive name, intent, files you will touch
2. Before editing any file: lock it via TMC
3. Broadcast your plan before starting work
4. Check messages every few minutes
5. Release locks immediately when done
6. Never edit a locked file -- wait or find another approach

## Hard Rules -- Universal (no exceptions)

- **DO NOT use git commands.** No `git add`, `git commit`, `git push`, `git checkout`, `git merge`, `git rebase`, or any other git command. CI and GitHub Actions handle git.
- **ZERO DUPLICATION.** Before writing any code, search the codebase for existing implementations. Move code, don't copy it.
- **NO THROWING EXCEPTIONS.** Return `Result<T,E>`, `Option<T>`, or the language equivalent. Exceptions are only for unrecoverable bugs (panic-level).
- **NO REGEX on structured data.** Never parse JSON, YAML, TOML, code, or any structured format with regex. Use proper parsers, AST tools, or library functions.
- **NO PLACEHOLDERS.** If something isn't implemented, leave a loud compilation error with TODO. Never write code that silently does nothing.
- **Functions < 20 lines.** Refactor aggressively. If a function exceeds 20 lines, split it.
- **Files < 500 lines.** If a file exceeds 500 lines, extract modules.
- **100% test coverage is the goal.** Never delete or skip tests. Never remove assertions.
- **Prefer E2E/integration tests.** Unit tests are acceptable only for isolating problems.
- **Heavy logging everywhere.** See Logging Standards section below.
- **No suppressing linter warnings.** Fix the code, not the linter.
- **Pure functions** over statements
- **Every spec section MUST have a unique, hierarchical, non-numeric ID.** Format: `[GROUP-TOPIC]` or `[GROUP-TOPIC-DETAIL]` (e.g., `[AUTH-TOKEN-VERIFY]`, `[CI-TIMEOUT]`). The first word is the **group** -- all sections in the same group MUST be adjacent in the spec's TOC. NEVER use sequential numbers like `[SPEC-001]`. All code, tests, and design docs that implement or relate to a spec section MUST reference its ID in a comment (e.g., `// Implements [AUTH-TOKEN-VERIFY]`). This enables cross-referencing across specs, code, and tests -- grep `[AUTH-` to find every auth spec, its code, and its tests.

## Logging Standards

- **Use a structured logging library.** Never use `Console.WriteLine` or `Debug.WriteLine` for diagnostics. Use `Microsoft.Extensions.Logging`.
- **Log at entry/exit of all significant operations.** Use appropriate levels: `error`, `warn`, `info`, `debug`, `trace`.
- **Logging must be throughout the app.** Every service, handler, and non-trivial operation should log. Silent failures are forbidden.
- **SaaS / server apps:** Log to the database for persistence and queryability. Log calls that write to the database or file MUST be async or run on a background thread -- never block the request path with I/O logging.
- **NEVER log personal data.** No names, emails, addresses, phone numbers, IP addresses (unless required for security audit with explicit consent), or any PII.
- **NEVER log secrets.** No API keys, tokens, passwords, connection strings, or credentials. If you need to confirm a key is loaded, log a truncated hash or just `"API key: present"`.
- **Structured fields over string interpolation.** Log `{ "userId": 42, "action": "checkout" }` not `"User 42 performed checkout"`. This enables filtering and aggregation.

### Logging Libraries

| Language | Library | Notes |
|----------|---------|-------|
| C# | `Microsoft.Extensions.Logging` |  |

## Hard Rules -- C#

- No throwing exceptions -- return `Result<T,E>` or `Option<T>`
- No `!` null-forgiving operator
- No `as` casts -- use pattern matching
- No `dynamic`
- Nullable reference types enabled everywhere
- Records for immutable data
- Install common packages in the build props
- Avoid classes. Use static methods as pure functions
- All tables must have a SINGLE primary key
- Primary keys MUST be UUIDs
- No in-memory dbs -- real dbs all the way
- No raw SQL inserts/updates -- use generated extensions
- Use DataProvider Migrations to spin up DBs -- SQL for creating db schema = ILLEGAL
- Use `ImmutableList`, `FrozenSet`, or `ImmutableArray` instead of `List<T>`
- All public members require XMLDOC (except in test projects)
- One type per file (except small records)
- No commented-out code -- delete it
- Medical data must follow [FHIR R5 spec](https://build.fhir.org/resourcelist.html)

### Mandatory Packages (C# Only)

Always include these 3 in the Directory.Build.props:
```xml
<ItemGroup>
    <!-- Microsoft .NET Analyzers -->
    <PackageReference Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="9.0.0">
        <PrivateAssets>all</PrivateAssets>
        <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>

    <!-- Result types for Railway Oriented Programming -->
    <PackageReference Include="Outcome" Version="1.0.0" />

    <!-- Exhaustive pattern matching analyzer -->
    <PackageReference Include="Exhaustion" Version="1.0.0">
        <PrivateAssets>all</PrivateAssets>
        <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
</ItemGroup>
```

## Testing Rules

- **Never delete a failing test.** Fix the code or fix the test expectation -- never delete.
- **Never skip a test** without a ticket number and expiry date in the skip reason.
- **Assertions must be specific.** `assert True` without a condition is illegal.
- **No try/catch in tests** that swallows the exception and asserts success.
- **Tests must be deterministic.** No sleep(), no relying on timing, no random state.
- **E2E tests: black-box only.** Only interact via public APIs, UI commands, or CLI. Never call internal methods or manipulate internal state from a test.
- **Never use Fluent Assertions.**

## Build Commands (exact -- cross-platform via GNU Make)

All `make` targets work on Linux, macOS, and Windows. The Makefile uses OS detection to select portable commands. On Windows, install GNU Make via `choco install make` or use the one bundled with Git for Windows.

```bash
make start-docker   # build dashboard + spin up the full stack via docker compose
make start-local    # run all APIs locally against docker Postgres
make ci             # lint + test + coverage-check + build (full CI simulation)
make build          # compile everything
make test           # run tests with coverage
make lint           # run all linters
make fmt            # format all code
make fmt-check      # check formatting (CI uses this)
make coverage-check # assert coverage thresholds
make clean          # remove build artifacts
make setup          # post-create dev environment setup
make db-up          # start Postgres (pgvector) container
make db-down        # stop Postgres container
make db-migrate     # apply YAML schemas to all databases
make db-reset       # destroy DB volume and recreate
```

## Repo Structure

```
HealthcareSamples/
+-- .github/workflows/     # CI/CD pipelines
+-- .claude/skills/        # Claude Code skills
+-- Clinical/
|   +-- Clinical.Api/           # REST API (PostgreSQL) - FHIR Patient, Encounter, Condition, MedicationRequest
|   +-- Clinical.Api.Tests/     # E2E tests
|   +-- Clinical.Sync/          # Pulls Practitioner data from Scheduling
+-- Scheduling/
|   +-- Scheduling.Api/         # REST API (PostgreSQL) - FHIR Practitioner, Appointment, Schedule, Slot
|   +-- Scheduling.Api.Tests/   # E2E tests
|   +-- Scheduling.Sync/        # Pulls Patient data from Clinical
+-- ICD10/
|   +-- ICD10.Api/              # REST API (PostgreSQL + pgvector) - ICD-10 codes, ACHI codes, embeddings
|   +-- ICD10.Api.Tests/        # E2E tests
|   +-- ICD10.Cli/              # Interactive TUI client
|   +-- ICD10.Cli.Tests/        # CLI E2E tests
|   +-- embedding-service/      # Python FastAPI embedding service
|   +-- scripts/                # DB import + embedding generation
+-- Dashboard/
|   +-- Dashboard.Web/          # React UI (H5 transpiler C#->JavaScript)
|   +-- Dashboard.Integration.Tests/  # Integration tests
+-- Shared/
|   +-- Authorization/          # Shared authorization library
+-- docker/                     # Docker compose and configuration
+-- scripts/                    # Startup and cleanup scripts
+-- docs/
|   +-- specs/                  # Specification documents
|   +-- plans/                  # Implementation plans with TODO checklists
+-- .gitignore
+-- CLAUDE.md                   # Agent instructions (this file)
+-- AGENTS.md                   # Pointer to CLAUDE.md
+-- Makefile
+-- HealthcareSamples.sln
+-- Directory.Build.props
+-- coverlet.runsettings
```

## Data Ownership

| Domain | Owns | Receives via Sync |
|--------|------|-------------------|
| Clinical | fhir_Patient, fhir_Encounter, fhir_Condition, fhir_MedicationRequest | sync_Provider |
| Scheduling | fhir_Practitioner, fhir_Appointment, fhir_Schedule, fhir_Slot | sync_ScheduledPatient |
| ICD10 | icd10_chapter, icd10_block, icd10_category, icd10_code, achi_block, achi_code | N/A (read-only reference) |

## Claude Code Skills

- [Claude Code Skills Overview](https://platform.claude.com/docs/en/agents-and-tools/agent-skills/overview)
- [The Complete Guide to Building Skills for Claude (PDF)](https://resources.anthropic.com/hubfs/The-Complete-Guide-to-Building-Skill-for-Claude.pdf)
