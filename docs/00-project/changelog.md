# Changelog

All notable changes to this project are documented in this file, newest first.

---

## 2026-11-01 — Complete project, domain, API, security, testing, infrastructure, operations, and UI documentation

### Added

- Project roadmap in `docs/00-project/roadmap.md` detailing Phases 1–7 and current implementation status across layers.
- Architecture Decision Records in `docs/03-architecture/decisions/`: `ADR-001.md` (Modular Monolith & Clean Architecture), `ADR-002.md` (PostgreSQL with EF Core & InMemory Fallback), and `ADR-003.md` (Stateless JWT Bearer with Stateful Refresh Token Rotation).
- Complete REST API endpoint specifications in `docs/05-api/`: `account.md`, `asset.md`, `order.md`, `transaction.md`, `dashboard.md`, `user.md`, `role.md`, `permission.md`, and updated module tables in `api-design.md`.
- Security specifications in `docs/06-security/authentication.md` and `docs/06-security/authorization.md`.
- Testing strategy and per-module test scenarios in `docs/07-testing/testing-strategy.md`.
- Infrastructure and local setup guides in `docs/08-infrastructure/local-development.md`, `docker.md`, and `deployment.md`.
- Operations and runbook documentation in `docs/09-operations/logging.md`, `monitoring.md`, and `troubleshooting.md`.
- Frontend screen design specifications in `docs/10-frontend/`: `dashboard.md`, `portfolios.md`, `trading.md`, `transactions.md`, and updated `pages.md`.

### Changed

- Enriched `docs/02-domain/domain-model.md` and `docs/02-domain/business-rules.md` with exact domain entity contracts, weighted-average position arithmetic, realized/unrealized/total P/L formulas, and simulated `MARKET` vs. `LIMIT` order execution rules.

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