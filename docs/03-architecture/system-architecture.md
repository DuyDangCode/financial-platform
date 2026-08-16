# System Architecture

## 1. Purpose

This document describes the runtime structure and major components of the Financial Portfolio & Trading Operations Platform.

It focuses on how requests, business operations, data persistence, external services, and asynchronous processing interact.

---

# 2. High-Level Architecture

```text
                         ┌───────────────┐
                         │    Client     │
                         │ Web / Mobile  │
                         └───────┬───────┘
                                 │
                              HTTPS
                                 │
                                 ▼
                         ┌───────────────┐
                         │      API      │
                         │ ASP.NET Core  │
                         └───────┬───────┘
                                 │
                                 ▼
                    ┌────────────────────────┐
                    │      Application       │
                    │                        │
                    │ Commands / Queries     │
                    │ Use Case Handlers      │
                    └───────────┬────────────┘
                                │
                                ▼
                    ┌────────────────────────┐
                    │        Domain          │
                    │                        │
                    │ Entities               │ 
                    │ Aggregates             │ 
                    │ Value Objects          │  
                    │ Domain Services        │  
                    │ Domain Events          │  
                    └───────────┬────────────┘
                                │
                    ┌───────────┴────────────┐
                    │                        │
                    ▼                        ▼
          ┌──────────────────┐      ┌──────────────────┐
          │  Infrastructure  │      │ External Services│
          │                  │      │                  │
          │ EF Core          │      │ Market Data      │
          │ Redis            │      │ Email            │
          │ Messaging        │      │ Cloud Services   │
          └────────┬─────────┘      └──────────────────┘
                   │
                   ▼
             ┌────────────┐
             │ PostgreSQL │
             └────────────┘
```

---

# 3. Request Flow

A typical synchronous request follows:

```text
Client
  ↓
HTTP Request
  ↓
API Endpoint
  ↓
Authentication / Authorization
  ↓
Application Command / Query
  ↓
Application Handler
  ↓
Domain Logic
  ↓
Repository Abstraction
  ↓
Infrastructure
  ↓
EF Core
  ↓
PostgreSQL
  ↓
Response
```

---

# 4. Example — Place Order

A typical order creation workflow:

```text
POST /api/orders
        ↓
OrderController
        ↓
PlaceOrderCommand
        ↓
PlaceOrderHandler
        ↓
Load Portfolio
        ↓
Load Asset
        ↓
Validate Business Rules
        ↓
Create Order
        ↓
Persist Order
        ↓
Return Response
```

The controller does not contain trading business rules.

---

# 5. Example — Execute Order

The execution workflow is:

```text
Order
  ↓
Execution Service
  ↓
Validate Order State
  ↓
Execute Order
  ↓
Order → FILLED
  ↓
Create Transaction
  ↓
Update Position
  ↓
Update Portfolio State
  ↓
Publish Domain/Application Events
```

The exact synchronous or asynchronous boundaries may evolve as the system develops.

---

# 6. Persistence Architecture

The persistence stack is:

```text
Domain Entity
      ↓
EF Core Configuration
      ↓
DbContext
      ↓
Repository
      ↓
PostgreSQL
```

Domain entities must not contain EF Core-specific implementation details.

Persistence configuration belongs to Infrastructure.

---

# 7. Database Ownership

The initial application uses a shared PostgreSQL database.

Logical ownership is maintained by module boundaries:

```text
Identity
 └── Users

Portfolio
 ├── Portfolios
 └── Positions

Trading
 ├── Orders
 ├── Executions
 └── Transactions

MarketData
 ├── Assets
 └── MarketPrices

Audit
 └── AuditRecords
```

A shared database does not imply unrestricted access between modules.

---

# 8. Authentication Flow

The authentication flow is:

```text
Client
  ↓
Login Request
  ↓
Authentication Service
  ↓
Validate Credentials
  ↓
Issue Token
  ↓
Client Stores Token
  ↓
Authenticated API Request
  ↓
Token Validation
  ↓
Authorization
  ↓
Application
```

The exact token strategy will be documented separately in the security documentation.

---

# 9. Caching Architecture

Redis may be introduced for data where caching provides measurable value.

Potential candidates include:

```text
Market Prices
Portfolio Summaries
Frequently Accessed Reference Data
```

The general flow is:

```text
Application
    ↓
Cache
    ├── Hit → Return Cached Data
    │
    └── Miss
          ↓
       Database
          ↓
       Update Cache
          ↓
       Return Data
```

The cache must not become the authoritative source of financial state.

PostgreSQL remains the source of truth for transactional financial data.

---

# 10. Asynchronous Processing

The system may introduce background processing for operations that do not need to block the initial HTTP request.

Potential workloads include:

- Market data updates.
- Notifications.
- Audit processing.
- Reporting.
- Event-driven workflows.

Potential architecture:

```text
Application
    ↓
Domain / Integration Event
    ↓
Outbox
    ↓
Message Broker
    ↓
Consumer
    ↓
Background Worker
```

This architecture will only be introduced when the corresponding engineering requirement exists.

---

# 11. External Market Data

Market data should be isolated behind an abstraction.

Example:

```text
Application / Domain
        ↓
IMarketDataProvider
        ↓
Infrastructure Adapter
        ↓
External Market Data API
```

The core domain should not know which external provider supplies the data.

---

# 12. Error Handling

Errors are categorized into:

### Validation Errors

Examples:

```text
Invalid quantity
Invalid order type
Missing required field
```

### Business Errors

Examples:

```text
Order cannot be cancelled
Portfolio is inactive
Asset is inactive
Insufficient position
```

### Infrastructure Errors

Examples:

```text
Database unavailable
External API unavailable
Message broker unavailable
```

Infrastructure failures should not leak implementation-specific details to API clients.

---

# 13. Observability

The system should provide:

- Structured logs.
- Correlation IDs.
- Request logging.
- Error logging.
- Health checks.
- Application metrics.

A typical request should be traceable across:

```text
HTTP Request
    ↓
Application Use Case
    ↓
Database Operation
    ↓
External Service
```

where applicable.

---

# 14. Deployment Architecture

The initial deployment model is:

```text
                Internet
                   │
                   ▼
            ┌──────────────┐
            │ Load Balancer│
            └──────┬───────┘
                   │
                   ▼
          ┌─────────────────┐
          │ ASP.NET Core App│
          └────────┬────────┘
                   │
        ┌──────────┴──────────┐
        ▼                     ▼
 ┌──────────────┐      ┌──────────────┐
 │ PostgreSQL   │      │    Redis     │
 └──────────────┘      └──────────────┘
```

The exact cloud architecture will be defined when deployment infrastructure is introduced.

---

# 15. Failure Boundaries

The system should distinguish between:

```text
Application Failure
Database Failure
External Service Failure
Message Broker Failure
Cache Failure
```

Critical financial operations should not depend on cache availability.

For example:

```text
Redis unavailable
      ↓
Application continues using database
```

where appropriate.

---

# 16. Source of Truth

The system follows these principles:

### Financial State

PostgreSQL is the authoritative source for:

- Orders.
- Executions.
- Transactions.
- Positions.
- Portfolio state.

### Cache

Redis is a performance optimization and is not authoritative.

### Events

Events represent facts or messages used for communication.

They should not replace authoritative transactional state unless explicitly designed as an event-sourced system.

---

# 17. Scalability Model

The initial system is designed to scale vertically and horizontally as a modular monolith.

```text
                 Load Balancer
                      │
          ┌───────────┼───────────┐
          ▼           ▼           ▼
       App #1       App #2       App #3
          │           │           │
          └───────────┼───────────┘
                      ▼
                  PostgreSQL
```

Shared state should remain external to individual application instances.

This allows multiple application instances to process requests without relying on local in-memory state.

---

# 18. Architecture Evolution

The system should evolve incrementally:

```text
Phase 1

Modular Monolith
      +
PostgreSQL
```

↓

```text
Phase 2

Modular Monolith
      +
Redis
      +
Background Workers
```

↓

```text
Phase 3

Modular Monolith
      +
Outbox
      +
Message Broker
```

↓

```text
Phase 4

Extract only justified modules
into independent services
```

Microservices are considered an architectural evolution rather than the starting point.
