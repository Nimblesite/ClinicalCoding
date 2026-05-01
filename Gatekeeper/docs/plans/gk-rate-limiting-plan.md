# Plan: GK-RATELIMIT — Distributed Rate Limiting Implementation

> Spec: `docs/specs/gk-rate-limiting.md`  
> Date: 2026-05-02

## Context

Gatekeeper has zero rate limiting. Auth endpoints can be brute-forced indefinitely. The Gigs codebase has a working distributed rate limiter using Postgres as the coordination store. This plan ports that code, makes it database-agnostic (`IDbConnection`), integrates it with the DataProvider schema, and applies it to all auth endpoints with per-IP and per-account limits. Account lockout (tracked in `gk_user`) is included.

Source to port: `/Users/christianfindlay/Documents/Code/gigs/src/dotnet/Gigs.Api/RateLimiting/`
- `DistributedRateLimiter.cs`
- `RateLimitResult.cs`
- `RateLimitMiddleware.cs`

## Critical Files

| File | Change |
|---|---|
| `Gatekeeper/Gatekeeper.Api/gatekeeper-schema.yaml` | Add `gk_rate_limit` table; add `locked_until`, `token_version`, `failed_login_count` to `gk_user` |
| `Gatekeeper/Gatekeeper.Api/Program.cs` | Register rate limiting middleware; wire per-endpoint limits |
| NEW: `Gatekeeper/Gatekeeper.Api/RateLimiting/RateLimitResult.cs` | Discriminated union: Allowed/Delayed/Rejected |
| NEW: `Gatekeeper/Gatekeeper.Api/RateLimiting/IRateLimitStore.cs` | Interface |
| NEW: `Gatekeeper/Gatekeeper.Api/RateLimiting/PostgresRateLimitStore.cs` | `IDbConnection`-backed implementation |
| NEW: `Gatekeeper/Gatekeeper.Api/RateLimiting/InMemoryRateLimitStore.cs` | Fallback implementation |
| NEW: `Gatekeeper/Gatekeeper.Api/RateLimiting/IClientIpExtractor.cs` | Interface |
| NEW: `Gatekeeper/Gatekeeper.Api/RateLimiting/DefaultClientIpExtractor.cs` | CF-Connecting-IP, X-Real-IP, X-Forwarded-For, RemoteIpAddress |
| NEW: `Gatekeeper/Gatekeeper.Api/RateLimiting/RateLimitOptions.cs` | Per-endpoint configuration record |
| NEW: `Gatekeeper/Gatekeeper.Api/RateLimiting/DistributedRateLimiter.cs` | Core limiter logic |
| NEW: `Gatekeeper/Gatekeeper.Api/RateLimiting/RateLimitMiddleware.cs` | ASP.NET Core middleware |
| NEW: `Gatekeeper/Gatekeeper.Api/RateLimiting/RateLimitCleanupService.cs` | `IHostedService` cleanup |
| `Gatekeeper/Gatekeeper.Api.Tests/` | New rate limit integration tests |

## Verification

```bash
make db-reset && make db-migrate
make build
make test
```

Manual:
- Send 6 requests to `/auth/login/complete` with wrong credential → 6th returns 429
- Send 21 requests to `/auth/login/begin` from same IP → over hard limit returns 429
- Send 11 requests → above soft limit → observe delayed responses
- Confirm `gk_rate_limit` rows written with correct bucket keys
- Confirm `gk_user.locked_until` set after 5 failed logins
- Confirm `/auth/login/begin` returns 429 while locked

---

## TODO

### Schema

- [ ] Add `gk_rate_limit` table to `gatekeeper-schema.yaml`: columns `id Text PK`, `request_count Int`, `window_start DateTime`; index on `window_start`
- [ ] Confirm `gk_user` already has `locked_until`, `token_version`, `failed_login_count` (added by auth plan — coordinate)

### Port from Gigs

- [ ] Create `Gatekeeper/Gatekeeper.Api/RateLimiting/` directory
- [ ] Create `RateLimitResult.cs`: discriminated union `Allowed`, `Delayed(int DelayMs)`, `Rejected(int RetryAfterSeconds)` — no Npgsql types
- [ ] Create `IRateLimitStore.cs`: `IncrementAsync(string key, int windowSeconds) Task<int>`, `ResetAsync(string key) Task`, `CleanupExpiredAsync() Task`
- [ ] Create `PostgresRateLimitStore.cs`: port from Gigs; replace `NpgsqlConnection` with `IDbConnection`; use `gk_rate_limit` table; atomic INSERT ON CONFLICT DO UPDATE with window reset logic
- [ ] Create `InMemoryRateLimitStore.cs`: `ConcurrentDictionary`-backed; used as fallback and in unit tests
- [ ] Create `IClientIpExtractor.cs` interface
- [ ] Create `DefaultClientIpExtractor.cs`: check `CF-Connecting-IP` → `X-Real-IP` → first non-private `X-Forwarded-For` → `RemoteIpAddress`
- [ ] Create `RateLimitOptions.cs`: record with `SoftLimit`, `HardLimit`, `WindowSeconds`, `DelayPerExcessMs`
- [ ] Create `DistributedRateLimiter.cs`: accept `IRateLimitStore`, `IClientIpExtractor`, `ILogger`; implement `CheckAsync(string key, RateLimitOptions options) Task<RateLimitResult>`; catch all store exceptions → log warning → return `Allowed` (graceful degradation)
- [ ] Create `RateLimitMiddleware.cs`: extract IP; build per-IP bucket key; call `DistributedRateLimiter.CheckAsync`; handle Allowed/Delayed/Rejected; return 429 with `Retry-After` header on Rejected
- [ ] Create `RateLimitCleanupService.cs`: `IHostedService`; call `CleanupExpiredAsync` every 60 seconds; log deleted count at Debug level

### Wire into Program.cs

- [ ] Register `IRateLimitStore` as `PostgresRateLimitStore` (singleton with `IDbConnection` factory)
- [ ] Register `IClientIpExtractor` as `DefaultClientIpExtractor`
- [ ] Register `DistributedRateLimiter` as singleton
- [ ] Register `RateLimitCleanupService` as hosted service
- [ ] Apply `RateLimitMiddleware` to `/auth/register/begin` (IP: soft=5, hard=10, window=60)
- [ ] Apply `RateLimitMiddleware` to `/auth/register/complete` (IP: soft=3, hard=5, window=60)
- [ ] Apply `RateLimitMiddleware` to `/auth/login/begin` (IP: soft=10, hard=20, window=60)
- [ ] Apply `RateLimitMiddleware` to `/auth/login/complete` (IP: soft=3, hard=5, window=60; account: soft=5, hard=10, window=900)
- [ ] Apply `RateLimitMiddleware` to `/auth/supabase/exchange` (IP: soft=10, hard=20, window=60)
- [ ] Apply `RateLimitMiddleware` to `/authz/check` (IP: soft=100, hard=200, window=60)
- [ ] Apply `RateLimitMiddleware` to `/authz/evaluate` (IP: soft=50, hard=100, window=60)

### Account lockout (coordinate with auth plan)

- [ ] On failed `/auth/login/complete`: increment `gk_user.failed_login_count`; if count == 5, set `locked_until = now + 15min`, reset count
- [ ] On `/auth/login/begin`: check `locked_until`; return 429 + `Retry-After` if locked
- [ ] On successful login: reset `failed_login_count = 0`, clear `locked_until`
- [ ] Write `security.account.locked` audit event on lockout

### Tests

- [ ] Test: soft limit — 4th request delayed, not rejected
- [ ] Test: hard limit — 6th request rejected with 429
- [ ] Test: per-account limit — 6th failed login attempt from different IPs still rejected (account limit)
- [ ] Test: rate limit store failure → request allowed (graceful degradation)
- [ ] Test: IP extraction from `CF-Connecting-IP` header
- [ ] Test: IP extraction from `X-Forwarded-For` skips private IPs
- [ ] Test: cleanup service deletes expired rows
- [ ] Test: 5 failed logins → `locked_until` set → next `/login/begin` returns 429
- [ ] Test: account unlocks after `locked_until` expires
- [ ] Test: InMemoryRateLimitStore — basic increment and window reset
