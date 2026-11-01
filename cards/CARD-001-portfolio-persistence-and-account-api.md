# [CARD-001] Implement Portfolio Persistence and Account Creation API

- **Status:** TODO
- **Priority:** P1 - High
- **Type:** Feature
- **Estimated Effort:** 3-4 hours
- **Created Date:** 2026-11-01
- **Related Docs:** `docs/05-api/account.md`, `docs/00-project/roadmap.md`, `docs/02-domain/business-rules.md`

---

## 1. Context & Problem Statement (The "Why")

Phase 1 (Identity & Authentication) is functional, and the core domain entities for Phase 2 (`Portfolio`, `Position`, `Asset`) already exist with verified unit tests. However, `Portfolio` lacks EF Core persistence mapping, repository abstractions, and API exposure (`AccountController` is currently a 0-byte placeholder). Implementing portfolio creation and listing is the primary blocker on the critical path to allow investors to hold assets and place simulated trading orders.

> **Pre-requisite note:** Before compiling, clean up or revert the draft syntax error in `src/FinancialPlatform.Api/Controllers/TransactionController.cs` so the solution builds cleanly.

## 2. Technical Scope & Target Files (The "Where")

- `src/FinancialPlatform.Domain/Interfaces/IPortfolioRepository.cs` — Define repository contract (`GetByIdAsync`, `GetByUserIdAsync`, `AddAsync`, `UpdateAsync`, `ExistsByNameForUserAsync`).
- `src/FinancialPlatform.Infrastructure/Persistence/Configurations/PortfolioConfiguration.cs` — Define EF Core entity configuration (table `Portfolios`, keys, constraints, max lengths, unique index on `(UserId, Name)`).
- `src/FinancialPlatform.Infrastructure/Persistence/AppDbContext.cs` — Register `DbSet<Portfolio> Portfolios`.
- `src/FinancialPlatform.Infrastructure/Persistence/Repositories/PortfolioRepository.cs` — Implement `IPortfolioRepository` backed by `AppDbContext`.
- `src/FinancialPlatform.Infrastructure/DependencyInjection.cs` — Register `IPortfolioRepository` -> `PortfolioRepository` in DI container.
- `src/FinancialPlatform.Application/Features/Account/Commands/CreatePortfolio/CreatePortfolioCommand.cs` — Command model carrying `UserId`, `Name`, `BaseCurrency`, `Notes`.
- `src/FinancialPlatform.Application/Features/Account/Commands/CreatePortfolio/CreatePortfolioCommandHandler.cs` — Orchestrate portfolio creation using `Portfolio.Create()` and save via `IPortfolioRepository`.
- `src/FinancialPlatform.Application/Features/Account/DTOs/PortfolioResponse.cs` — API response DTO matching `docs/05-api/account.md`.
- `src/FinancialPlatform.Application/DependencyInjection.cs` — Register `CreatePortfolioCommandHandler` in DI.
- `src/FinancialPlatform.Api/Controllers/AccountController.cs` — Implement `POST /api/account/create` and `GET /api/account/list` with `[Authorize]`, extracting `UserId` from `ClaimsPrincipal`.
- `tests/FinancialPlatform.UnitTests/Application/Features/Account/CreatePortfolioCommandHandlerTests.cs` — Unit tests covering success, validation, and duplicate name constraints.

## 3. Implementation Requirements (The "What")

1. **Data / State Layer:**
   - Create `IPortfolioRepository` in Domain with methods to fetch by ID, fetch all portfolios for a user, check existence by name per user, add, and update.
   - Implement `PortfolioConfiguration` in Infrastructure with:
     - `builder.HasKey(p => p.Id);`
     - `builder.Property(p => p.Name).IsRequired().HasMaxLength(128);`
     - `builder.Property(p => p.BaseCurrency).IsRequired().HasMaxLength(3);`
     - `builder.Property(p => p.Notes).HasMaxLength(512);`
     - `builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(32);`
     - Unique index on `(UserId, Name)` to prevent duplicate portfolio names for the same investor (BR-003).
   - Expose `DbSet<Portfolio> Portfolios => Set<Portfolio>();` in `AppDbContext`.

2. **Core Logic / API:**
   - Implement `CreatePortfolioCommand` and `CreatePortfolioCommandHandler`:
     - Validate input rules: name required, max length 128 chars; currency valid 3-letter ISO (default `"USD"`); notes optional max 512 chars.
     - Check if a portfolio with the same name already exists for the user; throw `DomainException("Portfolio name already exists.")` if duplicated.
     - Call `Portfolio.Create(command.UserId, command.Name, command.BaseCurrency, command.Notes)`.
     - Persist via `IPortfolioRepository.AddAsync`.
     - Return `PortfolioResponse`.
   - Wire handler in `Application/DependencyInjection.cs` and repository in `Infrastructure/DependencyInjection.cs`.
   - Implement `AccountController`:
     - Route `api/[controller]`, `[ApiController]`, `[Authorize]`.
     - Inject `CreatePortfolioCommandHandler`.
     - Endpoint `POST api/account/create`: extract `UserId` from JWT `ClaimTypes.NameIdentifier`, invoke handler, wrap in `ApiResponse<PortfolioResponse>.Created(...)` (HTTP 201).
     - Endpoint `GET api/account/list`: list portfolios owned by caller (`ApiResponse<IReadOnlyList<PortfolioResponse>>.Ok(...)`).

3. **Edge Cases & Constraints:**
   - Enforce user isolation: A user must only see or create portfolios associated with their own `UserId` (BR-036).
   - Ensure `BaseCurrency` is normalized to uppercase 3 letters.
   - Wrap all responses in standard `ApiResponse<T>` envelope.
   - Domain validation exceptions (`DomainException`) must be mapped to HTTP 400 via existing `GlobalExceptionMiddleware`.

## 4. Acceptance Criteria (Definition of Done)

- [ ] Uncommitted syntax error in `src/FinancialPlatform.Api/Controllers/TransactionController.cs` is resolved so the build passes.
- [ ] `IPortfolioRepository` and `PortfolioConfiguration` are implemented with unique index on `(UserId, Name)`.
- [ ] `POST /api/account/create` returns `201 Created` with `ApiResponse<PortfolioResponse>` when valid payload is submitted with valid JWT.
- [ ] Submitting a duplicate portfolio name for the same user returns `400 Bad Request` with appropriate error message.
- [ ] Submitting without Bearer token returns `401 Unauthorized`.
- [ ] Unit tests for `CreatePortfolioCommandHandler` cover valid creation, duplicate name check, and invalid currency/name inputs.
- [ ] `dotnet build` and `dotnet test tests/FinancialPlatform.UnitTests --no-restore` pass with zero errors and zero regressions.

## 5. Verification & Test Plan

- **Compilation check:**
  ```bash
  dotnet build FinancialPlatform.sln --no-restore
  ```
- **Unit test execution:**
  ```bash
  dotnet test tests/FinancialPlatform.UnitTests --no-restore
  ```
- **Manual verification via Swagger or HTTP request:**
  1. Authenticate via `POST /api/auth/login` to obtain access token.
  2. Send `POST /api/account/create` with header `Authorization: Bearer {token}` and payload:
     ```json
     {
       "name": "Main Growth",
       "baseCurrency": "USD",
       "notes": "Primary investment portfolio"
     }
     ```
  3. Verify response status is `201 Created` and `data.name` is `"Main Growth"`.

