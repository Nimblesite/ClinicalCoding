# Plan: GK-AUTH — Authentication Implementation

> Spec: `docs/specs/gk-authentication.md`  
> Date: 2026-05-02

## Context

Passkey authentication logic lived in `Program.cs` mixed with routing. Sessions were never written to `gk_session`. Sign counts were not validated. No Supabase provider existed. JWT claims were missing `aud`, `iss`, `ver`. This plan extracted, hardened, and extended authentication.

## Critical Files

| File | Change |
|---|---|
| `Gatekeeper/Gatekeeper.Api/Program.cs` | Extract auth logic; keep only routing + DI wiring |
| `Gatekeeper/Gatekeeper.Api/TokenService.cs` | `IDbConnection`; `aud`/`iss`/`ver` claims; `is_active` + version checks |
| `Gatekeeper/Gatekeeper.Api/gatekeeper-schema.yaml` | New columns + `gk_user_identity` table |
| NEW: `Gatekeeper/Gatekeeper.Api/IAuthProvider.cs` | Interface + shared request/result records |
| NEW: `Gatekeeper/Gatekeeper.Api/PasskeyAuthProvider.cs` | Extracted + hardened passkey flow |
| NEW: `Gatekeeper/Gatekeeper.Api/SupabaseAuthProvider.cs` | Supabase JWKS validation + token exchange |
| `Shared/Authorization/AuthRecords.cs` | Add `TokenVersion`, `Issuer`, `Audience` to `AuthClaims` |
| `Shared/Authorization/AuthHelpers.cs` | Add `aud`/`iss`/`ver` validation; add 10s HttpClient timeout |
| `Gatekeeper/Gatekeeper.Api.Tests/AuthenticationTests.cs` | New tests: sign count, session creation, lockout, Supabase |

## Verification

```bash
make db-reset && make db-migrate
make build
make test
```

---

## TODO

### Schema

- [x] Add `locked_until` to `gk_user` in `gatekeeper-schema.yaml`
- [x] Add `token_version Int default 0` to `gk_user`
- [x] Add `failed_login_count Int default 0` to `gk_user`
- [x] Add `gk_user_identity` table (provider, external_id, user_id, linked_at)
- [x] Add `used_at DateTime nullable` to `gk_challenge`
- [x] Rename `gk_credential.sign_count` → `last_sign_count`

### IAuthProvider abstraction

- [x] Create `IAuthProvider.cs`: `IChallengeAuthProvider`, `ITokenExchangeProvider` interfaces + all request/result/error records
- [x] Create `PasskeyAuthProvider.cs`: registration + login extracted from `Program.cs`; accepts `IDbConnection`, `IFido2`
- [x] Create `SupabaseAuthProvider.cs`: JWKS fetch with 1h cache; RS256/EdDSA only; validate `exp`, `aud`, `iss`; reject HS256; links identity via `gk_user_identity`
- [x] Update `Program.cs`: register both providers; route `/auth/*` through providers; add `/auth/supabase/exchange` and `/auth/identity/link/supabase`

### Passkey hardening

- [x] Sign count validation: reject if `newCount <= storedCount && newCount != 0`
- [x] Atomic challenge deletion immediately on use
- [x] Delete prior pending challenge on new `/begin` call
- [x] Reduce challenge TTL 300s → 120s
- [x] `ATTESTATION_PREFERENCE` env var (`none`/`indirect`/`direct`)

### Session management

- [x] Write `gk_session` row on successful `/auth/login/complete` (user_id, credential_id, ip, user_agent, expires_at)
- [x] Write `gk_session` row on successful `/auth/register/complete`
- [x] `TokenService.ValidateTokenAsync`: enforce HS256 alg server-side
- [x] `TokenService.ValidateTokenAsync`: validate `aud`/`iss` when configured
- [x] Add `POST /auth/logout-all`: increment `gk_user.token_version`
- [x] `GET /auth/session`: check `is_active` and `token_version`

### JWT hardening

- [x] Add `aud`, `iss`, `ver` claims to `TokenService.CreateToken`
- [ ] Configurable signing algorithm via `AUTH_SIGNING_ALGORITHM` env var
- [x] Enforce algorithm server-side (never trust `alg` header)
- [x] Reduce access token lifetime to 15 minutes
- [x] Update `AuthClaims` record: add `TokenVersion`, `Issuer`, `Audience`
- [x] Update `AuthHelpers.ValidateTokenLocally`: validate `aud`, `iss`, `ver`
- [x] Add 10s timeout to `HttpClient` in `AuthHelpers.CheckPermissionAsync`

### Account lockout

- [x] On failed login: increment `failed_login_count`; lock at 5 failures for 15 min
- [ ] On `/auth/login/begin`: check `locked_until`; return 429 + `Retry-After` if locked (N/A for usernameless passkey — check is in CompleteLoginAsync)
- [x] On successful login: reset `failed_login_count = 0`

### Tests

- [ ] Test: sign count clone detected → 401
- [ ] Test: challenge used twice → second use rejected
- [ ] Test: `gk_session` row written on login success
- [ ] Test: inactive user token rejected
- [ ] Test: logout-all invalidates previously valid token
- [ ] Test: Supabase provider — valid RS256 JWT accepted (mock JWKS)
- [ ] Test: Supabase provider — HS256 JWT rejected
- [ ] Test: Supabase provider — expired token rejected
- [ ] Test: 5 failed logins → account locked → 429 on next begin
- [ ] Test: account auto-unlocks after `locked_until` passes
