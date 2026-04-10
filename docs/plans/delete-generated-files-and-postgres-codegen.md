# Plan: Delete Committed Generated Code & Switch to Postgres-Based Generation

DataProvider reference code here:
/Users/christianfindlay/Documents/Code/ai_cms

## Progress Checklist

- [x] Step 1 — Add `dataprovider-postgres` (0.2.7-beta), `lql-postgres` (0.1.8-beta), bump `migration-cli` (0.2.2-beta) to `.config/dotnet-tools.json`
- [x] Step 2 — Create `docker/docker-compose.db.yml`
- [x] Step 3 — Add `db-up`, `db-down`, `db-reset`, `db-wait`, `db-migrate` Makefile targets and wire `build`/`test`/`lint` to depend on `db-migrate`
- [x] Step 4 — Update each `DataProvider.json` for Postgres (Clinical, Scheduling, Gatekeeper, ICD10) — connection string + `schema` `main` → `public`, drop `excludeColumns`
- [x] Step 5 — Update each API `.csproj` (Clinical, Scheduling, Gatekeeper, ICD10) — switch to `dataprovider-postgres`, switch LQL to `lql-postgres`, drop `IgnoreExitCode`, delete `CreateDatabaseSchema` target, drop SQLite `icd10.db` `<Content>`
- [x] Step 6 — Delete tracked `Generated/` files from git, update root `.gitignore`, simplify `ICD10/.gitignore`
- [x] Step 7 — Update `.github/workflows/ci.yml` to use `make db-up` / `make db-migrate` instead of inline `services.postgres`
- [x] Step 8 — Patch consumer C# (Gatekeeper/Clinical/Scheduling/ICD10) for new generated record shape (`Result<Guid?>` instead of `Result<int>`, snake_case fields preserved, IDbTransaction overloads)
- [x] Verify — `make build` succeeds with 0 errors / 0 warnings; full `HealthcareSamples.sln` builds clean; all `Generated/` content regenerated each build through `dataprovider-postgres` against live Postgres

## Context

Generated `.g.cs` files are currently committed to git in three of four API projects (Clinical, Scheduling, Gatekeeper). The fourth (ICD10) already excludes them via a per-folder `.gitignore`. This causes constant noise:

- The current uncommitted modification to `Gatekeeper/Gatekeeper.Api/Generated/CheckResourceGrant.g.cs` shows only the **order of parameters** changed between two runs of the generator. Output is non-deterministic across machines/runs.
- Worse, the current generator (`dataprovider-sqlite`) reads schema from a local SQLite mirror created from YAML by `migration-cli --provider sqlite`. SQLite has no real type system, so every generated column comes back as `string`. Look at `CheckResourceGrant.g.cs` lines 102–126: `id`, `granted_at`, `expires_at`, `permission_id` are all `string` when they should be `Guid` / `DateTimeOffset`. This is a latent runtime bug in addition to the file-churn problem.
- The MSBuild target uses `IgnoreExitCode="true"` on the codegen step, so silent generation failures get masked and someone could be tempted to hand-edit the resulting stale files.

The goal: **all four projects must regenerate their data-access code from a live Postgres database on every build**, the generated files must never be committed, and there must never be any need to manually edit generated code. Build/test/CI must spin up the Postgres container before invoking the generators.

---

## Recommended Approach (Summary)

1. Switch all four projects from `dataprovider-sqlite` to `dataprovider-postgres` for accurate type introspection.
2. Move the migration step from inside each `.csproj` to a single `make db-migrate` target that applies YAML schemas to the live Postgres via `migration-cli --provider postgres`.
3. Add `make db-up` / `make db-down` targets that start/stop the Postgres container via `docker compose`. Make `db-up` a hard prerequisite of `make build`, `make test`, `make ci`. The MSBuild codegen target stays inside each `.csproj` so IDE rebuilds also regenerate, but it now expects Postgres to be reachable and **fails loudly** if it isn't (no more `IgnoreExitCode`).
4. Update the GitHub Actions workflow to use the same `make db-up && make db-migrate` flow instead of the inline `services:` Postgres block, so local and CI run the identical pipeline.
5. Delete tracked `Generated/` files from git, add a single root `**/Generated/` ignore rule, and consolidate the per-folder ICD10 ignores.

---

## Critical Files

| Path | Change |
| --- | --- |
| `.gitignore` (root) | Add `**/Generated/`, `*.generated.sql`, `*.db` |
| `ICD10/.gitignore` | Remove now-redundant lines (`Generated/`, `*.generated.sql`, `*.db`) |
| `.config/dotnet-tools.json` | Add `nimblesite.dataprovider.postgres.cli` (replaces sqlite version for codegen) |
| `Makefile` | Add `db-up`, `db-down`, `db-reset`, `db-migrate` targets; make `build`/`test`/`ci` depend on them |
| `docker/docker-compose.db.yml` (NEW) | Stripped-down compose with only the `db` service for use during build |
| `Clinical/Clinical.Api/Clinical.Api.csproj` | Switch Exec to `dataprovider-postgres`, remove `migration-cli` Target, remove `IgnoreExitCode` |
| `Scheduling/Scheduling.Api/Scheduling.Api.csproj` | Same |
| `Gatekeeper/Gatekeeper.Api/Gatekeeper.Api.csproj` | Same |
| `ICD10/ICD10.Api/ICD10.Api.csproj` | Same |
| `Clinical/Clinical.Api/DataProvider.json` | Update `connectionString` to Postgres; change `tables[].schema` from `main` to `public` |
| `Scheduling/Scheduling.Api/DataProvider.json` | Same |
| `Gatekeeper/Gatekeeper.Api/DataProvider.json` | Same |
| `ICD10/ICD10.Api/DataProvider.json` | Same |
| `.github/workflows/ci.yml` | Remove `services.postgres` block; add `make db-up` step before lint/test/build |

---

## Step-by-Step Plan

### Step 1 — Add `dataprovider-postgres` to dotnet tools

Edit `.config/dotnet-tools.json` and add an entry alongside the existing tools:

```json
"nimblesite.dataprovider.postgres.cli": {
  "version": "0.2.0-beta",
  "commands": ["dataprovider-postgres"],
  "rollForward": false
}
```

Keep `nimblesite.dataprovider.sqlite.cli` for now (ICD10 may still use it temporarily — see Step 8).

### Step 2 — Create a build-only docker compose file

Create `docker/docker-compose.db.yml` containing only the `db` service from the existing `docker/docker-compose.yml` (lines 2–16). This is the file `make db-up` will invoke. Reuses the existing init scripts in `docker/init-db/` which create the four databases (`gatekeeper`, `clinical`, `scheduling`, `icd10`) and roles.

### Step 3 — Add Makefile targets

Insert into [Makefile](Makefile) after the existing `setup` target:

```make
# =============================================================================
# DATABASE LIFECYCLE (required for code generation + tests)
# =============================================================================

DB_COMPOSE := docker compose -f docker/docker-compose.db.yml
DB_PASSWORD ?= changeme

## db-up: Start Postgres container and wait until healthy
db-up:
	@echo "==> Starting Postgres..."
	@$(DB_COMPOSE) up -d
	@echo "==> Waiting for Postgres to become healthy..."
	@for i in $$(seq 1 30); do \
	  if $(DB_COMPOSE) exec -T db pg_isready -U postgres >/dev/null 2>&1; then \
	    echo "Postgres ready."; exit 0; \
	  fi; sleep 1; \
	done; \
	echo "FAIL: Postgres never became healthy"; $(DB_COMPOSE) logs db; exit 1

## db-down: Stop Postgres container (preserves volume)
db-down:
	@echo "==> Stopping Postgres..."
	@$(DB_COMPOSE) down

## db-reset: Drop volume and restart Postgres clean
db-reset:
	@echo "==> Resetting Postgres (DROP volumes)..."
	@$(DB_COMPOSE) down -v
	@$(MAKE) db-up
	@$(MAKE) db-migrate

## db-migrate: Apply YAML schemas to live Postgres for all four databases
db-migrate: db-up
	@echo "==> Applying schemas..."
	dotnet migration-cli --schema Gatekeeper/Gatekeeper.Api/gatekeeper-schema.yaml \
	  --output "Host=localhost;Database=gatekeeper;Username=gatekeeper;Password=$(DB_PASSWORD)" \
	  --provider postgres
	dotnet migration-cli --schema Clinical/Clinical.Api/clinical-schema.yaml \
	  --output "Host=localhost;Database=clinical;Username=clinical;Password=$(DB_PASSWORD)" \
	  --provider postgres
	dotnet migration-cli --schema Scheduling/Scheduling.Api/scheduling-schema.yaml \
	  --output "Host=localhost;Database=scheduling;Username=scheduling;Password=$(DB_PASSWORD)" \
	  --provider postgres
	dotnet migration-cli --schema ICD10/ICD10.Api/icd10-schema.yaml \
	  --output "Host=localhost;Database=icd10;Username=icd10;Password=$(DB_PASSWORD)" \
	  --provider postgres
```

Then change the existing primary targets so they depend on a live, migrated database:

```make
build: db-migrate
	@echo "==> Building..."
	dotnet build HealthcareSamples.sln --configuration Release

test: db-migrate
	@echo "==> Testing..."
	dotnet test ...

lint: fmt-check db-migrate
	@echo "==> Linting..."
	dotnet build HealthcareSamples.sln --configuration Release
```

`db-migrate` itself depends on `db-up`, so the chain `make ci → lint → db-migrate → db-up` guarantees the container is started before any `dotnet` invocation. Add `db-up`, `db-down`, `db-reset`, `db-migrate` to the `.PHONY` list and to the `help` target.

### Step 4 — Update each `DataProvider.json` for Postgres

For all four files (`Clinical/Clinical.Api/DataProvider.json`, `Scheduling/Scheduling.Api/DataProvider.json`, `Gatekeeper/Gatekeeper.Api/DataProvider.json`, `ICD10/ICD10.Api/DataProvider.json`):

1. Replace the `connectionString` field. Example for Gatekeeper:
   ```json
   "connectionString": "Host=localhost;Database=gatekeeper;Username=gatekeeper;Password=changeme"
   ```
   This is a **dev/codegen-only** connection string. Runtime uses `appsettings.json` / env vars and is unaffected. The plaintext `changeme` here is acceptable because the same default already lives in `docker/docker-compose.yml` line 24 and `.github/workflows/ci.yml` line 24.

2. Change every `"schema": "main"` to `"schema": "public"`. SQLite's default schema is `main`; Postgres's is `public`.

### Step 5 — Update each API `.csproj`

For each of the four API csproj files, apply these changes (example shown for `Gatekeeper.Api.csproj`; the same pattern applies to the others — just delete the `CreateDatabaseSchema` Target since migrations now run via `make db-migrate`):

**Delete** the `CreateDatabaseSchema` Target entirely (lines 33–40 in Gatekeeper, equivalent lines in the others). Migrations are now a Makefile concern.

**Replace** the body of `TranspileLqlAndGenerateDataProvider`:

```xml
<Target
  Name="TranspileLqlAndGenerateDataProvider"
  BeforeTargets="BeforeCompile;CoreCompile"
  Inputs="$(MSBuildProjectDirectory)/DataProvider.json;@(AdditionalFiles);@(LqlFiles)"
  Outputs="$(MSBuildProjectDirectory)/Generated/.timestamp"
>
  <RemoveDir Directories="$(MSBuildProjectDirectory)/Generated" />
  <MakeDir Directories="$(MSBuildProjectDirectory)/Generated" />
  <ItemGroup>
    <LqlFiles Include="$(MSBuildProjectDirectory)/**/*.lql" />
  </ItemGroup>
  <Exec
    Command="dotnet lqlcli-sqlite --input &quot;%(LqlFiles.Identity)&quot; --output &quot;%(LqlFiles.RootDir)%(LqlFiles.Directory)%(LqlFiles.Filename).generated.sql&quot;"
    Condition="'$(EnableLqlTranspile)' == 'true' and @(LqlFiles) != ''"
    WorkingDirectory="$(MSBuildProjectDirectory)" />
  <Exec
    Command="dotnet dataprovider-postgres --project-dir &quot;$(MSBuildProjectDirectory)&quot; --config &quot;$(MSBuildProjectDirectory)/DataProvider.json&quot; --out &quot;$(MSBuildProjectDirectory)/Generated&quot;"
    WorkingDirectory="$(MSBuildProjectDirectory)"
    StandardOutputImportance="High"
    StandardErrorImportance="High" />
  <Touch Files="$(MSBuildProjectDirectory)/Generated/.timestamp" AlwaysCreate="true" />
  <ItemGroup>
    <Compile Include="$(MSBuildProjectDirectory)/Generated/**/*.g.cs" />
  </ItemGroup>
</Target>
```

Notable diffs from the current target:

- `dataprovider-sqlite` → `dataprovider-postgres`
- `--connection-type NpgsqlConnection` flag removed (the postgres tool always emits Npgsql code)
- **`IgnoreExitCode="true"` removed** from the codegen step. If Postgres is unreachable, generation fails, and so does the build. This is the enforcement mechanism for "code must always be generated, never hand-edited".
- `ContinueOnError="WarnAndContinue"` removed from the LQL transpile step for the same reason.

The `<Compile Remove="Generated/**" />` ItemGroup at the top of each csproj stays unchanged.

### Step 6 — Delete tracked Generated files and update gitignore

```bash
git rm -r --cached Clinical/Clinical.Api/Generated
git rm -r --cached Scheduling/Scheduling.Api/Generated
git rm -r --cached Gatekeeper/Gatekeeper.Api/Generated
```

ICD10 has zero tracked `Generated/` files (verified via `git ls-files`); skip it.

Edit [.gitignore](.gitignore) and add to the C#/.NET section after line 70 (`obj/`):

```gitignore
# Generated code (regenerated at build time by TranspileLqlAndGenerateDataProvider)
**/Generated/
# LQL transpile output
*.generated.sql
# SQLite databases (legacy from previous codegen approach; safe to keep ignored)
*.db
```

Edit [ICD10/.gitignore](ICD10/.gitignore) and **delete** lines 1–4 (`# Generated files`, `*.generated.sql`, `*.db`, `Generated/`) — they are now redundant. Leave the Python and IDE sections alone.

### Step 7 — Update the GitHub Actions workflow

In [.github/workflows/ci.yml](.github/workflows/ci.yml):

1. **Delete** the `services.postgres` block (lines 19–31). CI will now use the same `docker compose` flow as local dev via `make db-up`.
2. **Insert** a new step right after `dotnet tool restore` (between current line 59 and line 61):
   ```yaml
   - name: Start database
     run: make db-up
   - name: Apply schemas
     run: make db-migrate
   ```
3. The existing `Lint` / `Test` / `Build` steps already invoke `make`, and those targets now depend on `db-migrate` (which depends on `db-up`), so even if the explicit steps above were removed the chain would still work. We add them explicitly for clarity in the CI log and to fail fast at a recognizable step name.
4. The embedding-service step at lines 42–56 stays as-is — it's unrelated.
5. Move the `Build` step (currently at line 88, the LAST step) to be the FIRST `make` invocation, BEFORE `Lint`, `Test`, `Coverage check`. Right now `make build` runs after `make test`, which is backwards: tests can't run if the build is broken, so build must come first. Order should be: db-up → db-migrate → build → lint → test → coverage-check → upload.

### Step 8 — Decide ICD10's fate

ICD10's codegen currently uses the SQLite path *and* references `EnableLqlTranspile=true` for `.lql` files. After this plan, ICD10 must also use `dataprovider-postgres`. Apply the same `.csproj` and `DataProvider.json` changes as the other three. This means the ICD10 SQLite-mirror approach goes away entirely. Verify that no test or runtime code depends on the on-disk `icd10.db` SQLite file (`<Content Include="icd10.db" Condition="Exists('icd10.db')">` in the csproj — that line should also be deleted, since the runtime is Postgres). Once ICD10 is migrated, `nimblesite.dataprovider.sqlite.cli` and `nimblesite.lql.cli.sqlite` can be reviewed for removal from `.config/dotnet-tools.json` (out of scope for this plan if anything still depends on them).

---

## Verification

1. **Local fresh-clone smoke test:**
   ```bash
   make clean
   make db-reset      # drops volume, starts Postgres clean, applies all schemas
   make ci            # lint + test + build, all from a clean slate
   git status         # MUST be clean — no Generated/ files showing up
   ```
   Expected: every project regenerates its `Generated/*.g.cs` files from the live Postgres schema, all tests pass, working tree is clean.

2. **Type-correctness spot check:** Open `Gatekeeper/Gatekeeper.Api/Generated/CheckResourceGrant.g.cs` after a successful build. The columns `id`, `permission_id`, `granted_at`, `expires_at` should now be `Guid` and `DateTimeOffset`, NOT `string`. This is the proof that Postgres-based introspection is wired up correctly.

3. **Failure mode check:**
   ```bash
   make db-down
   dotnet build Gatekeeper/Gatekeeper.Api/Gatekeeper.Api.csproj
   ```
   Expected: build FAILS with a clear error from `dataprovider-postgres` saying it cannot connect. This verifies that `IgnoreExitCode` removal works — generation failures are now loud.

4. **Re-build idempotency:**
   ```bash
   make build
   make build
   git status
   ```
   Expected: clean working tree after both builds. (Note: even if the Nimblesite generator's output ordering is non-deterministic upstream, this no longer matters because the files are gitignored. See Concern (a).)

5. **CI verification:** Push the branch and confirm the GitHub Actions run goes through the steps in this order: `Start database` → `Apply schemas` → `Build` → `Lint` → `Test` → `Coverage check`. All green.

---

## Concerns / Follow-ups (out of scope but worth tracking)

**(a) Non-deterministic generator output.** The `CheckResourceGrant.g.cs` parameter-order drift you observed comes from dictionary iteration order in `dataprovider-sqlite`. After this plan it stops mattering for git but should still be filed against `/Users/christianfindlay/Documents/Code/gigs/DataProvider` so future debugging diffs across machines stay quiet. Fix: sort keys with `StringComparer.Ordinal` before emitting parameter lists.

**(b) Connection string secrecy.** `DataProvider.json` will contain `Password=changeme` in plaintext. Acceptable because (i) it's a dev-only password matching `docker-compose.yml` and CI defaults, (ii) the file is committed in source already, (iii) production runtime uses `appsettings.json` / env vars and is unaffected. If a real password ever appears here, it's a process failure regardless of file format. Optional follow-up: support `${DB_PASSWORD}` env var substitution in `DataProvider.json` upstream.

**(c) MSBuild target re-runs every build.** The current target's `<RemoveDir>` deletes `.timestamp` immediately, so the `Inputs`/`Outputs` incremental check always sees outputs as missing and re-runs. This is wasteful but pre-existing; not changed by this plan. Follow-up: drop `RemoveDir`/`MakeDir` and let `dataprovider-postgres` overwrite in place, so MSBuild's incremental check actually works.

**(d) `make lint` runs `csharpier check`.** Once generated files are produced fresh on every build, csharpier may flag them. Either (i) the generator must emit csharpier-clean output, or (ii) add `Generated/` to `.csharpierignore`. Recommend (ii) — generated code should never be linted.

**(e) ICD10 SQLite leftovers.** Once Step 8 lands, the `*.db` files (`icd10.db`, `clinical.db`, `scheduling.db`, `gatekeeper.db`) and `*.generated.sql` files left over from prior builds become orphans. `make clean` should be extended to remove them, or they should be deleted from any working trees as a one-time cleanup.

**(f) `dataprovider-postgres` argument set.** Confirmed via DataProvider source at `/Users/christianfindlay/Documents/Code/gigs/DataProvider/DataProvider/DataProvider.Postgres.Cli`: it accepts `--project-dir`, `--config`, `--out` and reads the connection string from `DataProvider.json`. No `--connection-string` flag exists, which is why we put the connection string in the JSON.
