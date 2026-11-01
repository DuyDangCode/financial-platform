# Transactions History

Route: `/transactions` · File: `app/transactions/page.tsx`
Status: Designed (this document)

## Purpose

Immutable financial ledger and transaction history screen (`US-011`, `FR-018`, `FR-019`, `BR-025..027`). Allows investors to inspect completed executions and filter historical transactions by portfolio, asset symbol, side (`BUY`/`SELL`), and date range.

## Layout

Desktop wireframe (single-column workspace with filter bar + ledger table, max-width 1280px):

```text
┌──────────────────────────────────────────────────────────────────────────┐
│ HEADER  ◆ Financial Platform   [Dashboard] [Portfolios] [Trade] [History]│
├──────────────────────────────────────────────────────────────────────────┤
│ H1: Transaction History                                                  │
│ p: Immutable record of executed trades across your portfolios.           │
│                                                                          │
│ FILTERS                                                                  │
│ [Portfolio: Long-Term Growth ▼] [Symbol: All] [Side: All ▼] [Date Range] │
│                                                                          │
│ ┌────────────────────┬──────┬──────┬──────────┬──────────┬─────────────┐ │
│ │ Executed At (UTC)  │ Sym  │ Side │ Quantity │ Price    │ Gross Amount│ │
│ ├────────────────────┼──────┼──────┼──────────┼──────────┼─────────────┤ │
│ │ 2026-11-01 19:30   │ AAPL │ BUY  │ 10.0000  │ $195.00  │ $1,950.00   │ │
│ │ 2026-10-28 14:15   │ MSFT │ SELL │ 5.0000   │ $420.00  │ $2,100.00   │ │
│ └────────────────────┴──────┴──────┴──────────┴──────────┴─────────────┘ │
└──────────────────────────────────────────────────────────────────────────┘
```

## Components

| Component | File | Content / data | Behavior |
| --- | --- | --- | --- |
| `TransactionFilters` | `app/transactions/transaction-filters.tsx` | Portfolio selector, symbol input, side dropdown (`ALL`, `BUY`, `SELL`), `from`/`to` date inputs | Updates query parameters and re-fetches `GET /api/transaction/list` |
| `TransactionsTable` | `app/transactions/transactions-table.tsx` | Ledger rows (`executedAt`, `symbol`, `side`, `quantity`, `price`, `grossAmount`, `fee`) | Clicking a row opens `TransactionDetailDrawer` |
| `TransactionDetailDrawer` | `app/transactions/transaction-drawer.tsx` | Displays `transactionId`, `orderId`, `executionId`, `portfolioId`, and full snapshot breakdown | Read-only inspection (`GET /api/transaction/{transactionId}`) |

## Visual Style

- `BUY` badge: `bg-emerald-500/10 text-emerald-400 border border-emerald-500/30 px-2 py-0.5 rounded text-xs font-semibold`.
- `SELL` badge: `bg-rose-500/10 text-rose-400 border border-rose-500/30 px-2 py-0.5 rounded text-xs font-semibold`.
- All numeric columns (`Quantity`, `Price`, `Gross Amount`, `Fee`) right-aligned with `font-mono tabular-nums`.

## States

| State | Presentation |
| --- | --- |
| Loading | 6 skeleton table rows |
| Empty (no matching transactions) | Empty state message: "No transactions match the selected filters" + `[ Clear filters ]` button |
| Error | Alert banner (`role="alert"`) with `[ Retry ]` |

## Responsive Behavior

| Breakpoint | Changes |
| --- | --- |
| `< 640px` | Filter controls stack full-width; table scrolls horizontally with sticky `Sym` and `Gross Amount` columns |
| `≥ 640px` | Inline filter bar + full table layout |

## Interactions & Flows

- Changing any filter updates the URL search params (`?portfolioId=...&symbol=...&side=...`) and fetches matching records.
- Clicking a transaction row opens the detail drawer showing traceability IDs (`OrderId`, `ExecutionId`) per `BR-027`.

## Data & API Mapping

| Element | Endpoint (`docs/05-api/transaction.md`) |
| --- | --- |
| Portfolio selector | `GET /api/account/list` |
| Transactions list | `GET /api/transaction/list?portfolioId={guid}&symbol={sym}&side={side}&from={from}&to={to}` |
| Transaction detail | `GET /api/transaction/{transactionId}` |

## Accessibility Notes

- Filter inputs have explicit `<label>` elements.
- Table uses semantic `<table>`, `<thead>`, `<tbody>`, and `<th scope="col">`.
