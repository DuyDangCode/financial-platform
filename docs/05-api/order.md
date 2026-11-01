# Order & Execution API

> **Status: Planned.** No `OrderController` exists yet. The `Order`, `Execution`, `Transaction`, and `Position` domain entities are implemented in `FinancialPlatform.Domain`.

## Overview

| Controller | Resource | Purpose |
| --- | --- | --- |
| Order | `api/order` | Order placement, validation, simulated execution, cancellation, and order history (FR-011..017, US-007..010, BR-008..019). |

All endpoints require authentication (`[Authorize]`), verify that the target portfolio belongs to the authenticated user (BR-036), and wrap successful responses in the standard [`ApiResponse<T>`](./api-design.md#3-response-envelope) envelope. Error semantics follow [`error-handling.md`](./error-handling.md).

## Endpoints

| Method | Path | Auth Required | Description |
| --- | --- | --- | --- |
| POST | `/api/order/place` | Yes | Create a new `BUY` or `SELL` order (`MARKET` or `LIMIT`) and optionally execute it immediately. |
| POST | `/api/order/{orderId}/execute` | Yes | Trigger simulated execution for a `PENDING` order against current market price. |
| POST | `/api/order/{orderId}/cancel` | Yes | Cancel a `PENDING` order. |
| GET | `/api/order/{orderId}` | Yes | Retrieve a specific order and its execution details. |
| GET | `/api/order/list` | Yes | List and filter orders for a portfolio. |

---

## Place Order

```text
POST /api/order/place
```

Creates a new trading order (`Order.Create(...)`) after validating domain preconditions (FR-011, FR-012, US-007, US-008):

1. The caller owns the target `Portfolio` (BR-036) and `Portfolio.Status == ACTIVE` (BR-004, BR-009).
2. The target `Asset` exists and `Asset.Status == ACTIVE` (BR-007, BR-010).
3. `quantity > 0` (BR-008).
4. If `orderType == "LIMIT"`, `limitPrice > 0` is required; if `orderType == "MARKET"`, `limitPrice` must be omitted or `null` (BR-011).
5. If `side == "SELL"`, the portfolio holds a `Position` for the asset with `Position.Quantity >= quantity` (BR-023).
6. When `autoExecute` is `true` (default for MVP synchronous trading flow):
   - A `MARKET` order transitions `PENDING → PROCESSING → FILLED` immediately at `Asset.CurrentPrice`, creating an `Execution`, `Transaction`, and updating `Position` in the same transaction.
   - A `LIMIT` order executes immediately if the market price satisfies the limit (`BUY`: `CurrentPrice <= LimitPrice`; `SELL`: `CurrentPrice >= LimitPrice`); otherwise it remains `PENDING`.

Request body (`PlaceOrderRequest`):

| Field | Type | Required | Constraints |
| --- | --- | --- | --- |
| `portfolioId` | guid | Yes | Non-empty Guid of an active portfolio owned by caller. |
| `symbol` | string | Yes | Ticker symbol of an active asset (e.g., `"AAPL"`). |
| `side` | string | Yes | `"BUY"` or `"SELL"`. |
| `orderType` | string | Yes | `"MARKET"` or `"LIMIT"`. |
| `quantity` | decimal | Yes | `> 0` (`numeric(18,8)`). |
| `limitPrice` | decimal \| null | Conditional | Required and `> 0` when `orderType == "LIMIT"`; `null` when `"MARKET"`. |
| `autoExecute` | boolean | No | Defaults to `true`. If `false`, order stays `PENDING` until `/execute` is called. |

Example:

```json
{
  "portfolioId": "6c85f64e-8a11-4d40-92b7-4e1179b0b123",
  "symbol": "AAPL",
  "side": "BUY",
  "orderType": "MARKET",
  "quantity": 10,
  "limitPrice": null,
  "autoExecute": true
}
```

Responses:

| Status | Meaning |
| --- | --- |
| 200 | Order created (and executed if eligible). |
| 400 | Validation error or domain invariant violation (inactive portfolio/asset, insufficient position for SELL, invalid quantity/limit price). |
| 401 | Missing, invalid, or expired access token. |
| 409 | Concurrency conflict (`DbUpdateConcurrencyException`) during position/order update. |

Returns: `OrderDetailResponse`

| Field | Type | Description |
| --- | --- | --- |
| `id` | guid | Order ID. |
| `portfolioId` | guid | Portfolio ID. |
| `assetId` | guid | Asset ID. |
| `symbol` | string | Asset ticker symbol. |
| `side` | string | `"BUY"` or `"SELL"`. |
| `orderType` | string | `"MARKET"` or `"LIMIT"`. |
| `quantity` | decimal | Ordered quantity (`numeric(18,8)`). |
| `limitPrice` | decimal \| null | Limit price (`numeric(18,4)`), or `null` for `MARKET`. |
| `filledQuantity` | decimal | Executed quantity (`0` until filled). |
| `remainingQuantity` | decimal | `quantity - filledQuantity`. |
| `status` | string | `PENDING`, `PROCESSING`, `FILLED`, `REJECTED`, or `CANCELLED`. |
| `rejectionReason` | string \| null | Populated if `status == "REJECTED"`. |
| `createdAt` | datetime (UTC) | Order creation timestamp. |
| `completedAt` | datetime (UTC) \| null | Terminal state timestamp (`FILLED`, `REJECTED`, `CANCELLED`). |
| `execution` | `ExecutionSummaryDto` \| null | Execution details when `status == "FILLED"`. |

`ExecutionSummaryDto` fields:

| Field | Type | Description |
| --- | --- | --- |
| `executionId` | guid | Execution ID. |
| `transactionId` | guid | Resulting Transaction ID. |
| `executedQuantity` | decimal | Filled quantity. |
| `executionPrice` | decimal | Simulated fill price. |
| `grossAmount` | decimal | `executedQuantity × executionPrice`. |
| `fee` | decimal | Execution fee (`0.0000` in MVP). |
| `executedAt` | datetime (UTC) | Execution timestamp. |

---

## Execute Pending Order

```text
POST /api/order/{orderId}/execute
```

Processes and executes an existing `PENDING` order against the asset's current market price within a single atomic database transaction (`Order → Execution → Transaction → Position → AuditLog`, FR-016, FR-017, BR-018, BR-019, BR-031..033).

Idempotency guarantee (FR-029, US-017): If the order is already `FILLED` (or a duplicate execution request hits the unique index `IX_Executions_OrderId`), the endpoint returns the existing `OrderDetailResponse` without applying a second transaction or position update.

Route parameters:

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `orderId` | guid | Yes | Target order ID. |

Responses:

| Status | Meaning |
| --- | --- |
| 200 | Order processed (either `FILLED`, `REJECTED` if business rules failed at execution time, or remained `PENDING` if limit price not met). |
| 400 | Order is in a non-executable state (`CANCELLED` or `REJECTED`) or not owned by caller (`DomainException`). |
| 401 | Missing, invalid, or expired access token. |
| 409 | Concurrent modification conflict (`xmin`). |

Returns: `OrderDetailResponse`.

---

## Cancel Order

```text
POST /api/order/{orderId}/cancel
```

Cancels an eligible (`PENDING`) order (`Order.Cancel()`, FR-014, US-009, BR-012, BR-013) and records an `ORDER_CANCELLED` audit event. Filled, rejected, or already cancelled orders cannot be cancelled.

Route parameters:

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `orderId` | guid | Yes | Target order ID. |

Responses:

| Status | Meaning |
| --- | --- |
| 200 | Order cancelled (`status = "CANCELLED"`, `completedAt` set). |
| 400 | Order is not in `PENDING` status (e.g., already `FILLED`) or not owned by caller (`DomainException`). |
| 401 | Missing, invalid, or expired access token. |
| 409 | Concurrent modification conflict (`xmin`). |

Returns: `OrderDetailResponse`.

---

## Get Order Details

```text
GET /api/order/{orderId}
```

Retrieves a specific order along with its execution summary if filled (FR-015).

Responses:

| Status | Meaning |
| --- | --- |
| 200 | Order returned. |
| 400 | Order not found or not owned by caller (`DomainException`). |
| 401 | Missing, invalid, or expired access token. |

Returns: `OrderDetailResponse`.

---

## List / Filter Orders

```text
GET /api/order/list?portfolioId={guid}&status=FILLED&symbol=AAPL&from=2026-01-01T00:00:00Z&to=2026-12-31T23:59:59Z
```

Retrieves order history for a portfolio owned by the authenticated user, ordered by `CreatedAt DESC` (FR-015, US-010).

Query parameters:

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `portfolioId` | guid | Yes | Target portfolio ID. |
| `status` | string | No | Filter by `PENDING`, `PROCESSING`, `FILLED`, `REJECTED`, or `CANCELLED`. |
| `symbol` | string | No | Filter by asset ticker symbol (case-insensitive). |
| `side` | string | No | Filter by `BUY` or `SELL`. |
| `from` | datetime (UTC) | No | Inclusive lower bound on `CreatedAt`. |
| `to` | datetime (UTC) | No | Inclusive upper bound on `CreatedAt`. |

Responses:

| Status | Meaning |
| --- | --- |
| 200 | Filtered order list returned. |
| 400 | Portfolio not found, not owned by caller, or invalid filter values. |
| 401 | Missing, invalid, or expired access token. |

Returns: `IReadOnlyList<OrderDetailResponse>`.
