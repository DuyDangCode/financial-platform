# User Stories

## 1. Purpose

This document defines the primary user stories for the Financial Portfolio & Trading Operations Platform.

User stories describe the system from the perspective of its users and provide a bridge between product requirements and implementation tasks.

---

# 2. User Roles

The system initially defines two primary roles:

- Investor
- Administrator

---

# 3. Investor Stories

## US-001 — Register an Account

**As an investor,**

I want to create an account

**so that**

I can access the platform and manage my investment portfolios.

### Acceptance Criteria

- The user can provide the required registration information.
- The system validates the registration data.
- The system prevents duplicate accounts.
- The password is securely stored.
- A successful registration creates a new user account.

---

## US-002 — Authenticate

**As an investor,**

I want to log in to the platform

**so that**

I can securely access my account.

### Acceptance Criteria

- Valid credentials result in successful authentication.
- Invalid credentials are rejected.
- A successful login produces an authentication token.
- Protected resources require authentication.

---

## US-003 — Create a Portfolio

**As an investor,**

I want to create a portfolio

**so that**

I can manage my investments separately.

### Acceptance Criteria

- An authenticated user can create a portfolio.
- The portfolio belongs to the authenticated user.
- The portfolio receives a unique identifier.
- The portfolio has an initial state.

---

## US-004 — View My Portfolios

**As an investor,**

I want to view my portfolios

**so that**

I can monitor my investments.

### Acceptance Criteria

- The user can retrieve their portfolios.
- Portfolio data includes relevant summary information.
- Users cannot access portfolios belonging to other users.

---

## US-005 — View Asset Information

**As an investor,**

I want to search for financial assets

**so that**

I can decide which assets I want to trade.

### Acceptance Criteria

- Users can search assets by symbol.
- Users can retrieve asset details.
- Asset information includes the current available price.

---

## US-006 — View My Positions

**As an investor,**

I want to view the positions held by my portfolio

**so that**

I can understand what assets I currently own.

### Acceptance Criteria

- Users can view positions for their portfolio.
- Each position includes quantity and valuation information.
- Position information reflects executed trading activity.

---

## US-007 — Create a Buy Order

**As an investor,**

I want to create a buy order

**so that**

I can acquire an asset.

### Acceptance Criteria

- The user can specify the portfolio.
- The user can specify the asset.
- The user can specify the quantity.
- The user can specify the order type.
- The system validates the order.
- An invalid order is rejected.

---

## US-008 — Create a Sell Order

**As an investor,**

I want to create a sell order

**so that**

I can reduce or close a position.

### Acceptance Criteria

- The user can specify the portfolio.
- The user can specify the asset.
- The user can specify the quantity.
- The system verifies that the order satisfies the applicable business rules.
- Invalid orders are rejected.

---

## US-009 — Cancel an Order

**As an investor,**

I want to cancel an eligible order

**so that**

I can prevent it from being executed.

### Acceptance Criteria

- The user can cancel an eligible order.
- An already filled order cannot be cancelled.
- The system records the cancellation.
- The order state changes according to the defined lifecycle.

---

## US-010 — View Order History

**As an investor,**

I want to view my order history

**so that**

I can review my trading activity.

### Acceptance Criteria

- Users can retrieve their orders.
- Orders can be filtered by status.
- Orders can be filtered by asset.
- Orders can be filtered by date range.

---

## US-011 — View Transaction History

**As an investor,**

I want to view my transaction history

**so that**

I can review executed trading activities.

### Acceptance Criteria

- Users can retrieve their transactions.
- Transactions are associated with the relevant portfolio and asset.
- Users cannot access transactions belonging to other users.

---

## US-012 — View Portfolio Performance

**As an investor,**

I want to view my portfolio performance

**so that**

I can understand how my investments are performing.

### Acceptance Criteria

- The system calculates the current portfolio value.
- The system calculates unrealized P/L.
- The system can calculate realized P/L where applicable.
- Performance data is based on portfolio positions and market prices.

---

## US-013 — View Account Activity

**As an investor,**

I want to review important account activity

**so that**

I can understand what actions have occurred on my account.

### Acceptance Criteria

- Important activities are recorded.
- Activity records include timestamps.
- Relevant activities can be associated with the affected resource.

---

# 4. Administrator Stories

## US-014 — View Users

**As an administrator,**

I want to view registered users

**so that**

I can manage the platform.

### Acceptance Criteria

- Administrators can retrieve user information.
- Sensitive credentials are never exposed.
- Normal investors cannot access administrative user data.

---

## US-015 — Manage User Status

**As an administrator,**

I want to enable or disable user accounts

**so that**

I can control access to the platform.

### Acceptance Criteria

- Only authorized administrators can change user status.
- Disabled users cannot perform protected operations.
- The status change is recorded.

---

## US-016 — Review Audit Activity

**As an administrator,**

I want to review important system activities

**so that**

I can investigate operational or security issues.

### Acceptance Criteria

- Administrators can retrieve relevant audit records.
- Audit records include the actor, action, timestamp, and affected resource where applicable.
- Audit records cannot be modified through normal application operations.

---

# 5. System-Level Stories

## US-017 — Prevent Duplicate Processing

**As a system,**

I want retried operations to be handled safely

**so that**

a temporary failure does not create duplicate financial effects.

### Acceptance Criteria

- Retryable operations support idempotent processing.
- Duplicate execution requests do not create duplicate transactions.
- Position updates are not applied more than once for the same execution.

---

## US-018 — Maintain Financial Consistency

**As a system,**

I want related financial updates to be processed consistently

**so that**

orders, transactions, and positions cannot become inconsistent.

### Acceptance Criteria

- Related updates are handled within an appropriate consistency boundary.
- Failed operations do not leave partial financial state.
- Concurrent operations are handled safely.

---

# 6. Story Priorities

| Priority | Description |
|---|---|
| P0 | Required for the core system |
| P1 | Required for MVP |
| P2 | Important future capability |
| P3 | Optional enhancement |

---

# 7. MVP Stories

The MVP focuses on:

- US-001 — Register an Account
- US-002 — Authenticate
- US-003 — Create a Portfolio
- US-004 — View My Portfolios
- US-005 — View Asset Information
- US-006 — View My Positions
- US-007 — Create a Buy Order
- US-008 — Create a Sell Order
- US-009 — Cancel an Order
- US-010 — View Order History
- US-011 — View Transaction History
- US-012 — View Portfolio Performance
- US-017 — Prevent Duplicate Processing
- US-018 — Maintain Financial Consistency
