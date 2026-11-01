# Portfolios & Positions

Route: `/portfolios` · File: `app/portfolios/page.tsx`
Status: Designed (this document)

## Purpose

Dedicated portfolio management and position inspection screen (`US-003`, `US-004`, `US-006`, `US-012`, `FR-004..006`, `FR-009`, `FR-020..021`). Allows investors to create, rename, activate/disable portfolios, and inspect open and closed positions with per-asset allocation weights.

## Layout

Desktop wireframe (left portfolio selector rail + right detail workspace, max-width 1280px):

```text
┌──────────────────────────────────────────────────────────────────────────┐
│ HEADER  ◆ Financial Platform   [Dashboard] [Portfolios] [Trade] [History]│
├────────────────────────┬─────────────────────────────────────────────────┤
│ PORTFOLIOS      [+New] │ H1: Long-Term Growth    [ACTIVE] [Edit] [Trade] │
│                        │ Notes: Core US equities and index ETFs          │
│ ▸ Long-Term Growth     │                                                 │
│   $31,500.00  ACTIVE   │ Market Value: $31,500.00  Cost: $28,300.00      │
│                        │ Unrealized: ▲ +$3,200.00  Realized: ▲ +$850.00  │
│   Swing ETFs           │                                                 │
│   $11,400.00  ACTIVE   │ POSITIONS                    [x] Include closed │
│                        │ ┌──────┬──────┬────────┬────────┬─────────────┐ │
│   Archived Test        │ │Sym   │Qty   │Avg Cost│Cur Prc │Unrealized   │ │
│   $0.00       DISABLED │ ├──────┼──────┼────────┼────────┼─────────────┤ │
│                        │ │AAPL  │20.00 │$180.00 │$195.00 │▲ +$300.00   │ │
│                        │ │MSFT  │10.00 │$400.00 │$420.00 │▲ +$200.00   │ │
│                        │ └──────┴──────┴────────┴────────┴─────────────┘ │
└────────────────────────┴─────────────────────────────────────────────────┘
```

## Components

| Component | File | Content / data | Behavior |
| --- | --- | --- | --- |
| `PortfolioSidebar` | `app/portfolios/portfolio-sidebar.tsx` | Selectable list of user portfolios + `+ New` button | Selecting an item loads `GET /api/account/{id}` and `GET /api/account/{id}/positions` |
| `PortfolioHeader` | `app/portfolios/portfolio-header.tsx` | Name, status badge (`ACTIVE`/`DISABLED`), notes, Edit modal trigger, Status toggle button | Calls `POST /api/account/{id}/update` and `POST /api/account/{id}/status` |
| `PositionsTable` | `app/portfolios/positions-table.tsx` | Table of positions with toggle for `includeClosed` | Displays `symbol`, `quantity`, `averageEntryPrice`, `currentPrice`, `marketValue`, `unrealizedPnL`, `realizedPnL` |
| `CreateEditPortfolioModal` | `app/portfolios/portfolio-modal.tsx` | Form for `name`, `baseCurrency`, `notes` | Client validation (`name <= 128`, `notes <= 512`) |

## Visual Style

- Active portfolio item in sidebar: `bg-emerald-500/10 border-emerald-500/40 text-zinc-50`.
- Status badge: `ACTIVE` → `bg-emerald-500/10 text-emerald-400`; `DISABLED` → `bg-zinc-800 text-zinc-400`.
- Closed position rows (`isClosed == true`): muted `text-zinc-500` with `CLOSED` pill badge.

## States

| State | Presentation |
| --- | --- |
| Loading | Sidebar and position table skeleton rows |
| No portfolios | Centered onboarding card prompting portfolio creation |
| Empty positions (0 holdings) | Table empty state: "No positions in this portfolio yet" + `[ Place your first order ]` link to `/trading` |
| Error | Inline alert banner with `[ Retry ]` |

## Responsive Behavior

| Breakpoint | Changes |
| --- | --- |
| `< 768px` | Portfolio selector collapses into a top `<select>` dropdown; positions table scrolls horizontally |
| `≥ 768px` | 280px left rail + fluid right workspace |

## Interactions & Flows

- Toggle `Include closed` checkbox → re-fetches `GET /api/account/{portfolioId}/positions?includeClosed=true`.
- Click `Disable` on an `ACTIVE` portfolio → confirmation prompt noting that disabled portfolios reject new orders (BR-004) → `POST /api/account/{portfolioId}/status`.

## Data & API Mapping

| Element | Endpoint (`docs/05-api/account.md`) |
| --- | --- |
| Portfolio list | `GET /api/account/list` |
| Create portfolio | `POST /api/account/create` |
| Portfolio detail | `GET /api/account/{portfolioId}` |
| Update portfolio | `POST /api/account/{portfolioId}/update` |
| Toggle status | `POST /api/account/{portfolioId}/status` |
| Positions table | `GET /api/account/{portfolioId}/positions?includeClosed={bool}` |

## Accessibility Notes

- Modal dialog traps focus, closes on `Escape`, and binds `aria-labelledby` to the modal title.
- Status badges include explicit text (`ACTIVE`, `DISABLED`, `CLOSED`), never color dots alone.
