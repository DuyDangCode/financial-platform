# Dashboard — Portfolio Overview

Route: `/dashboard` · File: `app/dashboard/page.tsx`
Status: Designed (this document)

## Purpose

Primary authenticated workspace for investors (`US-004`, `US-006`, `US-012`, `FR-005`, `FR-020`, `FR-021`). Displays aggregated net worth, unrealized/realized P/L, portfolio cards, top open positions, and recent transactions fetched from `GET /api/dashboard/overview` (`docs/05-api/dashboard.md`).

## Layout

Desktop wireframe (top navigation bar + 12-column content grid, max-width 1280px):

```text
┌──────────────────────────────────────────────────────────────────────────┐
│ HEADER  ◆ Financial Platform   [Dashboard] [Portfolios] [Trade] [History]│
├──────────────────────────────────────────────────────────────────────────┤
│ H1: Portfolio Overview                           [ + Create Portfolio ]  │
│                                                                          │
│ ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐ ┌──────────┐ │
│ │ TOTAL VALUE     │ │ UNREALIZED P/L  │ │ REALIZED P/L    │ │ TOTAL P/L│ │
│ │ $42,900.00      │ │ ▲ +$3,200.00    │ │ ▲ +$850.00      │ │ ▲ +$4,050│ │
│ └─────────────────┘ └─────────────────┘ └─────────────────┘ └──────────┘ │
│                                                                          │
│ ┌───────────────────────────────────────────┐ ┌────────────────────────┐ │
│ │ MY PORTFOLIOS (cards / table)             │ │ TOP POSITIONS          │ │
│ │ • Long-Term Growth   ACTIVE   $31,500.00  │ │ AAPL   20 sh   $3,900  │ │
│ │ • Swing ETFs         ACTIVE   $11,400.00  │ │ MSFT   10 sh   $4,200  │ │
│ └───────────────────────────────────────────┘ └────────────────────────┘ │
│                                                                          │
│ ┌──────────────────────────────────────────────────────────────────────┐ │
│ │ RECENT TRANSACTIONS (last 10)                                        │ │
│ │ 2026-11-01  BUY   AAPL   10 @ $195.00   $1,950.00   [View Order]     │ │
│ └──────────────────────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────────────────────┘
```

Mobile (`< 640px`): KPI summary cards stack into a 2×2 grid; Portfolios and Top Positions stack vertically (`1 col`); Recent Transactions table becomes horizontally scrollable with sticky symbol column.

## Components

| Component | File | Content / data | Behavior |
| --- | --- | --- | --- |
| `AppNav` | `components/app-nav.tsx` | Links to `/dashboard`, `/portfolios`, `/trading`, `/transactions` + user menu | Client component; redirects unauthenticated visitors to `/login` |
| `KpiSummaryStrip` | `app/dashboard/kpi-strip.tsx` | Total Market Value, Unrealized P/L, Realized P/L, Total P/L | Right-aligned tabular numbers (`tabular-nums`), gain/loss arrow icons |
| `PortfolioListCard` | `app/dashboard/portfolio-list.tsx` | List of user portfolios with status badge and value | Clicking a row navigates to `/portfolios?id={id}`; `+ Create Portfolio` opens modal |
| `TopPositionsCard` | `app/dashboard/top-positions.tsx` | Top 5 open positions by market value | Shows symbol, quantity, avg entry, current price, unrealized P/L |
| `RecentTransactionsTable` | `app/dashboard/recent-transactions.tsx` | Last 10 executed transactions | Links to `/transactions` |

## Visual Style

Consistent with [`home.md`](./home.md):

- Page surface: `bg-zinc-950` (`#09090B`), cards `bg-zinc-900 border border-zinc-800 rounded-xl p-6`.
- Financial numbers: `font-mono tabular-nums text-right`. Explicit currency formatting (`$1,234.56`).
- Gains: `text-emerald-400` (`#34D399`) paired with `▲ +` prefix.
- Losses: `text-red-400` (`#F87171`) paired with `▼ -` prefix.
- Neutral / zero: `text-zinc-400` (`#A1A1AA`) paired with `—` prefix.

## States

| State | Presentation |
| --- | --- |
| Loading | 4 pulsing KPI skeleton cards (`bg-zinc-800/60 animate-pulse h-28`) + table row skeletons |
| Empty (0 portfolios) | Dashed empty state card: "You don't have any portfolios yet" with primary CTA `[ Create your first portfolio ]` |
| Error | Alert banner (`role="alert"`) with server error message and `[ Retry ]` button |
| Populated | Full KPI strip, portfolio list, top positions, and recent transactions |

## Responsive Behavior

| Breakpoint | Changes |
| --- | --- |
| `< 640px` | KPI grid `grid-cols-2`; main panels `grid-cols-1`; nav links collapse into drawer |
| `640–1024px` | KPI grid `grid-cols-2`; main panels `grid-cols-1` |
| `≥ 1024px` | KPI grid `grid-cols-4`; Portfolios (7 cols) + Top Positions (5 cols) side by side |

## Interactions & Flows

- Unauthenticated mount → `router.replace("/login")`.
- Clicking `+ Create Portfolio` → opens dialog posting to `POST /api/account/create`, then re-fetches overview.
- Clicking a portfolio row → navigates to `/portfolios` with that portfolio selected.

## Data & API Mapping

| Element | Endpoint / field (`docs/05-api/dashboard.md`) |
| --- | --- |
| Overview data | `GET /api/dashboard/overview` → `InvestorDashboardResponse` |
| KPI cards | `totalMarketValue`, `totalUnrealizedPnL`, `totalRealizedPnL`, `totalPnL` |
| Portfolios list | `portfolios[]` (`PortfolioSummaryResponse`) |
| Top positions | `topPositions[]` (`PositionResponse`) |
| Recent transactions | `recentTransactions[]` (`TransactionResponse`) |

## Accessibility Notes

- Single `h1` ("Portfolio Overview"); section cards titled with `h2`.
- Positive/negative P/L values always include `▲ +` or `▼ -` text symbols alongside color so screen readers and color-blind users distinguish gains from losses.
- Data tables include `<caption>` (visually hidden) and `<th scope="col">`.
