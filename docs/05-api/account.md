# Account (Portfolio) API

> **Status: Planned.** `AccountController` (`src/FinancialPlatform.Api/Controllers/AccountController.cs`) currently exists as a 0-byte placeholder. The `Portfolio` and `Position` domain entities are implemented in `FinancialPlatform.Domain`.

## Overview

| Controller | Resource | Purpose |
| --- | --- | --- |
| Account | `api/account` | Portfolio creation, updates, position tracking, and portfolio valuation/performance (FR-004..006, FR-009..010, FR-020..021, US-003, US-004, US-006, US-012). |

All endpoints require authentication (`[Authorize]`), enforce resource ownership (`Portfolio.UserId == currentUserId` per BR-036 unless caller has `Administrator` role), and wrap successful responses in the standard [`ApiResponse<T>`](./api-design.md#3-response-envelope) envelope. Error semantics follow [`error-handling.md`](./error-handling.md).

## Endpoints

| Method | Path | Auth Required | Description |
| --- | --- | --- | --- |
| POST | `/api/account/create` | Yes | Create a new investment portfolio for the current user. |
| GET | `/api/account/list` | Yes | List all portfolios owned by the current user with summary valuation. |
| GET | `/api/account/{portfolioId}` | Yes | Retrieve full portfolio details, valuation, P/L, and open positions. |
| POST | `/api/account/{portfolioId}/update` | Yes | Rename a portfolio or update its notes. |
| POST | `/api/account/{portfolioId}/status` | Yes | Activate or disable a portfolio. |
| GET | `/api/account/{portfolioId}/positions` | Yes | List positions held by the portfolio (open by default, optionally including closed). |
| GET | `/api/account/{portfolioId}/performance` | Yes | Retrieve detailed portfolio valuation and P/L breakdown. |

---

## Create Portfolio

```text
POST /api/account/create
```

Creates a new portfolio owned by the authenticated user in `ACTIVE` status (FR-004, US-003, BR-003, BR-005).

Request body (`CreatePortfolioRequest`):

| Field | Type | Required | Constraints |
| --- | --- | --- | --- |
| `name` | string | Yes | Non-empty, max 128 chars, unique per user. |
| `baseCurrency` | string | No | 3-letter ISO 4217 code (defaults to `"USD"`). |
| `notes` | string | No | Max 512 chars. |

Example:

```json
{
  "name": "Long-Term Growth",
  "baseCurrency": "USD",
  "notes": "Core US equities and index ETFs"
}
```

Responses:

| Status | Meaning |
| --- | --- |
| 200 | Portfolio created and returned. |
| 400 | Validation failure or domain rule violation (`DomainException`). |
| 401 | Missing, invalid, or expired access token. |
| 409 | A portfolio with the same name already exists for this user. |

Returns: `PortfolioSummaryResponse`

| Field | Type | Description |
| --- | --- | --- |
| `id` | guid | Unique portfolio identifier. |
| `userId` | guid | Owner user identifier. |
| `name` | string | Portfolio name. |
| `baseCurrency` | string | 3-letter currency code (`USD`). |
| `status` | string | `ACTIVE` or `DISABLED`. |
| `notes` | string \| null | Optional notes. |
| `marketValue` | decimal | Current total market value (`0.0000` on creation). |
| `unrealizedPnL` | decimal | Current unrealized P/L (`0.0000` on creation). |
| `realizedPnL` | decimal | Accumulated realized P/L (`0.0000` on creation). |
| `totalPnL` | decimal | `unrealizedPnL + realizedPnL`. |
| `createdAt` | datetime (UTC) | Creation timestamp. |

---

## List My Portfolios

```text
GET /api/account/list
```

Returns all portfolios belonging to the authenticated user along with computed market value and P/L summaries (FR-005, US-004).

Responses:

| Status | Meaning |
| --- | --- |
| 200 | List of portfolios returned. |
| 401 | Missing, invalid, or expired access token. |

Returns: `IReadOnlyList<PortfolioSummaryResponse>`.

---

## Get Portfolio Details

```text
GET /api/account/{portfolioId}
```

Retrieves a single portfolio's metadata, computed valuation, open positions, and recent transactions (FR-005, US-004, BR-036).

Route parameters:

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `portfolioId` | guid | Yes | Target portfolio ID. |

Responses:

| Status | Meaning |
| --- | --- |
| 200 | Portfolio details returned. |
| 400 | Portfolio not found or access denied (`DomainException`). |
| 401 | Missing, invalid, or expired access token. |

Returns: `PortfolioDetailResponse`

| Field | Type | Description |
| --- | --- | --- |
| `id` | guid | Portfolio ID. |
| `name` | string | Portfolio name. |
| `baseCurrency` | string | Currency code (`USD`). |
| `status` | string | `ACTIVE` or `DISABLED`. |
| `notes` | string \| null | Portfolio description/notes. |
| `marketValue` | decimal | `Σ(Position.Quantity × Asset.CurrentPrice)` (BR-029). |
| `costBasis` | decimal | `Σ(Position.Quantity × Position.AverageEntryPrice)`. |
| `unrealizedPnL` | decimal | `Σ((Asset.CurrentPrice - Position.AverageEntryPrice) × Position.Quantity)` (BR-030). |
| `realizedPnL` | decimal | `Σ(Position.RealizedPnL)` across all positions in the portfolio. |
| `totalPnL` | decimal | `unrealizedPnL + realizedPnL`. |
| `positions` | array | List of `PositionResponse` items for open positions. |
| `createdAt` | datetime (UTC) | Creation timestamp. |
| `updatedAt` | datetime (UTC) \| null | Last modification timestamp. |

---

## Update Portfolio

```text
POST /api/account/{portfolioId}/update
```

Updates editable portfolio metadata (`Name`, `Notes`) for a portfolio owned by the current user (FR-006).

Request body (`UpdatePortfolioRequest`):

| Field | Type | Required | Constraints |
| --- | --- | --- | --- |
| `name` | string | Yes | Non-empty, max 128 chars, unique per user. |
| `notes` | string | No | Max 512 chars. |

Example:

```json
{
  "name": "US Tech & ETFs",
  "notes": "Updated focus on large-cap tech and S&P 500 ETF"
}
```

Responses:

| Status | Meaning |
| --- | --- |
| 200 | Portfolio updated. |
| 400 | Validation error, portfolio not found, or unauthorized (`DomainException`). |
| 401 | Missing, invalid, or expired access token. |
| 409 | Duplicate portfolio name or concurrent modification conflict (`xmin`). |

Returns: `PortfolioSummaryResponse`.

---

## Change Portfolio Status

```text
POST /api/account/{portfolioId}/status
```

Activates or disables a portfolio (`Portfolio.Activate()` / `Portfolio.Disable()`). A disabled portfolio cannot accept new orders (BR-004, BR-009).

Request body (`UpdatePortfolioStatusRequest`):

| Field | Type | Required | Constraints |
| --- | --- | --- | --- |
| `status` | string | Yes | `"ACTIVE"` or `"DISABLED"`. |

Example:

```json
{
  "status": "DISABLED"
}
```

Responses:

| Status | Meaning |
| --- | --- |
| 200 | Status updated. |
| 400 | Invalid status value or portfolio not found. |
| 401 | Missing, invalid, or expired access token. |

Returns: `PortfolioSummaryResponse`.

---

## List Portfolio Positions

```text
GET /api/account/{portfolioId}/positions?includeClosed=false
```

Retrieves positions held by the specified portfolio (FR-009, US-006). By default, only open positions (`Quantity > 0`) are returned; passing `includeClosed=true` also returns historical closed positions (`Quantity == 0`) with their accumulated `RealizedPnL`.

Query parameters:

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `includeClosed` | boolean | No | `false` | Whether to include positions where `Quantity == 0`. |

Responses:

| Status | Meaning |
| --- | --- |
| 200 | Positions returned. |
| 400 | Portfolio not found or not owned by caller. |
| 401 | Missing, invalid, or expired access token. |

Returns: `IReadOnlyList<PositionResponse>`

| Field | Type | Description |
| --- | --- | --- |
| `id` | guid | Position ID. |
| `portfolioId` | guid | Owning portfolio ID. |
| `assetId` | guid | Held asset ID. |
| `symbol` | string | Asset ticker symbol (e.g., `"AAPL"`). |
| `assetName` | string | Asset name. |
| `quantity` | decimal | Units currently held (`numeric(18,8)`). |
| `averageEntryPrice` | decimal \| null | Weighted average entry price (`null` when `isClosed` is `true`). |
| `currentPrice` | decimal | Current market price from `Asset.CurrentPrice`. |
| `marketValue` | decimal | `quantity × currentPrice` (BR-028). |
| `costBasis` | decimal | `quantity × (averageEntryPrice ?? 0)`. |
| `unrealizedPnL` | decimal | `(currentPrice - averageEntryPrice) × quantity` (`0` when closed, BR-030). |
| `unrealizedPnLPercent` | decimal | Percentage gain/loss relative to `averageEntryPrice` (`0` when closed). |
| `realizedPnL` | decimal | Cumulative realized P/L from historical sells (`numeric(18,4)`). |
| `isClosed` | boolean | `true` when `quantity == 0`. |

---

## Get Portfolio Performance

```text
GET /api/account/{portfolioId}/performance
```

Calculates and returns the portfolio's current valuation and P/L metrics alongside per-position allocation weights (FR-020, FR-021, US-012, BR-028..030).

Responses:

| Status | Meaning |
| --- | --- |
| 200 | Performance breakdown returned. |
| 400 | Portfolio not found or not owned by caller. |
| 401 | Missing, invalid, or expired access token. |

Returns: `PortfolioPerformanceResponse`

| Field | Type | Description |
| --- | --- | --- |
| `portfolioId` | guid | Portfolio ID. |
| `baseCurrency` | string | Valuation currency (`USD`). |
| `totalMarketValue` | decimal | `Σ(Position Market Value)` for open positions. |
| `totalCostBasis` | decimal | `Σ(Position Cost Basis)` for open positions. |
| `unrealizedPnL` | decimal | Total unrealized P/L across open positions. |
| `unrealizedPnLPercent` | decimal | `(unrealizedPnL / totalCostBasis) × 100` (`0` when `totalCostBasis == 0`). |
| `realizedPnL` | decimal | Total realized P/L across all positions (open and closed). |
| `totalPnL` | decimal | `unrealizedPnL + realizedPnL`. |
| `openPositionsCount` | integer | Number of positions with `Quantity > 0`. |
| `calculatedAt` | datetime (UTC) | Timestamp of valuation computation. |
| `allocations` | array | Per-asset allocation breakdown (`symbol`, `marketValue`, `weightPercent`, `unrealizedPnL`). |
