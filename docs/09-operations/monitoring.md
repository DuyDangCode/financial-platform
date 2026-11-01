# Monitoring & Observability

## 1. Purpose

This document defines the health checks, operational metrics, and monitoring strategy for the Financial Portfolio & Trading Operations Platform (FR-026, NFR-002, NFR-006).

> **Status: Planned (Phase 4–7).**

---

## 2. Health Checks

The API should expose standard ASP.NET Core Health Check endpoints so Docker, load balancers, and orchestrators can verify liveness and readiness:

| Endpoint | Probe Type | Checks Performed | Expected Response |
| --- | --- | --- | --- |
| `GET /health/live` | Liveness | Verifies the ASP.NET Core HTTP process is responsive (no external dependency checks). | `200 OK` (`Healthy`) |
| `GET /health/ready` | Readiness | Verifies `AppDbContext` can open a connection and execute `SELECT 1` against PostgreSQL 16. | `200 OK` (`Healthy`) or `503 Service Unavailable` (`Unhealthy`) |

### Implementation Sketch (`Program.cs`)

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(name: "database", tags: new[] { "ready" });

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });
```

---

## 3. Key Operational & Business Metrics

Using `.NET 8` `System.Diagnostics.Metrics` (`Meter`), track the following indicators:

### 3.1. Technical / HTTP Metrics

| Metric | Type | Description | Alert Threshold (Guideline) |
| --- | --- | --- | --- |
| `http.server.request.duration` | Histogram (`ms`) | Request latency by route and status code | p95 `> 500ms` |
| `http.server.5xx.count` | Counter | Unhandled exceptions caught by `GlobalExceptionMiddleware` | Any spike `> 0` |
| `db.concurrency.conflicts` | Counter | Count of `DbUpdateConcurrencyException` (`xmin` conflicts) on `Positions` / `Orders` | Sustained rate indicates hot-row contention |

### 3.2. Financial Domain Metrics

| Metric | Type | Description | Purpose |
| --- | --- | --- | --- |
| `auth.login.attempts` | Counter (tagged by `outcome: success\|failure`) | Login attempts | Detect brute-force or credential-stuffing spikes |
| `trading.orders.placed` | Counter (tagged by `side`, `orderType`) | New orders created | Track trading volume |
| `trading.orders.outcomes` | Counter (tagged by `status: FILLED\|REJECTED\|CANCELLED`) | Order terminal state distribution | Spike in `REJECTED` indicates validation/price feed issues |
| `trading.execution.duration` | Histogram (`ms`) | Time to execute `Order → Execution → Transaction → Position` unit of work | Measure database transaction latency |
| `marketdata.price.staleness` | Gauge (`seconds`) | `UtcNow - MIN(Assets.PriceUpdatedAt)` for active assets | Detect stalled market price refresh worker |

---

## 4. Administrative Monitoring View (FR-026, US-016)

In addition to infrastructure metrics, administrators inspect business-level operational health through `GET /api/dashboard/admin-summary` (documented in [`dashboard.md`](../05-api/dashboard.md)):

- Active vs. disabled user counts.
- Pending order backlog (`Status == PENDING`).
- Recent security and trading events from `AuditLogs` (`USER_STATUS_CHANGED`, `ORDER_REJECTED`, `ORDER_FILLED`).
