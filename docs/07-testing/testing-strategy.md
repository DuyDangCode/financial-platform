# Testing Strategy

## 1. Purpose

This document defines the testing strategy, test project structure, conventions, and verification scenarios for the Financial Portfolio & Trading Operations Platform (NFR-004, NFR-005).

---

## 2. Test Pyramid & Projects

The repository contains two xUnit test projects under `tests/`:

```text
                  ┌───────────────────────────────┐
                  │   Integration / API Tests     │
                  │ FinancialPlatform.Integration │
                  │  (WebApplicationFactory +     │
                  │   PostgreSQL Testcontainers)  │
                  └───────────────┬───────────────┘
                                  │
          ┌───────────────────────┴───────────────────────┐
          │                  Unit Tests                   │
          │         FinancialPlatform.UnitTests           │
          │  (Domain Entities + Application Handlers +    │
          │   Controller Envelope Mapping w/ Hand Fakes)  │
          └───────────────────────────────────────────────┘
```

| Project | Status | References | Purpose |
| --- | --- | --- | --- |
| `tests/FinancialPlatform.UnitTests` | **Implemented** (175+ passing tests) | `Api`, `Application`, `Domain` (intentionally does **not** reference `Infrastructure`) | Fast, deterministic verification of domain invariants, state machines, position/P&L arithmetic, application command/query handlers, and controller responses. |
| `tests/FinancialPlatform.IntegrationTests` | **Planned** (currently contains an empty stub `UnitTest1.cs`) | `Api`, `Application`, `Domain`, `Infrastructure` | End-to-end verification of HTTP pipeline, EF Core mappings, PostgreSQL constraints, `xmin` concurrency tokens, and atomic transaction rollback using real PostgreSQL 16 containers. |

---

## 3. Running Tests

From the repository root:

```bash
# Run all test projects
dotnet test

# Run only unit tests
dotnet test tests/FinancialPlatform.UnitTests

# Run a single test class or method by filter
dotnet test tests/FinancialPlatform.UnitTests --filter FullyQualifiedName~OrderTests
```

Frontend lint check (in `frontend/financial-platform-web`):

```bash
npm run lint
```

---

## 4. Unit Testing Conventions (`FinancialPlatform.UnitTests`)

### 4.1. Directory Organization

```text
tests/FinancialPlatform.UnitTests/
├── Api/
│   └── Controllers/
│       └── AuthControllerTests.cs
├── Application/
│   └── Features/
│       └── <Module>/<UseCase>HandlerTests.cs
├── Domain/
│   └── Entities/
│       ├── AssetTests.cs
│       ├── AuditLogTests.cs
│       ├── ExecutionTests.cs
│       ├── OrderTests.cs
│       ├── PortfolioTests.cs
│       ├── PositionTests.cs
│       ├── RoleTests.cs
│       ├── TransactionTests.cs
│       ├── UserRoleTests.cs
│       └── UserTests.cs
└── TestSupport/
    └── AuthFakes.cs
```

### 4.2. Hand-Written Fakes over Mocking Frameworks

Because `FinancialPlatform.UnitTests` does not reference `FinancialPlatform.Infrastructure` or heavy third-party mocking libraries, application and controller tests use explicit, reusable in-memory fakes located in `tests/FinancialPlatform.UnitTests/TestSupport/` (such as `FakeUserRepository`, `FakeRefreshTokenRepository`, `FakePasswordResetTokenRepository`, `StubPasswordHasher`, and `FakeAuthSessionIssuer` in `AuthFakes.cs`).

When implementing new modules (`Portfolio`, `Trading`, `MarketData`, `Audit`), extend `TestSupport/` with corresponding fakes:

- `FakePortfolioRepository : IPortfolioRepository`
- `FakeAssetRepository : IAssetRepository`
- `FakePositionRepository : IPositionRepository`
- `FakeOrderRepository : IOrderRepository`
- `FakeExecutionRepository : IExecutionRepository`
- `FakeTransactionRepository : ITransactionRepository`
- `FakeAuditLogRepository : IAuditLogRepository`

### 4.3. Naming & Structure

- Follow **Arrange – Act – Assert** structure.
- Name test methods descriptively: `<MethodOrUseCase>_<Scenario>_<ExpectedOutcome>` (e.g., `ApplySell_WhenQuantityExceedsHolding_ThrowsDomainException`, `Cancel_WhenOrderIsAlreadyFilled_ThrowsDomainException`).
- Use `[Theory]` and `[InlineData]` for boundary values (zero, negative, whitespace, max-length strings, invalid enum transitions).

---

## 5. Required Test Scenarios by Module

When manually implementing each module, verify the following test cases:

### 5.1. Domain Layer (Implemented in `Domain/Entities/*Tests.cs`)

- **`PortfolioTests`**: Creation validation (`UserId`, `Name <= 128`, 3-letter uppercase `BaseCurrency`), `Rename`, `Activate` / `Disable`.
- **`AssetTests`**: Uppercase symbol normalization (`" aapl "` → `"AAPL"`), negative price rejection, `RefreshPrice` timestamp update, `Activate` / `Deactivate`.
- **`PositionTests`**:
  - First buy sets `AverageEntryPrice = price` and `Quantity = qty`.
  - Subsequent buy computes weighted average: `(prevQty * prevAvg + qty * price) / (prevQty + qty)`.
  - Partial sell preserves `AverageEntryPrice`, decreases `Quantity`, and accumulates `RealizedPnL += (sellPrice - avgEntry) * soldQty`.
  - Full sell (`Quantity == 0`) sets `IsClosed == true` and resets `AverageEntryPrice = null`.
  - Selling more than `Quantity` throws `DomainException` (BR-023).
- **`OrderTests`**:
  - `MARKET` order forces `LimitPrice = null`; `LIMIT` order requires `LimitPrice > 0` (BR-011).
  - Valid transitions: `PENDING → PROCESSING → FILLED`, `PENDING → CANCELLED`, `PENDING/PROCESSING → REJECTED` (BR-012).
  - Illegal transitions throw `DomainException`: cancelling a `FILLED` order (BR-013), filling a `CANCELLED` or `REJECTED` order (BR-014, BR-015), overfilling `ExecutedQuantity > RemainingQuantity` (BR-017).
- **`ExecutionTests` & `TransactionTests`**: Positive quantity and price guards, non-negative fee, and immutable `GrossAmount = Quantity * Price` calculation.

### 5.2. Application Layer Handlers (To Implement in Phases 2–4)

- **Portfolio Handlers**:
  - Reject portfolio creation with duplicate name for the same user.
  - Reject viewing or updating a portfolio owned by another user (BR-036).
  - Calculate portfolio valuation (`TotalMarketValue`, `UnrealizedPnL`, `RealizedPnL`, `TotalPnL`) accurately across multiple open and closed positions.
- **Order & Execution Handlers**:
  - Reject order creation when `Portfolio.Status == DISABLED` (BR-004, BR-009) or `Asset.Status == INACTIVE` (BR-007, BR-010).
  - Reject `SELL` order creation when `Position` is missing or `Position.Quantity < Order.Quantity` (BR-023).
  - Execute `MARKET` buy/sell order end-to-end: verify `Order.Status == FILLED`, `Execution` created, `Transaction` created with matching `GrossAmount`, `Position` updated, and `AuditLog` recorded.
  - Execute `LIMIT` buy order only when `Asset.CurrentPrice <= Order.LimitPrice`; leave `PENDING` when `CurrentPrice > LimitPrice`.
  - Execute `LIMIT` sell order only when `Asset.CurrentPrice >= Order.LimitPrice`; leave `PENDING` when `CurrentPrice < LimitPrice`.
  - Verify idempotent execution: calling execute twice on the same order does not create a second `Transaction` or double-count `Position.Quantity` (BR-019, BR-032).

### 5.3. Integration Tests (`FinancialPlatform.IntegrationTests` — Planned Phase 4)

- **Database Schema & Constraints**: Run migrations against a real PostgreSQL 16 container and verify unique indexes (`IX_Users_Email`, `IX_Assets_Symbol`, `IX_Positions_PortfolioId_AssetId`, `IX_Executions_OrderId`, `IX_Transactions_ExecutionId`).
- **Transactional Atomicity (FR-027, BR-033)**: Simulate a failure partway through order execution and assert that `Orders`, `Executions`, `Transactions`, and `Positions` roll back completely with zero partial state persisted.
- **Optimistic Concurrency (FR-028, BR-034)**: Simulate two concurrent `SELL` executions against the same `Position` row and verify that PostgreSQL `xmin` causes the conflicting update to throw `DbUpdateConcurrencyException` rather than allowing a negative or corrupted position.
