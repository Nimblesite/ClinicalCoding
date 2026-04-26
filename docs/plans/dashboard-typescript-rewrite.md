# Dashboard TypeScript Plan

<!-- agent-pmo:2efd847 -->

## [DASH-TS-PLAN-CURRENT]

The dashboard codebase is `Dashboard/dashboard-ts`.

Completed cleanup:

- Removed the C# dashboard project from the solution.
- Removed the C# dashboard integration test project from the solution and
  coverage thresholds.
- Removed the dashboard compiler tool from `.config/dotnet-tools.json`.
- Rewired Makefile, CI, and Docker to build the TypeScript dashboard.

## [DASH-TS-PLAN-VERIFY]

Required verification:

- `dotnet restore`
- `dotnet build HealthcareSamples.sln --configuration Release --no-restore`
- `make fmt CHECK=1`
- `make lint`
- `make test`
- `make build`

`make lint`, `make test`, and `make build` require Docker because the .NET
database code generation path depends on migrated Postgres schemas.
