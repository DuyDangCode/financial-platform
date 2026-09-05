---
name: db-design
description: Design database schema, EF Core entities, configurations, migrations, and repositories for PostgreSQL in the .NET backend. Use when adding entities, tables, relationships, indexes, constraints, or persistence code.
---

# DB Design (EF Core + PostgreSQL)

## Where things live

| Concern | Location |
|---|---|
| Entity | `src/FinancialPlatform.Domain/Entities/<Entity>.cs` |
| Repository interface | `src/FinancialPlatform.Domain/Interfaces/I<Name>Repository.cs` |
| EF configuration | `src/FinancialPlatform.Infrastructure/Persistence/Configurations/<Entity>Configuration.cs` |
| Repository implementation | `src/FinancialPlatform.Infrastructure/Persistence/Repositories/<Name>Repository.cs` |
| DbContext + migrations | `src/FinancialPlatform.Infrastructure/Persistence/AppDbContext.cs` |

## Entity conventions

Follow `User.cs`: private setters, private ctor, public `static Create(...)` factory, validation throwing `DomainException`, inherit `BaseEntity` (Guid Id). Normalize emails/usernames to lowercase.
EF configurations implement `IEntityTypeConfiguration<T>` with explicit `HasKey`/`HasMaxLength`/unique indexes and are auto-applied via `ApplyConfigurationsFromAssembly` — still add the `DbSet<>` property to `AppDbContext`.

## Migrations & provider trap

- Provider is Npgsql **only when** `ConnectionStrings:DefaultConnection` exists; otherwise `AppDbContext` silently falls back to EF InMemory ("FinancialPlatformDemoDb"). Migrations generated against InMemory are bogus — run against docker-compose Postgres 16 first (`docker compose up -d postgres`).
- Add migration: `dotnet ef migrations add <Name> --project src/FinancialPlatform.Infrastructure --startup-project src/FinancialPlatform.Api`
- `SeedData.SeedAsync` runs on every startup from `Program.cs` (no-op if users exist, uses `EnsureCreatedAsync`) — coordinate seed logic with model changes.

## Repository rules

- Narrow async methods only (`GetByEmailAsync`, `GetByIdAsync`, `AddAsync`, `UpdateAsync`, ...); implementations take `AppDbContext` and call `SaveChangesAsync` internally.
- `CancellationToken cancellationToken = default` parameter on every method.
- Register in `Infrastructure/DependencyInjection.cs`: `AddScoped<I<Name>Repository, <Name>Repository>()`.

## Schema guidelines

- Money as `decimal` — never float/double.
- Explicit constraint/index names (`IX_<Table>_<Column>`), unique index on natural keys.
- Prefer hard deletes + unique filtered indexes over soft-delete columns unless requirements say otherwise.
