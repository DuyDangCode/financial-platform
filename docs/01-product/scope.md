# Project Scope

## 1. Purpose

This document defines the boundaries of the Financial Portfolio & Trading Operations Platform.

The purpose of this document is to prevent uncontrolled feature expansion and to establish clear boundaries for the MVP and future development phases.

The project is primarily an engineering learning project focused on building a production-oriented backend system around realistic financial workflows.

---

# 2. Product Boundary

The system simulates a financial portfolio and trading platform.

The platform allows users to:

- Manage their accounts.
- Create and manage portfolios.
- View supported financial assets.
- Create and manage trading orders.
- Track positions.
- View transactions.
- Monitor portfolio performance.
- Review relevant account and trading activities.

The system is designed to simulate trading workflows rather than execute real-money trades.

---

# 3. In Scope

The following capabilities are part of the project scope.

## 3.1. User Management

- User registration.
- User authentication.
- User authorization.
- User profile management.
- Basic administrative user management.

---

## 3.2. Portfolio Management

- Portfolio creation.
- Portfolio updates.
- Portfolio retrieval.
- Portfolio summary.
- Portfolio valuation.
- Portfolio performance.

---

## 3.3. Asset Management

- Asset creation and management.
- Asset lookup.
- Asset search.
- Asset metadata.
- Asset price information.

Initial asset types may include:

- Stocks.
- ETFs.

Additional asset types may be introduced later.

---

## 3.4. Position Management

- Position creation.
- Position updates.
- Position retrieval.
- Position valuation.
- Unrealized P/L calculation.

Positions are derived from executed trading activity.

---

## 3.5. Order Management

The MVP supports:

- BUY orders.
- SELL orders.
- MARKET orders.
- LIMIT orders.

The system supports:

- Order creation.
- Order validation.
- Order processing.
- Order cancellation.
- Order retrieval.
- Order history.
- Order state transitions.

---

## 3.6. Order Execution

The system includes a simulated order execution workflow.

The execution engine is responsible for simulating the outcome of eligible orders.

A successful execution results in:

Order
  ↓
Execution
  ↓
Transaction
  ↓
Position Update
  ↓
Portfolio Update

The execution mechanism is intentionally simplified and does not represent a real exchange matching engine.

## 3.7. Transaction Management

The system maintains transaction records generated from executed orders.

Users can:

View transaction details.
View transaction history.
Filter transactions.
Review historical trading activity.
## 3.8. Market Data

The MVP uses simulated or seeded market data.

Market data may be provided through:

Seeded database records.
Mock data.
A replaceable market-data provider.

The market-data abstraction should allow an external provider to be introduced later without coupling the domain layer to a specific provider.

## 3.9. Audit

The system records important activities such as:

Authentication events.
Portfolio changes.
Order creation.
Order cancellation.
Order execution.
Transaction creation.

Audit data is intended to support debugging, security investigation, and business traceability.

# 4. Out of Scope

The following capabilities are explicitly outside the initial project scope.

## 4.1. Real-Money Trading

The system will not:

Execute real-money trades.
Hold real customer funds.
Connect to a user's bank account.
Provide custody services.
## 4.2. Direct Broker Integration

The MVP will not integrate directly with brokers such as:

Interactive Brokers.
Alpaca.
Binance.
Other real-world brokerage platforms.

Broker integration may be considered in a future phase.

## 4.3. Real Exchange Matching Engine

The project will not attempt to reproduce a production-grade exchange matching engine.

The system will use a simplified execution model.

It will not initially implement:

Full order book management.
Price-time priority matching.
High-frequency trading.
Exchange-grade market infrastructure.
## 4.4. Investment Advisory

The system will not provide:

Financial advice.
Investment recommendations.
Guaranteed returns.
Personalized investment strategies.
## 4.5. Automated Trading

The MVP will not include:

Fully automated trading strategies.
Trading bots using real funds.
High-frequency trading.
Autonomous trading agents.

Strategy simulation may be considered as a future feature.

## 4.6. Advanced Financial Instruments

The initial project will not support complex financial instruments such as:

Options.
Futures.
Bonds.
Derivatives.
Margin trading.
Short selling.

These may be introduced in future iterations if they provide meaningful engineering or domain-learning value.

## 4.7. Social Features

The platform will not initially include:

Social feeds.
User following.
Public portfolios.
Copy trading.
Social trading.
Chat or messaging.
## 4.8. Advanced Analytics

The MVP will provide basic portfolio performance calculations.

The following are outside the initial scope:

Advanced portfolio optimization.
Quantitative factor models.
Machine learning-based predictions.
Automated strategy generation.
Complex risk modeling.
# 5. Technical Scope

The project is primarily focused on backend engineering.

The initial technical scope includes:

ASP.NET Core.
C#.
REST APIs.
Relational database.
Entity Framework Core.
Authentication and authorization.
Domain-driven modeling.
Modular monolith architecture.
Automated testing.
Docker.
CI/CD.
Cloud deployment.
Logging and monitoring.

Additional infrastructure technologies should only be introduced when they solve an identified engineering problem.

Potential technologies include:

Redis.
Message brokers.
Background workers.
Event-driven processing.
Cloud services.
# 6. Architecture Boundary

The initial system will follow a modular monolith architecture.

The system should maintain clear boundaries between:

API
  ↓
Application
  ↓
Domain
  ↑
Infrastructure

The domain layer should remain independent from infrastructure and external service implementations.

The architecture should allow individual modules to evolve independently without prematurely splitting the system into microservices.

# 7. MVP Boundary

The MVP is considered complete when a user can perform the following end-to-end workflow:

Register
   ↓
Login
   ↓
Create Portfolio
   ↓
View Assets
   ↓
Create Order
   ↓
Validate Order
   ↓
Process Order
   ↓
Execute Order
   ↓
Create Transaction
   ↓
Update Position
   ↓
Update Portfolio
   ↓
View Portfolio Performance

The MVP must also demonstrate:

Authentication.
Authorization.
Transactional consistency.
Basic concurrency handling.
Automated tests.
Error handling.
Logging.
Dockerized local deployment.
# 8. Development Phases
## Phase 1 — Foundation

Focus:

Project setup.
Architecture.
Authentication.
User management.
Database foundation.
Basic API infrastructure.
## Phase 2 — Core Portfolio

Focus:

Portfolio management.
Asset management.
Position management.
Portfolio valuation.
## Phase 3 — Trading Workflow

Focus:

Order management.
Order lifecycle.
Order validation.
Simulated execution.
Transactions.
Position updates.
## Phase 4 — Reliability

Focus:

Transaction management.
Concurrency control.
Idempotency.
Error handling.
Audit logging.
Integration testing.
## Phase 5 — Performance

Focus:

Query optimization.
Database indexing.
Redis caching.
Performance testing.

Technologies should only be introduced when a measurable performance problem exists.

## Phase 6 — Asynchronous Processing

Potential additions:

Background workers.
Domain events.
Integration events.
Message broker.
Outbox pattern.

This phase is intended to explore distributed-system concepts without prematurely converting the application into microservices.

## Phase 7 — Production Readiness

Focus:

Docker.
CI/CD.
Cloud deployment.
Configuration management.
Monitoring.
Logging.
Health checks.
Operational documentation.
# 9. Future Scope

The following features may be considered after the core system is stable.

## Trading
Advanced order types.
Stop orders.
Trailing stops.
Order book simulation.
More realistic execution models.
## Market Data
Real-time market data.
Historical price data.
External market-data providers.
## Portfolio
Advanced performance analytics.
Portfolio risk metrics.
Asset allocation analysis.
Benchmark comparison.
## Trading Strategies
Strategy backtesting.
Paper trading.
Strategy simulation.
Algorithmic trading.
## Infrastructure
Advanced event-driven architecture.
Distributed processing.
Service decomposition.
Kubernetes.
Advanced observability.
# 10. Scope Management Rules

The project follows these rules when evaluating new features.

## Rule 1 — The feature must solve a real product or engineering problem

A technology or feature should not be added simply because it is popular.

For example:

"I want to use Kafka"

is not sufficient justification.

Instead:

"We need reliable asynchronous event processing
between order execution and downstream consumers."

provides a valid engineering reason.

## Rule 2 — Prefer simple solutions first

The project should evolve incrementally.

For example:

Synchronous processing
        ↓
Background processing
        ↓
Message broker
        ↓
Distributed processing

Only move to the next level when the current architecture no longer satisfies the requirements.

## Rule 3 — Avoid premature microservices

The system will remain a modular monolith until there is a clear reason to introduce service boundaries.

The goal is to learn distributed systems, not to create unnecessary operational complexity.

## Rule 4 — New requirements must update the scope

When a new feature is proposed, determine whether it belongs to:

Current MVP.
Current development phase.
Future scope.
Explicitly out of scope.

The scope document should be updated when the project's boundaries change.

# 11. Scope Change Process

A significant scope change should follow this process:

New Feature
    ↓
Define the Problem
    ↓
Evaluate Product Value
    ↓
Evaluate Engineering Value
    ↓
Determine Dependencies
    ↓
Determine Scope
    ↓
Update Requirements
    ↓
Update Roadmap
    ↓
Update Architecture / Domain Docs if necessary
    ↓
Implement

Architectural changes should also be documented through an ADR when appropriate.

# 12. Definition of Scope Completion

A feature is considered within the project scope only when:

Its requirements are documented.
Its business rules are defined.
Its architectural impact is understood.
Its implementation is tested.
Relevant documentation is updated.

A feature is not considered complete simply because its code has been implemented.
