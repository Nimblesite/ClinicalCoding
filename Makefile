# agent-pmo:80947ac
# =============================================================================
# Makefile — HealthcareSamples
# Cross-platform: Linux, macOS, Windows (via GNU Make)
# =============================================================================

.PHONY: build test lint fmt clean ci setup db-up db-down db-reset db-wait db-migrate start-local start-docker

# -----------------------------------------------------------------------------
# OS Detection
# -----------------------------------------------------------------------------
ifeq ($(OS),Windows_NT)
  SHELL := powershell.exe
  .SHELLFLAGS := -NoProfile -Command
  RM = Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
  MKDIR = New-Item -ItemType Directory -Force
  HOME ?= $(USERPROFILE)
else
  RM = rm -rf
  MKDIR = mkdir -p
endif

# Per-project coverage thresholds live in this JSON file. Each test
# project gets its own minimum line-rate. `make test` enforces these after
# each project. Bump thresholds to floor(measured) - 1 when coverage increases.
COVERAGE_THRESHOLDS_FILE ?= coverage-thresholds.json

# Postgres dev database (docker compose). Override in CI via env vars.
DB_COMPOSE_FILE ?= docker/docker-compose.db.yml
DB_PASSWORD ?= changeme
DB_HOST ?= localhost
DB_PORT ?= 5432
PG_BASE_URL ?= Host=$(DB_HOST);Port=$(DB_PORT);Username=postgres;Password=$(DB_PASSWORD)

# =============================================================================
# PRIMARY TARGETS
# =============================================================================

## build: Compile/assemble all artifacts (requires running Postgres + migrated schemas)
build: db-migrate
	@echo "==> Building..."
	dotnet build HealthcareSamples.sln --configuration Release

# Test projects in execution order. Cheapest / most foundational first so a
# break in a lower layer fails the run immediately, before slower E2E suites.
TEST_PROJECTS = \
  Gatekeeper/Gatekeeper.Api.Tests/Gatekeeper.Api.Tests.csproj \
  Clinical/Clinical.Api.Tests/Clinical.Api.Tests.csproj \
  Scheduling/Scheduling.Api.Tests/Scheduling.Api.Tests.csproj \
  ICD10/ICD10.Api.Tests/ICD10.Api.Tests.csproj \
  ICD10/ICD10.Cli.Tests/ICD10.Cli.Tests.csproj \
  Dashboard/Dashboard.Integration.Tests/Dashboard.Integration.Tests.csproj

## test: Run full test suite with coverage (FAIL FAST)
##   - Stops at the first failing test inside an assembly (xunit stopOnFail)
##   - Stops at the first failing assembly across the suite (set -e)
##   - After each project, checks coverage against threshold from $(COVERAGE_THRESHOLDS_FILE)
##     and fails immediately if below.
test: db-migrate
	@echo "==> Testing (fail-fast)..."
	@command -v jq >/dev/null 2>&1 || { echo "FAIL: jq is required (brew install jq / apt-get install jq)"; exit 1; }
	@if [ ! -f "$(COVERAGE_THRESHOLDS_FILE)" ]; then \
	  echo "FAIL: $(COVERAGE_THRESHOLDS_FILE) not found"; exit 1; \
	fi
	@set -e; \
	rm -rf TestResults; \
	default=$$(jq -r '.default_threshold' $(COVERAGE_THRESHOLDS_FILE)); \
	for proj in $(TEST_PROJECTS); do \
	  proj_dir=$$(dirname "$$proj"); \
	  echo ""; \
	  echo "==> Testing $$proj"; \
	  dotnet test "$$proj" --configuration Release \
	    --settings coverlet.runsettings \
	    --collect:"XPlat Code Coverage" \
	    --results-directory "TestResults/$$proj_dir" \
	    --verbosity normal \
	    || { echo ""; echo "FAIL: $$proj failed -- aborting remaining test projects"; exit 1; }; \
	  cobertura=$$(find "TestResults/$$proj_dir" -name 'coverage.cobertura.xml' 2>/dev/null | head -1); \
	  threshold=$$(jq -r --arg p "$$proj_dir" --arg d "$$default" '.projects[$$p].threshold // ($$d | tonumber)' $(COVERAGE_THRESHOLDS_FILE)); \
	  if [ -z "$$cobertura" ]; then \
	    echo "FAIL ($$proj_dir): no coverage.cobertura.xml"; exit 1; \
	  fi; \
	  line_rate=$$(awk 'match($$0, /line-rate="[0-9.]+"/) { s=substr($$0, RSTART+11, RLENGTH-12); print s; exit }' "$$cobertura"); \
	  pct=$$(awk "BEGIN{printf \"%.1f\", $${line_rate:-0}*100}"); \
	  pct_int=$$(awk "BEGIN{printf \"%d\", $${line_rate:-0}*100}"); \
	  if [ "$$pct_int" -lt "$$threshold" ]; then \
	    printf "FAIL %-44s %s%% < %s%%\n" "$$proj_dir" "$$pct" "$$threshold"; \
	    exit 1; \
	  else \
	    printf "OK   %-44s %s%% >= %s%%\n" "$$proj_dir" "$$pct" "$$threshold"; \
	  fi; \
	done

## lint: Run all linters (fails on any warning). Format check runs FIRST.
lint: db-migrate
	@echo "==> Checking format..."
	dotnet csharpier check .
	@echo "==> Linting..."
	dotnet build HealthcareSamples.sln --configuration Release

## fmt: Format all code in-place
fmt:
	@echo "==> Formatting..."
	dotnet csharpier format .

## clean: Remove all build artifacts
clean:
	@echo "==> Cleaning..."
ifeq ($(OS),Windows_NT)
	Get-ChildItem -Recurse -Directory -Include bin,obj | Remove-Item -Recurse -Force
	$(RM) TestResults
else
	find . -type d \( -name bin -o -name obj \) | xargs rm -rf
	$(RM) TestResults
endif

## ci: lint + test + build (full CI simulation -- test includes coverage checks)
ci: lint test build

## setup: Post-create dev environment setup
setup:
	@echo "==> Setting up development environment..."
	dotnet tool restore
	dotnet restore
	@echo "==> Setup complete. Run 'make ci' to validate."

# =============================================================================
# DEV DATABASE (Postgres via docker compose)
# =============================================================================

## db-up: Start Postgres (pgvector) container in background
db-up:
	@echo "==> Starting Postgres..."
	DB_PASSWORD=$(DB_PASSWORD) docker compose -f $(DB_COMPOSE_FILE) up -d
	@$(MAKE) db-wait

## db-down: Stop and remove Postgres container (preserves volume)
db-down:
	@echo "==> Stopping Postgres..."
	docker compose -f $(DB_COMPOSE_FILE) down

## db-reset: Destroy DB volume and recreate from init scripts
db-reset:
	@echo "==> Resetting Postgres (DESTRUCTIVE)..."
	docker compose -f $(DB_COMPOSE_FILE) down -v
	DB_PASSWORD=$(DB_PASSWORD) docker compose -f $(DB_COMPOSE_FILE) up -d
	@$(MAKE) db-wait

## db-wait: Block until Postgres healthcheck reports healthy
db-wait:
	@echo "==> Waiting for Postgres to be ready..."
	@for i in $$(seq 1 60); do \
	  STATUS=$$(docker inspect --format '{{.State.Health.Status}}' healthcaresamples-db 2>/dev/null || echo "missing"); \
	  if [ "$$STATUS" = "healthy" ]; then echo "Postgres ready"; exit 0; fi; \
	  sleep 1; \
	done; \
	echo "FAIL: Postgres did not become healthy"; \
	docker logs healthcaresamples-db 2>&1 | tail -50; \
	exit 1

## db-migrate: Ensure DB is up and apply YAML schemas via DataProviderMigrate to all four databases
db-migrate: db-up
	@echo "==> Migrating Postgres schemas..."
	dotnet DataProviderMigrate --schema Gatekeeper/Gatekeeper.Api/gatekeeper-schema.yaml \
	  --output "$(PG_BASE_URL);Database=gatekeeper" --provider postgres
	dotnet DataProviderMigrate --schema Clinical/Clinical.Api/clinical-schema.yaml \
	  --output "$(PG_BASE_URL);Database=clinical" --provider postgres
	dotnet DataProviderMigrate --schema Scheduling/Scheduling.Api/scheduling-schema.yaml \
	  --output "$(PG_BASE_URL);Database=scheduling" --provider postgres
	dotnet DataProviderMigrate --schema ICD10/ICD10.Api/icd10-schema.yaml \
	  --output "$(PG_BASE_URL);Database=icd10" --provider postgres

# =============================================================================
# RUN THE STACK
# =============================================================================

## start-docker: Build the dashboard locally then start the full docker compose stack
##   Usage: make start-docker [BUILD=1]
##     BUILD=1   force image rebuild (passes --build to docker compose up)
start-docker:
	@echo "==> Building Dashboard locally (H5 requires native build)..."
	cd Dashboard/Dashboard.Web && \
	  dotnet publish -c Release -o ../../docker/dashboard-build --nologo -v q
	@echo "==> Starting docker stack..."
	cd docker && docker compose up $(if $(BUILD),--build,)

# Embedded runner for the local dev stack. Inlined as a `define` block so the
# orchestration (background processes, trap-based cleanup, log prefixing) runs
# in a single shell — Make's default one-shell-per-line model can't express it.
define START_LOCAL_RUNNER
set -e
PIDS=()

cleanup() {
    echo ""
    echo "Shutting down..."
    for pid in "$${PIDS[@]}"; do
        kill "$$pid" 2>/dev/null || true
    done
    wait 2>/dev/null || true
    echo "All services stopped."
}
trap cleanup EXIT INT TERM

DB_PASS="$${DB_PASSWORD:-changeme}"
VENV_DIR="ICD10/.venv"
EMBED_DIR="ICD10/embedding-service"

echo "Starting Embedding Service on :8000 (model loading may take a moment)..."
"$$VENV_DIR/bin/python" -m uvicorn main:app --host 0.0.0.0 --port 8000 \
    --app-dir "$$EMBED_DIR" 2>&1 | sed 's/^/  [embedding]  /' &
PIDS+=($$!)

populate_icd10() {
    local CONN_STR="Host=localhost;Database=icd10;Username=icd10;Password=$$DB_PASS"
    local SCRIPTS_DIR="ICD10/scripts/CreateDb"

    echo "  [icd10-import] Waiting for ICD10 API..."
    for i in $$(seq 1 60); do
        if curl -sf http://localhost:5090/health >/dev/null 2>&1; then
            echo "  [icd10-import] ICD10 API is up."
            break
        fi
        sleep 2
    done

    echo "  [icd10-import] Waiting for embedding service..."
    for i in $$(seq 1 120); do
        if curl -sf http://localhost:8000/health >/dev/null 2>&1; then
            echo "  [icd10-import] Embedding service ready."
            break
        fi
        sleep 2
    done

    local CHAPTERS
    CHAPTERS=$$(curl -sf http://localhost:5090/api/icd10/chapters 2>/dev/null || echo "[]")
    if [ "$$CHAPTERS" = "[]" ] || [ "$$CHAPTERS" = "" ]; then
        echo "  [icd10-import] No ICD10 data found. Running full Postgres import..."
        EMBEDDING_SERVICE_URL="http://localhost:8000" \
            "$$VENV_DIR/bin/python" "$$SCRIPTS_DIR/import_postgres.py" \
            --connection-string "$$CONN_STR" \
            || echo "  [icd10-import] Import encountered errors (check logs above)"
    else
        echo "  [icd10-import] ICD10 codes already populated. Generating missing embeddings..."
        EMBEDDING_SERVICE_URL="http://localhost:8000" \
            "$$VENV_DIR/bin/python" "$$SCRIPTS_DIR/import_postgres.py" \
            --connection-string "$$CONN_STR" --embeddings-only \
            || echo "  [icd10-import] Embedding generation encountered errors"
    fi
}

echo "Starting Gatekeeper.Api on :5002..."
ConnectionStrings__Postgres="Host=localhost;Database=gatekeeper;Username=gatekeeper;Password=$$DB_PASS" \
    dotnet run --no-build --project Gatekeeper/Gatekeeper.Api/Gatekeeper.Api.csproj --no-launch-profile \
    --urls "http://localhost:5002" 2>&1 | sed 's/^/  [gatekeeper] /' &
PIDS+=($$!)

echo "Starting Clinical.Api on :5080..."
ConnectionStrings__Postgres="Host=localhost;Database=clinical;Username=clinical;Password=$$DB_PASS" \
    dotnet run --no-build --project Clinical/Clinical.Api/Clinical.Api.csproj --no-launch-profile \
    --urls "http://localhost:5080" 2>&1 | sed 's/^/  [clinical]   /' &
PIDS+=($$!)

echo "Starting Scheduling.Api on :5001..."
ConnectionStrings__Postgres="Host=localhost;Database=scheduling;Username=scheduling;Password=$$DB_PASS" \
    dotnet run --no-build --project Scheduling/Scheduling.Api/Scheduling.Api.csproj --no-launch-profile \
    --urls "http://localhost:5001" 2>&1 | sed 's/^/  [scheduling] /' &
PIDS+=($$!)

echo "Starting ICD10.Api on :5090..."
ConnectionStrings__Postgres="Host=localhost;Database=icd10;Username=icd10;Password=$$DB_PASS" \
    dotnet run --no-build --project ICD10/ICD10.Api/ICD10.Api.csproj --no-launch-profile \
    --urls "http://localhost:5090" 2>&1 | sed 's/^/  [icd10]      /' &
PIDS+=($$!)

echo "Starting Dashboard on :5173..."
python3 -m http.server 5173 --directory Dashboard/Dashboard.Web/wwwroot 2>&1 | sed 's/^/  [dashboard]  /' &
PIDS+=($$!)

populate_icd10 &
PIDS+=($$!)

echo ""
echo "════════════════════════════════════════"
echo "  Gatekeeper:  http://localhost:5002"
echo "  Clinical:    http://localhost:5080"
echo "  Scheduling:  http://localhost:5001"
echo "  ICD10:       http://localhost:5090"
echo "  Embedding:   http://localhost:8000"
echo "  Dashboard:   http://localhost:5173"
echo "════════════════════════════════════════"
echo "  Press Ctrl+C to stop all services"
echo ""

wait
endef
export START_LOCAL_RUNNER

## start-local: Run all APIs locally against the docker Postgres dev DB
##   Builds projects in Debug, dashboard in Release, then runs everything in
##   the foreground with prefixed log output. Ctrl+C cleans up all children.
start-local: db-up
	@echo "==> Setting up Python environment..."
	@if [ ! -d ICD10/.venv ]; then python3 -m venv ICD10/.venv; fi
	@ICD10/.venv/bin/pip install -q -r ICD10/embedding-service/requirements.txt psycopg2-binary click requests
	@echo "==> Building all projects..."
	dotnet build Gatekeeper/Gatekeeper.Api/Gatekeeper.Api.csproj --nologo -v q
	dotnet build Clinical/Clinical.Api/Clinical.Api.csproj --nologo -v q
	dotnet build Scheduling/Scheduling.Api/Scheduling.Api.csproj --nologo -v q
	dotnet build ICD10/ICD10.Api/ICD10.Api.csproj --nologo -v q
	dotnet build Dashboard/Dashboard.Web/Dashboard.Web.csproj -c Release --nologo -v q
	@bash -c "$$START_LOCAL_RUNNER"
