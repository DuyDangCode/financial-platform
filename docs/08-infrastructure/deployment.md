# Deployment & CI/CD

## 1. Purpose

This document defines the continuous integration, continuous delivery (CI/CD), environment configuration, and deployment strategy for the Financial Portfolio & Trading Operations Platform (NFR-007).

> **Status: Planned (Phase 7).** The project currently runs locally via `dotnet run` and `docker-compose.yml`.

---

## 2. Deployment Topology

As defined in [`system-architecture.md`](../03-architecture/system-architecture.md) §14 and §17, the platform deploys as a stateless **Modular Monolith** behind a TLS-terminating load balancer or reverse proxy:

```text
                    Internet (HTTPS)
                           │
                           ▼
               ┌───────────────────────┐
               │ Reverse Proxy / ALB   │
               └───────────┬───────────┘
                           │
             ┌─────────────┴─────────────┐
             ▼                           ▼
   ┌───────────────────┐       ┌───────────────────┐
   │  Next.js Web App  │       │ FinancialPlatform │
   │   (Frontend)      │──────►│   .Api (.NET 8)   │
   └───────────────────┘       └─────────┬─────────┘
                                         │
                               ┌─────────┴─────────┐
                               ▼                   ▼
                       ┌──────────────┐    ┌──────────────┐
                       │ PostgreSQL 16│    │ Redis Cache  │
                       │ (Primary DB) │    │  (Optional)  │
                       └──────────────┘    └──────────────┘
```

Because `FinancialPlatform.Api` stores session state in PostgreSQL (`RefreshTokens`) and issues stateless JWTs, multiple API instances can run horizontally without sticky sessions.

---

## 3. Environment Configuration Matrix

ASP.NET Core binds hierarchical configuration using double underscores (`__`) in environment variables:

| Configuration Key | Env Variable Name | Development (`appsettings.json`) | Staging / Production |
| --- | --- | --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `ASPNETCORE_ENVIRONMENT` | `Development` | `Production` |
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` | `Host=localhost;Port=5432;Database=financial_platform;...` | Managed PostgreSQL 16 connection string with `SSL Mode=Require` |
| `Jwt:Issuer` | `Jwt__Issuer` | `dev` | Production issuer URI |
| `Jwt:Audience` | `Jwt__Audience` | `dev` | Production audience URI |
| `Jwt:Key` | `Jwt__Key` | Overridden via `user-secrets` (`>= 32` bytes) | Injected from Secret Manager (`>= 32` cryptographically random bytes) |
| `Jwt:ExpiryMinutes` | `Jwt__ExpiryMinutes` | `60` | `15`–`60` |

### Production Guardrails

1. **Swagger Disabled:** `Program.cs` enables `UseSwagger()` and `UseSwaggerUI()` strictly inside `if (app.Environment.IsDevelopment())`.
2. **Password Reset Code Redacted:** `ForgotPasswordCommandHandler` only returns `resetCode` in the response payload when `IWebHostEnvironment.IsDevelopment()` is `true`.
3. **No InMemory Fallback in Production:** Production deployments must fail fast at startup if `ConnectionStrings:DefaultConnection` is missing rather than silently falling back to EF InMemory.

---

## 4. Database Migration Strategy on Deployment

1. **Development:** Developers generate migrations via `dotnet ef migrations add` and apply them via `dotnet ef database update`.
2. **CI/CD & Production:**
   - Generate an idempotent SQL migration bundle or script during the CI build:
     ```bash
     dotnet ef migrations script --idempotent \
       --project src/FinancialPlatform.Infrastructure \
       --startup-project src/FinancialPlatform.Api \
       -o artifacts/migrate.sql
     ```
   - Execute the migration step as a dedicated pre-deployment job before rolling out new API container instances.
   - Ensure all schema changes are backward-compatible during rolling updates.

---

## 5. CI/CD Pipeline Stages

A standard GitHub Actions / GitLab CI pipeline for this repository consists of four sequential stages:

1. **Restore & Build:**
   - `dotnet restore FinancialPlatform.sln`
   - `dotnet build FinancialPlatform.sln --configuration Release --no-restore`
2. **Automated Tests & Lint:**
   - `dotnet test tests/FinancialPlatform.UnitTests --configuration Release --no-build`
   - `dotnet test tests/FinancialPlatform.IntegrationTests --configuration Release --no-build` (with Docker service / Testcontainers)
   - `cd frontend/financial-platform-web && npm ci && npm run lint && npm run build`
3. **Package:**
   - Build and tag Docker images for `FinancialPlatform.Api` and `financial-platform-web`.
4. **Deploy:**
   - Apply EF Core migrations against target PostgreSQL database.
   - Roll out container images and verify `/health/ready` returns `200 OK`.
