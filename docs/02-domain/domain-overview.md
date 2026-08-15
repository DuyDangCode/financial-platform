# Domain Overview

## 1. Purpose

This document provides a high-level overview of the business domain represented by the Financial Portfolio & Trading Operations Platform.

The domain model represents financial concepts and business rules independently of HTTP, databases, frameworks, and infrastructure technologies.

---

# 2. Domain

The core domain is **Investment Portfolio and Trading Operations**.

The system models the lifecycle of financial orders and their effects on portfolios.

The central business flow is:

```text
Portfolio
    ↓
Order
    ↓
Execution
    ↓
Transaction
    ↓
Position
    ↓
Portfolio Valuation
```

---

# 3. Core Domain Concepts

## 3.1. User

A User represents an individual who interacts with the platform.

A user may own one or more portfolios.

---

## 3.2. Portfolio

A Portfolio represents a collection of investment positions managed by a user.

A portfolio:

- Belongs to a user.
- Contains positions.
- Contains trading activity.
- Has its own lifecycle.
- Represents an independent investment context.

---

## 3.3. Asset

An Asset represents a tradable financial instrument.

Examples include:

- Stocks.
- ETFs.

An asset has a unique market symbol within its asset universe.

---

## 3.4. Position

A Position represents an asset currently held by a portfolio.

A position is derived from executed trading activity.

A position contains concepts such as:

- Asset.
- Quantity.
- Average entry price.
- Current valuation.

---

## 3.5. Order

An Order represents an investor's intention to buy or sell an asset.

An order contains:

- Portfolio.
- Asset.
- Side.
- Quantity.
- Order type.
- Status.

An order has a defined lifecycle.

---

## 3.6. Execution

An Execution represents the successful or attempted execution of an order.

It captures the execution outcome, including information such as:

- Execution price.
- Executed quantity.
- Execution timestamp.

---

## 3.7. Transaction

A Transaction represents a completed financial movement resulting from an execution.

Transactions provide an immutable historical record of trading activity.

---

## 3.8. Market Price

A Market Price represents the currently available price of an asset.

Market prices are external information from the perspective of the core domain.

The domain should not depend on a specific market-data provider.

---

## 3.9. Audit Event

An Audit Event represents an important action or state change that must be traceable.

Examples:

- Order created.
- Order cancelled.
- Order executed.
- Portfolio modified.

---

# 4. Domain Relationships

The major relationships are:

```text
User
 └── Portfolio
      ├── Position
      │    └── Asset
      │
      ├── Order
      │    └── Asset
      │
      └── Transaction
           └── Asset
```

The trading workflow creates a temporal relationship:

```text
Order
  ↓
Execution
  ↓
Transaction
  ↓
Position
```

---

# 5. Core Domain Rules

The domain must guarantee important invariants.

Examples include:

- An order must have a valid quantity.
- An order must reference a valid asset.
- An order must belong to a portfolio.
- An order can only transition between valid states.
- A filled order cannot be cancelled.
- A position cannot become invalid through an execution.
- The same execution must not create duplicate financial effects.

Detailed rules are documented in `business-rules.md`.

---

# 6. Domain Boundaries

The domain must remain independent from:

- ASP.NET Core.
- Entity Framework Core.
- PostgreSQL.
- Redis.
- Kafka.
- HTTP.
- External APIs.
- Cloud providers.

The domain may define abstractions where required, while concrete infrastructure implementations remain outside the domain.

---

# 7. Aggregate Candidates

The initial domain model identifies the following aggregate candidates:

### Portfolio Aggregate

Responsible for:

- Portfolio state.
- Positions.
- Portfolio-level invariants.

### Order Aggregate

Responsible for:

- Order lifecycle.
- Order state transitions.
- Order-level invariants.

User management may remain a separate domain model from the core trading domain.

The final aggregate boundaries may evolve as implementation and requirements become clearer.

---

# 8. Domain Events

The domain may produce events representing meaningful business state changes.

Potential events include:

```text
OrderCreated
OrderCancelled
OrderFilled
OrderRejected
PositionUpdated
TransactionCreated
```

Domain events should represent business facts rather than infrastructure operations.

For example:

```text
OrderFilled
```

is a domain fact.

While:

```text
PublishOrderFilledToKafka
```

is an infrastructure concern.

---

# 9. Domain Philosophy

The domain model should represent business concepts rather than database tables.

The system should avoid creating domain entities solely because a database table exists.

Similarly, database implementation details should not determine core business rules.

The domain model should remain understandable even if:

- The database changes.
- The API framework changes.
- The message broker changes.
- The application becomes distributed.
