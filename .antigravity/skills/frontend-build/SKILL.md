---
name: frontend-build
description: Implement frontend features end-to-end in the Next.js app at frontend/financial-platform-web — scaffolding pages/components, wiring backend API calls, styling, and verifying with lint/dev server. Use when asked to build, implement, add, or change frontend functionality.
---

# Frontend Build (Next.js)

Target: `frontend/financial-platform-web/` — Next.js 16, React 19, TypeScript, Tailwind v4, Node 22 pinned via root `mise.toml`.

## Order of operations

1. Read `frontend/financial-platform-web/AGENTS.md` first, then the relevant bundled docs under `node_modules/next/dist/docs/` for every Next API you touch. This version breaks from older training data. Do not remove the auto-generated block from AGENTS.md.
2. Check existing structure (`app/`, theme tokens in `app/globals.css`) and extend rather than reinvent. Confirm any new npm dependency with the user before installing.
3. Build with Server Components by default; colocate route files under `app/`; extract components into `components/` once reused.
4. Style with Tailwind v4 utilities; new design tokens go into `@theme inline` in `globals.css` — never create a `tailwind.config.js`.
5. Wire data to the backend API (https://localhost:7290, JSON envelope `{ success, message, data, error }` from `ApiResponse<T>`); implement loading/error states explicitly; keep auth tokens out of logs.

## Verify

- `npm run lint` must pass.
- `npm run dev` and manually exercise the changed flow before reporting done.
