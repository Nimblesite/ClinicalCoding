# Plan: GK-AUTHZ — Authorization Implementation

> Spec: `docs/specs/gk-authorization.md`  
> Date: 2026-05-02

## Context

RBAC for roles, direct permissions, and resource grants works correctly. Role hierarchy (`parent_role_id`) and the ABAC policy engine (`gk_policy`) are schema-only with no code. Scope types are hardcoded. Authorization decisions are not audited. Batch size is unlimited.

## Critical Files

| File | Change |
|---|---|
| `Gatekeeper/Gatekeeper.Api/AuthorizationService.cs` | Role hierarchy; policy engine; extensible scope; audit |
| `Gatekeeper/Gatekeeper.Api/gatekeeper-schema.yaml` | Add missing indexes |
| `Gatekeeper/Gatekeeper.Api/Program.cs` | Wire `PolicyEngine`; batch limit; `/admin/policies` |
| NEW: `Gatekeeper/Gatekeeper.Api/PolicyEngine.cs` | ABAC condition evaluator |
| `Gatekeeper/Gatekeeper.Api.Tests/AuthorizationTests.cs` | Tests for hierarchy, ABAC, scope types, audit |

---

## TODO

### Schema indexes

- [ ] Add index `IX_gk_session_revoked` on `gk_session(is_revoked, expires_at)`
- [ ] Add index `IX_gk_resource_grant_user_perm` on `gk_resource_grant(user_id, permission_id)`
- [ ] Add index `IX_gk_user_permission_user_expires` on `gk_user_permission(user_id, expires_at)`

### Role hierarchy

- [ ] Add `GetRoleChain` SQL query (recursive CTE walking `parent_role_id`, depth limit 10)
- [ ] Update `AuthorizationService.CheckPermissionAsync`: resolve full role chain before permission lookup
- [ ] Cycle guard: depth > 10 → log `Warning`, use only direct roles
- [ ] Test: 2-level hierarchy — child inherits parent permissions
- [ ] Test: 3-level hierarchy — grandchild inherits all ancestor permissions
- [ ] Test: cycle in `parent_role_id` does not hang
- [ ] Test: expired role excludes parent permissions

### ABAC policy engine

- [ ] Create `PolicyEngine.cs`: load `gk_policy` rows; `PolicyContext` record (user, resource, env)
- [ ] Implement condition operators: `$eq`, `$neq`, `$in`, `$nin`, `$gt`, `$lt`, `$prefix`, `$contains`, `$and`, `$or`, `$ref`
- [ ] Evaluation order: deny first → resource grants → RBAC → allow policies → default deny
- [ ] Wire into `AuthorizationService.CheckPermissionAsync`
- [ ] Add `GET /admin/policies` and `POST /admin/policies` endpoints
- [ ] Test: deny policy overrides RBAC allow
- [ ] Test: allow policy grants when RBAC denies
- [ ] Test: `$eq`, `$in`, `$ref`, `$and` operators
- [ ] Test: priority ordering
- [ ] Test: inactive policy not evaluated

### Scope extension

- [ ] Add `organization`, `department`, `tenant` scope types to `AuthorizationService`
- [ ] Make scope matching table-driven (not hardcoded switch)
- [ ] Test: org scope — allowed in same org, denied in different org
- [ ] Test: department scope matching

### Audit integration

- [ ] Wire `IAuditLogger` into `AuthorizationService.CheckPermissionAsync`
- [ ] Add `source` field to `/authz/check` response
- [ ] Test: audit row written for allowed decision
- [ ] Test: audit row written for denied decision
- [ ] Test: audit logger failure does NOT fail the check

### Endpoint hardening

- [x] Batch size limit on `POST /authz/evaluate`: reject if `checks.Count > 50`
- [ ] Pagination on `GET /authz/permissions`: `?limit=50&offset=0`
- [ ] Test: evaluate with 51 checks returns 400
