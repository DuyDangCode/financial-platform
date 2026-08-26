---
name: frontend-review
description: Review Next.js frontend changes in frontend/financial-platform-web for correctness, Next 16 pitfalls, Tailwind v4 usage, accessibility, hydration issues, and lint cleanliness. Use when asked to review frontend code, diffs, or PRs.
---

# Frontend Review (Next.js)

Review target: everything under `frontend/financial-platform-web/`. Read-only unless asked to fix findings.

## Before judging anything

The app is **Next.js 16 + React 19** with intentional breaking changes vs older Next versions. Verify any suspicious-looking Next API against the bundled docs in `node_modules/next/dist/docs/` before flagging it — e.g. `LayoutProps<"/">` in `app/layout.tsx` is correct here, not a bug.

## Checklist

**Correctness**
- Server Component by default; `"use client"` justified by actual interactivity.
- Async `params`/`searchParams` handled per Next 16 docs.
- Loading/error states exist for every fetch; no deprecated fetch/data patterns per bundled docs.
- List keys stable; no hydration mismatches (no `Date.now()`/`Math.random()`/`window` during render).
- The auto-generated warning block in `frontend/financial-platform-web/AGENTS.md` untouched in diffs (re-added by `next dev`; leave alone).

**Styling**
- Tailwind v4 CSS-first: new tokens go into `@theme inline` in `app/globals.css`; no reintroduced `tailwind.config.js`; no scattered arbitrary values where a token belongs.
- Responsive classes present; usable tap targets on mobile.

**Quality**
- `npm run lint` passes (eslint flat config).
- No new npm dependencies without user consent.
- No hardcoded secrets; `NEXT_PUBLIC_` env vars only for truly public values.

**A11y & UX**
- Semantic elements, labeled inputs, visible focus, alt text.
- Numbers right-aligned with `tabular-nums`; consistent currency formatting; gains/losses not color-only.

## Output format

Group findings as Blocker / Should fix / Nit, each with `file:line` and a concrete suggestion.
