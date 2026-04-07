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

# Coverage threshold (override in CI via env var or per-repo)
COVERAGE_THRESHOLD ?= 80

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
test: db-migrate
	@echo "==> Testing (fail-fast)..."
	@set -e; \
	for proj in $(TEST_PROJECTS); do \
	  echo ""; \
	  echo "==> Testing $$proj"; \
	  dotnet test "$$proj" --configuration Release \
	    --settings coverlet.runsettings \
	    --collect:"XPlat Code Coverage" \
	    --results-directory TestResults \
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

## coverage-check: Assert thresholds (exits non-zero if below)
coverage-check:
	@echo "==> Checking coverage thresholds..."
	@COBERTURA=$$(find TestResults -name 'coverage.cobertura.xml' | head -1); \
	if [ -z "$$COBERTURA" ]; then echo "FAIL: No coverage.cobertura.xml found"; exit 1; fi; \
	LINE_RATE=$$(awk 'match($$0, /line-rate="[0-9.]+"/) { s=substr($$0, RSTART+11, RLENGTH-12); print s; exit }' "$$COBERTURA"); \
	PCT=$$(awk "BEGIN{printf \"%.1f\", $${LINE_RATE:-0}*100}"); \
	PCT_INT=$$(awk "BEGIN{printf \"%d\", $${LINE_RATE:-0}*100}"); \
	echo "Line coverage: $${PCT}% (threshold: $(COVERAGE_THRESHOLD)%)"; \
	if [ "$$PCT_INT" -lt "$(COVERAGE_THRESHOLD)" ]; then \
	  echo "FAIL: $${PCT}% < $(COVERAGE_THRESHOLD)%"; exit 1; \
	else \
	  echo "OK: $${PCT}% >= $(COVERAGE_THRESHOLD)%"; \
	fi

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

## db-migrate: Ensure DB is up and apply YAML schemas via migration-cli to all four databases
db-migrate: db-up
	@echo "==> Migrating Postgres schemas..."
	dotnet migration-cli --schema Gatekeeper/Gatekeeper.Api/gatekeeper-schema.yaml \
	  --output "$(PG_BASE_URL);Database=gatekeeper" --provider postgres
	dotnet migration-cli --schema Clinical/Clinical.Api/clinical-schema.yaml \
	  --output "$(PG_BASE_URL);Database=clinical" --provider postgres
	dotnet migration-cli --schema Scheduling/Scheduling.Api/scheduling-schema.yaml \
	  --output "$(PG_BASE_URL);Database=scheduling" --provider postgres
	dotnet migration-cli --schema ICD10/ICD10.Api/icd10-schema.yaml \
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
