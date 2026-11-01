# Local Development Guide

## 1. Purpose

This document describes how to set up, configure, run, and debug the Financial Portfolio & Trading Operations Platform (both the `.NET 8` backend and the `Next.js 16` frontend) on a local development machine.

---

## 2. Prerequisites

| Tool | Required Version | Purpose |
| --- | --- | --- |
| **.NET SDK** | `8.0.x` | Building, running, and testing `FinancialPlatform.sln` |
| **Node.js** | `22.x` (pinned in root `mise.toml`) | Running `frontend/financial-platform-web` |
| **Docker & Docker Compose** | Docker Engine 24+ / Compose v2 | Running local PostgreSQL 16 (`docker-compose.yml`) |
| **EF Core CLI (`dotnet-ef`)** | `8.0.x` | Generating and applying EF Core database migrations |

Verify your environment from the repository root:

```bash
dotnet --version
node --version
docker compose version
```

---

## 3. Database Setup (PostgreSQL 16 vs. InMemory Fallback)

### 3.1. Running PostgreSQL 16 via Docker Compose (Recommended)

Start the PostgreSQL 16 container defined in `docker-compose.yml`:

```bash
docker compose up -d postgres
```

Default connection parameters (matching `src/FinancialPlatform.Api/appsettings.json`):

| Setting | Value |
| --- | --- |
| Host | `localhost` |
| Port | `5432` |
| Database | `financial_platform` |
| Username | `postgres` |
| Password | `postgres` (local development only) |
| Volume | `postgres_data` |

### 3.2. Automatic InMemory Fallback

In `src/FinancialPlatform.Infrastructure/DependencyInjection.cs`, if `ConnectionStrings:DefaultConnection` is empty or omitted, `AppDbContext` automatically falls back to `options.UseInMemoryDatabase("FinancialPlatformDemoDb")`.

> **Important:** Use the InMemory fallback only for quick stateless API demos. Always run PostgreSQL 16 when creating EF Core migrations or testing relational constraints and `xmin` concurrency tokens.

### 3.3. Database Schema & Seeding

In `src/FinancialPlatform.Infrastructure/Persistence/SeedData.cs`, `SeedData.SeedAsync(app.Services)`:

1. Calls `dbContext.Database.EnsureCreatedAsync()`.
2. Seeds the demo user `demo@financialplatform.com` (password: `Demo@123`) if the `Users` table is empty.

Note: In `src/FinancialPlatform.Api/Program.cs` (line 57), the `SeedData.SeedAsync` call is currently commented out (`// await FinancialPlatform.Infrastxructure.Persistence.SeedData.SeedAsync(app.Services);`). Uncomment and fix the namespace typo (`Infrastructure`) when you want automatic schema creation and seeding on startup, or apply EF Core migrations directly:

```bash
dotnet ef migrations add <MigrationName> \
  --project src/FinancialPlatform.Infrastructure \
  --startup-project src/FinancialPlatform.Api

dotnet ef database update \
  --project src/FinancialPlatform.Infrastructure \
  --startup-project src/FinancialPlatform.Api
```

---

## 4. Running the Backend (`.NET 8` API)

From the repository root:

```bash
# Build the entire solution
dotnet build FinancialPlatform.sln

# Run the Web API project
dotnet run --project src/FinancialPlatform.Api
```

### Ports & Swagger UI

Defined in `src/FinancialPlatform.Api/Properties/launchSettings.json`:

- **HTTPS URL:** `https://localhost:7290`
- **HTTP URL:** `http://localhost:5150`
- **Swagger UI (Development):** `https://localhost:7290/swagger` or `http://localhost:5150/swagger`

### Secret Configuration

Never commit real credentials to `appsettings.json`. Use `.NET User Secrets` or environment variables for local overrides (such as a 32+ character `Jwt:Key` required by `HMAC-SHA256`):

```bash
dotnet user-secrets init --project src/FinancialPlatform.Api
dotnet user-secrets set "Jwt:Key" "dev-only-super-secret-signing-key-at-least-32-bytes" --project src/FinancialPlatform.Api
```

---

## 5. Running the Frontend (`Next.js 16`)

The frontend lives in `frontend/financial-platform-web` as an independent npm project:

```bash
cd frontend/financial-platform-web

# Install dependencies
npm install

# Start the Next.js development server (default http://localhost:3000)
npm run dev

# Run ESLint
npm run lint
```

By default, the frontend targets `process.env.NEXT_PUBLIC_API_URL` (falling back to `https://localhost:7290`).

---

## 6. Typical Local Development Workflow (Manual Feature Implementation)

When building a new feature manually based on `docs/`:

1. **Review the Specification:**
   - Check requirements (`docs/01-product/requirements.md`), business rules (`docs/02-domain/business-rules.md`), table schema (`docs/04-database/database-design.md`), and endpoint contract (`docs/05-api/<module>.md`).
2. **Domain Layer (`src/FinancialPlatform.Domain`):**
   - Implement or verify the entity, enums, domain exceptions, and repository interface (`Interfaces/I<Name>Repository.cs`).
3. **Infrastructure Layer (`src/FinancialPlatform.Infrastructure`):**
   - Add `DbSet<T>` to `AppDbContext.cs`.
   - Create `Persistence/Configurations/<Entity>Configuration.cs`.
   - Implement `Persistence/Repositories/<Name>Repository.cs` and register it in `Infrastructure/DependencyInjection.cs`.
4. **Application Layer (`src/FinancialPlatform.Application`):**
   - Create vertical slices under `Features/<Area>/Commands|<Queries>/<UseCase>/` (`<UseCase>Command.cs` + `<UseCase>CommandHandler.cs`).
   - Register each handler in `Application/DependencyInjection.cs`.
5. **API Layer (`src/FinancialPlatform.Api`):**
   - Add request/response DTOs under `Models/Request/` and `Models/Response/`.
   - Implement the controller action returning `ApiResponse<T>.Ok(...)` and map any new domain exception in `GlobalExceptionMiddleware.cs`.
6. **Verify with Tests (`tests/FinancialPlatform.UnitTests`):**
   - Add unit tests for domain rules, handlers (using fakes in `TestSupport/`), and controller actions.
   - Run `dotnet test`.
