# Plan: GK-DB — Database Independence Implementation

> Spec: `docs/specs/gk-db-independence.md`  
> Date: 2026-05-02

## Context

`NpgsqlConnection` appears in ~60 locations across Gatekeeper service and test files. Every business logic method, service, and test helper is Postgres-specific. All 19 SQL files in `Sql/` are hand-written Postgres dialect. This plan: replaces all `NpgsqlConnection` in business logic with `IDbConnection`, migrates 18 SQL files to LQL (1 replaced by generated CRUD), tracks 2 LQL gaps, and creates RLS policy DDL with DataProvider gap issues.

This plan MUST be completed first — it unblocks all other Gatekeeper plans since every other plan's code must use `IDbConnection`.

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

### LQL files — create (Gatekeeper/Gatekeeper.Api/Lql/)

- [ ] Create `GetUserByEmail.lql`: `gk_user |> filter(fn(r) => r.gk_user.Email = @email and r.gk_user.IsActive = true) |> select(...)`
- [ ] Create `GetUserById.lql`: filter on `Id = @id`
- [ ] Create `GetAllUsers.lql`: all columns, `order_by(DisplayName)`
- [ ] Create `GetUserCredentials.lql`: filter on `UserId = @userId`
- [ ] Create `GetCredentialById.lql`: `join(gk_user, on = gk_credential.UserId = gk_user.Id)` + `filter(Id = @id and IsActive = true)` + select all credential cols + `DisplayName`, `Email`
- [ ] Create `GetCredentialsByUserId.lql`: filter on `UserId = @userId`
- [ ] Create `GetSessionById.lql`: `join(gk_user)` + `filter(Id = @id and IsRevoked = false and ExpiresAt > @now and IsActive = true)`
- [ ] Create `GetSessionForRevoke.lql`: filter on `Id = @jti`, select all session columns
- [ ] Create `GetSessionRevoked.lql`: filter on `Id = @jti`, select only `IsRevoked`
- [ ] Create `GetChallengeById.lql`: filter on `Id = @id and ExpiresAt > @now`
- [ ] Create `GetUserRoles.lql`: `join(gk_role, on = gk_user_role.RoleId = gk_role.Id)` + `filter(UserId = @userId and (ExpiresAt is null or ExpiresAt > @now))`
- [ ] Create `GetAllRoles.lql`: all columns, `order_by(Name)`
- [ ] Create `GetRolePermissions.lql`: `join(gk_role_permission)` + `join(gk_permission)` + `filter(RoleId = @roleId)` + select permission cols + `granted_at`
- [ ] Create `GetPermissionByCode.lql`: filter on `Code = @code`
- [ ] Create `GetAllPermissions.lql`: all columns, `order_by(ResourceType, Action)`
- [ ] Create `CheckResourceGrant.lql`: `join(gk_permission)` + `filter(UserId = @userId and ResourceType = @resourceType and ResourceId = @resourceId and PermissionCode = @permissionCode and (ExpiresAt is null or ExpiresAt > @now))`
- [ ] Create `CountSystemRoles.lql`: `filter(IsSystem = true)` + `select(count)`
- [ ] Create `GetActivePolicies.lql`: `filter(IsActive = true and (ResourceType = @resourceType or ResourceType = '*') and (Action = @action or Action = '*'))` + `order_by(Priority desc)`

### SQL gap files — annotate

- [ ] Add gap comment to top of `GetUserPermissions.sql`: `-- [LQL-GAP-UNION] UNION ALL not yet supported in LQL. Track: <GH issue URL>`
- [ ] Add gap comment to top of `CheckPermission.sql`: `-- [LQL-GAP-EXISTS] Correlated EXISTS not yet supported in LQL. Track: <GH issue URL>`

### SQL files — delete (after LQL confirmed building)

- [ ] Delete `Sql/GetUserByEmail.sql`
- [ ] Delete `Sql/GetUserById.sql`
- [ ] Delete `Sql/GetAllUsers.sql`
- [ ] Delete `Sql/GetUserCredentials.sql`
- [ ] Delete `Sql/GetCredentialById.sql`
- [ ] Delete `Sql/GetCredentialsByUserId.sql`
- [ ] Delete `Sql/GetSessionById.sql`
- [ ] Delete `Sql/GetSessionForRevoke.sql`
- [ ] Delete `Sql/GetSessionRevoked.sql`
- [ ] Delete `Sql/GetChallengeById.sql`
- [ ] Delete `Sql/GetUserRoles.sql`
- [ ] Delete `Sql/GetAllRoles.sql`
- [ ] Delete `Sql/GetRolePermissions.sql`
- [ ] Delete `Sql/GetPermissionByCode.sql`
- [ ] Delete `Sql/GetAllPermissions.sql`
- [ ] Delete `Sql/CheckResourceGrant.sql`
- [ ] Delete `Sql/CountSystemRoles.sql`
- [ ] Delete `Sql/GetActivePolicies.sql`
- [ ] Delete `Sql/RevokeSession.sql` (replaced by generateUpdate)

### DataProvider.json update

- [ ] Switch all 18 migrated query entries from `sqlFile` to `lqlFile`
- [ ] Add `"generateUpdate": true` to `gk_session` table entry
- [ ] Confirm build succeeds after each batch of changes

### IDbConnection sweep

- [ ] Update `AuthorizationService.cs`: all method parameters `NpgsqlConnection` → `IDbConnection`
- [ ] Update `TokenService.cs`: all method parameters `NpgsqlConnection` → `IDbConnection`
- [ ] Update `DatabaseSetup.cs`: method parameter `NpgsqlConnection` → `IDbConnection`
- [ ] Rewrite `JunctionTableInserts.cs`: replace `NpgsqlCommand`, `NpgsqlParameter`, `NpgsqlTransaction` with `IDbCommand`, `IDataParameter`, `IDbTransaction`; update `BindParameters` to use `IDbCommand`
- [ ] Update `Program.cs`: remove direct `NpgsqlConnection` usage outside the factory lambda; downstream calls use `IDbConnection`
- [ ] Update `AuthorizationTests.cs`: replace `NpgsqlConnection` in `OpenConnection()`, `CreateTestDb()`, `CleanupTestDb()` with `IDbConnection` where possible; keep `new NpgsqlConnection(...)` only in factory setup
- [ ] Update `TokenServiceTests.cs`: same as above

### RLS policies

- [ ] Create `docs/specs/rls-policies.sql` with policies for `gk_session`, `gk_credential`, `gk_challenge`; each prefixed `-- [RLS-GAP-DATAPROVIDER]`
- [ ] Add `db-rls` Makefile target that applies `rls-policies.sql` to Postgres

### GitHub issues (log against DataProvider repo)

- [ ] Log issue: LQL UNION / UNION ALL support `[LQL-GAP-UNION]`
- [ ] Log issue: LQL correlated EXISTS subquery support `[LQL-GAP-EXISTS]`
- [ ] Log issue: YAML schema RLS policy declarations `[RLS-GAP-DATAPROVIDER]`
- [ ] Log issue: LQL UPDATE...RETURNING support `[LQL-GAP-UPDATE-RETURNING]`
- [ ] Record issue URLs in respective gap comment lines in the SQL files

### Verification

- [ ] Run grep verification (see above) — zero NpgsqlConnection/NpgsqlCommand/NpgsqlTransaction outside allowed files
- [ ] Run `make build` — zero warnings, zero errors
- [ ] Run `make test` — all tests pass
