# Plan: GK-AUTHZ — Authorization Implementation

> Spec: `docs/specs/gk-authorization.md`  
> Date: 2026-05-02

## Context

The existing RBAC engine works correctly for roles, direct permissions, and resource grants. Two major features are schema-only with no code: role hierarchy (`parent_role_id` traversal) and the ABAC policy engine (`gk_policy` table). Scope types are hardcoded to `"all"` and `"record"` — extensible scope types (org, dept, tenant) need to be added. The `reason` field in `gk_user_permission.granted_by` is never populated. Authorization decisions are not audited.

## Critical Files

| File | Change |
|---|---|
| `Gatekeeper/Gatekeeper.Api/AuthorizationService.cs` | Role hierarchy traversal; policy engine integration; extensible scope matching; audit events |
| `Gatekeeper/Gatekeeper.Api/gatekeeper-schema.yaml` | Indexes: `gk_resource_grant(user_id, permission_id)`, `gk_user_permission(user_id, expires_at)`, `gk_session(is_revoked, expires_at)` |
| `Gatekeeper/Gatekeeper.Api/Program.cs` | Wire `PolicyEngine`; add batch size limit to `/authz/evaluate`; add `/admin/policies` endpoints |
| NEW: `Gatekeeper/Gatekeeper.Api/PolicyEngine.cs` | ABAC condition evaluator |
| NEW: `Gatekeeper/Gatekeeper.Api/Lql/GetRoleChain.lql` | Walk `parent_role_id` chain (or SQL with recursive CTE if LQL lacks recursion — gap tracked) |
| `Gatekeeper/Gatekeeper.Api.Tests/AuthorizationTests.cs` | Tests for hierarchy, ABAC, scope types, audit |

## Verification

```bash
make db-reset && make db-migrate
make test
```

Manual:
- Create role `superadmin` → `admin` (parent: superadmin) → `manager` (parent: admin)
- Assign user to `manager` role
- Check permission defined only on `superadmin` → must be allowed
- Create deny policy for `archived` resources → check access to archived resource → must be denied even if RBAC allows
- Check `/authz/check` → confirm `gk_audit_log` row written

---

## TODO

### Schema indexes

- [ ] Add index `IX_gk_session_revoked` on `gk_session(is_revoked, expires_at)` to `gatekeeper-schema.yaml`
- [ ] Add index `IX_gk_resource_grant_user_perm` on `gk_resource_grant(user_id, permission_id)` to `gatekeeper-schema.yaml`
- [ ] Add index `IX_gk_user_permission_user_expires` on `gk_user_permission(user_id, expires_at)` to `gatekeeper-schema.yaml`

### Role hierarchy

- [ ] Write `GetRoleChain` query: iterative walk of `parent_role_id` with depth limit 10 (use raw SQL with recursive CTE if LQL doesn't support recursion; add `// [LQL-GAP-RECURSIVE]` comment if so)
- [ ] Update `AuthorizationService.CheckPermissionAsync`: resolve full role chain before permission lookup
- [ ] Add cycle guard: if depth > 10, log `Warning` and use only direct roles (no exception thrown)
- [ ] Test: 2-level hierarchy — child inherits parent permissions
- [ ] Test: 3-level hierarchy — grandchild inherits all ancestor permissions
- [ ] Test: cycle in `parent_role_id` does not hang (depth guard triggers)
- [ ] Test: expired role assignment does not include parent role permissions

### ABAC policy engine

- [ ] Create `PolicyEngine.cs`: load active `gk_policy` rows via `GetActivePoliciesAsync(resourceType, action)`
- [ ] Implement `PolicyContext` record: `UserContext{Id, Roles, Department, ...}`, `ResourceContext{Type, Id, OwnerId, Status, Tags, ...}`, `EnvContext{Hour, DayOfWeek, Ip}`
- [ ] Implement condition JSON evaluator: operators `$eq`, `$neq`, `$in`, `$nin`, `$gt`, `$lt`, `$prefix`, `$contains`, `$and`, `$or`, `$ref`
- [ ] Implement evaluation order: deny policies (highest priority first) → resource grants → direct permissions → RBAC → allow policies → default deny
- [ ] Wire `PolicyEngine` into `AuthorizationService.CheckPermissionAsync` as step 1 (deny) and step 5 (allow)
- [ ] Update `AuthorizationService`: accept optional `PolicyContext?` parameter; skip policy evaluation if null
- [ ] Add `POST /admin/policies` endpoint (requires `admin:policies:write`)
- [ ] Add `GET /admin/policies` endpoint (requires `admin:policies:read`)
- [ ] Test: deny policy overrides RBAC allow
- [ ] Test: allow policy grants access when RBAC denies
- [ ] Test: `$eq` condition evaluation
- [ ] Test: `$in` condition evaluation
- [ ] Test: `$ref` cross-context field comparison (user.id == resource.owner_id)
- [ ] Test: `$and` compound condition
- [ ] Test: priority ordering — higher priority deny wins over lower priority allow
- [ ] Test: inactive policy (`is_active = false`) is not evaluated

### Scope extension

- [ ] Add scope types `organization`, `department`, `tenant` to `AuthorizationService` scope matching
- [ ] Make scope matching table-driven: `IReadOnlyDictionary<string, Func<string, string?, bool>>` injected at startup
- [ ] Test: `organization` scope — user with org-scoped permission allowed for resource in same org, denied for different org
- [ ] Test: `department` scope — correct department-scoped matching

### Audit integration

- [ ] Wire `IAuditLogger` into `AuthorizationService.CheckPermissionAsync`: write `authz.permission.allowed` or `authz.permission.denied` after every decision
- [ ] Add `source` field to `PermissionResult`: `"resource-grant"`, `"direct-permission"`, `"rbac"`, `"abac-allow"`, `"abac-deny"`, `"default-deny"`
- [ ] Update `/authz/check` response to include `source`
- [ ] Test: audit log row written for allowed decision
- [ ] Test: audit log row written for denied decision
- [ ] Test: audit log failure does NOT fail the authorization check

### Endpoint hardening

- [ ] Add batch size limit to `POST /authz/evaluate`: reject if `checks.Count > 50` with 400
- [ ] Add pagination to `GET /authz/permissions`: `?limit=50&offset=0`
- [ ] Test: evaluate with 51 checks returns 400
