# Dashboard.Web — TypeScript Rewrite Implementation Plan

**Status:** EXECUTING — Director driving, Cline as research-only
**Spec:** `docs/specs/dashboard-typescript-rewrite.md`
**Section ID prefix:** `[DASH-TS-PLAN-*]`

## [DASH-TS-PLAN-COEXIST] PIVOT — coexistence mode

User instruction (overrides original Phase 0): **DO NOT delete the C#
code.** The legacy `Dashboard/Dashboard.Web/` (H5) and
`Dashboard/Dashboard.Integration.Tests/` projects stay on disk and
remain in the .sln. The TypeScript rewrite lives at a parallel path
`Dashboard/dashboard-ts/`. The two coexist until the user explicitly
green-lights the legacy delete (post-parity).

## [DASH-TS-PLAN-TODO] Live TODO checklist

User answers to open questions (locked in): cognitive complexity 15,
**TS path is `Dashboard/dashboard-ts/`** (NOT `Dashboard.Web`),
`functional/no-let` ON, `no-default-export` ON with entry-point override,
`Dashboard.Integration.Tests/` PRESERVED.

- [x] **Phase 0** — Coexistence wiring (NO deletes)
  - [x] Create `Dashboard/dashboard-ts/` (Cline scaffolded)
  - [x] Add Makefile targets `dashboard-ts-build/dev/test` alongside legacy
  - [x] Update `.gitignore` for `Dashboard/dashboard-ts/{node_modules,dist,coverage}`
  - [x] Update `CLAUDE.md` repo structure to mention both
  - [ ] Commit `feat(dashboard): scaffold parallel TS rewrite`
- [x] **Phase 1** — Scaffold Vite+TS+React, typecheck + build GREEN
- [x] **Phase 2** — Foundation: api/{client,config,clinical,scheduling,icd10,gatekeeper}, auth/{storage,context,webauthn}, lib/{logger,error-handlers}, types/{auth,fhir,icd10}, 12 unit tests GREEN
- [x] **Phase 3** — Shell: AuthProvider, hash router, LoginPage (passkey), Sidebar, Header, AppShell, route guard
- [x] **Phase 4** — Read-only pages: dashboard, patients, practitioners, appointments, calendar + use-* hooks
- [x] **Phase 5** — Edit pages: edit-patient, edit-appointment with RHF + Zod + mutations
- [x] **Phase 6** — Clinical Coding: 3-mode search (AI/keyword/lookup), ACHI toggle, result list, detail panel, copy button
- [ ] **Phase 7** — Final `pnpm check` + parity + commit
- [ ] **Phase 1** — Scaffold Vite+TS+React, lints clean on Hello world
- [ ] **Phase 2** — Foundation: api/auth/lib/types + 100% unit coverage
- [ ] **Phase 3** — Shell: routing + auth gate + real passkey login
- [ ] **Phase 4** — Read-only pages (dashboard, patients, practitioners, appts, calendar)
- [ ] **Phase 5** — Edit pages (patient, appointment) with RHF+Zod
- [ ] **Phase 6** — Clinical Coding screen (semantic, keyword, lookup)
- [ ] **Phase 7** — Parity check + cleanup + final commit

Cline ticks boxes as work lands. Director reviews ticks against
verification gates before unlocking the next phase.

This plan executes the spec top to bottom. Each phase ends in a green
state (typecheck + lint + tests + build all pass). No phase is "skipped
ahead" — phase N+1 does not start until phase N is verified.

## [DASH-TS-PLAN-PHASE0] Phase 0 — Clean break

**Goal:** Repo no longer contains any trace of C#, H5, or the legacy
dashboard project.

**Steps:**

1. Delete files:
   - `Dashboard/Dashboard.Web/` entire directory
   - `Dashboard/Dashboard.Integration.Tests/` entire directory
   - All `*.png` files in repo root left over from Playwright debugging
     (`dashboard-*.png`, `ccp-*.png`, `patients-*.png`, `login-*.png`,
     `after-edit-*.png`)
2. Remove `Dashboard.Web` and `Dashboard.Integration.Tests` entries from
   any `.sln` file.
3. Search for and remove every reference to:
   - `H5`, `h5.json`, `h5.target`, `h5-compiler`
   - `Dashboard.Web.csproj`
   - `dotnet build` paths under `Dashboard/`
4. Update `Makefile` so dashboard targets call `pnpm` instead of `dotnet`.
5. Update `docker/` Dockerfiles and compose files so the dashboard
   service builds from `pnpm`.
6. Update `CLAUDE.md` to remove "(H5 transpiler C# -> JS)" and replace
   with "(TypeScript + React + Vite)".
7. Update `.gitignore` to add:
   ```
   # Dashboard build outputs
   Dashboard/Dashboard.Web/dist/
   Dashboard/Dashboard.Web/node_modules/
   Dashboard/Dashboard.Web/.vite/
   Dashboard/Dashboard.Web/coverage/
   Dashboard/Dashboard.Web/playwright-report/
   Dashboard/Dashboard.Web/test-results/

   # Debug screenshots
   *.png
   !docs/**/*.png
   ```
8. Commit message: `chore(dashboard): remove legacy H5 C# project`.

**Verification:**
- `git grep -i "h5\b"` returns nothing under `Dashboard/`
- `git grep "Dashboard.Web.csproj"` returns nothing
- `find Dashboard -name '*.cs'` returns nothing
- `find . -maxdepth 1 -name '*.png'` returns nothing
- `make build` succeeds (now a no-op for the dashboard since the new
  project does not exist yet)

**User checkpoint:** "Yes, blow it away" required before executing.

## [DASH-TS-PLAN-PHASE1] Phase 1 — Scaffold

**Goal:** A new TypeScript+React+Vite project at `Dashboard/Dashboard.Web/`
that builds, lints, and serves a "Hello" page.

**Steps:**

1. `cd Dashboard && pnpm create vite@latest Dashboard.Web -- --template react-ts`
2. `cd Dashboard.Web && pnpm install`
3. Replace `tsconfig.json` and `tsconfig.node.json` with the strict
   config from the spec ([DASH-TS-TSCONFIG]).
4. Add dev dependencies (one command):
   ```
   pnpm add -D \
     @eslint/js typescript-eslint \
     eslint-plugin-react eslint-plugin-react-hooks eslint-plugin-react-refresh \
     eslint-plugin-jsx-a11y eslint-plugin-import-x eslint-plugin-unicorn \
     eslint-plugin-promise eslint-plugin-sonarjs eslint-plugin-no-secrets \
     eslint-plugin-eslint-comments eslint-plugin-functional \
     eslint-plugin-regexp eslint-plugin-security eslint-plugin-deprecation \
     eslint-plugin-perfectionist eslint-plugin-tsdoc \
     eslint-plugin-vitest eslint-plugin-playwright \
     prettier prettier-plugin-organize-imports \
     vitest @vitest/coverage-v8 @testing-library/react @testing-library/jest-dom \
     @playwright/test \
     jsdom
   ```
5. Add runtime dependencies:
   ```
   pnpm add react react-dom react-router-dom @tanstack/react-query \
     react-hook-form @hookform/resolvers zod
   ```
6. Create `eslint.config.ts` with the full rule set from
   [DASH-TS-LINT-RULES]. Start from the user's baseline; layer the
   additional rules on top.
7. Create `.prettierrc.cjs` and `.prettierignore`.
8. Create `vitest.config.ts` with `environment: 'jsdom'` and a `setupFiles`
   pointing at `src/test/setup.ts`.
9. Create `playwright.config.ts` pointing at `http://localhost:5173`.
10. Create `src/main.tsx`, `src/App.tsx` with minimal "Hello" content.
11. Copy the existing `wwwroot/css/{variables,base,components}.css` over
    to `src/styles/` and import them from `main.tsx`. (This is the only
    artifact preserved from the deleted project.)
12. Copy `wwwroot/img/` to `public/img/`.
13. Add `package.json` scripts:
    ```jsonc
    {
      "scripts": {
        "dev": "vite",
        "build": "tsc -b && vite build",
        "preview": "vite preview",
        "typecheck": "tsc --noEmit",
        "lint": "eslint . --max-warnings=0",
        "format": "prettier --check .",
        "format:fix": "prettier --write .",
        "test": "vitest run",
        "test:watch": "vitest",
        "test:coverage": "vitest run --coverage",
        "e2e": "playwright test",
        "check": "pnpm typecheck && pnpm lint && pnpm format && pnpm test && pnpm build"
      }
    }
    ```
14. Update `Dashboard/Dashboard.Web/Dockerfile` to a multi-stage build:
    `node:20-alpine` → `pnpm install && pnpm build` → `nginx:alpine` serving `dist/`.

**Verification:**
- `pnpm dev` shows Hello page at http://localhost:5173
- `pnpm check` is green
- `docker compose build dashboard` succeeds
- `docker compose up dashboard` serves the Hello page
- `pnpm lint` reports zero warnings (the Vite template files must be
  rewritten or deleted to pass the strict ruleset)

**User checkpoint:** "Vite + nginx serves the empty app, lints clean."

## [DASH-TS-PLAN-PHASE2] Phase 2 — Foundation (no UI)

**Goal:** All non-UI infrastructure: API client, auth, types, logger.
Pure TypeScript modules with full unit test coverage.

**Files created:**

```
src/
  api/
    client.ts          # apiFetch + ApiError
    client.test.ts
    gatekeeper.ts      # loginWithPasskey, registerWithPasskey, logout
    gatekeeper.test.ts
    clinical.ts        # getPatients, getPatient, createPatient, updatePatient, ...
    clinical.test.ts
    scheduling.ts      # getPractitioners, getAppointments, ...
    scheduling.test.ts
    icd10.ts           # getChapters, semanticSearch, ...
    icd10.test.ts
  auth/
    auth-context.tsx   # AuthProvider + useAuth hook
    auth-storage.ts    # localStorage helpers
    auth-storage.test.ts
    webauthn.ts        # base64url + ArrayBuffer helpers
    webauthn.test.ts
  lib/
    logger.ts
    logger.test.ts
    install-error-handlers.ts  # window.onerror, unhandledrejection
  types/
    fhir.ts            # Patient, Practitioner, Appointment, ...
    icd10.ts           # Icd10Code, AchiCode, SemanticSearchResult, ...
    auth.ts            # AuthUser, PasskeyAuthResult
```

**Steps:**

1. Implement `webauthn.ts` with base64url encode/decode (no `Buffer`,
   pure TypedArray + `btoa`/`atob`).
2. Implement `apiFetch` per [DASH-TS-API], including the `Content-Type`
   guard (only when body is present), `ApiError` class, and 401 handling.
3. Implement `gatekeeper.ts` `loginWithPasskey()` / `registerWithPasskey()`
   / `logout()`.
4. Implement `clinical.ts` / `scheduling.ts` / `icd10.ts` as thin typed
   wrappers — one function per endpoint, no logic.
5. Implement `auth-storage.ts` / `auth-context.tsx`.
6. Implement `logger.ts` and `install-error-handlers.ts`.
7. Write Vitest unit tests for every module. **Coverage must hit
   100% for these foundation files** before phase 3 starts.

**Verification:**
- `pnpm test:coverage` → 100% line + branch on `src/api`, `src/auth`,
  `src/lib`
- `pnpm check` is green
- A manual `curl` against the running stack confirms the typed wrappers
  produce identical request bodies to the legacy dashboard

## [DASH-TS-PLAN-PHASE3] Phase 3 — Shell

**Goal:** Routing, auth gate, sidebar, header, login page. The dashboard
shell renders, the user can sign in with a real passkey, and the user
can sign out.

**Files created:**

```
src/
  App.tsx                      # router root + auth gate
  routes.tsx                   # createHashRouter config
  components/
    sidebar.tsx
    sidebar.test.tsx
    header.tsx
    header.test.tsx
    icons.tsx                  # all SVGs as TS components
    modal.tsx
    modal.test.tsx
    data-table.tsx
    data-table.test.tsx
    error-boundary.tsx
  pages/
    login-page.tsx
    login-page.test.tsx
    dashboard-page.tsx         # placeholder for now
    not-found-page.tsx
```

**Steps:**

1. `App.tsx` mounts `<AuthProvider>` and `<RouterProvider>`.
2. `routes.tsx` defines a hash router with a top-level loader that
   redirects to `/login` when `auth.isAuthenticated` is false.
3. `login-page.tsx` calls `loginWithPasskey()` / `registerWithPasskey()`.
4. `sidebar.tsx` renders the nav sections and the sign-out footer using
   real `currentUser` from `useAuth()`.
5. `header.tsx` renders the title bar + global search input.
6. Component tests use `@testing-library/react` and mock the auth context.

**Verification:**
- `pnpm dev` → log in with a real passkey → see empty dashboard shell
  with username in sidebar → click sign out → return to login
- `pnpm check` is green
- Playwright smoke test: `tests/e2e/login.spec.ts` logs in via the
  WebAuthn virtual authenticator and asserts the dashboard renders

**User checkpoint:** "Login works in the browser with a real passkey."

## [DASH-TS-PLAN-PHASE4] Phase 4 — Read-only pages

**Goal:** Dashboard, Patients list, Practitioners list, Appointments list,
Calendar pages render real data via TanStack Query.

**Files created:**

```
src/pages/
  dashboard-page.tsx          # full implementation
  dashboard-page.test.tsx
  patients-page.tsx
  patients-page.test.tsx
  practitioners-page.tsx
  practitioners-page.test.tsx
  appointments-page.tsx
  appointments-page.test.tsx
  calendar-page.tsx
  calendar-page.test.tsx
src/hooks/
  use-patients.ts             # wraps useQuery + clinical.getPatients
  use-practitioners.ts
  use-appointments.ts
```

**Steps:**

1. Wrap `App.tsx` with `<QueryClientProvider>`.
2. Implement each `use-*.ts` hook as a thin typed wrapper around
   `useQuery`.
3. Implement each page component using only those hooks — no `fetch`,
   no `useEffect` data fetching.
4. Tests mock the React Query client and assert loading / error / success
   states.

**Verification:**
- All counts and lists in the dashboard match what the legacy app showed
- Network tab confirms each request uses the bearer token
- `pnpm check` is green

## [DASH-TS-PLAN-PHASE5] Phase 5 — Edit pages

**Goal:** Edit Patient and Edit Appointment pages with form state +
mutations.

**Files created:**

```
src/pages/
  edit-patient-page.tsx
  edit-patient-page.test.tsx
  edit-appointment-page.tsx
  edit-appointment-page.test.tsx
src/hooks/
  use-update-patient.ts       # wraps useMutation + clinical.updatePatient
  use-update-appointment.ts
src/forms/
  patient-form.tsx            # React Hook Form + Zod schema
  appointment-form.tsx
  schemas.ts                  # zod schemas
```

**Steps:**

1. Define Zod schemas mirroring the FHIR types.
2. Build forms with React Hook Form.
3. Mutations invalidate the query cache on success so list pages refresh.
4. Tests cover validation errors, submit success, and submit failure.

**Verification:**
- Edit a patient → save → return to list → see new value
- Validation errors block submission with inline messages
- `pnpm check` is green

## [DASH-TS-PLAN-PHASE6] Phase 6 — Clinical Coding (the screen that matters)

**Goal:** Full Clinical Coding search page with semantic, keyword, and
lookup modes.

**Files created:**

```
src/pages/
  clinical-coding-page.tsx
  clinical-coding-page.test.tsx
src/components/
  coding/
    search-bar.tsx
    mode-tabs.tsx
    results-list.tsx
    code-detail-panel.tsx
    achi-toggle.tsx
    copy-button.tsx
src/hooks/
  use-icd10-search.ts
  use-icd10-lookup.ts
  use-semantic-search.ts
```

**Steps:**

1. Implement the three search modes via three separate hooks. Tab
   switching just selects which hook to call.
2. Results list virtualizes long result sets (use `@tanstack/react-virtual`
   if necessary; otherwise plain rendering up to 200 items).
3. Code detail panel shows the selected code's description, chapter,
   block, related codes, and a copy-to-clipboard button.
4. ACHI toggle is a controlled checkbox that re-runs the search.
5. Tests cover each search mode against a mocked icd10 client.

**Verification:**
- "AI Search" with `chest pain` returns ranked semantic results
- "Keyword Search" with `R07` returns prefix matches
- "Code Lookup" with `R07.4` returns the exact code or a fallback list
- Selecting a result shows the detail panel
- Copy button copies the code to the clipboard
- `pnpm check` is green
- Playwright E2E test exercises all three modes

**User checkpoint:** "Clinical Coding screen has full parity with the
legacy app."

## [DASH-TS-PLAN-PHASE7] Phase 7 — Parity check + cleanup

**Goal:** Visual + functional parity with the legacy app, dead code
removed, docs updated.

**Steps:**

1. Run a side-by-side visual comparison against the original screenshots
   the user provided. File issues for any visual regressions.
2. Run the full Playwright suite against a staging deploy.
3. Delete any unused CSS classes (run `pnpm exec stylelint` or a
   purge tool to find them).
4. Update `CLAUDE.md` `Repo Structure` section to describe the new
   stack accurately.
5. Update `README.md` (if any) under `Dashboard/Dashboard.Web/`.
6. Final commit: `feat(dashboard): TypeScript rewrite parity complete`.

**Verification:**
- Every legacy feature works in the new app
- `pnpm check` is green
- `playwright test` is green
- `git grep -i "h5\b"` returns nothing anywhere in the repo

## [DASH-TS-PLAN-RISKS]

| Risk | Mitigation |
|---|---|
| WebAuthn ceremony JSON shape mismatch with `fido2-net-lib` | Phase 2 unit tests load the exact `OptionsJson` shapes captured from a real working session and assert byte-for-byte that the encoded request matches |
| TanStack Query cache invalidation bugs | Phase 5 tests cover the invalidation matrix; default `staleTime: 0` to be conservative |
| 100% coverage demand vs reality | Coverage gate set at 95% line / 90% branch initially; raised to 100% after parity in Phase 7 |
| Lint config too strict to ship | Phase 1 has a "make lint pass on a Hello world" step; if a rule blocks every file, it's reconsidered then, not later |
| docker-compose service name change | Keep service name `dashboard` and the published port `5173` so other compose entries don't change |
| Existing E2E tests in `Dashboard.Integration.Tests` lost | They tested the dead C# app; equivalent tests are written fresh in Playwright per phase |

## [DASH-TS-PLAN-USER-CHECKPOINTS]

The user is asked to confirm at exactly these moments and **only** these
moments:

1. Before Phase 0 deletes anything: "blow it away?"
2. After Phase 1: "shell builds and lints clean?"
3. After Phase 3: "real passkey login works?"
4. After Phase 6: "Clinical Coding has parity?"

Between checkpoints I push through without asking permission for each
file edit.
