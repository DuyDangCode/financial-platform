# Transaction API

> **Status: Planned.** `TransactionController` (`src/FinancialPlatform.Api/Controllers/TransactionController.cs`) currently exists as a 0-byte placeholder. The `Transaction` domain entity (`src/FinancialPlatform.Domain/Entities/Transaction.cs`) is implemented in `FinancialPlatform.Domain`.

## Overview

| Controller | Resource | Purpose |
| --- | --- | --- |
| Transaction | `api/transaction` | Immutable financial transaction history and single-transaction inspection (FR-018, FR-019, US-011, BR-025..027). |

Transactions are created automatically inside the order execution workflow (`POST /api/order/place` or `POST /api/order/{orderId}/execute`) and are strictly immutable (insert-only, BR-026). All endpoints require authentication (`[Authorize]`), enforce portfolio ownership (BR-036), and wrap successful responses in the standard [`ApiResponse<T>`](./api-design.md#3-response-envelope) envelope. Error semantics follow [`error-handling.md`](./error-handling.md).

## Endpoints

| Method | Path | Auth Required | Description |
| --- | --- | --- | --- |
| GET | `/api/transaction/list` | Yes | List and filter immutable transactions for a portfolio. |
| GET | `/api/transaction/{transactionId}` | Yes | Retrieve a specific transaction by its identifier. |

---

## List Portfolio Transactions

```text
GET /api/transaction/list?portfolioId={guid}&symbol=AAPL&side=BUY&from=2026-01-01T00:00:00Z&to=2026-12-31T23:59:59Z
```

Retrieves the historical transactions for a portfolio owned by the authenticated user, sorted by `ExecutedAt DESC` (FR-019, US-011).

Query parameters:

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `portfolioId` | guid | Yes | Portfolio ID owned by the caller. |
| `symbol` | string | No | Filter by asset symbol (case-insensitive, e.g., `"AAPL"`). |
| `side` | string | No | Filter by transaction side (`"BUY"` or `"SELL"`). |
| `orderId` | guid | No | Filter by originating order ID. |
| `from` | datetime (UTC) | No | Inclusive start timestamp on `ExecutedAt`. |
| `to` | datetime (UTC) | No | Inclusive end timestamp on `ExecutedAt`. |

Responses:

| Status | Meaning |
| --- | --- |
| 200 | Matching transactions returned. |
| 400 | Portfolio not found, not owned by caller, or invalid filter parameters (`DomainException`). |
| 401 | Missing, invalid, or expired access token. |

Returns: `IReadOnlyList<TransactionResponse>`

| Field | Type | Description |
| --- | --- | --- |
| `id` | guid | Transaction ID. |
| `orderId` | guid | Originating Order ID (BR-027). |
| `executionId` | guid | Originating Execution ID. |
| `portfolioId` | guid | Portfolio ID. |
| `assetId` | guid | Asset ID. |
| `symbol` | string | Asset ticker symbol (e.g., `"AAPL"`). |
| `assetName` | string | Asset display name. |
| `side` | string | `"BUY"` or `"SELL"`. |
| `quantity` | decimal | Executed quantity (`numeric(18,8)`). |
| `price` | decimal | Execution price per unit (`numeric(18,4)`). |
| `grossAmount` | decimal | Immutable snapshot `quantity × price` (`numeric(18,4)`). |
| `fee` | decimal | Execution fee (`numeric(18,4)`). |
| `netAmount` | decimal | For `BUY`: `grossAmount + fee`; for `SELL`: `grossAmount - fee`. |
| `executedAt` | datetime (UTC) | Execution timestamp. |

---

## Get Transaction Details

```text
GET /api/transaction/{transactionId}
```

Retrieves a single immutable transaction record and verifies that the caller owns the parent portfolio (or holds the `Administrator` role, FR-019, US-011, BR-036).

Route parameters:

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `transactionId` | guid | Yes | Transaction ID. |

Responses:

| Status | Meaning |
| --- | --- |
| 200 | Transaction details returned. |
| 400 | Transaction not found or caller is not authorized to view it (`DomainException`). |
| 401 | Missing, invalid, or expired access token. |

Returns: `TransactionResponse`.
