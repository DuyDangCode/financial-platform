# Project Roadmap

## 1. Purpose

This document tracks the development roadmap of the **Financial Portfolio & Trading Operations Platform**, mapping product capabilities and engineering goals from [`scope.md`](../01-product/scope.md) and [`requirements.md`](../01-product/requirements.md) into concrete, sequential implementation phases.

### Status Legend

- **Completed** — Implemented and verified with automated tests.
- **In Progress** — Partially implemented (e.g., domain model exists, persistence/API pending).
- **Planned** — Specified in documentation, ready for manual implementation.

---

## 2. Current Implementation Snapshot

| Area | Domain Layer | Application Layer | Infrastructure Layer | API Layer | Unit Tests |
|---|---|---|---|---|---|
| **Identity & Auth** | Completed (`User`, `RefreshToken`, `PasswordResetToken`) | Completed (7 command handlers) | Completed (`AppDbContext`, EF configs, repositories, JWT/PBKDF2) | Completed (`AuthController`) | Completed |
| **RBAC (`Role`, `UserRole`)** | Completed (`Role`, `UserRole`) | Planned | Planned | Placeholder (`RoleController`, `PermissionController`, `UserController`) | Completed (entity tests) |
| **Portfolio & Position** | Completed (`Portfolio`, `Position`) | Planned | Planned | Placeholder (`AccountController`, `DashboardController`) | Completed (entity tests) |
| **Asset & Market Data** | Completed (`Asset`) | Planned | Planned | Planned (`AssetController`) | Completed (entity tests) |
| **Trading (`Order`, `Execution`, `Transaction`)** | Completed (`Order`, `Execution`, `Transaction`) | Planned | Planned | Placeholder (`TransactionController`), Planned (`OrderController`) | Completed (entity tests) |
| **Audit (`AuditLog`)** | Completed (`AuditLog`) | Planned | Planned | Planned | Completed (entity tests) |

---

## 3. Phased Implementation Plan

### Phase 1 — Foundation & Identity (Status: Mostly Completed)

**Goal:** Establish Clean Architecture boundaries, database connectivity, error handling middleware, and end-to-end authentication flows.

- [x] Create `.NET 8` solution (`Api`, `Application`, `Domain`, `Infrastructure`, `UnitTests`, `IntegrationTests`).
- [x] Implement `BaseEntity`, `DomainException` hierarchy, and `GlobalExceptionMiddleware` (`ApiResponse<T>` / `ErrorResponse` envelopes).
- [x] Configure `AppDbContext` with PostgreSQL (`Npgsql`) and automatic `InMemory` fallback when `ConnectionStrings:DefaultConnection` is absent.
- [x] Implement Identity domain entities (`User`, `RefreshToken`, `PasswordResetToken`) and EF Core configurations.
- [x] Implement PBKDF2-SHA256 password hashing (`PasswordHasher`), JWT token issuance (`JwtTokenGenerator`), and refresh token rotation (`AuthSessionIssuer`).
- [x] Implement `AuthController` endpoints (`register`, `login`, `refresh`, `logout`, `change-password`, `forgot-password`, `reset-password`).
- [x] Scaffold Next.js 16 frontend (`frontend/financial-platform-web`) and UI specs for `/`, `/login`, `/register`.
- [ ] **Remaining Phase 1 Tasks:**
  - [ ] Implement RBAC persistence (`RoleConfiguration`, `UserRoleConfiguration`, `IRoleRepository`) and include role claims in `JwtTokenGenerator`.
  - [ ] Implement `UserController`, `RoleController`, and `PermissionController` endpoints (see [`user.md`](../05-api/user.md), [`role.md`](../05-api/role.md), [`permission.md`](../05-api/permission.md)).
  - [ ] Normalize email input in read queries (`UserRepository.GetByEmailAsync`) and enforce `user.IsActive` check on token refresh and protected operations (BR-002).

---

### Phase 2 — Core Portfolio, Assets & Positions (Status: In Progress — Domain Done)

**Goal:** Allow investors to create and manage portfolios, browse tradable assets, and view positions and portfolio valuations (FR-004..010, FR-020..023).

- [x] Implement `Portfolio`, `Asset`, and `Position` domain entities and enums (`PortfolioStatus`, `AssetType`, `AssetStatus`).
- [x] Write domain unit tests for weighted-average buy calculations, long-only sell guards, realized P/L accumulation, and asset symbol normalization.
- [ ] **Remaining Phase 2 Tasks:**
  - [ ] Add EF Core configurations (`PortfolioConfiguration`, `AssetConfiguration`, `PositionConfiguration`) with `numeric(18,4)` / `numeric(18,8)` precision and `xmin` concurrency tokens.
  - [ ] Transition from `Database.EnsureCreatedAsync()` to EF Core Migrations (`dotnet ef migrations add ...`) and seed initial `STOCK` and `ETF` assets in `SeedData`.
  - [ ] Define and implement `IPortfolioRepository`, `IAssetRepository`, and `IPositionRepository`.
  - [ ] Implement Application commands and queries for Portfolio CRUD, Asset lookup/price refresh, and Portfolio Valuation calculation (`Portfolio Value = Σ(Quantity × CurrentPrice)`, Unrealized/Realized P/L).
  - [ ] Implement `AccountController` (`api/account`), `AssetController` (`api/asset`), and `DashboardController` (`api/dashboard`) (see [`account.md`](../05-api/account.md), [`asset.md`](../05-api/asset.md), [`dashboard.md`](../05-api/dashboard.md)).

---

### Phase 3 — Trading Workflow & Order Execution (Status: In Progress — Domain Done)

**Goal:** Enable end-to-end simulated order placement, validation, execution, transaction recording, and position updates (FR-011..019).

- [x] Implement `Order`, `Execution`, and `Transaction` domain entities and enums (`OrderSide`, `OrderType`, `OrderStatus`).
- [x] Write domain unit tests covering the order state machine (`PENDING → PROCESSING → FILLED`, `PENDING/PROCESSING → REJECTED`, `PENDING → CANCELLED`), limit price rules, and immutable snapshots.
- [ ] **Remaining Phase 3 Tasks:**
  - [ ] Add EF Core configurations (`OrderConfiguration`, `ExecutionConfiguration`, `TransactionConfiguration`) with unique idempotency indexes `IX_Executions_OrderId` and `IX_Transactions_ExecutionId`.
  - [ ] Define and implement `IOrderRepository`, `IExecutionRepository`, and `ITransactionRepository`.
  - [ ] Implement the simulated order execution workflow in the Application layer:
    - Validate portfolio is `ACTIVE` (BR-004, BR-009) and owned by caller (BR-036).
    - Validate asset is `ACTIVE` (BR-007, BR-010).
    - Validate `SELL` order against current position quantity (`Position.Quantity >= Order.Quantity`, BR-023).
    - Evaluate `MARKET` vs `LIMIT` price condition against `Asset.CurrentPrice`.
    - Execute atomically in a single unit of work: `Order.StartProcessing()` → `Order.Fill()` → `Execution.Create()` → `Transaction.Create()` → `Position.ApplyBuy()/ApplySell()`.
  - [ ] Implement `OrderController` (`api/order`) and `TransactionController` (`api/transaction`) (see [`order.md`](../05-api/order.md), [`transaction.md`](../05-api/transaction.md)).

---

### Phase 4 — Reliability, Concurrency, Idempotency & Audit (Status: In Progress — Domain Done)

**Goal:** Harden transactional boundaries, prevent race conditions and duplicate financial effects, and record auditable events (FR-024, FR-027..029).

- [x] Implement `AuditLog` domain entity and unit tests.
- [ ] **Remaining Phase 4 Tasks:**
  - [ ] Add `AuditLogConfiguration` (`bigint` identity PK, `jsonb` details) and `IAuditLogRepository`.
  - [ ] Introduce a unit-of-work / transaction scope abstraction (`IUnitOfWork`) so multi-repository operations (`Order` + `Execution` + `Transaction` + `Position` + `AuditLog`) commit or roll back atomically in a single PostgreSQL transaction.
  - [ ] Map `DbUpdateConcurrencyException` (from PostgreSQL `xmin` conflicts) and unique constraint violations (idempotent execution retries) in `GlobalExceptionMiddleware` and handlers.
  - [ ] Implement `SaveChangesInterceptor` to populate `UpdatedAt` and `UpdatedBy` on `BaseEntity` mutations.
  - [ ] Build real integration tests in `tests/FinancialPlatform.IntegrationTests` using `WebApplicationFactory<Program>` and Testcontainers PostgreSQL 16.

---

### Phase 5 — Performance & Caching (Status: Planned)

**Goal:** Measure and optimize read-heavy paths without compromising transactional consistency (NFR-002).

- [ ] Benchmark portfolio valuation, order history, and asset lookup queries.
- [ ] Introduce `IMarketDataCache` / Redis caching for `Asset` prices and read-heavy portfolio summaries using the cache-aside pattern.
- [ ] Ensure graceful degradation when Redis is unavailable (fallback to PostgreSQL per [`system-architecture.md`](../03-architecture/system-architecture.md) §15).

---

### Phase 6 — Asynchronous Processing & Event-Driven Workflows (Status: Planned)

**Goal:** Decouple non-blocking workloads and explore reliable event publishing within the modular monolith.

- [ ] Introduce background workers (`IHostedService` / `BackgroundService`) for periodic market price refresh (FR-023) and deferred limit-order evaluation.
- [ ] Implement the Transactional Outbox pattern (`OutboxMessage` table) for domain/integration events (`OrderFilled`, `TransactionCreated`, `PositionUpdated`).
- [ ] Connect a message broker consumer with idempotent inbox tracking (`ProcessedMessage`).

---

### Phase 7 — Production Readiness & Operations (Status: Planned)

**Goal:** Package, deploy, and monitor the platform in a reproducible containerized environment (NFR-006, NFR-007).

- [ ] Add multi-stage `Dockerfile` for `FinancialPlatform.Api` and `financial-platform-web`, and expand `docker-compose.yml` for full-stack local runs.
- [ ] Add `/health` and `/health/ready` endpoints verifying PostgreSQL connectivity.
- [ ] Add correlation ID middleware and structured JSON logging for production environments.
- [ ] Configure CI/CD pipeline (build, lint, unit tests, integration tests, container image build).

---

## 4. Definition of Done (Per Feature)

A feature on this roadmap is complete only when:

1. **Documentation:** Requirements, domain rules, database schema, and API specifications in `docs/` are up to date.
2. **Domain:** Business invariants are enforced inside `FinancialPlatform.Domain` with zero infrastructure dependencies.
3. **Application & Infrastructure:** Vertical command/query slice is wired in `DependencyInjection.cs` and persisted via EF Core configurations.
4. **API:** Controller returns `ApiResponse<T>` and maps errors through `GlobalExceptionMiddleware`.
5. **Verification:** Unit tests (and integration tests where applicable) pass (`dotnet test`) with zero warnings or regressions.
