# Domain Model

## 1. Purpose

This document defines the core domain entities, value objects, aggregates, and domain relationships of the Financial Portfolio & Trading Operations Platform.

The model focuses on business behavior and invariants rather than persistence details.

---

# 2. Entity Overview

The domain model (`src/FinancialPlatform.Domain/Entities/`) contains:

```text
Identity & Cross-Cutting      Portfolio Module          Trading Module
├── User                      ├── Portfolio             ├── Order
├── RefreshToken              ├── Asset                 ├── Execution
├── PasswordResetToken        └── Position              └── Transaction
├── Role
├── UserRole
└── AuditLog
```

Supporting enums (`src/FinancialPlatform.Domain/Enums/`) include:

```text
PortfolioStatus (ACTIVE, DISABLED)
AssetType       (STOCK, ETF)
AssetStatus     (ACTIVE, INACTIVE)
OrderSide       (BUY, SELL)
OrderType       (MARKET, LIMIT)
OrderStatus     (PENDING, PROCESSING, FILLED, REJECTED, CANCELLED)
```

Supporting value objects (conceptual / planned refactoring targets) include:

```text
Money
Quantity
AssetSymbol
OrderId
PortfolioId
UserId
```

---

# 3. Identity & Audit Entities

## 3.1. User (`User.cs` — Implemented)

### Responsibility

Represents an account holder using the platform.

### Key Attributes

```text
Id (Guid, inherited from BaseEntity)
UserName (string, required, <= 256 chars, trimmed)
Email (string, required, <= 256 chars, normalized lowercase)
PasswordHash (string, required, <= 512 chars)
FirstName (string?)
LastName (string?)
DisplayName (string?)
PhoneNumber (string?)
IsActive (bool, default true)
CreatedAt / CreatedBy / UpdatedAt / UpdatedBy (from BaseEntity)
```

### Behavior

- `User.Create(userName, email, passwordHash, firstName, lastName, displayName, phoneNumber)` — Factory validating invariants and normalizing email to lowercase.
- `Activate()` — Sets `IsActive = true`.
- `Deactivate()` — Sets `IsActive = false`.
- `UpdateProfile(firstName, lastName, displayName, phoneNumber)` — Updates optional profile fields.
- `SetPassword(passwordHash)` — Replaces password hash after validation.

---

## 3.2. RefreshToken & PasswordResetToken (`RefreshToken.cs`, `PasswordResetToken.cs` — Implemented)

- **`RefreshToken`**: Tracks a 7-day opaque session renewal token (`UserId`, `Token`, `ExpiresAt`, `RevokedAt`, computed `IsActive`). Supports `RefreshToken.Create(...)` and idempotent `Revoke()`.
- **`PasswordResetToken`**: Tracks a 15-minute one-time reset code (`UserId`, `Code`, `ExpiresAt`, `UsedAt`, computed `IsActive`). Supports `PasswordResetToken.Create(...)` and `MarkUsed()`.

---

## 3.3. Role & UserRole (`Role.cs`, `UserRole.cs` — Implemented in Domain)

- **`Role`**: Named authorization role (`Id`, `Name` <= 64 chars). Created via `Role.Create(name)`. Seeded roles: `Investor`, `Administrator`.
- **`UserRole`**: Join entity (`UserId`, `RoleId`, `AssignedAt`). Intentionally does **not** inherit `BaseEntity` because the physical table uses a composite primary key `(UserId, RoleId)`. Created via `UserRole.Create(userId, roleId)`.

---

## 3.4. AuditLog (`AuditLog.cs` — Implemented in Domain)

- **`AuditLog`**: Append-only record of business and security events (`Id` as `long` database identity, `ActorUserId`, `Action` <= 64 chars, `EntityType` <= 64 chars, `EntityId` <= 64 chars, `OccurredAt`, `IpAddress` <= 45 chars, `Details` JSON string). Intentionally does **not** inherit `BaseEntity`. Created via `AuditLog.Create(...)`.

---

# 4. Portfolio (`Portfolio.cs` — Implemented in Domain)

## Responsibility

Represents an investment portfolio owned by a user.

## Key Attributes

```text
Id (Guid)
UserId (Guid, non-empty)
Name (string, required, <= 128 chars, trimmed)
BaseCurrency (string, 3-letter ISO 4217 uppercase, default "USD")
Status (PortfolioStatus: ACTIVE | DISABLED, default ACTIVE)
Notes (string?, <= 512 chars)
CreatedAt / UpdatedAt (from BaseEntity)
```

## Behavior

- `Portfolio.Create(userId, name, baseCurrency = "USD", notes = null)` — Factory method.
- `Rename(name)` — Validates and updates `Name`.
- `Activate()` — Sets `Status = PortfolioStatus.ACTIVE`.
- `Disable()` — Sets `Status = PortfolioStatus.DISABLED`.

## Invariants

- A portfolio must belong to exactly one user (`UserId != Guid.Empty`).
- `Name` is required and at most 128 characters; unique per user at the persistence level.
- `BaseCurrency` is normalized to a 3-character uppercase ISO code (default `USD`).
- A `DISABLED` portfolio cannot accept new trading activity (BR-004).

---

# 5. Asset (`Asset.cs` — Implemented in Domain)

## Responsibility

Represents a tradable financial instrument and its latest simulated market price.

## Key Attributes

```text
Id (Guid)
Symbol (string, required, <= 16 chars, normalized uppercase)
Name (string, required, <= 256 chars, trimmed)
AssetType (AssetType: STOCK | ETF)
CurrentPrice (decimal, >= 0)
Currency (string, 3-letter ISO 4217 uppercase, default "USD")
PriceUpdatedAt (DateTime UTC)
Status (AssetStatus: ACTIVE | INACTIVE, default ACTIVE)
```

## Behavior

- `Asset.Create(symbol, name, assetType, currentPrice, currency = "USD")` — Normalizes `Symbol` and `Currency` to uppercase and sets `PriceUpdatedAt = DateTime.UtcNow`.
- `RefreshPrice(price)` — Updates `CurrentPrice` (must be `>= 0`) and refreshes `PriceUpdatedAt = DateTime.UtcNow`.
- `Activate()` — Sets `Status = AssetStatus.ACTIVE`.
- `Deactivate()` — Sets `Status = AssetStatus.INACTIVE`.

## Invariants

- `Symbol` must be non-empty, uppercase, `<= 16` chars, and unique across the asset catalog.
- `CurrentPrice` must not be negative (`>= 0`).
- An `INACTIVE` asset cannot be used for new orders (BR-007).

---

# 6. Position (`Position.cs` — Implemented in Domain)

## Responsibility

Represents the quantity and cost basis of one asset held within one portfolio.

## Key Attributes

```text
Id (Guid)
PortfolioId (Guid, non-empty)
AssetId (Guid, non-empty)
Quantity (decimal, >= 0)
AverageEntryPrice (decimal?, null when Quantity == 0)
RealizedPnL (decimal, default 0)
IsClosed (computed bool: Quantity == 0)
```

## Behavior

- `Position.Create(portfolioId, assetId)` — Initializes a zero-quantity closed position (`Quantity = 0`, `AverageEntryPrice = null`, `RealizedPnL = 0`).
- `ApplyBuy(quantity, price)` — Increases `Quantity` and updates `AverageEntryPrice` using the weighted-average cost formula:
  - If `Quantity == 0`: `AverageEntryPrice = price`
  - If `Quantity > 0`: `AverageEntryPrice = (previousQty * previousAvg + quantity * price) / (previousQty + quantity)`
- `ApplySell(quantity, price)` — Enforces long-only rule (`quantity <= Quantity`), accumulates `RealizedPnL += (price - AverageEntryPrice) * quantity`, decreases `Quantity -= quantity`, and resets `AverageEntryPrice = null` when `Quantity == 0`.

## Invariants

- `Quantity` must never be negative (`Quantity >= 0`, long-only MVP per BR-023).
- Both buy and sell quantities and prices must be strictly positive (`> 0`).
- Selling more than the held `Quantity` throws `DomainException`.
- When `Quantity == 0` (closed position), `AverageEntryPrice` is `null`.

---

# 7. Order (`Order.cs` — Implemented in Domain)

## Responsibility

Represents an investor's intent to buy or sell an asset, enforcing the order state machine.

## Key Attributes

```text
Id (Guid)
PortfolioId (Guid, non-empty)
AssetId (Guid, non-empty)
Side (OrderSide: BUY | SELL)
OrderType (OrderType: MARKET | LIMIT)
Quantity (decimal, > 0)
LimitPrice (decimal?, required and > 0 when LIMIT; null when MARKET)
FilledQuantity (decimal, default 0, <= Quantity)
RemainingQuantity (computed decimal: Quantity - FilledQuantity)
Status (OrderStatus: PENDING | PROCESSING | FILLED | REJECTED | CANCELLED)
RejectionReason (string?, <= 256 chars, set on REJECTED)
CompletedAt (DateTime? UTC, set on FILLED, REJECTED, or CANCELLED)
CreatedAt / UpdatedAt (from BaseEntity)
```

## Order Side

```text
BUY
SELL
```

## Order Type

```text
MARKET
LIMIT
```

## Order Status

```text
PENDING
PROCESSING
FILLED
REJECTED
CANCELLED
```

## Behavior

- `Order.Create(portfolioId, assetId, side, orderType, quantity, limitPrice = null)` — Validates inputs; requires positive `limitPrice` for `LIMIT` orders and ignores/nullifies `limitPrice` for `MARKET` orders. Starts in `PENDING`.
- `StartProcessing()` — Transitions `PENDING → PROCESSING`.
- `Fill(executedQuantity)` — Transitions `PROCESSING → FILLED`, adds `executedQuantity` to `FilledQuantity` (must satisfy `0 < executedQuantity <= RemainingQuantity`), and sets `CompletedAt = DateTime.UtcNow`.
- `Reject(reason)` — Transitions `PENDING` or `PROCESSING → REJECTED`, records trimmed `RejectionReason` (`1..256` chars), and sets `CompletedAt = DateTime.UtcNow`.
- `Cancel()` — Transitions `PENDING → CANCELLED` and sets `CompletedAt = DateTime.UtcNow`.

## Invariants

- `Quantity > 0`.
- `LIMIT` orders require `LimitPrice > 0`; `MARKET` orders always have `LimitPrice == null`.
- State transitions strictly follow the state machine (BR-012).
- `FILLED`, `REJECTED`, and `CANCELLED` are terminal states (`CompletedAt != null`).

---

# 8. Execution (`Execution.cs` — Implemented in Domain)

## Responsibility

Represents an immutable execution record generated when the simulated execution engine fills an order.

## Key Attributes

```text
Id (Guid)
OrderId (Guid, non-empty)
ExecutedQuantity (decimal, > 0)
ExecutionPrice (decimal, > 0)
Fee (decimal, >= 0, default 0)
ExecutedAt (DateTime UTC)
```

## Behavior

- `Execution.Create(orderId, executedQuantity, executionPrice, fee = 0, executedAt = null)` — Creates an immutable execution record.

## Invariants

- `ExecutedQuantity > 0` and `ExecutionPrice > 0`.
- `Fee >= 0`.
- `ExecutedQuantity <= Order.Quantity` (enforced by `Order.Fill(executedQuantity)` during the execution workflow).
- One execution per order in the MVP (enforced by unique index `IX_Executions_OrderId`).

---

# 9. Transaction (`Transaction.cs` — Implemented in Domain)

## Responsibility

Represents an immutable historical financial transaction produced by a successful order execution.

## Key Attributes

```text
Id (Guid)
OrderId (Guid, non-empty)
ExecutionId (Guid, non-empty)
PortfolioId (Guid, non-empty)
AssetId (Guid, non-empty)
Side (OrderSide: BUY | SELL)
Quantity (decimal, > 0)
Price (decimal, > 0)
GrossAmount (decimal, computed at creation as Quantity * Price)
Fee (decimal, >= 0, default 0)
ExecutedAt (DateTime UTC)
```

## Behavior

- `Transaction.Create(orderId, executionId, portfolioId, assetId, side, quantity, price, fee = 0, executedAt = null)` — Creates an immutable financial snapshot with `GrossAmount = quantity * price`.

## Characteristics

Transactions represent historical financial facts. Once created, transaction records are never modified or deleted.

---

# 10. Value Objects

## 10.1. Money

Represents a monetary amount and currency.

```text
Amount
Currency
```

Money should not be represented using floating-point arithmetic.

---

## 10.2. Quantity

Represents the quantity of an asset.

The representation should support the required precision for the supported asset types.

---

## 10.3. AssetSymbol

Represents a normalized market symbol.

Examples:

```text
AAPL
MSFT
SPY
```

---

# 11. Aggregates

## 11.1. Portfolio Aggregate

The Portfolio aggregate protects portfolio-level invariants.

```text
Portfolio
 ├── Position
 └── Portfolio-level rules
```

Potential responsibilities:

- Manage portfolio state.
- Apply position changes.
- Protect portfolio-level invariants.

---

## 11.2. Order Aggregate

The Order aggregate protects order lifecycle rules.

```text
Order
 └── Order state machine
```

Potential responsibilities:

- Validate order state.
- Start processing.
- Fill.
- Reject.
- Cancel.

---

# 12. Aggregate Interaction

The aggregates interact through application services and domain events.

Example:

```text
PlaceOrder
    ↓
Order Aggregate
    ↓
OrderCreated
    ↓
Order Processing
    ↓
OrderFilled
    ↓
Transaction Created
    ↓
Portfolio / Position Updated
```

An aggregate should not directly depend on another aggregate's infrastructure implementation.

---

# 13. Domain Services

Domain services should be introduced only when a business rule does not naturally belong to a single entity or aggregate.

Potential domain services include:

```text
PortfolioValuationService
OrderExecutionService
PnlCalculationService
```

These should contain domain logic rather than application orchestration or infrastructure operations.

---

# 14. Domain Events

Potential domain events include:

```text
OrderCreated
OrderProcessingStarted
OrderFilled
OrderRejected
OrderCancelled
PositionUpdated
TransactionCreated
```

Events represent facts that have already happened.

They should not contain infrastructure-specific behavior.

---

# 15. Domain Model Evolution

The domain model is expected to evolve as the project gains more realistic requirements.

New entities, value objects, aggregates, or domain services should only be introduced when they represent meaningful business concepts or protect important business rules.
