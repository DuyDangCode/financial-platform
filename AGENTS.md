# AGENTS.md

## Layout

Two independent apps in one repo:

- **Backend (.NET 8)**: `FinancialPlatform.sln` at root. Clean Architecture layers in `src/` (Api, Application, Domain, Infrastructure), xUnit tests in `tests/`.
- **Frontend**: Next.js app in `frontend/financial-platform-web` — separate npm project, not in the .NET solution. Node version pinned via root `mise.toml`.

Read `frontend/financial-platform-web/AGENTS.md` before touching frontend code: this Next.js version has breaking changes vs training data; consult the bundled docs it references (`node_modules/next/dist/docs/`).

## Commands

Backend (repo root):

- Build all: `dotnet build FinancialPlatform.sln`
- All tests: `dotnet test`
- Unit tests only: `dotnet test tests/FinancialPlatform.UnitTests`
- Single test: `dotnet test <project> --filter FullyQualifiedName~<name>`
- Run API: `dotnet run --project src/FinancialPlatform.Api` — ports come from launchSettings: https://localhost:7290 / http://localhost:5150 (not the default 5000/7000 range). Swagger UI in Development.

Frontend (in `frontend/financial-platform-web`): `npm run dev`, `npm run lint`.

`tests/FinancialPlatform.IntegrationTests` is currently an empty stub — no real integration tests or Testcontainers yet, despite what the README implies.

## Docs vs reality

Only Identity/Auth is implemented (register, login, refresh, logout, password reset/change). Role/Permission/Account/Dashboard/Transaction controllers are zero-byte placeholders; Portfolio, Trading, Market Data modules don't exist yet. Treat `README.md` and `docs/` (numbered topic folders) as the target roadmap — several doc files are empty skeletons (e.g. `ADR-001.md`, `08-infrastructure/local-development.md`). Trust code over prose.

## Backend conventions

- **No MediatR.** Handlers are plain classes registered manually in `src/FinancialPlatform.Application/DependencyInjection.cs` — a new handler won't resolve until added there. Controllers inject handlers directly.
- Features are vertical slices: `Application/Features/<Area>/Commands/<Name>/<Name>Command.cs` + `<Name>CommandHandler.cs`. Infra interfaces live in `Application/Abstractions/`; implementations in `Infrastructure/` (e.g. `Identity/`, `Persistence/Repositories/`), wired in `Infrastructure/DependencyInjection.cs`.
- Every endpoint returns the `ApiResponse<T>` envelope (`Success`/`Message`/`Data`/`Error`). `GlobalExceptionMiddleware` maps domain exceptions to HTTP codes (base `DomainException` → 400, `UserAlreadyExistsException` → 409, `InvalidCredentialsException` → 401, else 500) — new domain exceptions need a mapping there.
- `AppDbContext` silently falls back to EF InMemory when `ConnectionStrings:DefaultConnection` is missing, so the API runs without Postgres. `docker-compose.yml` provides Postgres 16 matching appsettings defaults (db `financial_platform`, postgres/postgres).
- `SeedData.SeedAsync` runs on every startup from `Program.cs`.
- Test projects reference Api/Application/Domain but **not Infrastructure** — unit tests use hand-written fakes (`tests/FinancialPlatform.UnitTests/TestSupport/AuthFakes.cs`) and cannot touch `AppDbContext` or repositories as-is.

## Security

Dev-only secrets (JWT signing key, DB password) live in committed `appsettings.json`. Never add real credentials there — use user-secrets or environment variables.
