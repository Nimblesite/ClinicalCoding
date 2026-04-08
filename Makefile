# agent-pmo:29b9dcf
# =============================================================================
# Standard Makefile — HealthcareSamples
# Cross-platform: Linux, macOS, Windows (via GNU Make)
# =============================================================================

.PHONY: build test lint fmt fmt-check clean check ci coverage coverage-check setup db-up db-down db-reset db-wait db-migrate

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
# project gets its own minimum line-rate; bump them via `make coverage-check`
# output minus 1 percentage point (rounding margin).
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
##   - Each project's coverage lands under TestResults/<project-dir>/ so
##     `make coverage-check` can attribute results back to a project.
test: db-migrate
	@echo "==> Testing (fail-fast)..."
	@set -e; \
	rm -rf TestResults; \
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
	done

## lint: Run all linters (fails on any warning)
lint: fmt-check db-migrate
	@echo "==> Linting..."
	dotnet build HealthcareSamples.sln --configuration Release

## fmt: Format all code in-place
fmt:
	@echo "==> Formatting..."
	dotnet csharpier format .

## fmt-check: Check formatting without modifying
fmt-check:
	@echo "==> Checking format..."
	dotnet csharpier check .

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

## check: lint + test (pre-commit)
check: lint test

## ci: lint + test + build (full CI simulation)
ci: lint test build

## coverage: Generate coverage report
coverage:
	@echo "==> Coverage report..."
	reportgenerator \
	  -reports:"TestResults/**/coverage.cobertura.xml" \
	  -targetdir:coverage/html \
	  -reporttypes:Html
	@echo "==> HTML report: coverage/html/index.html"

## coverage-check: Assert per-project line-rate >= threshold from $(COVERAGE_THRESHOLDS_FILE)
##   The JSON file declares { "default_threshold": N, "projects": { "<dir>": { "threshold": N } } }.
##   A project that is missing from the file inherits "default_threshold".
##   When coverage actually goes UP, edit the file: floor(measured) - 1 to leave a rounding cushion.
coverage-check:
	@echo "==> Checking coverage thresholds (file: $(COVERAGE_THRESHOLDS_FILE))..."
	@command -v jq >/dev/null 2>&1 || { echo "FAIL: jq is required (brew install jq / apt-get install jq)"; exit 1; }
	@if [ ! -f "$(COVERAGE_THRESHOLDS_FILE)" ]; then \
	  echo "FAIL: $(COVERAGE_THRESHOLDS_FILE) not found"; exit 1; \
	fi
	@set -e; \
	default=$$(jq -r '.default_threshold' $(COVERAGE_THRESHOLDS_FILE)); \
	any_failed=0; \
	for proj in $(TEST_PROJECTS); do \
	  proj_dir=$$(dirname "$$proj"); \
	  cobertura=$$(find "TestResults/$$proj_dir" -name 'coverage.cobertura.xml' 2>/dev/null | head -1); \
	  threshold=$$(jq -r --arg p "$$proj_dir" --arg d "$$default" '.projects[$$p].threshold // ($$d | tonumber)' $(COVERAGE_THRESHOLDS_FILE)); \
	  if [ -z "$$cobertura" ]; then \
	    echo "FAIL ($$proj_dir): no coverage.cobertura.xml under TestResults/$$proj_dir"; \
	    any_failed=1; continue; \
	  fi; \
	  line_rate=$$(awk 'match($$0, /line-rate="[0-9.]+"/) { s=substr($$0, RSTART+11, RLENGTH-12); print s; exit }' "$$cobertura"); \
	  pct=$$(awk "BEGIN{printf \"%.1f\", $${line_rate:-0}*100}"); \
	  pct_int=$$(awk "BEGIN{printf \"%d\", $${line_rate:-0}*100}"); \
	  if [ "$$pct_int" -lt "$$threshold" ]; then \
	    printf "FAIL %-44s %s%% < %s%%\n" "$$proj_dir" "$$pct" "$$threshold"; \
	    any_failed=1; \
	  else \
	    printf "OK   %-44s %s%% >= %s%%\n" "$$proj_dir" "$$pct" "$$threshold"; \
	  fi; \
	done; \
	if [ "$$any_failed" -ne 0 ]; then \
	  echo ""; \
	  echo "FAIL: one or more projects below threshold (see $(COVERAGE_THRESHOLDS_FILE))"; \
	  exit 1; \
	fi; \
	echo ""; \
	echo "OK: all projects meet their coverage thresholds"

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
# HELP
# =============================================================================
help:
	@echo "Available targets:"
	@echo "  build          - Compile/assemble all artifacts"
	@echo "  test           - Run full test suite with coverage"
	@echo "  lint           - Run all linters (errors mode)"
	@echo "  fmt            - Format all code in-place"
	@echo "  fmt-check      - Check formatting (no modification)"
	@echo "  clean          - Remove build artifacts"
	@echo "  check          - lint + test (pre-commit)"
	@echo "  ci             - lint + test + build (full CI)"
	@echo "  coverage       - Generate and open coverage report"
	@echo "  coverage-check - Assert coverage thresholds"
	@echo "  setup          - Post-create dev environment setup"
