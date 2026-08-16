# Financial Platform

A backend-heavy financial portfolio and trading operations platform built with
.NET 8 and designed as a modular monolith.

## Overview

Financial Platform allows users to:

- Create and manage investment portfolios.
- View tradable assets.
- Create buy and sell orders.
- Manage order lifecycles.
- Track executions and transactions.
- Manage portfolio positions.
- Calculate portfolio valuation and P/L.
- Review trading and account activity.

The system is intentionally designed as a **Modular Monolith** rather than
starting with microservices.

The goal is to build a system that can evolve into a more distributed
architecture when there is a real technical reason to do so.

## Goals

This project focuses on practical backend engineering skills:

- ASP.NET Core Web API
- Clean Architecture
- Domain-Driven Design
- Modular Monolith
- REST API design
- PostgreSQL
- Entity Framework Core
- Transactions and concurrency
- Authentication and Authorization
- Redis
- Background processing
- Message brokers
- Outbox Pattern
- Unit and Integration Testing
- Docker
- CI/CD
- Logging and Monitoring
- Cloud deployment

## Technology Stack

### Backend

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- C#

### Database

- PostgreSQL

### Caching

- Redis

### Messaging

- Message Broker
- Outbox Pattern

### Testing

- xUnit
- Integration Testing
- Testcontainers

### Infrastructure

- Docker
- CI/CD
- Cloud Platform

### Observability

- Structured Logging
- Health Checks
- Metrics
- Distributed Tracing

## Architecture

The application follows a **Modular Monolith + Clean Architecture** approach.

```text
                         ┌──────────────────────┐
                         │        Client        │
                         └──────────┬───────────┘
                                    │
                                    ▼
                         ┌──────────────────────┐
                         │         API          │
                         │    ASP.NET Core      │
                         └──────────┬───────────┘
                                    │
                                    ▼
                         ┌──────────────────────┐
                         │    Application       │
                         │                      │
                         │ Commands / Queries   │
                         │ Use Cases             │
                         └──────────┬───────────┘
                                    │
                                    ▼
                         ┌──────────────────────┐
                         │        Domain        │
                         │                      │
                         │ Entities             │
                         │ Aggregates            │
                         │ Value Objects         │
                         │ Business Rules        │
                         └──────────────────────┘
                                    ▲
                                    │
                         ┌──────────┴───────────┐
                         │    Infrastructure    │
                         │                      │
                         │ EF Core              │
                         │ PostgreSQL            │
                         │ Redis                 │
                         │ Messaging             │
                         │ External Services     │
                         └──────────────────────┘
```

### Dependency Direction

```text
API
 ├── Application
 └── Infrastructure

Application
 └── Domain

Infrastructure
 ├── Application
 └── Domain
```

The Domain layer does not depend on Infrastructure or API.

## Solution Structure

```text
FinancialPlatform/
│
├── FinancialPlatform.sln
├── README.md
├── .gitignore
│
├── docs/
│   ├── product-overview.md
│   ├── requirements.md
│   ├── scope.md
│   ├── user-stories.md
│   ├── business-rules.md
│   ├── domain-overview.md
│   ├── domain-model.md
│   ├── architecture-overview.md
│   └── system-architecture.md
│
├── src/
│   ├── FinancialPlatform.Api/
│   ├── FinancialPlatform.Application/
│   ├── FinancialPlatform.Domain/
│   └── FinancialPlatform.Infrastructure/
│
└── tests/
    ├── FinancialPlatform.UnitTests/
    └── FinancialPlatform.IntegrationTests/
```

## Core Modules

### Identity

- Users
- Authentication
- Authorization
- User status

### Portfolio

- Portfolios
- Positions
- Portfolio valuation
- Portfolio performance

### Trading

- Orders
- Order lifecycle
- Executions
- Transactions

### Market Data

- Assets
- Market prices
- Market data providers

### Audit

- Audit events
- Activity history
- Traceability

## Core Business Flow

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

A typical flow:

```text
User
 ↓
Create Order
 ↓
Validate Business Rules
 ↓
Process Order
 ↓
Execute Order
 ↓
Create Transaction
 ↓
Update Position
 ↓
Update Portfolio State
```

## Domain Concepts

Core domain entities:

```text
User
Portfolio
Position
Asset
Order
Execution
Transaction
```

Important value objects:

```text
Money
Quantity
AssetSymbol
```

The domain is responsible for enforcing business invariants such as:

- Order quantity must be positive.
- Inactive assets cannot receive new orders.
- Filled orders cannot be cancelled.
- Cancelled orders cannot be executed.
- Executions must not be processed more than once.
- Financial state must remain consistent.

Detailed rules are documented in `docs/business-rules.md`.

## Getting Started

### Prerequisites

Install:

- .NET 8 SDK
- Docker
- Git

Verify .NET:

```bash
dotnet --version
```

The project should report a .NET 8 SDK version.

### Clone

```bash
git clone <repository-url>
cd FinancialPlatform
```

### Restore

```bash
dotnet restore
```

### Build

```bash
dotnet build
```

### Run Tests

```bash
dotnet test
```

Unit tests:

```bash
dotnet test tests/FinancialPlatform.UnitTests
```

Integration tests:

```bash
dotnet test tests/FinancialPlatform.IntegrationTests
```

### Run the API

```bash
dotnet run --project src/FinancialPlatform.Api
```

Swagger/OpenAPI will be available in the development environment.

## Database

PostgreSQL is the primary transactional database and the source of truth for
financial state.

The database stores information such as:

```text
Users
Portfolios
Positions
Assets
Orders
Executions
Transactions
Audit Records
```

Redis, when introduced, is only a performance optimization and is not the
authoritative source of financial state.

## Configuration

Do not commit real secrets.

Use mechanisms such as:

- .NET User Secrets
- Environment Variables
- Docker Secrets
- Cloud Secret Management

Never commit real credentials, passwords, API keys, or private certificates.

## Testing Strategy

### Unit Tests

Focus on isolated business behavior:

```text
Order cancellation rules
Position calculations
Money calculations
Domain invariants
Application handlers
```

### Integration Tests

Verify interactions between real application components:

```text
HTTP Request
    ↓
API
    ↓
Application
    ↓
Infrastructure
    ↓
PostgreSQL
```

Testcontainers may be used to run infrastructure dependencies during
integration tests.

## Development Principles

### Keep the Domain Independent

The Domain project should not depend on:

- ASP.NET Core
- Entity Framework Core
- PostgreSQL
- Redis
- Kafka
- HTTP
- Cloud providers

### Keep Business Rules in the Domain

Controllers should not contain core business rules.

Repositories should not contain business rules.

Infrastructure should not decide business behavior.

### Application Orchestrates Use Cases

Application handlers coordinate operations but should not become a second
domain layer.

### Infrastructure Implements Technical Details

Infrastructure contains implementations for:

- Database access
- External APIs
- Caching
- Messaging
- Background processing

## Architecture Evolution

### Phase 1 — Core Application

```text
Modular Monolith
+
ASP.NET Core
+
PostgreSQL
+
EF Core
```

### Phase 2 — Reliability

```text
+
Unit Tests
+
Integration Tests
+
Transactions
+
Concurrency Control
```

### Phase 3 — Performance

```text
+
Redis
+
Caching
+
Background Workers
```

### Phase 4 — Asynchronous Processing

```text
+
Message Broker
+
Outbox Pattern
+
Event-driven workflows
```

### Phase 5 — Production Engineering

```text
+
Docker
+
CI/CD
+
Logging
+
Metrics
+
Tracing
+
Cloud Deployment
```

### Phase 6 — Architectural Evolution

Only when justified:

```text
Modular Monolith
       ↓
Extract selected modules
       ↓
Independent Services
```

Microservices are not the initial goal.

## Documentation

Project documentation is stored under `docs/`.

| Document | Purpose |
| --- | --- |
| `product-overview.md` | Product and business context |
| `requirements.md` | Functional and non-functional requirements |
| `scope.md` | Project boundaries |
| `user-stories.md` | User-facing requirements |
| `business-rules.md` | Domain rules and invariants |
| `domain-overview.md` | High-level domain concepts |
| `domain-model.md` | Detailed domain model |
| `architecture-overview.md` | Architectural principles |
| `system-architecture.md` | Runtime/system architecture |

Architecture decisions should be recorded separately as ADRs.

## Roadmap

- [x] Define product requirements
- [x] Define domain model
- [x] Define business rules
- [x] Define architecture
- [ ] Create .NET solution
- [ ] Configure project references
- [ ] Configure EF Core
- [ ] Configure PostgreSQL
- [ ] Create database schema
- [ ] Implement Identity module
- [ ] Implement Portfolio module
- [ ] Implement Trading module
- [ ] Implement Market Data module
- [ ] Add authentication and authorization
- [ ] Add unit tests
- [ ] Add integration tests
- [ ] Add transaction/concurrency handling
- [ ] Add Redis
- [ ] Add background processing
- [ ] Add message broker
- [ ] Implement Outbox Pattern
- [ ] Add Docker
- [ ] Add CI/CD
- [ ] Add observability
- [ ] Deploy to cloud

## Project Status

**Status:** Development

**Framework:** .NET 8

**Architecture:** Modular Monolith + Clean Architecture

**Primary Database:** PostgreSQL

**License:** TBD
