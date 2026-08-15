# Domain Model

## 1. Purpose

This document defines the core domain entities, value objects, aggregates, and domain relationships of the Financial Portfolio & Trading Operations Platform.

The model focuses on business behavior and invariants rather than persistence details.

---

# 2. Entity Overview

The initial domain model contains:

```text
User
Portfolio
Asset
Position
Order
Execution
Transaction
```

Supporting value objects include:

```text
Money
Quantity
AssetSymbol
OrderId
PortfolioId
UserId
```

---

# 3. User

## Responsibility

Represents an account holder using the platform.

## Key Attributes

```text
UserId
Email
DisplayName
Status
CreatedAt
```

## Behavior

- Activate.
- Disable.
- Update profile information.

---

# 4. Portfolio

## Responsibility

Represents an investment portfolio owned by a user.

## Key Attributes

```text
PortfolioId
UserId
Name
Status
CreatedAt
UpdatedAt
```

## Behavior

- Create portfolio.
- Rename portfolio.
- Activate portfolio.
- Disable portfolio.
- Add or update positions.
- Calculate portfolio value.

## Invariants

- A portfolio must belong to exactly one user.
- A disabled portfolio cannot accept new trading activity.
- A portfolio identifier must be unique.
- Portfolio state transitions must be valid.

---

# 5. Asset

## Responsibility

Represents a tradable financial instrument.

## Key Attributes

```text
AssetId
Symbol
Name
AssetType
Status
```

## Behavior

- Activate.
- Disable.

## Invariants

- Symbol must be unique within its asset universe.
- An inactive asset cannot be used for new orders.

---

# 6. Position

## Responsibility

Represents the quantity of an asset currently held by a portfolio.

## Key Attributes

```text
PositionId
PortfolioId
AssetId
Quantity
AverageEntryPrice
```

## Behavior

- Increase quantity.
- Decrease quantity.
- Recalculate average entry price.
- Calculate unrealized P/L.

## Invariants

- Position quantity must not become invalid.
- A sell operation must satisfy the applicable position rules.
- Average entry price must remain mathematically consistent with the position state.

---

# 7. Order

## Responsibility

Represents an investor's intent to buy or sell an asset.

## Key Attributes

```text
OrderId
PortfolioId
AssetId
Side
OrderType
Quantity
LimitPrice
Status
CreatedAt
UpdatedAt
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

- Create.
- Start processing.
- Fill.
- Reject.
- Cancel.

## Invariants

- Quantity must be greater than zero.
- Limit orders must have a valid limit price.
- An order must belong to an active portfolio.
- An order must reference an active asset.
- Invalid state transitions are rejected.
- A filled order cannot be cancelled.
- A cancelled order cannot be filled.

---

# 8. Execution

## Responsibility

Represents an execution result for an order.

## Key Attributes

```text
ExecutionId
OrderId
ExecutedQuantity
ExecutionPrice
ExecutedAt
```

## Invariants

- Executed quantity must be greater than zero.
- Executed quantity must not exceed the order quantity.
- An execution must belong to a valid order.
- The same execution must not be applied more than once.

---

# 9. Transaction

## Responsibility

Represents a completed financial transaction resulting from an order execution.

## Key Attributes

```text
TransactionId
OrderId
PortfolioId
AssetId
Side
Quantity
Price
ExecutedAt
```

## Characteristics

Transactions represent historical financial facts.

Once created, transaction records should be treated as immutable from normal application operations.

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
