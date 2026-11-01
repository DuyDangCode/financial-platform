# Troubleshooting Guide

## 1. Purpose

This runbook documents common development, configuration, database, and runtime issues encountered when working on the Financial Portfolio & Trading Operations Platform, along with their root causes and step-by-step fixes.

---

## 2. Startup & Configuration Issues

### 2.1. `IDX10720` / `IDX10653`: JWT Signing Key Is Too Short

- **Symptom:** Calling `POST /api/auth/register` or `POST /api/auth/login` throws a `500 Internal Server Error`, and the console log shows:
  `IDX10720: Unable to create KeyedHashAlgorithm for algorithm 'HS256', the key size must be greater than: '256' bits, key has '40' bits.`
- **Root Cause:** `src/FinancialPlatform.Api/appsettings.json` commits a placeholder `"Key": "local"` (5 bytes = 40 bits), whereas `.NET 8` `SymmetricSecurityKey` with `HmacSha256` requires a key of at least **32 bytes (256 bits)**.
- **Fix:** Configure a 32+ character development signing key via User Secrets or `appsettings.Development.json`:
  ```bash
  dotnet user-secrets set "Jwt:Key" "dev-only-super-secret-signing-key-at-least-32-bytes" --project src/FinancialPlatform.Api
  ```

---

### 2.2. Demo User (`demo@financialplatform.com`) Does Not Exist on Startup

- **Symptom:** Logging in with `demo@financialplatform.com` / `Demo@123` returns `401 Invalid email or password`, or tables do not exist in PostgreSQL.
- **Root Cause:** In `src/FinancialPlatform.Api/Program.cs` (line 57), the startup seed call is commented out and has a typo in the namespace (`Infrastxructure`):
  ```csharp
  // await FinancialPlatform.Infrastxructure.Persistence.SeedData.SeedAsync(app.Services);
  ```
- **Fix:** Uncomment line 57 in `src/FinancialPlatform.Api/Program.cs` and fix the namespace to `FinancialPlatform.Infrastructure.Persistence.SeedData.SeedAsync(app.Services);`.

---

## 3. Database & EF Core Issues

### 3.1. EF Core Migrations Fail or Generate Empty/Broken Snapshot

- **Symptom:** `dotnet ef migrations add ...` fails or ignores PostgreSQL-specific configurations (`xmin`, `numeric(18,4)`).
- **Root Cause:** `AppDbContext` falls back to `UseInMemoryDatabase("FinancialPlatformDemoDb")` when `ConnectionStrings:DefaultConnection` is missing (`src/FinancialPlatform.Infrastructure/DependencyInjection.cs`).
- **Fix:**
  1. Ensure `ConnectionStrings:DefaultConnection` is present in `src/FinancialPlatform.Api/appsettings.json`.
  2. Start PostgreSQL 16 via Docker Compose:
     ```bash
     docker compose up -d postgres
     ```
  3. Always pass `--startup-project src/FinancialPlatform.Api` when running `dotnet ef`:
     ```bash
     dotnet ef migrations add <Name> --project src/FinancialPlatform.Infrastructure --startup-project src/FinancialPlatform.Api
     ```

---

### 3.2. `EnsureCreatedAsync()` Blocks EF Core Migrations (`Relation "Users" Already Exists`)

- **Symptom:** Running `dotnet ef database update` against a local database created earlier by `SeedData.SeedAsync` fails with `42P07: relation "Users" already exists` because `EnsureCreatedAsync()` does not create the `__EFMigrationsHistory` table.
- **Fix:** In local development, reset the Docker volume once when switching from `EnsureCreatedAsync()` to EF Core Migrations:
  ```bash
  docker compose down -v
  docker compose up -d postgres
  dotnet ef database update --project src/FinancialPlatform.Infrastructure --startup-project src/FinancialPlatform.Api
  ```
  Then replace `dbContext.Database.EnsureCreatedAsync()` with `dbContext.Database.MigrateAsync()` in `SeedData.cs`.

---

## 4. Authentication & Business Logic Issues

### 4.1. Login Fails When Email Contains Uppercase Characters

- **Symptom:** A user registers with `User@Example.com`, which succeeds, but logging in with `User@Example.com` returns `401 Invalid email or password`.
- **Root Cause:** `User.Create(...)` normalizes `Email` to lowercase (`email.Trim().ToLowerInvariant()`) before saving, whereas `UserRepository.GetByEmailAsync(string email)` compares `u.Email == email` without lowercasing the lookup argument (tracked in [`database-design.md`](../04-database/database-design.md) §14 #1).
- **Fix:** Normalize the email parameter inside `UserRepository.GetByEmailAsync`:
  ```csharp
  var normalizedEmail = email.Trim().ToLowerInvariant();
  return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
  ```

---

### 4.2. New Command/Query Handler Fails with `InvalidOperationException: Unable to resolve service`

- **Symptom:** Calling a newly implemented controller endpoint returns `500 Internal Server Error`, and logs show `Unable to resolve service for type 'FinancialPlatform.Application.Features...Handler'`.
- **Root Cause:** The project does not use MediatR or assembly scanning for handlers. Every handler must be registered manually.
- **Fix:** Register the handler in `src/FinancialPlatform.Application/DependencyInjection.cs`:
  ```csharp
  services.AddScoped<MyNewCommandHandler>();
  ```
  And if it depends on a new repository, register the repository in `src/FinancialPlatform.Infrastructure/DependencyInjection.cs`.

---

### 4.3. Concurrent Order Execution / Position Update Returns `409 Conflict` (`DbUpdateConcurrencyException`)

- **Symptom:** Under concurrent requests for the same portfolio or position, EF Core throws `DbUpdateConcurrencyException`.
- **Root Cause:** PostgreSQL `xmin` optimistic concurrency token detected that another transaction modified the `Portfolio`, `Position`, or `Order` row between read and write (FR-028, BR-034).
- **Fix:**
  - Ensure `GlobalExceptionMiddleware` maps `DbUpdateConcurrencyException` to `409 Conflict` with a safe retry message (`"The resource was modified by another request. Please retry."`).
  - Never disable concurrency tokens on `Positions` or `Orders`—the conflict is preventing a race condition (such as double-selling the same shares).
