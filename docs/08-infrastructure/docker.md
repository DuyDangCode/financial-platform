# Docker Configuration

## 1. Purpose

This document describes the containerization setup of the Financial Portfolio & Trading Operations Platform, covering the current PostgreSQL 16 `docker-compose.yml` service and the planned multi-stage `Dockerfile` specifications for the backend API and frontend web application (NFR-007).

---

## 2. Current Docker Compose Setup (`docker-compose.yml` — Implemented)

At the repository root, `docker-compose.yml` provisions a persistent **PostgreSQL 16** instance for local development and integration testing:

```yaml
services: 
  postgres: 
    image: postgres:16
    container_name: financial-platform-postgres
    restart: unless-stopped
    
    environment: 
      POSTGRES_DB: financial_platform
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres

    ports:
      - "5432:5432"

    volumes:
      - postgres_data:/var/lib/postgresql/data
volumes:
  postgres_data:
```

### Common Docker Compose Commands

```bash
# Start PostgreSQL in the background
docker compose up -d postgres

# Check container status and logs
docker compose ps
docker compose logs -f postgres

# Open an interactive psql shell inside the container
docker exec -it financial-platform-postgres psql -U postgres -d financial_platform

# Stop containers (preserves data in postgres_data volume)
docker compose stop

# Tear down containers and wipe the database volume (clean slate)
docker compose down -v
```

---

## 3. Planned Backend `Dockerfile` (Phase 7)

When containerizing `FinancialPlatform.Api`, use a multi-stage build at the repository root so only the compiled runtime artifacts are included in the final image:

```dockerfile
# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY FinancialPlatform.sln ./
COPY src/FinancialPlatform.Domain/FinancialPlatform.Domain.csproj src/FinancialPlatform.Domain/
COPY src/FinancialPlatform.Application/FinancialPlatform.Application.csproj src/FinancialPlatform.Application/
COPY src/FinancialPlatform.Infrastructure/FinancialPlatform.Infrastructure.csproj src/FinancialPlatform.Infrastructure/
COPY src/FinancialPlatform.Api/FinancialPlatform.Api.csproj src/FinancialPlatform.Api/

RUN dotnet restore src/FinancialPlatform.Api/FinancialPlatform.Api.csproj

COPY src/ src/
RUN dotnet publish src/FinancialPlatform.Api/FinancialPlatform.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

USER app
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "FinancialPlatform.Api.dll"]
```

### Key Container Security Practices

- Run as the non-root `app` user built into `.NET 8` runtime images (`USER app`).
- Bind HTTP on port `8080` inside the container and terminate TLS at the reverse proxy / load balancer.
- Inject `ConnectionStrings__DefaultConnection` and `Jwt__Key` via environment variables or secret mounts at runtime; never bake production secrets into the image layers.

---

## 4. Planned Full-Stack `docker-compose.yml` Evolution

In Phase 5–7, `docker-compose.yml` can be extended with optional profiles for `api`, `redis` (Phase 5 caching), and `web` (`frontend/financial-platform-web`):

| Service | Image / Build | Internal Port | Host Port | Purpose |
| --- | --- | --- | --- | --- |
| `postgres` | `postgres:16` | `5432` | `5432` | Authoritative relational store (Implemented) |
| `redis` | `redis:7-alpine` | `6379` | `6379` | Read cache for market prices & portfolio summaries (Planned Phase 5) |
| `api` | Build `.` (Backend Dockerfile) | `8080` | `5150` | ASP.NET Core Web API (Planned Phase 7) |
| `web` | Build `./frontend/financial-platform-web` | `3000` | `3000` | Next.js frontend (Planned Phase 7) |
