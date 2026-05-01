# Plan: GK-AUDIT — Audit Logging Implementation

> Spec: `docs/specs/gk-audit.md`  
> Date: 2026-05-02

## Context

Gatekeeper has no audit trail. `FileLoggerProvider` writes undifferentiated logs with potential PII. This plan adds a structured `gk_audit_log` table, `IAuditLogger`, wires events into all auth/authz decisions, and adds a paginated admin query endpoint.

---

## TODO

### Schema

- [ ] Add `gk_audit_log` table: `id`, `occurred_at`, `actor_id`, `event_type`, `resource_type`, `resource_id`, `outcome`, `reason`, `ip_address`, `metadata`
- [ ] Add indexes: `(actor_id, occurred_at)`, `(event_type, occurred_at)`, `(occurred_at)`

### IAuditLogger

- [ ] Create `IAuditLogger.cs` + `AuditEvent.cs`
- [ ] Create `AuditLogger.cs`: DataProvider insert; catch all; log `Warning` on failure; never throw
- [ ] Add `generateInsert: true` for `gk_audit_log` in `DataProvider.json`
- [ ] Register `IAuditLogger` as `AuditLogger` singleton in `Program.cs`

### Wire auth events

- [ ] `PasskeyAuthProvider`: `auth.register.begin`, `auth.register.success`, `auth.register.failure`
- [ ] `PasskeyAuthProvider`: `auth.login.begin`, `auth.login.success`, `auth.login.failure`
- [ ] Sign count clone → `security.credential.clone`
- [ ] `POST /auth/logout` → `auth.logout`
- [ ] `POST /auth/logout-all` → `auth.logout.all`
- [ ] Token revoked → `auth.token.revoked`
- [ ] User inactive → `auth.user.inactive`
- [ ] Token version mismatch → `auth.token.version_mismatch`
- [ ] Account locked → `security.account.locked`
- [ ] Rate limit rejected → `security.rate_limit.rejected`

### Wire authz events

- [ ] `AuthorizationService` allowed → `authz.permission.allowed`
- [ ] `AuthorizationService` denied → `authz.permission.denied`
- [ ] All writes fire-and-forget (`_ = auditLogger.LogAsync(...)`)

### PII validation

- [ ] Zero `email`/`display_name` in any `AuditEvent` metadata
- [ ] `auth.register.begin` uses `SHA-256(email)` as `email_hash`, not raw email

### Admin endpoint

- [ ] Create `GetAuditEvents.lql`: filter on `event_type`, `actor_id`, `outcome`, date range; `order_by(occurred_at desc)`; `limit`/`offset`
- [ ] Add `GET /admin/audit` endpoint (requires `admin:audit:read`)

### Tests

- [ ] Test: login success writes `auth.login.success` (no email in row)
- [ ] Test: login failure writes `auth.login.failure`
- [ ] Test: permission allowed writes `authz.permission.allowed`
- [ ] Test: permission denied writes `authz.permission.denied`
- [ ] Test: audit logger DB failure does NOT fail the caller
- [ ] Test: `GET /admin/audit` requires admin — 403 without it
- [ ] Test: `GET /admin/audit?eventType=auth.login` filters correctly
- [ ] Test: date range filtering works
- [ ] Test: no email in any audit row
