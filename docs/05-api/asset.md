# Asset & Market Data API

> **Status: Planned.** No `AssetController` exists yet. The `Asset` domain entity (`src/FinancialPlatform.Domain/Entities/Asset.cs`) and enums (`AssetType`, `AssetStatus`) are implemented in `FinancialPlatform.Domain`.

## Overview

| Controller | Resource | Purpose |
| --- | --- | --- |
| Asset | `api/asset` | Financial asset catalog lookup, search, market price refresh, and administrative asset management (FR-007..008, FR-022..023, US-005). |

All endpoints wrap successful responses in the standard [`ApiResponse<T>`](./api-design.md#3-response-envelope) envelope. Error semantics follow [`error-handling.md`](./error-handling.md).

## Endpoints

| Method | Path | Auth Required | Description |
| --- | --- | --- | --- |
| GET | `/api/asset/list` | Yes | List and search supported financial assets. |
| GET | `/api/asset/{symbol}` | Yes | Retrieve a single asset and its current price by ticker symbol. |
| POST | `/api/asset/create` | Yes (`Administrator`) | Register a new tradable asset in the catalog. |
| POST | `/api/asset/{symbol}/price` | Yes (`Administrator`) | Refresh the simulated market price of an asset. |
| POST | `/api/asset/{symbol}/status` | Yes (`Administrator`) | Activate or deactivate an asset for trading. |

---

## List / Search Assets

```text
GET /api/asset/list?search=AAP&assetType=STOCK&status=ACTIVE
```

Returns supported financial assets matching the optional filter criteria (FR-008, US-005). Symbol matching is case-insensitive (normalized to uppercase).

Query parameters:

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `search` | string | No | Prefix or substring filter on `Symbol` or `Name`. |
| `assetType` | string | No | Filter by `STOCK` or `ETF`. |
| `status` | string | No | Filter by `ACTIVE` or `INACTIVE` (defaults to all for admins, `ACTIVE` when trading). |

Responses:

| Status | Meaning |
| --- | --- |
| 200 | Matching assets returned. |
| 401 | Missing, invalid, or expired access token. |

Returns: `IReadOnlyList<AssetResponse>`

| Field | Type | Description |
| --- | --- | --- |
| `id` | guid | Asset unique identifier. |
| `symbol` | string | Uppercase ticker symbol (e.g., `"AAPL"`, `"SPY"`). |
| `name` | string | Full instrument name (e.g., `"Apple Inc."`). |
| `assetType` | string | `"STOCK"` or `"ETF"`. |
| `currentPrice` | decimal | Latest simulated market price (`numeric(18,4)`). |
| `currency` | string | 3-letter ISO currency code (`"USD"`). |
| `priceUpdatedAt` | datetime (UTC) | Timestamp when `currentPrice` was last refreshed. |
| `status` | string | `"ACTIVE"` or `"INACTIVE"`. |

---

## Get Asset by Symbol

```text
GET /api/asset/{symbol}
```

Retrieves detailed metadata and current market price for a single asset by its symbol (FR-008, US-005).

Route parameters:

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `symbol` | string | Yes | Ticker symbol (case-insensitive, e.g., `AAPL`). |

Responses:

| Status | Meaning |
| --- | --- |
| 200 | Asset returned. |
| 400 | Asset symbol not found (`DomainException`). |
| 401 | Missing, invalid, or expired access token. |

Returns: `AssetResponse`.

---

## Create Asset (Admin)

```text
POST /api/asset/create
```

Creates a new financial asset in the platform catalog (`Asset.Create(...)`, FR-007, BR-006). Requires `Administrator` privileges (BR-037).

Request body (`CreateAssetRequest`):

| Field | Type | Required | Constraints |
| --- | --- | --- | --- |
| `symbol` | string | Yes | Non-empty, max 16 chars; normalized to uppercase. |
| `name` | string | Yes | Non-empty, max 256 chars. |
| `assetType` | string | Yes | `"STOCK"` or `"ETF"`. |
| `currentPrice` | decimal | Yes | `>= 0`. |
| `currency` | string | No | 3-letter ISO 4217 code (defaults to `"USD"`). |

Example:

```json
{
  "symbol": "NVDA",
  "name": "NVIDIA Corporation",
  "assetType": "STOCK",
  "currentPrice": 125.5000,
  "currency": "USD"
}
```

Responses:

| Status | Meaning |
| --- | --- |
| 200 | Asset created and returned. |
| 400 | Validation error or domain rule violation (`DomainException`). |
| 401 | Missing, invalid, or expired access token. |
| 403 | Authenticated user is not an `Administrator`. |
| 409 | An asset with this symbol already exists (BR-006). |

Returns: `AssetResponse`.

---

## Refresh Asset Price (Admin / Market Simulator)

```text
POST /api/asset/{symbol}/price
```

Updates the current simulated price of an asset (`Asset.RefreshPrice(price)`, FR-022, FR-023) and updates `PriceUpdatedAt` to `DateTime.UtcNow`.

Request body (`RefreshAssetPriceRequest`):

| Field | Type | Required | Constraints |
| --- | --- | --- | --- |
| `price` | decimal | Yes | `>= 0` (`numeric(18,4)`). |

Example:

```json
{
  "price": 195.2500
}
```

Responses:

| Status | Meaning |
| --- | --- |
| 200 | Price updated and returned. |
| 400 | Negative price or asset not found (`DomainException`). |
| 401 | Missing, invalid, or expired access token. |
| 403 | Authenticated user is not an `Administrator`. |

Returns: `AssetResponse`.

---

## Update Asset Status (Admin)

```text
POST /api/asset/{symbol}/status
```

Activates or deactivates an asset (`Asset.Activate()` / `Asset.Deactivate()`). Deactivated (`INACTIVE`) assets cannot receive new trading orders (BR-007, BR-010), while existing positions remain visible.

Request body (`UpdateAssetStatusRequest`):

| Field | Type | Required | Constraints |
| --- | --- | --- | --- |
| `status` | string | Yes | `"ACTIVE"` or `"INACTIVE"`. |

Example:

```json
{
  "status": "INACTIVE"
}
```

Responses:

| Status | Meaning |
| --- | --- |
| 200 | Asset status updated. |
| 400 | Invalid status or symbol not found. |
| 401 | Missing, invalid, or expired access token. |
| 403 | Authenticated user is not an `Administrator`. |

Returns: `AssetResponse`.
