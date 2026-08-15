# Architecture Overview

## 1. Purpose

This document describes the high-level architecture of the Financial Portfolio & Trading Operations Platform.

The system is initially designed as a **Modular Monolith** using Clean Architecture principles and domain-oriented module boundaries.

The architecture prioritizes:

- Maintainability.
- Testability.
- Clear separation of concerns.
- Domain independence.
- Incremental scalability.
- Simplicity.

---

# 2. Architectural Style

The system uses a combination of:

- Modular Monolith.
- Clean Architecture.
- Domain-oriented design.
- Dependency Inversion.
- Vertical use-case organization where appropriate.

The initial deployment consists of a single application.

```text
┌──────────────────────────────────────────────┐
│              Financial Platform              │
│                                              │
│  ┌─────────┐ ┌──────────┐ ┌──────────────┐ │
│  │ Identity│ │ Portfolio│ │   Trading    │ │
│  │ Module  │ │  Module  │ │    Module    │ │
│  └─────────┘ └──────────┘ └──────────────┘ │
│                                              │
└──────────────────────────────────────────────┘
                    │
                    ▼
               PostgreSQL
```

Modules remain logically separated even though they are deployed together.

---

# 3. Architectural Principles

## 3.1. Dependency Inversion

High-level business logic must not depend directly on infrastructure technologies.

The dependency direction is:

```text
API
 ↓
Application
 ↓
Domain
 ↑
Infrastructure
```

Infrastructure implements abstractions required by the inner layers.

---

## 3.2. Domain Independence

The domain must not depend on:

- ASP.NET Core.
- Entity Framework Core.
- PostgreSQL.
- Redis.
- Kafka.
- HTTP clients.
- Cloud providers.

---

## 3.3. Business Logic Belongs to the Domain

Business rules should not be implemented inside:

- Controllers.
- Database repositories.
- HTTP clients.
- Background workers.

The domain is responsible for enforcing business invariants.

---

## 3.4. Application Orchestrates Use Cases

The Application layer coordinates business operations.

It is responsible for:

- Executing use cases.
- Coordinating domain objects.
- Managing application-level transactions.
- Calling repositories through abstractions.
- Publishing application/integration events.

The Application layer should not become a second domain layer.

---

## 3.5. Infrastructure Implements Technical Concerns

Infrastructure contains implementations for:

- Database access.
- External APIs.
- Caching.
- Messaging.
- File storage.
- Cloud services.

---

# 4. Layer Responsibilities

## API

Responsible for:

- HTTP endpoints.
- Request/response models.
- Authentication integration.
- Authorization integration.
- HTTP-specific validation.
- Mapping HTTP requests to application commands.

---

## Application

Responsible for:

- Commands.
- Queries.
- Handlers.
- Use-case orchestration.
- Application-level validation.
- Transaction boundaries.
- Coordinating repositories and external abstractions.

---

## Domain

Responsible for:

- Entities.
- Value objects.
- Aggregates.
- Domain services.
- Domain events.
- Business rules.
- Invariants.

---

## Infrastructure

Responsible for:

- EF Core.
- PostgreSQL.
- Repository implementations.
- Redis.
- Message brokers.
- External service integrations.
- Background processing.
- Technical infrastructure.

---

# 5. Module Boundaries

The initial modules are:

```text
Identity
Portfolio
Trading
MarketData
Audit
```

### Identity

Responsible for:

- Users.
- Authentication.
- Authorization.
- User status.

### Portfolio

Responsible for:

- Portfolios.
- Positions.
- Portfolio valuation.
- Portfolio performance.

### Trading

Responsible for:

- Orders.
- Order lifecycle.
- Executions.
- Transactions.

### MarketData

Responsible for:

- Assets.
- Market prices.
- Market data providers.

### Audit

Responsible for:

- Audit events.
- Activity history.
- Traceability.

---

# 6. Data Ownership

Each module should have clear ownership of its business data.

For example:

```text
Identity
    → Users

Portfolio
    → Portfolios
    → Positions

Trading
    → Orders
    → Executions
    → Transactions

MarketData
    → Assets
    → Prices

Audit
    → Audit Records
```

Other modules should not directly modify another module's owned data.

---

# 7. Communication Between Modules

Modules should communicate through:

- Application interfaces.
- Domain events.
- Application events.
- Explicit contracts.

Direct access to another module's internal implementation should be avoided.

---

# 8. Transaction Boundaries

Transactions should be defined around business operations.

For example:

```text
Execute Order
    ↓
Create Transaction
    ↓
Update Position
```

Operations that must remain consistent should be handled within an appropriate transaction boundary.

---

# 9. External Systems

Potential external infrastructure includes:

```text
PostgreSQL
Redis
Message Broker
Market Data Provider
Cloud Services
```

External dependencies should be accessed through abstractions where appropriate.

---

# 10. Scalability Strategy

The system follows an incremental scalability strategy.

### Stage 1

```text
Modular Monolith
+
PostgreSQL
```

### Stage 2

```text
Modular Monolith
+
Redis
+
Background Workers
```

### Stage 3

```text
Modular Monolith
+
Message Broker
+
Outbox Pattern
```

### Stage 4

Only when justified:

```text
Selected Modules
        ↓
Independent Services
```

The system should not adopt microservices before there is a clear operational or organizational reason.

---

# 11. Architecture Goals

The architecture should allow the project to demonstrate practical knowledge of:

- Clean Architecture.
- Domain modeling.
- Modular design.
- REST API design.
- Database transactions.
- Concurrency.
- Caching.
- Messaging.
- Event-driven architecture.
- Testing.
- CI/CD.
- Observability.
- Cloud deployment.

---

# 12. Architecture Evolution

Architecture decisions are expected to evolve.

Significant decisions should be recorded as Architecture Decision Records (ADRs).

Examples:

```text
ADR-001 — Modular Monolith
ADR-002 — PostgreSQL
ADR-003 — Authentication Strategy
ADR-004 — Redis
ADR-005 — Messaging
ADR-006 — Outbox Pattern
```

The architecture documentation describes the current system.

ADRs preserve the historical reasoning behind significant decisions.
