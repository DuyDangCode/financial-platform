# Authentication

## 1. Purpose

This document describes the authentication architecture, credential storage, token lifecycle, and security controls of the Financial Portfolio & Trading Operations Platform (FR-001, FR-002, NFR-001).

All components described under **Implemented** reflect the live code in `src/FinancialPlatform.Application/Features/Authentication/` and `src/FinancialPlatform.Infrastructure/Identity/`.

---

## 2. Architecture Overview

The platform uses a hybrid **Stateless JWT Access Token + Stateful Refresh Token** model:

```text
┌──────────┐     POST /api/auth/login      ┌──────────────────────────┐
│  Client  │ ────────────────────────────► │      AuthController      │
└────┬─────┘                               └────────────┬─────────────┘
     │                                                  │
     │                                                  ▼
     │                                     ┌──────────────────────────┐
     │                                     │    LoginCommandHandler   │
     │                                     │  1. GetByEmailAsync      │
     │                                     │  2. Check IsActive       │
     │                                     │  3. PasswordHasher.Verify│
     │                                     └────────────┬─────────────┘
     │                                                  │
     │                                                  ▼
     │                                     ┌──────────────────────────┐
     │     LoginResponse (JWT + Refresh)   │    AuthSessionIssuer     │
     │ ◄────────────────────────────────── │  1. Generate JWT (60m)   │
     │                                     │  2. Persist Refresh (7d) │
     └─────────────────────────────────────┴──────────────────────────┘
```

---

## 3. Password Hashing (`PasswordHasher.cs` — Implemented)

Passwords are never stored or logged in plain text. `PasswordHasher` (`src/FinancialPlatform.Infrastructure/Identity/PasswordHasher.cs`) implements `IPasswordHasher` using **PBKDF2-HMAC-SHA256**:

| Parameter | Value | Notes |
| --- | --- | --- |
| Algorithm | `Rfc2898DeriveBytes.Pbkdf2` (`HashAlgorithmName.SHA256`) | Standard .NET cryptographic primitive |
| Salt size | `16` bytes (`128` bits) | Generated per password via `RandomNumberGenerator.GetBytes(16)` |
| Subkey size | `32` bytes (`256` bits) | |
| Iterations | `100,000` | Balances brute-force resistance with login latency |
| Storage format | `{Base64(salt)}:{Base64(subkey)}` | Stored in `Users.PasswordHash` (`varchar(512)`) |
| Verification | `CryptographicOperations.FixedTimeEquals` | Prevents timing side-channel attacks |

---

## 4. Access Tokens (JWT Bearer — Implemented)

Issued by `JwtTokenGenerator` (`src/FinancialPlatform.Infrastructure/Identity/JwtTokenGenerator.cs`) and validated by the ASP.NET Core JWT Bearer middleware configured in `src/FinancialPlatform.Api/Program.cs`.

### 4.1. Claims Included

| Claim | Source | Purpose |
| --- | --- | --- |
| `sub` (`JwtRegisteredClaimNames.Sub`) | `user.Id.ToString()` | Primary user identifier resolved by `[Authorize]` endpoints |
| `email` (`JwtRegisteredClaimNames.Email`) | `user.Email` | Lowercase normalized user email |
| `name` (`JwtRegisteredClaimNames.Name`) | `user.UserName` | Username |
| `jti` (`JwtRegisteredClaimNames.Jti`) | `Guid.NewGuid().ToString()` | Unique token identifier per issuance |
| `role` (`ClaimTypes.Role` — *Planned*) | `UserRole` → `Role.Name` | Assigned role names (`Investor`, `Administrator`) |

### 4.2. Validation Rules (`Program.cs`)

- `ValidateIssuer = true` (`Jwt:Issuer`)
- `ValidateAudience = true` (`Jwt:Audience`)
- `ValidateLifetime = true`
- `ValidateIssuerSigningKey = true` (`HMAC-SHA256` with `Jwt:Key`)
- `ClockSkew = TimeSpan.Zero` — tokens expire at the exact `expires` timestamp with zero grace window.

---

## 5. Refresh Tokens & Session Lifecycle (`RefreshToken.cs` — Implemented)

Because JWT access tokens are stateless, long-lived session persistence and revocation are handled through stateful `RefreshToken` entities stored in PostgreSQL:

| Property | Behavior |
| --- | --- |
| Generation | 64 cryptographically random bytes (`RandomNumberGenerator.GetBytes(64)`), Base64-encoded (`RefreshTokenGenerator.cs`). |
| Lifetime | `7 days` from issuance (`DateTime.UtcNow.AddDays(7)`). |
| Active check | `token.IsActive` is `true` iff `RevokedAt == null && DateTime.UtcNow < ExpiresAt`. |
| Rotation (`POST /api/auth/refresh`) | `RefreshCommandHandler` verifies the token is active, checks `user.IsActive`, calls `existingToken.Revoke()`, persists the revocation, and issues a brand-new JWT and refresh token via `IAuthSessionIssuer`. |
| Logout (`POST /api/auth/logout`) | `LogoutCommandHandler` looks up the refresh token and calls `Revoke()` idempotently. |
| Global revocation | `ChangePasswordCommandHandler` and `ResetPasswordCommandHandler` call `IRefreshTokenRepository.RevokeAllForUserAsync(user.Id)`, invalidating all active refresh tokens when a user's password changes. |

---

## 6. Password Recovery (`PasswordResetToken.cs` — Implemented)

1. **Request (`POST /api/auth/forgot-password`):**
   - `ForgotPasswordCommandHandler` looks up the user by email.
   - If the user exists and is active, all previous reset codes for that user are invalidated (`InvalidateAllForUserAsync`), and a new 6-digit numeric code (`RandomNumberGenerator.GetInt32(100_000, 1_000_000)`) is persisted with a **15-minute** expiration.
   - In `Development` environment only (`IWebHostEnvironment.IsDevelopment()`), the `resetCode` is returned in `ForgotPasswordResponse` to allow local testing before an email adapter is wired. In non-Development environments, `resetCode` is always `null`.
2. **Consumption (`POST /api/auth/reset-password`):**
   - `ResetPasswordCommandHandler` loads the user by email and the active token by code (`GetActiveByCodeAsync`), verifies `token.UserId == user.Id` and `token.IsActive`, updates `user.SetPassword(...)`, calls `token.MarkUsed()`, and revokes all refresh tokens for the user.

---

## 7. Known Gaps & Production Hardening Checklist

| Item | Current State | Target Remediation |
| --- | --- | --- |
| JWT Signing Key | `appsettings.json` has `"Key": "local"` (too short for HMAC-SHA256 outside dev override) | Supply a `>= 32`-byte secret via `dotnet user-secrets` or environment variable `Jwt__Key`; never commit real keys. |
| Email lookup normalization | `User.Create` normalizes `Email` to lowercase, but `UserRepository.GetByEmailAsync` compares raw input (`u.Email == email`) | Normalize the query parameter (`email.Trim().ToLowerInvariant()`) in `UserRepository` and handlers. |
| Account enumeration in ForgotPassword | `ForgotPasswordResponse.Delivered` returns `false` when email is unknown | Always return `delivered: true` in production to prevent email enumeration. |
| Disabled user check on Refresh | `LoginCommandHandler` checks `if (!user.IsActive)`, whereas `RefreshCommandHandler` currently only checks `user is null` | Add `if (user is null \|\| !user.IsActive) throw new InvalidOrExpiredTokenException();` in `RefreshCommandHandler` (BR-002). |
