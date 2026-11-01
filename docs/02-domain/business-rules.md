# Business Rules

## 1. Purpose

This document defines the business rules and invariants of the Financial Portfolio & Trading Operations Platform.

Business rules describe what the system must consider valid or invalid from a domain perspective.

These rules should remain independent of API, database, and infrastructure implementation.

---

# 2. User Rules

## BR-001 — User Identity Must Be Unique

A user cannot register multiple accounts using the same unique identity.

The exact uniqueness criteria are defined by the authentication requirements.

---

## BR-002 — Disabled Users Cannot Perform Protected Operations

A disabled user must not be able to perform protected business operations.

---

# 3. Portfolio Rules

## BR-003 — Portfolio Must Belong to a User

Every portfolio must have exactly one owner.

---

## BR-004 — Disabled Portfolios Cannot Accept Trading Activity

A disabled portfolio cannot create or process new trading orders.

---

## BR-005 — Portfolio Must Have a Valid Identity

A portfolio must have:

- A unique identifier.
- An owner.
- A valid name.

---

# 4. Asset Rules

## BR-006 — Asset Symbol Must Be Unique

Two active assets cannot share the same symbol within the same asset universe.

---

## BR-007 — Inactive Assets Cannot Receive New Orders

An order cannot be created for an inactive asset.

---

# 5. Order Rules

## BR-008 — Order Quantity Must Be Positive

```text
Quantity > 0
```

Zero or negative quantities are invalid.

---

## BR-009 — Order Must Belong to an Active Portfolio

An order cannot be created for an inactive portfolio.

---

## BR-010 — Order Must Reference an Active Asset

An order cannot be created for an inactive asset.

---

## BR-011 — Limit Orders Require a Limit Price

A LIMIT order must contain a valid limit price.

A MARKET order must not require a limit price.

---

## BR-012 — Order State Transitions Must Be Valid

Valid transitions are:

```text
PENDING
   ├──→ PROCESSING
   └──→ CANCELLED

PROCESSING
   ├──→ FILLED
   └──→ REJECTED
```

Invalid transitions include:

```text
FILLED → CANCELLED
FILLED → PROCESSING
CANCELLED → FILLED
CANCELLED → PROCESSING
REJECTED → FILLED
REJECTED → PROCESSING
```

---

## BR-013 — Filled Orders Cannot Be Cancelled

Once an order has been filled, it represents a completed trading decision and cannot be cancelled.

---

## BR-014 — Cancelled Orders Cannot Be Executed

A cancelled order must not produce an execution.

---

## BR-015 — Rejected Orders Cannot Be Executed

A rejected order must not produce an execution.

---

# 6. Execution Rules

## BR-016 — Execution Quantity Must Be Positive

An execution must have:

```text
ExecutedQuantity > 0
```

---

## BR-017 — Execution Cannot Exceed Order Quantity

For a single execution:

```text
ExecutedQuantity <= Order.Quantity
```

If partial execution is supported in the future, the aggregate executed quantity must not exceed the original order quantity.

---

## BR-018 — Execution Can Only Occur for an Eligible Order

Only orders in an execution-eligible state (`PENDING → PROCESSING`) may be executed.

### Simulated Execution Pricing Rules (MVP)

When an order is evaluated against the target `Asset.CurrentPrice`:

1. **Pre-execution checks:**
   - The owning `Portfolio.Status` must be `ACTIVE` (if disabled after order placement, the order is `REJECTED` with reason `"Portfolio is disabled."`).
   - The `Asset.Status` must be `ACTIVE` and `Asset.CurrentPrice > 0` (otherwise `REJECTED` with reason `"Asset is inactive or has no valid market price."`).
   - For a `SELL` order, the portfolio must hold a `Position` for that asset with `Position.Quantity >= Order.Quantity` (otherwise `REJECTED` with reason `"Insufficient position quantity."`).
2. **Order Type evaluation:**
   - **`MARKET` order:** Executes immediately at `ExecutionPrice = Asset.CurrentPrice` for `ExecutedQuantity = Order.Quantity`.
   - **`LIMIT` `BUY` order:** Executes if `Asset.CurrentPrice <= Order.LimitPrice` at `ExecutionPrice = Asset.CurrentPrice`. If `Asset.CurrentPrice > Order.LimitPrice`, the order remains `PENDING` (or is rejected if synchronous-only mode is requested).
   - **`LIMIT` `SELL` order:** Executes if `Asset.CurrentPrice >= Order.LimitPrice` at `ExecutionPrice = Asset.CurrentPrice`. If `Asset.CurrentPrice < Order.LimitPrice`, the order remains `PENDING`.

---

## BR-019 — Execution Must Be Idempotent

Processing the same execution more than once must not create duplicate financial effects.

For example:

```text
One Execution
    ↓
One Transaction
    ↓
One Position Update
```

---

# 7. Position Rules

## BR-020 — Position Belongs to One Portfolio and One Asset

A position represents a single asset within a single portfolio (`Unique(PortfolioId, AssetId)`).

---

## BR-021 — BUY Increases Position Quantity

For a BUY transaction:

```text
New Quantity = Current Quantity + Executed Quantity
```

---

## BR-022 — SELL Decreases Position Quantity

For a SELL transaction:

```text
New Quantity = Current Quantity - Executed Quantity
```

---

## BR-023 — Position Cannot Become Invalid (Long-Only MVP)

The system must prevent a position from entering an invalid state:

```text
Position.Quantity >= 0
Executed Sell Quantity <= Position.Quantity
```

Short selling is outside the MVP scope. When `Position.Quantity` reaches `0`, the position is considered closed (`IsClosed == true`), its row is retained for historical `RealizedPnL` tracking, and `AverageEntryPrice` resets to `null`.

---

## BR-024 — Average Entry Price and Realized P/L Must Be Consistent

For supported long positions, the average entry price reflects the weighted-average acquisition cost of the remaining position:

- **On First BUY (when `Current Quantity == 0`):**
  ```text
  New AverageEntryPrice = ExecutionPrice
  ```
- **On Subsequent BUY (when `Current Quantity > 0`):**
  ```text
  New AverageEntryPrice =
      (Current Quantity × Current AverageEntryPrice + Executed Quantity × ExecutionPrice)
      / (Current Quantity + Executed Quantity)
  ```
- **On SELL (`0 < Executed Quantity <= Current Quantity`):**
  - `AverageEntryPrice` does **not** change when partially selling a position (unless `New Quantity == 0`, in which case `AverageEntryPrice` resets to `null`).
  - `RealizedPnL` accumulates the profit or loss locked in by the sale:
    ```text
    RealizedPnL += (ExecutionPrice - AverageEntryPrice) × Executed Quantity
    ```

All calculations use `decimal` arithmetic (`numeric(18,4)` for prices/amounts, `numeric(18,8)` for quantities).

---

# 8. Transaction Rules

## BR-025 — Successful Execution Produces a Transaction

A successfully executed order must produce the corresponding transaction with snapshot fields:

```text
GrossAmount = Quantity × Price
```

---

## BR-026 — Transactions Represent Historical Facts

Transaction records should be treated as immutable after creation.

Corrections should be handled through appropriate compensating operations rather than directly rewriting historical transactions.

---

## BR-027 — Transaction Must Reference Its Source Order

Every transaction must be traceable to the order (`OrderId`) and execution (`ExecutionId`) that produced it.

---

# 9. Portfolio Valuation Rules

## BR-028 — Position Value and Cost Basis

For an open position (`Quantity > 0`):

```text
Position Cost Basis  = Quantity × AverageEntryPrice
Position Market Value = Quantity × Asset.CurrentPrice
```

---

## BR-029 — Portfolio Value

The total market value of a portfolio is the sum of the market values of all open positions:

```text
Portfolio Market Value = Σ(Position.Quantity × Asset.CurrentPrice)
Portfolio Cost Basis   = Σ(Position.Quantity × Position.AverageEntryPrice)
```

Cash balance is not included in the initial simplified model unless explicitly introduced into the domain.

---

## BR-030 — Profit and Loss (Unrealized, Realized, and Total P/L)

For a single open long position (`Quantity > 0`):

```text
Position Unrealized P/L = (Asset.CurrentPrice - Position.AverageEntryPrice) × Position.Quantity
Position Unrealized P/L % = ((Asset.CurrentPrice - Position.AverageEntryPrice) / Position.AverageEntryPrice) × 100
```

At the portfolio level (across all positions belonging to the portfolio, including closed positions for realized P/L):

```text
Portfolio Unrealized P/L = Σ(Position Unrealized P/L for open positions)
Portfolio Realized P/L   = Σ(Position.RealizedPnL across all positions in portfolio)
Portfolio Total P/L      = Portfolio Unrealized P/L + Portfolio Realized P/L
```

The exact calculation may evolve when fees, multiple tax lots, or cash ledgers are introduced.

---

# 10. Consistency Rules

## BR-031 — Order, Transaction, and Position Must Remain Consistent

A successful execution must result in a consistent state across:

```text
Order
Transaction
Position
Portfolio
```

---

## BR-032 — Financial Effects Must Not Be Applied Twice

Retrying an execution or processing the same event multiple times must not duplicate:

- Transactions.
- Position updates.
- Portfolio valuation effects.

---

## BR-033 — Failed Operations Must Not Produce Partial Financial Effects

If a financial operation fails, the system must not leave partially applied financial changes.

---

# 11. Concurrency Rules

## BR-034 — Concurrent Operations Must Not Corrupt Financial State

Concurrent operations affecting the same portfolio, position, or order must be handled safely.

Examples:

```text
Two concurrent SELL orders
Two concurrent order executions
Concurrent portfolio updates
```

must not cause invalid state.

---

## BR-035 — Order State Changes Must Be Atomic

An order must not be observed in an invalid intermediate state.

---

# 12. Authorization Rules

## BR-036 — Users Can Only Operate on Authorized Resources

An investor must not access or modify:

- Another user's portfolio.
- Another user's orders.
- Another user's positions.
- Another user's transactions.

---

## BR-037 — Administrative Operations Require Administrative Privileges

Administrative operations must only be available to authorized administrators.

---

# 13. Rule Evolution

Business rules may evolve as new requirements are introduced.

When a business rule changes:

1. Update this document.
2. Update the affected domain model.
3. Update related requirements.
4. Update affected tests.
5. Create an ADR if the change has significant architectural consequences.
