# Plan: GK-DB — Database Independence Implementation

> Spec: `docs/specs/gk-db-independence.md`  
> Date: 2026-05-02

## Context

`NpgsqlConnection` appeared in ~60 locations across Gatekeeper service and test files. This plan replaces all `NpgsqlConnection` in business logic with `IDbConnection`, migrates 18 SQL files to LQL (1 replaced by generated CRUD), tracks 2 LQL gaps, and creates RLS policy DDL with DataProvider gap issues.

This plan MUST be completed first — it unblocks all other Gatekeeper plans.

## Critical Files

| File | Change |
|---|---|
| `Gatekeeper/Gatekeeper.Api/AuthorizationService.cs` | `NpgsqlConnection` → `IDbConnection` |
| `Gatekeeper/Gatekeeper.Api/TokenService.cs` | `NpgsqlConnection` → `IDbConnection` |
| `Gatekeeper/Gatekeeper.Api/DatabaseSetup.cs` | `NpgsqlConnection` → `IDbConnection` |
| `Gatekeeper/Gatekeeper.Api/JunctionTableInserts.cs` | Rewrite using `IDbConnection`/`IDbCommand`/`IDbTransaction` |
| `Gatekeeper/Gatekeeper.Api/Program.cs` | Keep `new NpgsqlConnection(...)` only in factory lambda |
| `Gatekeeper/Gatekeeper.Api/DataProvider.json` | Switch 18 entries to `lqlFile`; add `generateUpdate` on `gk_session` |
| `Gatekeeper/Gatekeeper.Api/Sql/GetUserPermissions.sql` | Add `[LQL-GAP-UNION]` comment |
| `Gatekeeper/Gatekeeper.Api/Sql/CheckPermission.sql` | Add `[LQL-GAP-EXISTS]` comment |
| `Gatekeeper/Gatekeeper.Api.Tests/AuthorizationTests.cs` | `NpgsqlConnection` → `IDbConnection` in helpers |
| `Gatekeeper/Gatekeeper.Api.Tests/TokenServiceTests.cs` | `NpgsqlConnection` → `IDbConnection` in helpers |
| NEW: `Gatekeeper/Gatekeeper.Api/Lql/` | 18 new LQL files |
| NEW: `docs/specs/rls-policies.sql` | Postgres RLS DDL with gap comments |

## Verification

```bash
grep -r "NpgsqlConnection" --include="*.cs" Gatekeeper/ Shared/ \
  | grep -v "\.g\.cs" | grep -v "Program\.cs" | grep -v "Factory\.cs"
# Must be empty

grep -r "NpgsqlCommand\|NpgsqlTransaction" --include="*.cs" Gatekeeper/ Shared/ \
  | grep -v "\.g\.cs"
# Must be empty

ls Gatekeeper/Gatekeeper.Api/Sql/
# Must contain only: GetUserPermissions.sql, CheckPermission.sql

ls Gatekeeper/Gatekeeper.Api/Lql/
# Must contain 18 .lql files

make build   # zero warnings
make test    # all tests pass
```

---

## TODO

### IDbConnection sweep

- [x] Update `AuthorizationService.cs`: all method parameters `NpgsqlConnection` → `IDbConnection`
- [x] Update `TokenService.cs`: all method parameters `NpgsqlConnection` → `IDbConnection`
- [x] Update `DatabaseSetup.cs`: method parameter `NpgsqlConnection` → `IDbConnection`
- [x] Rewrite `JunctionTableInserts.cs`: replace `NpgsqlCommand`, `NpgsqlParameter`, `NpgsqlTransaction` with `IDbCommand`, `IDataParameter`, `IDbTransaction`
- [x] Update `Program.cs`: `NpgsqlConnection` only in `OpenNpgsqlConnection` factory; downstream uses `IDbConnection`
- [x] Create `DbExtensions.cs`: single adapter file bridging `IDbConnection` to generated Npgsql extensions
- [ ] Update `AuthorizationTests.cs`: `OpenConnection()` returns `IDbConnection` (currently returns `NpgsqlConnection`)
- [ ] Update `TokenServiceTests.cs`: test helpers use `IDbConnection`

### LQL files — create (Gatekeeper/Gatekeeper.Api/Lql/)

- [ ] Create `GetUserByEmail.lql`
- [ ] Create `GetUserById.lql`
- [ ] Create `GetAllUsers.lql`
- [ ] Create `GetUserCredentials.lql`
- [ ] Create `GetCredentialById.lql`
- [ ] Create `GetCredentialsByUserId.lql`
- [ ] Create `GetSessionById.lql`
- [ ] Create `GetSessionForRevoke.lql`
- [ ] Create `GetSessionRevoked.lql`
- [ ] Create `GetChallengeById.lql`
- [ ] Create `GetUserRoles.lql`
- [ ] Create `GetAllRoles.lql`
- [ ] Create `GetRolePermissions.lql`
- [ ] Create `GetPermissionByCode.lql`
- [ ] Create `GetAllPermissions.lql`
- [ ] Create `CheckResourceGrant.lql`
- [ ] Create `CountSystemRoles.lql`
- [ ] Create `GetActivePolicies.lql`

### SQL gap files — annotate

- [ ] Add `-- [LQL-GAP-UNION]` comment to `GetUserPermissions.sql`
- [ ] Add `-- [LQL-GAP-EXISTS]` comment to `CheckPermission.sql`

### SQL files — delete (after LQL confirmed building)

- [ ] Delete all 18 migrated `.sql` files from `Sql/`
- [ ] Delete `Sql/RevokeSession.sql` (replaced by `generateUpdate`)

### DataProvider.json update

- [ ] Switch all 18 migrated query entries from `sqlFile` to `lqlFile`
- [ ] Add `"generateUpdate": true` to `gk_session` table entry

### RLS policies

- [ ] Create `Gatekeeper/docs/specs/rls-policies.sql` with policies for `gk_session`, `gk_credential`, `gk_challenge`
- [ ] Add `db-rls` Makefile target

### GitHub issues (log against DataProvider repo)

- [ ] Log issue: LQL UNION / UNION ALL support `[LQL-GAP-UNION]`
- [ ] Log issue: LQL correlated EXISTS subquery support `[LQL-GAP-EXISTS]`
- [ ] Log issue: YAML schema RLS policy declarations `[RLS-GAP-DATAPROVIDER]`
- [ ] Log issue: LQL UPDATE...RETURNING support `[LQL-GAP-UPDATE-RETURNING]`

### Verification

- [ ] Grep: zero `NpgsqlConnection` outside `Program.cs`, `DbExtensions.cs`, test factories, `.g.cs`
- [ ] Grep: zero `NpgsqlCommand`/`NpgsqlTransaction` outside `.g.cs`
- [ ] `make build` — zero warnings
- [ ] `make test` — all tests pass
