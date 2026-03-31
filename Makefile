# agent-pmo:d58c330
# =============================================================================
# Standard Makefile — HealthcareSamples
# Cross-platform: Linux, macOS, Windows (via GNU Make)
# =============================================================================

.PHONY: build test lint fmt fmt-check clean check ci coverage coverage-check setup

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

# =============================================================================
# PRIMARY TARGETS
# =============================================================================

## build: Compile/assemble all artifacts
build:
	@echo "==> Building..."
	dotnet build HealthcareSamples.sln --configuration Release

## test: Run full test suite with coverage
test:
	@echo "==> Testing..."
	dotnet test HealthcareSamples.sln --configuration Release \
	  --settings coverlet.runsettings \
	  --collect:"XPlat Code Coverage" \
	  --results-directory TestResults \
	  --verbosity normal

## lint: Run all linters (fails on any warning)
lint: fmt-check
	@echo "==> Linting..."
	dotnet build HealthcareSamples.sln --configuration Release

## fmt: Format all code in-place
fmt:
	@echo "==> Formatting..."
	dotnet csharpier .

## fmt-check: Check formatting without modifying
fmt-check:
	@echo "==> Checking format..."
	dotnet csharpier . --check

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
	LINE_RATE=$$(grep -oP 'line-rate="\K[^"]+' "$$COBERTURA" | head -1); \
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
	dotnet restore
	dotnet tool restore
	@echo "==> Setup complete. Run 'make ci' to validate."

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
