---
name: backend-build
description: Implement backend features end-to-end in the .NET 8 clean-architecture solution — domain entities, command/handler slices, controllers, EF persistence, DI wiring, and unit tests. Use when asked to build, implement, add, or change backend functionality.
---

# Backend Build (.NET)

Full-slice order — each step has a known trap:

1. **Domain**: entity in `Domain/Entities/` (private setters, public `static Create` factory, `DomainException` validation, inherit `BaseEntity` Guid Id). Exceptions in `Domain/Exceptions/`. Repo interface in `Domain/Interfaces/`.
2. **New domain exception? Map it** in the switch in `src/FinancialPlatform.Api/Middleware/GlobalExceptionMiddleware.cs`, otherwise it surfaces as a generic 500.
3. **Application slice**: `Features/<Area>/Commands/<Name>/<Name>Command.cs` (sealed record) + `<Name>CommandHandler.cs` with ctor-injected dependencies. DTOs near the feature.
4. **Register the handler** in `src/FinancialPlatform.Application/DependencyInjection.cs` (`services.AddScoped<XCommandHandler>()`) — manual step; skipping it only fails at runtime.
5. **Infrastructure**: `<Entity>Configuration : IEntityTypeConfiguration<T>` in `Persistence/Configurations/` + add `DbSet<>` to `AppDbContext`; repository in `Persistence/Repositories/`; register both repo and services in `Infrastructure/DependencyInjection.cs`. Generate migrations only with Npgsql active (needs `ConnectionStrings:DefaultConnection`, e.g. `docker compose up -d postgres`) — the silent InMemory fallback produces bogus migrations.
6. **API**: thin controller in `Api/Controllers/` (`[ApiController] [Route("api/[controller]")]`), request models in `Api/Models/Request/`, return `Ok(ApiResponse<T>.SuccessResponse(...))` everywhere.
7. **Tests**: xUnit in `tests/FinancialPlatform.UnitTests` with hand-written fakes in `TestSupport/` following `AuthFakes.cs` (test projects cannot reference Infrastructure). Name tests `MethodUnderTest_WithCondition_ExpectedResult`.

## Verify

- `dotnet build FinancialPlatform.sln`
- `dotnet test` (unit-only: `dotnet test tests/FinancialPlatform.UnitTests`)
- Single test: `dotnet test <project> --filter FullyQualifiedName~<TestName>`
- Smoke run: `docker compose up -d postgres` then `dotnet run --project src/FinancialPlatform.Api` — ports https://localhost:7290 / http://localhost:5150, Swagger in Development.
