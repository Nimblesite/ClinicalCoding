# Plan: GK-AUDIT — Audit Logging Implementation

> Spec: `docs/specs/gk-audit.md`  
> Date: 2026-05-02

## Context

Gatekeeper has no audit trail. `FileLoggerProvider` writes undifferentiated logs, with no separation between debug output and security events, and logs contain potential PII. This plan adds a structured `gk_audit_log` table, an `IAuditLogger` service, and wires audit events into every authentication and authorization decision. The admin audit endpoint enables security investigations.

## Critical Files

| File | Change |
|---|---|
| `Gatekeeper/Gatekeeper.Api/gatekeeper-schema.yaml` | Add `gk_audit_log` table with indexes |
| `Gatekeeper/Gatekeeper.Api/Program.cs` | Register `IAuditLogger`; wire into auth endpoints; add `GET /admin/audit` |
| `Gatekeeper/Gatekeeper.Api/AuthorizationService.cs` | Call `IAuditLogger` on every permission decision |
| `Gatekeeper/Gatekeeper.Api/PasskeyAuthProvider.cs` | Call `IAuditLogger` on auth events |
| NEW: `Gatekeeper/Gatekeeper.Api/IAuditLogger.cs` | Interface + `AuditEvent` record |
| NEW: `Gatekeeper/Gatekeeper.Api/AuditLogger.cs` | DataProvider-backed implementation |
| NEW: `Gatekeeper/Gatekeeper.Api/Lql/GetAuditEvents.lql` | Query for admin audit endpoint |
| `Gatekeeper/Gatekeeper.Api.Tests/` | Audit integration tests |

## Verification

```bash
make db-reset && make db-migrate
make build
make test
```

Manual:
- Login success → `gk_audit_log` row `auth.login.success` written
- Login failure → `auth.login.failure` row written; no email in row
- Permission check → `authz.permission.allowed` / `authz.permission.denied` row written
- Query `GET /admin/audit?eventType=auth.login` → correct rows returned
- Confirm no email or display_name in any audit row

---

## TODO

### Schema

- [ ] Add `gk_audit_log` table to `gatekeeper-schema.yaml`:
  - columns: `id Uuid PK`, `occurred_at DateTime NOT NULL`, `actor_id Uuid nullable`, `event_type Text NOT NULL`, `resource_type Text nullable`, `resource_id Text nullable`, `outcome Text NOT NULL`, `reason Text nullable`, `ip_address Text nullable`, `metadata Json nullable`
  - indexes: `(actor_id, occurred_at)`, `(event_type, occurred_at)`, `(occurred_at)`

### IAuditLogger

- [ ] Create `IAuditLogger.cs`: interface with `LogAsync(AuditEvent) Task`
- [ ] Create `AuditEvent.cs`: record with all fields from spec event catalogue
- [ ] Create `AuditLogger.cs`: use DataProvider generated `InsertGkAuditLogAsync`; catch all exceptions; log failures at `Warning`; never throw
- [ ] Add `generateInsert: true` for `gk_audit_log` in `DataProvider.json`
- [ ] Register `IAuditLogger` as `AuditLogger` singleton in `Program.cs`

### Wire auth events

- [ ] `PasskeyAuthProvider.BeginAuthAsync` → write `auth.register.begin` / `auth.login.begin`
- [ ] `PasskeyAuthProvider.CompleteAuthAsync` success → write `auth.register.success` / `auth.login.success` (metadata: credential_id)
- [ ] `PasskeyAuthProvider.CompleteAuthAsync` failure → write `auth.register.failure` / `auth.login.failure` (metadata: reason)
- [ ] Sign count clone detected → write `security.credential.clone` (metadata: credential_id, stored_count, received_count)
- [ ] `POST /auth/logout` → write `auth.logout`
- [ ] `POST /auth/logout-all` → write `auth.logout.all` (metadata: new_token_version)
- [ ] Token validation: `is_revoked` → write `auth.token.revoked`
- [ ] Token validation: `is_active = false` → write `auth.user.inactive`
- [ ] Token validation: version mismatch → write `auth.token.version_mismatch`
- [ ] Account lockout triggered → write `security.account.locked` (metadata: locked_until)
- [ ] Rate limit rejected → write `security.rate_limit.rejected` (metadata: bucket_key, count)

### Wire authz events

- [ ] `AuthorizationService.CheckPermissionAsync` allowed → write `authz.permission.allowed` (metadata: permission, resource_type, resource_id, source)
- [ ] `AuthorizationService.CheckPermissionAsync` denied → write `authz.permission.denied` (metadata: permission, resource_type, resource_id)
- [ ] Confirm audit write is fire-and-forget (`_ = auditLogger.LogAsync(...)`) — no await in hot path

### PII validation

- [ ] Review all `AuditEvent` usages: confirm zero `email`, `display_name` fields in metadata
- [ ] `auth.register.begin` uses `SHA-256(email)` not raw email for metadata `email_hash`

### Admin endpoint

- [ ] Create `GetAuditEvents.lql`: filter on `EventType like @eventType`, `ActorId = @actorId`, `Outcome = @outcome`, `OccurredAt >= @from`, `OccurredAt < @to`; `order_by(OccurredAt desc)`; support `limit`/`offset`
- [ ] Add `GET /admin/audit` endpoint (requires `admin:audit:read` permission): parse query params; call `GetAuditEventsAsync`; return paginated response

### Tests

- [ ] Test: login success writes `auth.login.success` audit row with correct actor_id (no email)
- [ ] Test: login failure writes `auth.login.failure` row
- [ ] Test: permission allowed writes `authz.permission.allowed` row
- [ ] Test: permission denied writes `authz.permission.denied` row
- [ ] Test: audit logger failure (simulate DB error) does NOT fail the calling operation
- [ ] Test: `GET /admin/audit` requires admin token — 403 without it
- [ ] Test: `GET /admin/audit?eventType=auth.login` returns only login events
- [ ] Test: `GET /admin/audit` respects `from`/`to` date range
- [ ] Test: no email in any audit row (schema-level assertion: query for email strings in metadata column)
