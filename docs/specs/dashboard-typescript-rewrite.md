# Dashboard TypeScript Specification

<!-- agent-pmo:2efd847 -->

## [DASH-TS-SCOPE]

The dashboard is a TypeScript, React, and Vite single-page application at
`Dashboard/dashboard-ts`.

There is no C# dashboard project. The only supported dashboard build is:

```bash
cd Dashboard/dashboard-ts
pnpm check
```

## [DASH-TS-CI]

CI validates the dashboard through Makefile targets:

- `make fmt CHECK=1` runs TypeScript format checks.
- `make lint` runs TypeScript typecheck, lint, and format checks.
- `make test` runs TypeScript unit tests.
- `make build` builds the Vite production bundle.

## [DASH-TS-DOCKER]

`docker/Dockerfile.dashboard` builds `Dashboard/dashboard-ts` with pnpm and
serves the generated `dist/` assets with nginx on port 5173.

## [DASH-TS-E2E]

Browser E2E specs live under `Dashboard/dashboard-ts/e2e`. They expect the
full stack and Vite dashboard to be running on localhost.
