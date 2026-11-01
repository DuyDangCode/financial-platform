# Dashboard API

> **Status: Planned.** `DashboardController` (`src/FinancialPlatform.Api/Controllers/DashboardController.cs`) currently exists as a 0-byte placeholder.

## Overview

| Controller | Resource | Purpose |
| --- | --- | --- |
| Dashboard | `api/dashboard` | Aggregated portfolio overview for investors and system-wide operational/audit summaries for administrators (FR-005, FR-020..021, FR-024, FR-026, US-012, US-013, US-016). |

All endpoints require authentication (`[Authorize]`) and wrap successful responses in the standard [`ApiResponse<T>`](./api-design.md#3-response-envelope) envelope. Error semantics follow [`error-handling.md`](./error-handling.md).

## Endpoints

| Method | Path | Auth Required | Description |
| --- | --- | --- | --- |
| GET | `/api/dashboard/overview` | Yes | Aggregated net worth, total P/L, portfolio summaries, top positions, and recent activity for the current investor. |
| GET | `/api/dashboard/admin-summary` | Yes (`Administrator`) | Platform-wide operational metrics and recent audit log entries for administrators. |

---

## Investor Dashboard Overview

```text
GET /api/dashboard/overview
```

Aggregates valuation across all portfolios owned by the authenticated user, returning total market value, unrealized/realized P/L, pending orders count, top positions by market value, and the 10 most recent transactions (FR-005, FR-020, FR-021, US-012).

Responses:

| Status | Meaning |
| --- | --- |
| 200 | Aggregated overview returned. |
| 401 | Missing, invalid, or expired access token. |

Returns: `InvestorDashboardResponse`

| Field | Type | Description |
| --- | --- | --- |
| `baseCurrency` | string | Aggregation currency (`"USD"`). |
| `totalPortfolios` | integer | Total portfolios owned by the user. |
| `activePortfolios` | integer | Count of portfolios in `ACTIVE` status. |
| `totalMarketValue` | decimal | Sum of market values across all open positions in the user's portfolios. |
| `totalCostBasis` | decimal | Sum of cost basis across all open positions. |
| `totalUnrealizedPnL` | decimal | Sum of unrealized P/L across open positions. |
| `totalRealizedPnL` | decimal | Sum of realized P/L across all positions. |
| `totalPnL` | decimal | `totalUnrealizedPnL + totalRealizedPnL`. |
| `pendingOrdersCount` | integer | Number of `PENDING` orders across the user's portfolios. |
| `portfolios` | array | List of `PortfolioSummaryResponse` items (see [`account.md`](./account.md)). |
| `topPositions` | array | Top 5 open positions by market value (`PositionResponse`). |
| `recentTransactions` | array | Up to 10 most recent transactions (`TransactionResponse`). |

---

## Administrator System & Audit Summary

```text
GET /api/dashboard/admin-summary?limit=50
```

Returns platform-level counts and the most recent `AuditLog` records for administrative monitoring and security investigation (FR-024, FR-026, US-016, BR-037).

Query parameters:

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `limit` | integer | No | `50` | Number of recent audit records to return (`1..200`). |
| `action` | string | No | `null` | Optional filter by audit action (e.g., `"ORDER_FILLED"`). |
| `actorUserId` | guid | No | `null` | Optional filter by actor user ID. |

Responses:

| Status | Meaning |
| --- | --- |
| 200 | Admin summary and audit records returned. |
| 401 | Missing, invalid, or expired access token. |
| 403 | Authenticated user is not an `Administrator`. |

Returns: `AdminDashboardResponse`

| Field | Type | Description |
| --- | --- | --- |
| `totalUsers` | integer | Total registered users. |
| `activeUsers` | integer | Users with `IsActive == true`. |
| `totalPortfolios` | integer | Total portfolios in the system. |
| `activeAssets` | integer | Assets with `Status == ACTIVE`. |
| `pendingOrdersCount` | integer | System-wide `PENDING` orders. |
| `totalTransactionsCount` | integer | Total executed transactions. |
| `recentAuditLogs` | array | List of `AuditLogDto` entries ordered by `OccurredAt DESC`. |

`AuditLogDto` fields:

| Field | Type | Description |
| --- | --- | --- |
| `id` | long | Monotonic `bigint` audit record ID. |
| `actorUserId` | guid \| null | User who performed the action (`null` for system actions). |
| `action` | string | Event code (e.g., `"USER_REGISTERED"`, `"ORDER_CREATED"`, `"ORDER_CANCELLED"`, `"ORDER_FILLED"`). |
| `entityType` | string \| null | Affected entity type (e.g., `"Order"`, `"Portfolio"`). |
| `entityId` | string \| null | Affected entity identifier. |
| `occurredAt` | datetime (UTC) | Event timestamp. |
| `ipAddress` | string \| null | Client IP address if captured. |
| `details` | string \| null | Serialized JSON metadata. |
