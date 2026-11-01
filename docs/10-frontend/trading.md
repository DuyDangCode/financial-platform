# Trading & Orders

Route: `/trading` · File: `app/trading/page.tsx`
Status: Designed (this document)

## Purpose

Order entry, asset market lookup, and order lifecycle management screen (`US-005`, `US-007..010`, `FR-008`, `FR-011..017`). Allows investors to search tradable assets, place `BUY` or `SELL` orders (`MARKET` or `LIMIT`), execute or cancel `PENDING` orders, and filter order history.

## Layout

Desktop wireframe (left Asset Browser + right Order Ticket, bottom Order History table):

```text
┌──────────────────────────────────────────────────────────────────────────┐
│ HEADER  ◆ Financial Platform   [Dashboard] [Portfolios] [Trade] [History]│
├──────────────────────────────────────────┬───────────────────────────────┤
│ MARKET ASSETS                            │ PLACE ORDER                   │
│ [ Search symbol or name...            ]  │ Portfolio: [Long-Term Gr. ▼]  │
│                                          │ Asset:     AAPL ($195.00)     │
│ Sym    Name                 Type   Price │ Side:      [ BUY ] [ SELL ]   │
│ AAPL   Apple Inc.           STOCK $195.00│ Type:      [MARKET] [LIMIT]   │
│ MSFT   Microsoft Corp.      STOCK $420.00│ Quantity*: [ 10.00000000   ]  │
│ SPY    SPDR S&P 500 ETF     ETF   $510.00│ Limit $:   [ (disabled)    ]  │
│                                          │ Est. Total: $1,950.00         │
│                                          │ [   Submit BUY Order   ]      │
├──────────────────────────────────────────┴───────────────────────────────┤
│ ORDER HISTORY  Filter: [Status: All ▼] [Symbol: All]                     │
│ Created           Sym   Side  Type    Qty   Limit    Status     Actions  │
│ 2026-11-01 19:30  AAPL  BUY   MARKET  10    —        FILLED     [Detail] │
│ 2026-11-01 19:25  MSFT  BUY   LIMIT   5     $395.00  PENDING    [Exec][X]│
└──────────────────────────────────────────────────────────────────────────┘
```

## Components

| Component | File | Content / data | Behavior |
| --- | --- | --- | --- |
| `AssetMarketTable` | `app/trading/asset-market-table.tsx` | Searchable list of `ACTIVE` assets and prices | Clicking an asset populates the Order Ticket |
| `OrderTicketForm` | `app/trading/order-ticket-form.tsx` | Portfolio selector, `BUY`/`SELL` toggle, `MARKET`/`LIMIT` toggle, `quantity`, `limitPrice` | Validates BR-008..011 client-side and posts to `POST /api/order/place` |
| `OrderHistoryTable` | `app/trading/order-history-table.tsx` | Portfolio orders with status badges and action buttons | `PENDING` rows show `[Execute]` (`POST /api/order/{id}/execute`) and `[Cancel]` (`POST /api/order/{id}/cancel`) |

## Visual Style

- `BUY` mode button: `bg-emerald-500 hover:bg-emerald-400 text-white`.
- `SELL` mode button: `bg-rose-600 hover:bg-rose-500 text-white`.
- Order status badges:
  - `FILLED`: `bg-emerald-500/10 text-emerald-400` + `✓`
  - `PENDING` / `PROCESSING`: `bg-amber-500/10 text-amber-300` + `◔`
  - `REJECTED`: `bg-red-500/10 text-red-400` + `✕`
  - `CANCELLED`: `bg-zinc-800 text-zinc-400` + `⊘`

## States

| State | Presentation |
| --- | --- |
| `MARKET` selected | `Limit Price` input is hidden or disabled (`value = ""`); Estimated Total uses `quantity × asset.currentPrice` |
| `LIMIT` selected | `Limit Price` input is required (`> 0`); Estimated Total uses `quantity × limitPrice` |
| Submitting order | Submit button disabled + spinner ("Placing order…") |
| Order rejected / error | Red alert banner above ticket showing domain error (e.g., `"Cannot sell more than the current position quantity."`) |
| Cancelling order | Row action button shows `"Cancelling..."` and disables until response |

## Responsive Behavior

| Breakpoint | Changes |
| --- | --- |
| `< 1024px` | Order Ticket stacks above Asset Market table; Order History scrolls horizontally |
| `≥ 1024px` | 7-column Asset Market table + 5-column sticky Order Ticket; full-width Order History below |

## Interactions & Flows

1. User selects an active portfolio and clicks an asset row (e.g., `AAPL`).
2. User chooses `BUY` or `SELL`, `MARKET` or `LIMIT`, enters `Quantity` (and `LimitPrice` if `LIMIT`), and submits.
3. On `200 OK`, Order History and portfolio positions refresh automatically, and a toast/status banner confirms whether the order was `FILLED` immediately or queued as `PENDING`.
4. Clicking `[Cancel]` on a `PENDING` order calls `POST /api/order/{orderId}/cancel`.

## Data & API Mapping

| Element | Endpoint (`docs/05-api/asset.md`, `docs/05-api/order.md`) |
| --- | --- |
| Asset list | `GET /api/asset/list?status=ACTIVE&search={q}` |
| Place order | `POST /api/order/place` |
| Execute pending order | `POST /api/order/{orderId}/execute` |
| Cancel pending order | `POST /api/order/{orderId}/cancel` |
| Order history | `GET /api/order/list?portfolioId={guid}&status={status}&symbol={symbol}` |

## Accessibility Notes

- `BUY`/`SELL` and `MARKET`/`LIMIT` segmented controls use `role="radiogroup"` or `<button aria-pressed="true|false">`.
- `Limit Price` field dynamically updates `aria-required` and `disabled` when switching between `MARKET` and `LIMIT`.
