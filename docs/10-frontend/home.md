# Home — Marketing Landing Page

Route: `/` · File: `app/page.tsx`
Status: Designed (this document)

## Purpose

Public marketing landing page for the **Financial Platform** (portfolio & trading
operations platform, see `docs/01-product/scope.md`). It introduces the product,
shows the roadmap feature set, and routes visitors to `/register` (primary) and
`/login` (secondary). When the visitor is already authenticated, the page greets
them and offers sign-out instead of auth CTAs.

This page performs no data fetching; the only dynamic input is the client-side
auth state from `lib/auth.ts`.

## Layout

Desktop wireframe (single column, centered content, max-width 1152px):

```
┌──────────────────────────────────────────────────────────────┐
│ HEADER  ◆ Financial Platform            [Log in] [Get started]│  ← sticky, blurred bg
├──────────────────────────────────────────────────────────────┤
│                                                              │
│   HERO                                                       │
│   eyebrow: PORTFOLIO & TRADING OPERATIONS                    │
│   H1: Your capital, clearly under control.                   │
│   p: Track portfolios, place simulated trades and follow     │
│      market data — one platform, built for clarity.          │
│   [ Create free account ]   [ Sign in ]                      │
│                                                              │
├──────────────────────────────────────────────────────────────┤
│   FEATURE GRID  (section title + 3 cards, equal height)      │
│   ┌───────────────┐ ┌───────────────┐ ┌───────────────┐      │
│   │ ◈ icon        │ │ ◈ icon        │ │ ◈ icon        │      │
│   │ Portfolio     │ │ Trading       │ │ Market data   │      │
│   │ tracking      │ │               │ │               │      │
│   │ Roadmap badge │ │ Roadmap badge │ │ Roadmap badge │      │
│   └───────────────┘ └───────────────┘ └───────────────┘      │
├──────────────────────────────────────────────────────────────┤
│ FOOTER  © 2026 Financial Platform · simulation disclaimer    │
└──────────────────────────────────────────────────────────────┘
```

Mobile (< 640px): header collapses to logo + "Get started" button only;
hero text centers, CTAs stack full-width; feature grid becomes 1 column.

### Authenticated variant of the CTA zone

When `isAuthenticated` (after hydration), the hero CTA block and the header
auth area swap:

```
HERO CTA ZONE (authed):
┌──────────────────────────────────────────────┐
│ ✓ Signed in as Thanh Duy (user@example.com)  │
│ You're ready to go. Sign out below or come   │
│ back later.                                  │
│ [ Sign out ]                                 │
└──────────────────────────────────────────────┘
```

Header (authed): `Signed in as <displayName>` text + `[ Sign out ]` button
instead of Log in / Get started.

## Components

| Component          | File                        | Content / data                                   | Behavior |
| ------------------ | --------------------------- | ------------------------------------------------ | -------- |
| `SiteHeader`       | `components/site-header.tsx`| Wordmark (link `/`), auth area                   | Client component; reads `useAuth()`; sticky top, backdrop blur |
| `HeroSection`      | inside `app/page.tsx`       | Eyebrow, H1, subheadline                         | Static (server-rendered) |
| `HomeCta`          | `components/home-cta.tsx`   | Guest CTAs ↔ signed-in panel                     | Client component; renders guest CTAs until mounted to avoid hydration mismatch |
| `FeatureGrid`      | inside `app/page.tsx`       | 3 static cards: Portfolio tracking, Trading, Market data | Static; each card carries a "Roadmap" badge (features not yet implemented backend-side) |
| `SiteFooter`       | `components/site-footer.tsx`| Copyright, product name, simulation disclaimer   | Static (server-rendered) |

## Visual Style

Dark, professional fintech look. Fixed dark palette (independent of OS
`prefers-color-scheme`):

| Role            | Token / class              | Value     |
| --------------- | -------------------------- | --------- |
| Page background | `bg-zinc-950`              | `#09090B` |
| Card surface    | `bg-zinc-900`              | `#18181B` |
| Border          | `border-zinc-800`          | `#27272A` |
| Heading text    | `text-zinc-50`             | `#FAFAFA` |
| Body text       | `text-zinc-400`            | `#A1A1AA` |
| Brand / primary | `emerald-500`, hover `emerald-400` | `#10B981` / `#34D399` |
| Primary label   | white on emerald           |           |
| Secondary btn   | transparent, `border-zinc-700`, hover `bg-zinc-800` | |
| Success icon    | `emerald-400` paired with ✓ glyph (never color alone) | |
| Focus ring      | `ring-emerald-400`         |           |

Typography: Geist Sans (`--font-geist-sans`) everywhere; H1 `text-4xl sm:text-5xl
font-semibold tracking-tight`; eyebrow `text-xs font-semibold tracking-widest
uppercase text-emerald-400`. Buttons `h-11 px-5 rounded-lg text-sm font-medium`.
Spacing: hero `py-24`, feature section `py-20`, card padding `p-6`, radius
`rounded-xl` cards / `rounded-lg` buttons.

Iconography: inline SVG line icons (chart, arrows-exchange, globe/pulse),
stroke `currentColor`, 24px, inside a `bg-emerald-500/10 text-emerald-400
rounded-lg p-2` chip.

## States

| State        | Presentation |
| ------------ | ------------ |
| Guest (default SSR + pre-hydration) | Log in / Get started buttons in header and hero |
| Authenticated | Header shows "Signed in as …" + Sign out; hero CTA zone replaced by signed-in panel (✓ icon + name + email) |
| Sign out clicked | Button disabled + "Signing out…" label until `logout()` resolves, then back to guest UI |
| Hover/focus | All links/buttons get visible focus ring (`focus-visible:ring-2`) and hover shade change |
| No loading/error/empty states | Page fetches nothing; auth read failures fall back silently to guest view |

## Responsive Behavior

| Breakpoint | Changes |
| ---------- | ------- |
| `< 640px` (mobile) | Header hides secondary links, keeps logo + Get started; hero text-center; CTAs stacked `w-full`; grid 1 col |
| `640–1024px` | Hero left-aligned, CTAs inline; grid stays 3 cols ≥ 768px (`md:grid-cols-3`), 1 col below |
| `≥ 1024px` | Full layout as wireframed, max-width 1152px centered |

## Interactions & Flows

- Entry: direct URL, or exit point from `/login`, `/register` after success/failure.
- `Create free account` → `next/link` to `/register`; `Log in` / `Sign in` → `/login`.
- `Sign out` → calls `logout()` from the auth context (best-effort API revoke then local clear); UI returns to guest state without navigation.
- Wordmark → `/`. Footer contains no navigation links (v1).

## Data & API Mapping

| Element | Source |
| ------- | ------ |
| Signed-in name / email | `useAuth().user.displayName` / `.email` ← `LoginResponse.displayName` / `LoginResponse.email` (`docs/05-api/auth.md`) |
| Sign out | `POST /api/auth/logout` body `{ refreshToken }` → best-effort, then clear local session |
| Feature cards | Static copy; features are roadmap scope per `docs/01-product/scope.md` (no API yet) |

## Accessibility Notes

- One `h1` (hero). Section headings `h2`.
- Header/nav landmarks: `<header>`, `<nav aria-label="Main">`, `<main>`, `<footer>`.
- Signed-in state announced by visible text + ✓ icon (not color alone); panel wrapped with `role="status"` so the post-hydration swap is announced politely.
- Sign out is a real `<button>`; CTAs are real links.
- Focus order: header → hero → CTAs → features → footer; visible `focus-visible` rings on all interactive elements.
