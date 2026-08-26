# Database Design

## 1. Purpose

This document defines the physical database design of the Financial Portfolio & Trading Operations Platform:

- Storage technology and environment.
- Naming, type, and value conventions.
- Per-table specifications (columns, constraints, indexes).
- Concurrency, consistency, and idempotency mechanics at the persistence level.
- Migration and seeding strategy.

The logical model (entities, relationships, lifecycle) is defined in `data-model.md`.

### Status Legend

Same as `data-model.md`: **Implemented** (in code today) / **Planned** (MVP target) / **Future**.

---

# 2. Technology and Environment

| Aspect | Decision |
|---|---|
| Engine | PostgreSQL 16 (`postgres:16` in `docker-compose.yml`). |
| Provider | Npgsql EF Core provider. |
| Database | `financial_platform` (matches committed dev connection string; dev-only credentials). |
| Port | 5432 via docker compose; named volume `postgres_data`. |
| Access | EF Core only; no raw SQL in application code today. |
| Fallback | Missing `ConnectionStrings:DefaultConnection` → EF InMemory (`FinancialPlatformDemoDb`). The InMemory provider ignores the physical design below (no relational constraints, no concurrency tokens) — integration tests must not rely on it for these guarantees. |

Current schema state: created via `Database.EnsureCreatedAsync()` in `SeedData`; the Migrations folder is empty. See §12.

---

# 3. General Conventions

| Convention | Decision | Rationale |
|---|---|---|
| Identifier case | PascalCase table and column names (EF convention, e.g. `Users`, `RefreshTokens`), quoted by Npgsql automatically | Continuity with existing implemented schema; changing to snake_case would require renaming existing tables and a documented migration decision |
| Primary keys | `uuid`, generated client-side (`Guid.NewGuid()` in `BaseEntity`) | Works identically on InMemory fallback; no DB round-trip for ids. Random v4 GUIDs cause index fragmentation at scale — see §13 |
| Foreign keys | `uuid` referencing PKs, `ON DELETE NO ACTION` (restrict) | Financial records are never deleted; users are soft-disabled |
| Timestamps | `timestamptz`, UTC only (`DateTime.UtcNow`) | Npgsql maps UTC-kind DateTime to timestamptz |
| Booleans | `boolean` with CLR-side defaults | EF does not create DB defaults from initializers; inserts always supply values |
| Nullability | Follows domain model; reference/navigation FKs required unless logically optional | |
| Soft delete | Not used; status flags instead (`IsActive`, portfolio/asset `Status`) | Preserves referential integrity of historical facts |
| Audit base columns | Every domain entity carries `Id`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy` | Matches `BaseEntity`. Note: `UpdatedAt`/`UpdatedBy` are not written by an interceptor yet — flagged in §14 |

---

# 4. Value Domains

| Domain | Type | Notes |
|---|---|---|
| Money / prices / amounts | `numeric(18,4)` | Never float/double (domain rule). 14 integer digits is ample for simulated instruments. Configure `HasPrecision(18, 4)` so EF validates before insert |
| Quantities | `numeric(18,8)` | Supports fractional shares/ETF units while keeping 10 integer digits |
| Currency | `char(3)` ISO 4217 | Single currency (`USD`) across MVP |
| Symbols | `varchar(16)`, uppercase | Normalized on write in the domain layer |
| Enums | `text` (see §5) | |
| Identifiers | `uuid` / `bigint identity` (AuditLog only) | |

---

# 5. Enum Storage

Enums are stored as text via `HasConversion<string>()`.

Rationale:

- Human-readable rows during debugging and ad-hoc queries.
- Adding a value never breaks reads (unlike renumbering ints).
- Cardinality is tiny; storage cost is irrelevant.
- Domain layer + validation remain the source of truth.

Optional hardening (recommended when migrations land): add CHECK constraints, e.g.

```sql
ALTER TABLE "Orders"
  ADD CONSTRAINT ck_orders_status
  CHECK ("Status" IN ('PENDING','PROCESSING','FILLED','REJECTED','CANCELLED'));
```

Native PostgreSQL enum types are rejected for now: altering them requires lock-heavy DDL and complicates the InMemory fallback story.

---

# 6. Table Specifications

## 6.1. Users — Implemented

Created by EF conventions + `UserConfiguration`.

| Column | Type | Null | Constraints / Indexes |
|---|---|---|---|
| Id | uuid | No | PK (client-generated) |
| UserName | varchar(256) | No | |
| Email | varchar(256) | No | Unique index `IX_Users_Email`. Stored lowercase (normalized on write) |
| PasswordHash | varchar(512) | No | Hash only, never plaintext |
| FirstName | varchar(128) | Yes | |
| LastName | varchar(128) | Yes | |
| DisplayName | varchar(256) | Yes | |
| PhoneNumber | varchar(32) | Yes | |
| IsActive | boolean | No | CLR default true |
| CreatedAt / CreatedBy / UpdatedAt / UpdatedBy | audit | mixed | Inherited from BaseEntity |

Known gap: read paths compare email exactly against normalized (lowercased) stored values — see §14.

## 6.2. RefreshTokens — Implemented

| Column | Type | Null | Constraints / Indexes |
|---|---|---|---|
| Id | uuid | No | PK |
| UserId | uuid | No | FK → Users.Id; index `IX_RefreshTokens_UserId` |
| Token | varchar(256) | No | Unique index `IX_RefreshTokens_Token`; lookup key for rotation/validation |
| ExpiresAt | timestamptz | No | Must be future at creation (domain-enforced) |
| RevokedAt | timestamptz | Yes | Set idempotently by Revoke() |
| audit columns | | | BaseEntity |

FK delete behavior currently follows EF convention for required relationships (cascade) because no `DeleteBehavior` is configured. Target: explicit `DeleteBehavior.NoAction` when configurations are revisited — see §14.

Candidate index improvement: partial unique index allowing only one active token per user if single-session policy is adopted later.

## 6.3. PasswordResetTokens — Implemented

| Column | Type | Null | Constraints / Indexes |
|---|---|---|---|
| Id | uuid | No | PK |
| UserId | uuid | No | FK → Users.Id; index `IX_PasswordResetTokens_UserId` |
| Code | varchar(16) | No | Non-unique index `IX_PasswordResetTokens_Code` (codes are short-lived OTPs) |
| ExpiresAt | timestamptz | No | |
| UsedAt | timestamptz | Yes | Consumed once via MarkUsed() |
| audit columns | | | BaseEntity |

## 6.4. Roles / UserRoles — Planned

Roles seeded once (`Investor`, `Administrator`); UserRoles assigned at registration and by admins.

| Table | Key columns | Constraints / Indexes |
|---|---|---|
| Roles | Id uuid PK; Name varchar(64) | Unique index on Name |
| UserRoles | UserId + RoleId | Composite PK; FKs → Users, Roles; `AssignedAt timestamptz NOT NULL` |

Authorization checks join Users → UserRoles → Roles; role claims ride inside the JWT, so this join is not on any hot path.

## 6.5. Portfolios — Planned

| Column | Type | Null | Constraints / Indexes |
|---|---|---|---|
| Id | uuid | No | PK |
| UserId | uuid | No | FK → Users.Id; index `IX_Portfolios_UserId` |
| Name | varchar(128) | No | Unique composite `(UserId, Name)` — prevents duplicate names per owner |
| BaseCurrency | char(3) | No | Default 'USD' (CLR-side) |
| Status | text | No | PortfolioStatus enum as text |
| Notes | varchar(512) | Yes | |
| audit columns | | | BaseEntity |
| Concurrency | xmin | | System-column token (§8) |

Portfolio valuation (value, P/L) is computed on read — deliberately not persisted.

## 6.6. Assets — Planned

Reference-style table, seeded and admin-managed; prices refreshed by market-data module (FR-022..023).

| Column | Type | Null | Constraints / Indexes |
|---|---|---|---|
| Id | uuid | No | PK |
| Symbol | varchar(16) | No | Unique index `IX_Assets_Symbol`; uppercase |
| Name | varchar(256) | No | |
| AssetType | text | No | AssetType enum as text |
| CurrentPrice | numeric(18,4) | No | Updated in place by price refresh |
| Currency | char(3) | No | Default 'USD' |
| PriceUpdatedAt | timestamptz | No | Staleness indicator for consumers |
| Status | text | No | ACTIVE blocks nothing; INACTIVE blocks new orders (BR-007) |
| audit columns | | | BaseEntity |

Search by symbol should be an exact/prefix match on the unique index; fuzzy name search is deferred (§13).

## 6.7. Positions — Planned

Hot mutable state; every execution that fills an order updates exactly one row here.

| Column | Type | Null | Constraints / Indexes |
|---|---|---|---|
| Id | uuid | No | PK |
| PortfolioId | uuid | No | FK → Portfolios.Id |
| AssetId | uuid | No | FK → Assets.Id |
| Quantity | numeric(18,8) | No | ≥ 0 enforced in domain (long-only MVP, BR-023) |
| AverageEntryPrice | numeric(18,4) | Yes | Null until first BUY; weighted-average maintained by domain |
| RealizedPnL | numeric(18,4) | No | Default 0 |
| audit columns | | | BaseEntity |

Constraints and indexes:

- Unique composite `(PortfolioId, AssetId)` — enforces BR-020 at the storage level.
- Composite index `(PortfolioId, AssetId)` serves the unique constraint and the standard position-list query.
- RowVersion: `UseXminAsConcurrencyToken()` (§8).

Rows are created on first buy, updated thereafter, never deleted (closed = quantity 0).

## 6.8. Orders — Planned

Highest-write-volume business table; also the state machine record (BR-012).

| Column | Type | Null | Constraints / Indexes |
|---|---|---|---|
| Id | uuid | No | PK |
| PortfolioId | uuid | No | FK → Portfolios.Id |
| AssetId | uuid | No | FK → Assets.Id |
| Side | text | No | OrderSide |
| OrderType | text | No | OrderType |
| Quantity | numeric(18,8) | No | > 0 (BR-008) |
| LimitPrice | numeric(18,4) | Yes | Required iff LIMIT (BR-011, domain-enforced) |
| FilledQuantity | numeric(18,8) | No | Default 0; ≤ Quantity (BR-017) |
| Status | text | No | OrderStatus |
| RejectionReason | varchar(256) | Yes | Set on REJECTED |
| CompletedAt | timestamptz | Yes | Terminal transition timestamp (fill/reject/cancel) |
| audit columns | | | BaseEntity |

Indexes (query patterns from FR-015 / US-010):

| Index | Columns | Serves |
|---|---|---|
| `IX_Orders_PortfolioId_CreatedAt` | PortfolioId, CreatedAt DESC | Order history, default sort |
| `IX_Orders_PortfolioId_Status` | PortfolioId, Status | Filter by status |
| `IX_Orders_AssetId` | AssetId | Filter by asset |
| `IX_Orders_Status_CreatedAt` | Status, CreatedAt | Admin/engine scan of pending orders |

Ownership check (`UserId`) is resolved via the parent Portfolio join rather than denormalizing UserId onto Orders.

Concurrency: xmin token (§8).

## 6.9. Executions — Planned

| Column | Type | Null | Constraints / Indexes |
|---|---|---|---|
| Id | uuid | No | PK |
| OrderId | uuid | No | FK → Orders.Id; **unique** index `IX_Executions_OrderId` (MVP full fill) |
| ExecutedQuantity | numeric(18,8) | No | > 0, ≤ Order.Quantity |
| ExecutionPrice | numeric(18,4) | No | Simulated fill price |
| Fee | numeric(18,4) | No | Default 0 |
| ExecutedAt | timestamptz | No | |
| audit columns | | | BaseEntity |

The unique index on `OrderId` is the primary idempotency mechanism (FR-029, BR-019): replaying an execution insert violates the constraint instead of duplicating financial effects. When partial fills arrive, drop uniqueness and enforce `SUM(ExecutedQuantity) ≤ Order.Quantity` in the domain plus a deferred-check strategy.

## 6.10. Transactions — Planned

Immutable financial fact; insert-only (BR-026). Denormalized attributes are snapshots taken at creation and never recomputed.

| Column | Type | Null | Constraints / Indexes |
|---|---|---|---|
| Id | uuid | No | PK |
| OrderId | uuid | No | FK → Orders.Id; indexed (traceability, BR-027) |
| ExecutionId | uuid | No | FK → Executions.Id; **unique** in MVP — second idempotency guard |
| PortfolioId | uuid | No | FK → Portfolios.Id (copy) |
| AssetId | uuid | No | FK → Assets.Id (copy) |
| Side | text | No | Copy of order side |
| Quantity | numeric(18,8) | No | Copy of executed quantity |
| Price | numeric(18,4) | No | Copy of execution price |
| GrossAmount | numeric(18,4) | No | `Quantity × Price` snapshot |
| Fee | numeric(18,4) | No | Copy |
| ExecutedAt | timestamptz | No | Business time from execution |
| audit columns | | | BaseEntity |

Indexes (query patterns from FR-019 / US-011):

| Index | Columns | Serves |
|---|---|---|
| UQ `IX_Transactions_ExecutionId` | ExecutionId | Idempotency |
| `IX_Transactions_PortfolioId_ExecutedAt` | PortfolioId, ExecutedAt DESC | History default view |
| `IX_Transactions_PortfolioId_AssetId_ExecutedAt` | PortfolioId, AssetId, ExecutedAt | Filter by asset |
| `IX_Transactions_OrderId` | OrderId | Trace drill-down |

No UPDATE path exists in application code by design; consider revoking UPDATE/DELETE privileges at the DB level in production hardening.

## 6.11. AuditLogs — Planned

Append-only; intentionally *not* a `BaseEntity` aggregate (no Guid PK, no update tracking).

| Column | Type | Null | Constraints / Indexes |
|---|---|---|---|
| Id | bigint | No | PK, `GENERATED ALWAYS AS IDENTITY` |
| ActorUserId | uuid | Yes | FK → Users.Id; null for system actions |
| Action | varchar(64) | No | e.g. `USER_REGISTERED`, `ORDER_CANCELLED` |
| EntityType | varchar(64) | Yes | Affected resource type |
| EntityId | varchar(64) | Yes | Affected resource id (string — polymorphic) |
| OccurredAt | timestamptz | No | DB default `now()` |
| IpAddress | varchar(45) | Yes | IPv4/IPv6 |
| Details | jsonb | Yes | Structured payload (before/after summaries, metadata) |

Indexes: `(ActorUserId, OccurredAt DESC)` and `(EntityType, EntityId, OccurredAt DESC)` for US-013/US-016 review flows.

Write pattern: same DB transaction as the business change where possible (FR-027); security events that must survive business rollback (e.g. failed login) may write in their own transaction. Growth management in §13.

---

# 7. Referential Graph Summary

```text
Users ──┬── RefreshTokens
        ├── PasswordResetTokens
        ├── Portfolios ──┬── Orders ──── Executions ──── Transactions
        │                ├── Positions                     │
        │                └── Transactions ─────────────────┤   (OrderId + ExecutionId)
        ├── Roles ◄── UserRoles
        └── AuditLogs

Assets ── Orders / Positions / Transactions   (reference data)
```

All edges restrict deletes; the graph is acyclic; no cross-module cascades exist.

---

# 8. Concurrency Control

Requirement: FR-028, BR-034, BR-035.

Mechanism: optimistic concurrency using the PostgreSQL system column `xmin`:

```csharp
modelBuilder.Entity<Position>().Property(p => p.Xmin).IsRowVersion(); // or UseXminAsConcurrencyToken()
```

Applied to: **Portfolios, Positions, Orders** (the mutable, financially sensitive tables). Immutable fact tables (Transactions, Executions, AuditLogs) need no tokens — they are never updated.

Behavior:

- Conflicting concurrent writes → `DbUpdateConcurrencyException` → mapped to HTTP 409 by the API layer; clients retry the operation.
- Position arithmetic races (two concurrent sells) are serialized by optimistic checks combined with the domain invariant `Quantity − Sold ≥ 0`; the loser fails cleanly.
- Order state transitions remain atomic within a single row update guarded by the xmin check (BR-035).
- Pessimistic locks (`FOR UPDATE`) or serializable isolation are not used in the MVP; revisit if contention becomes measurable.

Note: the InMemory fallback provider has no xmin support — concurrency behavior can only be verified against PostgreSQL (integration tests, Testcontainers).

---

# 9. Consistency and Idempotency

Requirements: FR-017, FR-027, FR-029; BR-031..033; US-017, US-018.

Consistency boundary: one application-level unit of work (single EF `SaveChanges` → single PostgreSQL transaction) covers the entire execution outcome:

```text
BEGIN
  1. UPDATE Orders        SET Status/FilledQuantity/CompletedAt
  2. INSERT Executions    (guarded by UQ(OrderId))
  3. INSERT Transactions  (guarded by UQ(ExecutionId))
  4. UPDATE/INSERT Positions (guarded by xmin)
COMMIT
```

Guarantees:

- Partial failure rolls back everything — no orphan executions or stale positions (BR-033).
- Duplicate processing attempts violate the unique guards and abort — one execution yields at most one transaction and one position effect (BR-032).
- Application code treats unique-violation on these indexes as an expected idempotent-success signal, not an error surfaced to users.
- Audit entries for successful executions commit atomically with them.

---

# 10. Security Considerations

- Only password hashes (max 512 chars) are stored; tokens/codes are opaque values.
- Committed dev credentials in `appsettings.json` are placeholders for local development only; production uses environment variables/user-secrets/NFR-001 practices.
- Least-privilege DB role (SELECT/INSERT/UPDATE only, no DDL) is a production-hardening item; append-only enforcement on Transactions/AuditLogs via grants complements the application rules.
- PII is limited to contact fields (names, email, phone); jsonb audit payloads must exclude credentials and hashes.
- Email uniqueness relies on write-time lowercase normalization; see §14 for the read-path gap.

---

# 11. Seeding Strategy

Current behavior (`SeedData.SeedAsync`, runs every startup):

1. `EnsureCreatedAsync()` builds schema if absent (dev convenience; see §12).
2. Seeds demo user `demo@financialplatform.com` when the Users table is empty.

Planned evolution (as modules land):

| Seed | When | Contents |
|---|---|---|
| Roles | Phase 2+ | Investor, Administrator (idempotent upsert) |
| Assets + prices | Phase 2 | Initial STOCK/ETF universe per FR-007; prices refreshed by market-data module |
| Demo user/portfolios | Dev only | Guarded by environment so production seeds reference data only |

Seeding must be idempotent (safe on every startup) — matching the current pattern.

---

# 12. Migrations

Current state: zero migrations; schema originates from `EnsureCreatedAsync()`. Acceptable for the Identity phase; becomes a liability as soon as planned tables appear, because EnsureCreated cannot evolve a schema.

Target approach (introduce before Phase 2 work merges):

```bash
dotnet ef migrations add Init --project src/FinancialPlatform.Infrastructure \
  --startup-project src/FinancialPlatform.Api
dotnet ef database update --project src/FinancialPlatform.Infrastructure \
  --startup-project src/FinancialPlatform.Api
```

Rules:

- One migration per coherent schema change, named after the feature (`AddPortfoliosAndAssets`, `AddTradingTables`, ...).
- First real migration must be reconciled against the EnsureCreated-shaped identity schema (either baseline the existing schema or accept a recreate in dev — decide explicitly, document in changelog).
- Migrations live in Infrastructure; they may configure provider-specific features (xmin tokens, CHECK constraints, identity columns) that the model cannot express portably.
- docker-compose consumers get schema via `database update` (or migrate-on-startup behind an env flag), replacing EnsureCreated.

---

# 13. Performance Considerations

Deferred until measured (Phase 5 discipline), but anticipated here so early choices do not block them:

| Topic | Plan |
|---|---|
| GUID index locality | Random v4 PKs fragment B-trees under load. Options: sequential GUID generator, or UUIDv7 when the platform supports it. No action until write volume justifies it |
| Asset search | Exact/prefix symbol lookups use the unique index; fuzzy name search would need pg_trgm GIN index — add only when search UX requires it |
| Covering indexes | Add INCLUDE columns after observing real query plans |
| AuditLogs growth | Append-heavy; plan range partitioning by OccurredAt (monthly) when volume warrants; archival policy afterwards |
| Transactions history | Same partitioning option by ExecutedAt; indexes above already lead with the tenant/portfolio key |
| Price staleness | Readers should treat Assets.CurrentPrice + PriceUpdatedAt as cacheable reference data (NFR-002 allows caching) |
| Connection pooling | Standard Npgsql pooling; no per-request new connections |

---

# 14. Known Gaps and Decisions Log

Tracked deviations between current implementation and this design; each needs a deliberate follow-up rather than silent drift:

| # | Item | Current State | Direction |
|---|---|---|---|
| 1 | Email case sensitivity | Stored emails lowercased on write, but login/register lookups compare raw input exactly — mixed-case input fails auth | Normalize query input (or store a separate normalizer column) |
| 2 | FK delete behavior | RefreshToken/PasswordResetToken FKs inherit EF's cascade-by-convention | Make delete rules explicit (`NoAction`) in configurations |
| 3 | UpdatedAt/UpdatedBy | Base audit columns exist but nothing maintains them on update | Introduce a SaveChanges interceptor when the first mutable business entity lands |
| 4 | Schema creation | `EnsureCreatedAsync`, empty Migrations folder | Adopt migrations before Phase 2 (§12) |
| 5 | Concurrency tokens | None exist yet | Add xmin tokens with the first planned tables (§8) |
| 6 | Decimal precision | No decimal properties yet | Apply `HasPrecision(18,4)/(18,8)` consistently as money/quantity columns are introduced (§4) |
| 7 | CHECK constraints for enums | Text enums validated only in domain | Add DB CHECK constraints alongside migrations (§5) |

Changes to any of these should be reflected here, in the changelog, and — where architectural — in `docs/03-architecture/decisions/`.
