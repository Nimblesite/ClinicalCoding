# Plan: GK-RATELIMIT — Distributed Rate Limiting Implementation

> Spec: `docs/specs/gk-rate-limiting.md`  
> Date: 2026-05-02

## Context

Gatekeeper has zero rate limiting. Auth endpoints can be brute-forced indefinitely. This plan ports the Gigs distributed rate limiter, makes it DB-agnostic, and applies it per-endpoint. Account lockout schema columns were added during the auth plan.

Source to port: `/Users/christianfindlay/Documents/Code/gigs/src/dotnet/Gigs.Api/RateLimiting/`

---

## TODO

### Schema

- [x] `gk_user.locked_until`, `token_version`, `failed_login_count` added (done in auth plan)
- [ ] Add `gk_rate_limit` table: `id Text PK`, `request_count Int`, `window_start DateTime`; index on `window_start`

### Port from Gigs

- [ ] Create `Gatekeeper/Gatekeeper.Api/RateLimiting/` directory
- [ ] Create `RateLimitResult.cs`: `Allowed`, `Delayed(int DelayMs)`, `Rejected(int RetryAfterSeconds)`
- [ ] Create `IRateLimitStore.cs`: `IncrementAsync`, `ResetAsync`, `CleanupExpiredAsync`
- [ ] Create `PostgresRateLimitStore.cs`: `IDbConnection`-backed; atomic INSERT ON CONFLICT DO UPDATE
- [ ] Create `InMemoryRateLimitStore.cs`: `ConcurrentDictionary`-backed fallback
- [ ] Create `IClientIpExtractor.cs` interface
- [ ] Create `DefaultClientIpExtractor.cs`: `CF-Connecting-IP` → `X-Real-IP` → `X-Forwarded-For` → `RemoteIpAddress`; skip private IPs
- [ ] Create `RateLimitOptions.cs`: `SoftLimit`, `HardLimit`, `WindowSeconds`, `DelayPerExcessMs`
- [ ] Create `DistributedRateLimiter.cs`: fail-open on store exception
- [ ] Create `RateLimitMiddleware.cs`: per-endpoint bucket keys; 429 + `Retry-After` on reject
- [ ] Create `RateLimitCleanupService.cs`: `IHostedService`; cleanup every 60s

### Wire into Program.cs

- [ ] Register `IRateLimitStore`, `IClientIpExtractor`, `DistributedRateLimiter`, `RateLimitCleanupService`
- [ ] Apply middleware to `/auth/register/begin` (IP: soft=5, hard=10, window=60s)
- [ ] Apply middleware to `/auth/register/complete` (IP: soft=3, hard=5, window=60s)
- [ ] Apply middleware to `/auth/login/begin` (IP: soft=10, hard=20, window=60s)
- [ ] Apply middleware to `/auth/login/complete` (IP: soft=3, hard=5, window=60s; account: soft=5, hard=10, window=900s)
- [ ] Apply middleware to `/auth/supabase/exchange` (IP: soft=10, hard=20, window=60s)
- [ ] Apply middleware to `/authz/check` (IP: soft=100, hard=200, window=60s)
- [ ] Apply middleware to `/authz/evaluate` (IP: soft=50, hard=100, window=60s)

### Account lockout

- [ ] On failed `/auth/login/complete`: increment `failed_login_count`; lock at 5 for 15 min
- [ ] On `/auth/login/begin`: 429 + `Retry-After` if locked
- [ ] On successful login: reset `failed_login_count = 0`
- [ ] Write `security.account.locked` audit event on lockout

### Tests

- [ ] Test: soft limit — request delayed, not rejected
- [ ] Test: hard limit — 429 returned
- [ ] Test: per-account limit across different IPs
- [ ] Test: store failure → request allowed (graceful degradation)
- [ ] Test: IP extraction from `CF-Connecting-IP`
- [ ] Test: `X-Forwarded-For` skips private IPs
- [ ] Test: cleanup deletes expired rows
- [ ] Test: 5 failed logins → locked → 429 on next begin
- [ ] Test: account auto-unlocks after expiry
- [ ] Test: `InMemoryRateLimitStore` increment and window reset
