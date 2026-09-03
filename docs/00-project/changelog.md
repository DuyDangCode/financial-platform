# Changelog

All notable changes to this project are documented in this file, newest first.

---

## 2026-08-29 — Portfolio & Trading domain entities

### Added

- Domain entities for the planned modules in `src/FinancialPlatform.Domain/Entities/`:
  - **Portfolio module:** `Portfolio` (rename/activate/disable), `Asset` (symbol normalized uppercase on write, price refresh, activate/deactivate), `Position` (weighted-average buy arithmetic, long-only sell with `Quantity >= sold`, realized PnL accumulation, closed-position reset).
  - **Trading module:** `Order` (full state machine `PENDING → PROCESSING → FILLED`, `PENDING/PROCESSING → REJECTED`, `PENDING → CANCELLED`, overfill guard `FilledQuantity <= Quantity`, market/limit contract), `Execution` (immutable, insert-only), `Transaction` (immutable financial snapshot with `GrossAmount` computed at creation).
  - **Identity / cross-cutting:** `Role`, `UserRole` (not a `BaseEntity` — composite key `(UserId, RoleId)` per physical spec), `AuditLog` (not a `BaseEntity` — `long` identity id, append-only factory).
- Enums in `src/FinancialPlatform.Domain/Enums/`: `PortfolioStatus`, `AssetType`, `AssetStatus`, `OrderSide`, `OrderType`, `OrderStatus` (stored as text in the eventual schema).
- Unit tests in `tests/FinancialPlatform.UnitTests/Domain/Entities/` covering creation validation, the order state machine (legal + illegal transitions), position arithmetic, portfolio/asset state transitions, symbol normalization, and snapshot entities — 138 new tests, full suite 175/175 green.

### Notes / scope

- Persistence (EF entity configurations, xmin concurrency tokens, migrations), repositories, handlers, controllers, and API endpoints for these modules are **not** part of this change — the DB model (`docs/04-database`) still marks those tables as Planned.
- `UserRole` deliberately does **not** extend `BaseEntity` (physical composite key; no `Id`/audit columns). `AuditLog` deliberately does **not** extend `BaseEntity` (`bigint` identity PK, no GUID).
- Design decision recorded from review: `Order.Fill` currently accepts a partial executed quantity and still marks the order `FILLED`; the MVP is strictly one full-fill execution per order — revisit when partial fills are introduced.