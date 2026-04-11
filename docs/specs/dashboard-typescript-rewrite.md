# Dashboard.Web — TypeScript Rewrite Specification

**Status:** Draft — awaiting sign-off
**Author:** Rewrite of legacy H5/C# dashboard
**Owner:** Dashboard team
**Section ID prefix:** `[DASH-TS-*]`

## [DASH-TS-OVERVIEW]

The existing `Dashboard/Dashboard.Web/` project is a C# codebase transpiled
to JavaScript via the H5 transpiler. H5 is unmaintained, lags the .NET BCL,
has no debugger story, and forced multiple bugs that wasted hours
(missing `Content-Type` header from anonymous-property-name emission,
auth flow lost during a refactor because the source of truth was a
3,623-line `index.html`, etc.).

This spec describes a **clean-break rewrite** of `Dashboard.Web` in modern
TypeScript with React 18, Vite, and an aggressive lint configuration.
**No C# or H5 artifact will remain.** Not a file, not a config flag, not a
comment. The replacement is a static SPA built with `pnpm build`, served
by nginx from `dist/`, and consuming the existing Clinical, Scheduling,
ICD-10, and Gatekeeper APIs unchanged.

## [DASH-TS-SCOPE]

### In scope (feature parity with current dashboard)

| ID | Feature |
|---|---|
| `[DASH-TS-AUTH]` | Passkey login + register via Gatekeeper, sign-out, auth gate, token persistence |
| `[DASH-TS-DASHBOARD]` | Dashboard overview page (metric cards, upcoming appointments, requests, quick actions) |
| `[DASH-TS-PATIENTS]` | Patients list, search, edit |
| `[DASH-TS-PRACTITIONERS]` | Practitioners list |
| `[DASH-TS-APPOINTMENTS]` | Appointments list, edit |
| `[DASH-TS-CALENDAR]` | Calendar / Schedule view |
| `[DASH-TS-CODING]` | **Clinical Coding search** — AI semantic, keyword, and code-lookup tabs; results panel; ACHI toggle; code detail panel; copy-to-clipboard. **Highest priority after auth.** |
| `[DASH-TS-SHELL]` | Sidebar nav, header with search box, modals, data tables, icons |
| `[DASH-TS-ROUTING]` | Hash-based deep linking (`#/patients/edit/:id` etc.) so URLs survive reloads |
| `[DASH-TS-LOGGING]` | Browser-side logger that surfaces fetch errors with full URL + body, hooks `window.onerror` and `unhandledrejection` |

### Out of scope for v1

- Encounters, Conditions, Medications, Settings pages — stay as placeholders
  matching the current dashboard.
- Server-side rendering. This is an internal admin UI.
- Migration of `Dashboard.Integration.Tests` (a .NET test project) — it
  tests the dead C# codebase and will be deleted; Playwright tests are
  written fresh post-parity.

## [DASH-TS-NON-FUNCTIONAL]

### [DASH-TS-NF-LANG] Language

- TypeScript only. **Zero JavaScript files in `src/`.** Build configs and
  ESLint configs may live in `.ts` (`vite.config.ts`, `eslint.config.ts`,
  `vitest.config.ts`).
- `tsconfig.json` enables every strictness flag the compiler offers
  (see [DASH-TS-TSCONFIG]).

### [DASH-TS-NF-LINT] Lint posture

The codebase must pass an aggressive ESLint configuration with **zero
warnings**. Disabling a rule requires an inline comment justifying it,
enforced by `eslint-comments/no-unused-disable` and `eslint-comments/require-description`.
See [DASH-TS-LINT] for the full rule list.

### [DASH-TS-NF-TESTS] Tests

- **Unit:** Vitest. Co-located `*.test.ts` files. Tests for every API
  helper, every pure function, and every reducer.
- **E2E:** Playwright. Black-box, against a live stack via docker-compose.
- **Coverage:** 100% goal per `CLAUDE.md`. No skipped tests.
- Tests are **deterministic** — no `setTimeout`-based waits, no random,
  no real time.

### [DASH-TS-NF-A11Y] Accessibility

- All interactive controls must have ARIA roles / labels.
- `eslint-plugin-jsx-a11y` runs in strict mode in CI.
- Color contrast and focus order verified by Playwright accessibility
  snapshots in the E2E suite.

### [DASH-TS-NF-PERF] Performance

- Initial bundle (gzipped) ≤ 200 kB excluding the React vendor chunk.
- Code-split per route via `React.lazy`.
- TanStack Query handles HTTP caching; no manual `useEffect` data fetching.

### [DASH-TS-NF-CSS] Styling

- Reuse the existing `variables.css`, `base.css`, `components.css`. They
  are already under the 1k LOC budget mandated by `CLAUDE.md`.
- **No CSS-in-JS, no Tailwind, no styled-components.** Plain CSS imported
  from `src/main.tsx`.

## [DASH-TS-STACK]

| Concern | Choice | Rationale |
|---|---|---|
| Language | **TypeScript 5.x** strict | mandated |
| UI library | **React 18** | matches existing component model and CSS class names |
| Build tool | **Vite 5.x** | fast HMR, native ESM, zero ceremony for TS+React |
| Package manager | **pnpm 9.x** | fast, content-addressed, lockfile-based |
| Routing | **React Router v6** with `createHashRouter` | preserves existing `#/...` deep links |
| Server state | **TanStack Query v5** | cache, retries, loading/error states; replaces bespoke `useFetch` |
| Local state | React `useState` / `useReducer` | no Redux, no Zustand |
| HTTP | native `fetch` wrapped in `src/api/client.ts` | one place for auth header, one place for `Content-Type`, one place for the 401-clear-and-redirect logic |
| Auth | localStorage (`gatekeeper_token`, `gatekeeper_user`) + `AuthContext` provider | matches the original working flow |
| Forms | React Hook Form + Zod | typed validation without ceremony |
| Tests (unit) | Vitest + @testing-library/react | |
| Tests (E2E) | Playwright | matches repo culture |
| Lint | ESLint 9 flat config + plugins (see [DASH-TS-LINT]) | |
| Format | Prettier 3 + `prettier-plugin-organize-imports` | |
| CI gate | `pnpm check` runs tsc + eslint + prettier + vitest + build | one command, all green or red |

## [DASH-TS-TSCONFIG]

`tsconfig.json` flags (all enabled, no exceptions):

```jsonc
{
  "compilerOptions": {
    "target": "ES2022",
    "module": "ESNext",
    "moduleResolution": "bundler",
    "lib": ["ES2022", "DOM", "DOM.Iterable"],
    "jsx": "react-jsx",

    // Strictness — every flag on
    "strict": true,
    "noImplicitAny": true,
    "strictNullChecks": true,
    "strictFunctionTypes": true,
    "strictBindCallApply": true,
    "strictPropertyInitialization": true,
    "alwaysStrict": true,
    "useUnknownInCatchVariables": true,

    // Beyond `strict`
    "noUncheckedIndexedAccess": true,
    "noImplicitOverride": true,
    "noFallthroughCasesInSwitch": true,
    "noImplicitReturns": true,
    "noUnusedLocals": true,
    "noUnusedParameters": true,
    "exactOptionalPropertyTypes": true,
    "forceConsistentCasingInFileNames": true,
    "verbatimModuleSyntax": true,
    "isolatedModules": true,
    "allowUnusedLabels": false,
    "allowUnreachableCode": false,
    "skipLibCheck": false,

    // Build
    "esModuleInterop": true,
    "resolveJsonModule": true,
    "noEmit": true
  },
  "include": ["src", "vite.config.ts", "vitest.config.ts", "eslint.config.ts"]
}
```

## [DASH-TS-LINT]

ESLint 9 flat config. The user-provided baseline (in the conversation
that produced this spec) is the **starting point**. The full set of
plugins and rules required:

### [DASH-TS-LINT-PLUGINS] Plugins

| Plugin | Purpose |
|---|---|
| `@eslint/js` | core JS recommended |
| `typescript-eslint` | `strictTypeChecked` + `stylisticTypeChecked` presets |
| `eslint-plugin-react` | `recommended` + `jsx-runtime` |
| `eslint-plugin-react-hooks` | `recommended-latest` |
| `eslint-plugin-react-refresh` | HMR safety |
| `eslint-plugin-jsx-a11y` | `strict` |
| `eslint-plugin-import-x` | TypeScript-aware import resolution, cycle detection |
| `eslint-plugin-unicorn` | `recommended` modern idioms |
| `eslint-plugin-promise` | `recommended-typescript` |
| `eslint-plugin-sonarjs` | cognitive complexity, duplicate code, dead branches |
| `eslint-plugin-no-secrets` | catches accidentally committed tokens |
| `eslint-plugin-eslint-comments` | enforces inline disable comments have descriptions |
| `eslint-plugin-functional` | `recommendedTypeChecked` — pushes toward immutability |
| `eslint-plugin-regexp` | regex correctness, perf, anti-ReDoS |
| `eslint-plugin-security` | XSS, eval, RCE smell tests |
| `eslint-plugin-deprecation` | flags use of `@deprecated` APIs |
| `eslint-plugin-perfectionist` | sorted imports, props, object keys |
| `eslint-plugin-tsdoc` | TSDoc syntax validation |
| `eslint-plugin-vitest` | test-only rules |
| `eslint-plugin-playwright` | E2E-only rules |

### [DASH-TS-LINT-RULES] Rule policy

Beyond plugin presets, **all** of the user's baseline rules are enabled
in `src/**/*.ts(x)`:

- All `@typescript-eslint/no-unsafe-*` → `error`
- `@typescript-eslint/no-explicit-any: error`
- `@typescript-eslint/no-non-null-assertion: error`
- `@typescript-eslint/strict-boolean-expressions` with **every** allow-flag
  set to `false`
- `@typescript-eslint/switch-exhaustiveness-check: error`
- `@typescript-eslint/no-floating-promises: error`
- `@typescript-eslint/no-misused-promises: error`
- `@typescript-eslint/explicit-function-return-type: error`
- `@typescript-eslint/explicit-member-accessibility: error`
- `@typescript-eslint/no-shadow: error`
- `@typescript-eslint/consistent-type-imports: error` (`fixStyle: inline-type-imports`)
- `@typescript-eslint/no-require-imports: error`
- `@typescript-eslint/prefer-readonly: error`
- `@typescript-eslint/require-array-sort-compare: error`
- `@typescript-eslint/promise-function-async: error`
- `@typescript-eslint/no-deprecated: error`
- `@typescript-eslint/consistent-type-assertions: { assertionStyle: 'never' }`
- `@typescript-eslint/no-unused-vars: error` (underscore prefix to ignore)
- `eqeqeq: ['error', 'always']`
- `no-param-reassign: error`
- `@typescript-eslint/consistent-type-definitions: ['error', 'interface']`
- `prefer-const: error`

**Additional rules turned ON beyond the baseline:**

- `@typescript-eslint/no-unnecessary-condition: error`
- `@typescript-eslint/no-unnecessary-type-arguments: error`
- `@typescript-eslint/no-unnecessary-type-assertion: error`
- `@typescript-eslint/no-unnecessary-boolean-literal-compare: error`
- `@typescript-eslint/prefer-nullish-coalescing: error`
- `@typescript-eslint/prefer-optional-chain: error`
- `@typescript-eslint/prefer-reduce-type-parameter: error`
- `@typescript-eslint/prefer-ts-expect-error: error`
- `@typescript-eslint/no-base-to-string: error`
- `@typescript-eslint/restrict-plus-operands: error`
- `@typescript-eslint/restrict-template-expressions: error`
- `@typescript-eslint/no-confusing-void-expression: error`
- `@typescript-eslint/no-meaningless-void-operator: error`
- `@typescript-eslint/no-redundant-type-constituents: error`
- `@typescript-eslint/no-useless-empty-export: error`
- `@typescript-eslint/method-signature-style: ['error', 'property']`
- `@typescript-eslint/array-type: ['error', { default: 'array-simple' }]`
- `react/jsx-no-leaked-render: error`
- `react/no-array-index-key: error`
- `react/no-unstable-nested-components: error`
- `react/jsx-key: ['error', { checkFragmentShorthand: true, warnOnDuplicates: true }]`
- `react/function-component-definition: ['error', { namedComponents: 'arrow-function' }]`
- `react-hooks/exhaustive-deps: error` (error, not warn)
- `import-x/no-cycle: error`
- `import-x/no-self-import: error`
- `import-x/no-useless-path-segments: error`
- `import-x/no-default-export: error` (named exports only, except for route components Vite expects)
- `import-x/no-extraneous-dependencies: error`
- `unicorn/filename-case: ['error', { case: 'kebabCase' }]`
- `unicorn/prefer-node-protocol: error`
- `unicorn/no-array-for-each: error`
- `unicorn/prefer-module: error`
- `unicorn/numeric-separators-style: error`
- `sonarjs/cognitive-complexity: ['error', 15]`
- `sonarjs/no-duplicate-string: ['error', { threshold: 3 }]`
- `sonarjs/no-identical-functions: error`
- `sonarjs/no-collapsible-if: error`
- `sonarjs/no-redundant-jump: error`
- `functional/no-let: error` (immutability by default; `const` only)
- `functional/immutable-data: error`
- `functional/no-loop-statements: error` (forces map/filter/reduce)
- `regexp/no-super-linear-backtracking: error`
- `regexp/no-empty-alternative: error`
- `security/detect-eval-with-expression: error`
- `security/detect-non-literal-fs-filename: error`
- `security/detect-unsafe-regex: error`
- `eslint-comments/no-unused-disable: error`
- `eslint-comments/require-description: error`
- `perfectionist/sort-imports: error`
- `perfectionist/sort-named-imports: error`
- `perfectionist/sort-jsx-props: error`
- `tsdoc/syntax: error`
- `no-console: ['error', { allow: ['warn', 'error'] }]`
- `no-debugger: error`
- `no-alert: error`
- `prefer-template: error`

### [DASH-TS-LINT-OFF] Rules intentionally OFF

Each must have a justification:

- `@typescript-eslint/no-magic-numbers` — too noisy for UI dimensions and HTTP status codes
- `unicorn/prevent-abbreviations` — too opinionated about identifier names
- `unicorn/no-null` — React legitimately uses `null` as a sentinel
- `react/react-in-jsx-scope` — superseded by `jsx: "react-jsx"`
- `import-x/no-default-export` is **off** for files that Vite/React Router require to be default-exported (route components, `App.tsx`); enforced by an override block

### [DASH-TS-LINT-TESTS] Test file overrides

In `src/**/*.test.ts(x)` and `tests/**/*`:

- `@typescript-eslint/no-non-null-assertion: off`
- `@typescript-eslint/no-unsafe-*: off`
- `@typescript-eslint/no-explicit-any: off`
- `@typescript-eslint/explicit-function-return-type: off`
- `@typescript-eslint/strict-boolean-expressions: off`
- `@typescript-eslint/no-unnecessary-condition: off`
- `functional/no-let: off`
- `sonarjs/no-duplicate-string: off`

## [DASH-TS-AUTH-FLOW]

### [DASH-TS-AUTH-LOGIN] Login (discoverable credentials)

1. User clicks **Sign in with Passkey**.
2. SPA POSTs `{}` to `${GATEKEEPER}/auth/login/begin`.
3. Server returns `{ ChallengeId, OptionsJson }` where `OptionsJson` is a
   stringified `PublicKeyCredentialRequestOptions` with `challenge` as
   base64url.
4. SPA decodes `challenge` to `ArrayBuffer`, deletes `allowCredentials`
   to force discoverable credentials, sets `timeout: 120_000`.
5. SPA calls `navigator.credentials.get({ publicKey })`.
6. SPA encodes `assertion.rawId`, `authenticatorData`, `clientDataJSON`,
   `signature`, and optional `userHandle` to base64url.
7. SPA POSTs `{ ChallengeId, OptionsJson, AssertionResponse }` to
   `/auth/login/complete`.
8. Server returns `{ Token, UserId, DisplayName, Email }`.
9. SPA writes `Token` to `localStorage['gatekeeper_token']` and `User`
   to `localStorage['gatekeeper_user']`.
10. AuthContext re-renders; router unmounts LoginPage; dashboard mounts.

### [DASH-TS-AUTH-REGISTER] Register

Same as login but `/auth/register/begin` is given `{ Email, DisplayName }`,
`navigator.credentials.create()` is called instead of `.get()`, and the
attestation object replaces the assertion in the complete request.

### [DASH-TS-AUTH-LOGOUT] Logout

1. POST `/auth/logout` with the current bearer (best effort; failures
   are logged and ignored).
2. Clear both localStorage keys.
3. AuthContext re-renders; router shows LoginPage.

### [DASH-TS-AUTH-401] 401 handling

The `apiFetch` wrapper inspects every response. On `401`:

1. Clear localStorage.
2. Reload the page.
3. The auth gate then renders LoginPage.

This matches the original (working) flow and prevents pages from rendering
with stale/expired tokens.

## [DASH-TS-API]

`src/api/client.ts` exports a single `apiFetch<T>(url, init)` that:

1. Reads token from `Auth.getToken()`.
2. Sets `Accept: application/json` always.
3. Sets `Authorization: Bearer ${token}` if a token exists.
4. Sets `Content-Type: application/json` **only** when `init.body` is
   present (avoids sending Content-Type on GETs and triggering preflight
   on simple requests).
5. Calls `fetch`.
6. Throws a typed `ApiError` (not a generic `Error`) carrying `status`,
   `url`, `body`, and `cause` on non-2xx.
7. On 401, calls `Auth.clear(); window.location.reload();` and throws.
8. Returns `await response.json() as T` for 2xx with body.

Per-domain modules (`gatekeeper.ts`, `clinical.ts`, `scheduling.ts`,
`icd10.ts`) export typed wrappers that call `apiFetch` with the right URL
and return typed results. **No domain module touches `fetch` directly.**

## [DASH-TS-LOGGING]

`src/lib/logger.ts` exports a single `logger` with `info | warn | error`.
- `info` → `console.log` only in development; no-op in production.
- `warn` and `error` → always to `console.warn`/`console.error`.
- Errors are captured by a global `window.addEventListener('error', ...)`
  and `unhandledrejection` handler installed in `main.tsx`, which logs
  the structured payload to `console.error`. This is the floor that
  prevents future "I can't see what's happening in the browser" episodes.

## [DASH-TS-CI]

`pnpm check` runs in this order, fail-fast:

1. `tsc --noEmit` (fail on any type error)
2. `eslint . --max-warnings=0` (fail on any warning)
3. `prettier --check .` (fail on any unformatted file)
4. `vitest run --coverage` (fail on test failure or coverage drop)
5. `pnpm build` (fail on bundler error)
6. `playwright test` (only in CI, not in local `pnpm check` by default)

Make targets:

- `make lint` → `pnpm --filter dashboard lint`
- `make test` → `pnpm --filter dashboard test`
- `make build` → `pnpm --filter dashboard build`
- `make ci` → `pnpm --filter dashboard check` then `playwright test`

## [DASH-TS-MIGRATION]

This is a **clean break**. Phase 0 of the implementation plan deletes the
entire C# project including:

- `Dashboard/Dashboard.Web/*.cs`
- `Dashboard/Dashboard.Web/*.csproj`
- `Dashboard/Dashboard.Web/h5.json`
- `Dashboard/Dashboard.Web/bin/`, `obj/`
- `Dashboard/Dashboard.Web/wwwroot/js/Dashboard.{js,min.js,js.map}`
- `Dashboard/Dashboard.Web/.config/dotnet-tools.json`
- The `Dashboard.Web` entry from any `.sln`
- `Dashboard/Dashboard.Integration.Tests/` (it tested the dead C# code)
- All `dashboard-*.png` debug screenshots in repo root
- All references to H5 / `dotnet build` for the dashboard in `Makefile`,
  `docker/`, and `CLAUDE.md`

After Phase 0, no file in the repo references C#, H5, or the old project.
`grep -r "H5\b" Dashboard/` returns nothing. `grep -r "h5.target" .`
returns nothing.

## [DASH-TS-OPEN-QUESTIONS]

These need answers before Phase 0 executes:

1. **Cognitive complexity threshold** — SonarJS default is 15. Drop to
   10 for "brutal", or keep 15? Spec assumes 15.
2. **Project path** — keep `Dashboard/Dashboard.Web/` (preserves
   docker-compose paths) or rename to kebab-case `Dashboard/dashboard-web/`?
   Spec assumes the existing path.
3. **Functional plugin strictness** — `eslint-plugin-functional` with
   `no-let` and `immutable-data` is enabled. This bans `let` entirely
   and forces `as const` / `Readonly<T>` discipline. Confirm or relax?
4. **Default-export ban** — Vite and React Router work fine with named
   exports; the only files that need defaults are the entry points.
   Confirm `import-x/no-default-export: error` with a narrow override
   for `src/main.tsx` and route components?
5. **`Dashboard.Integration.Tests`** — confirmed deletion?

Once these are answered, the plan in
`docs/plans/dashboard-typescript-rewrite.md` executes top to bottom.
