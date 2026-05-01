# Plan: GK-AUTH — Authentication Implementation

> Spec: `docs/specs/gk-authentication.md`  
> Date: 2026-05-02

## Context

The current passkey authentication logic lives entirely in `Program.cs` (758 lines), mixed with routing. Sessions are never written to `gk_session`. Sign counts are stored but never validated. Challenges are not atomically deleted on use. There is no Supabase provider. JWT claims are missing `aud`, `iss`, and `ver`. This plan extracts, hardens, and extends authentication.

## Critical Files

| File | Change |
|---|---|
| `Gatekeeper/Gatekeeper.Api/Program.cs` | Extract auth logic; keep only routing + DI wiring |
| `Gatekeeper/Gatekeeper.Api/TokenService.cs` | Add `aud`, `iss`, `ver` claims; add `is_active` + version checks |
| `Gatekeeper/Gatekeeper.Api/gatekeeper-schema.yaml` | Add `locked_until`, `token_version`, `failed_login_count` to `gk_user`; add `used_at` to `gk_challenge`; add `last_sign_count` to `gk_credential` |
| NEW: `Gatekeeper/Gatekeeper.Api/IAuthProvider.cs` | Interface + shared request/result records |
| NEW: `Gatekeeper/Gatekeeper.Api/PasskeyAuthProvider.cs` | Extracted + hardened passkey flow |
| NEW: `Gatekeeper/Gatekeeper.Api/SupabaseAuthProvider.cs` | Supabase JWKS validation + token exchange |
| `Shared/Authorization/AuthRecords.cs` | Add `TokenVersion`, `Issuer`, `Audience` to `AuthClaims` |
| `Shared/Authorization/AuthHelpers.cs` | Add `aud`/`iss`/`ver` validation; add 10s HttpClient timeout |
| `Gatekeeper/Gatekeeper.Api.Tests/AuthenticationTests.cs` | New tests: sign count, session creation, lockout, Supabase |

## Verification

```bash
make db-reset && make db-migrate  # schema changes applied
make build                         # zero warnings
make test                          # all tests pass
```

Manual:
- Register passkey → confirm `gk_session` row written
- Login with same credential twice → confirm sign_count incremented
- Force sign count regression → confirm 401 + audit event `security.credential.clone`
- Deactivate user → confirm token rejected
- Logout-all → confirm all previous tokens rejected

---

## TODO

### Schema

- [ ] Add `locked_until DateTime nullable` to `gk_user` in `gatekeeper-schema.yaml`
- [ ] Add `token_version Int default 0` to `gk_user` in `gatekeeper-schema.yaml`
- [ ] Add `failed_login_count Int default 0` to `gk_user` in `gatekeeper-schema.yaml`
- [ ] Add `used_at DateTime nullable` to `gk_challenge` in `gatekeeper-schema.yaml`
- [ ] Add `last_sign_count Int default 0` to `gk_credential` in `gatekeeper-schema.yaml` (rename existing `sign_count` → `last_sign_count`)

### IAuthProvider abstraction

- [ ] Create `IAuthProvider.cs`: interface + `AuthBeginRequest`, `AuthBeginResult`, `AuthCompleteRequest`, `AuthCompleteResult`, `AuthError` records
- [ ] Create `PasskeyAuthProvider.cs`: move registration and login logic from `Program.cs` into this class; accept `IDbConnection`, `IFido2`, `ITokenService`, `IAuditLogger`
- [ ] Create `SupabaseAuthProvider.cs`: JWKS fetch with 1h cache; RS256/EdDSA only; validate `exp`, `aud="authenticated"`, `iss=supabaseUrl`; reject HS256; upsert `gk_user` on first exchange
- [ ] Update `Program.cs`: register both providers; route `/auth/*` through provider dispatch; keep only routing + DI

### Passkey hardening

- [ ] Implement sign count validation in `PasskeyAuthProvider.CompleteAuthAsync`: reject if `newCount <= storedCount && newCount != 0`; write `security.credential.clone` audit event on rejection
- [ ] Implement atomic challenge invalidation: DELETE `gk_challenge` row immediately after verification (not soft delete)
- [ ] On new `/auth/register/begin` or `/auth/login/begin`: delete existing `gk_challenge` for same `(user_id, type)` before inserting new one
- [ ] Reduce challenge TTL from 300s → 120s in schema defaultValue and FIDO2 config
- [ ] Make attestation preference configurable via `ATTESTATION_PREFERENCE` env var (`none`/`indirect`/`direct`)

### Session management

- [ ] Write `gk_session` row on every successful `/auth/login/complete` (id=JTI, user_id, credential_id, ip, user_agent, expires_at)
- [ ] Write `gk_session` row on every successful `/auth/register/complete`
- [ ] Update `TokenService.ValidateTokenAsync`: check `gk_user.is_active = true`; return `AuthFailure` if false
- [ ] Update `TokenService.ValidateTokenAsync`: check JWT `ver` claim matches `gk_user.token_version`; return `AuthFailure` if mismatch
- [ ] Add `POST /auth/logout-all` endpoint: increment `gk_user.token_version`; return 204
- [ ] Update `GET /auth/session`: always check `is_revoked = false` and `is_active = true`

### JWT hardening

- [ ] Add `aud`, `iss`, `ver` claims to `TokenService.CreateToken`
- [ ] Make signing algorithm configurable via `AUTH_SIGNING_ALGORITHM` env var; default `HS256` in dev, `RS256` in prod
- [ ] Enforce algorithm server-side — never trust `alg` header from incoming JWT
- [ ] Reduce access token lifetime to 15 minutes
- [ ] Update `AuthClaims` record: add `TokenVersion int`, `Issuer string`, `Audience string`
- [ ] Update `AuthHelpers.ValidateTokenLocally`: validate `aud` and `iss` claims
- [ ] Add 10s timeout to `HttpClient` in `AuthHelpers.CheckPermissionAsync`

### Account lockout

- [ ] On failed `/auth/login/complete`: increment `failed_login_count`; if count reaches 5, set `locked_until = now + 15min`, reset count to 0; write `security.account.locked` audit event
- [ ] On `/auth/login/begin`: if `locked_until IS NOT NULL AND locked_until > now`, return 429 with `Retry-After` header
- [ ] On successful login: reset `failed_login_count = 0`

### Tests

- [ ] Test: sign count clone detected → 401 + audit event written
- [ ] Test: challenge used twice → second use rejected
- [ ] Test: `gk_session` row written on login success
- [ ] Test: inactive user token rejected
- [ ] Test: logout-all invalidates previously valid token
- [ ] Test: Supabase provider — valid RS256 JWT accepted (mock JWKS)
- [ ] Test: Supabase provider — HS256 JWT rejected
- [ ] Test: Supabase provider — expired token rejected
- [ ] Test: 5 failed logins → account locked → 429 on next begin
- [ ] Test: account auto-unlocks after `locked_until` passes
