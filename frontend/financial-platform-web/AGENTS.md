<!-- BEGIN:nextjs-agent-rules -->

# This is NOT the Next.js you know

This version has breaking changes — APIs, conventions, and file structure may all differ from your training data. Read the relevant guide in `node_modules/next/dist/docs/` (resolved from this file's directory; in monorepos the `next` package may not be visible from the repo root) before writing any code. Heed deprecation notices.

This block is written and re-added by `next dev` — verify at `node_modules/next/dist/server/lib/generate-agent-files.js`. Removing it from a diff only re-creates the uncommitted change; committing it with your work keeps the tree clean.

<!-- END:nextjs-agent-rules -->

---

# Frontend Project Conventions

## Stack

- **Next.js 16** (App Router, TypeScript)
- **React 19**
- **Tailwind CSS v4** — no `tailwind.config.js`; theme configured via `@theme` blocks in `app/globals.css`
- **ESLint v9** — flat config in `eslint.config.mjs`
- No UI component library (all components hand-written)

## Project Structure

```text
frontend/financial-platform-web/
├── app/                  # App Router pages and layouts
│   ├── layout.tsx        # Root layout (AuthProvider, Geist fonts, dark bg)
│   ├── page.tsx          # Landing page
│   ├── login/            # Login page + form
│   └── register/         # Register page + form
├── components/           # Shared UI components (flat directory)
├── lib/                  # Utilities and API client
│   ├── api.ts            # Fetch wrapper (envelope parsing, error handling)
│   └── auth.ts           # Auth functions (login/register/logout, localStorage)
└── public/               # Static assets
```

## Conventions

### File placement
- Pages go in `app/<route>/page.tsx` (server component by default).
- Client components use `"use client"` directive and live alongside their page or in `components/`.
- Shared utilities and API helpers go in `lib/`.

### Components
- Prefer server components. Only add `"use client"` when the component needs interactivity (event handlers, hooks, browser APIs).
- Components are co-located — no barrel files or `index.ts` re-exports.
- Naming: `kebab-case` for files, `PascalCase` for exported components.

### API integration
- All API calls go through `lib/api.ts`. Do not call `fetch` directly from components.
- The backend API base URL is read from `NEXT_PUBLIC_API_URL` env var (defaults to `https://localhost:7290`).
- The backend wraps responses in an `ApiResponse` envelope (`{ success, message?, data?, error? }`).
- Currently only `postJson` is implemented. Add `getJson`, `putJson`, `deleteJson` as needed.

### Auth
- Auth state is managed via React Context in `components/auth-provider.tsx`.
- Session is persisted in `localStorage` under key `fp.auth.session`.
- Access the auth context via `useAuth()` hook from `components/auth-provider`.

### Styling
- Tailwind CSS v4 — use utility classes directly in JSX.
- Dark theme is default (`bg-zinc-950`).
- Font: Geist (sans) + Geist Mono (mono) loaded via `next/font`.
- CSS custom properties defined in `app/globals.css`.

### Type safety
- TypeScript strict mode is enabled.
- Use the `@/*` path alias for imports from project root (e.g. `@/components/field`).
- Types for API responses are defined in `lib/api.ts`.

## Commands

```bash
npm run dev      # Start dev server (http://localhost:3000)
npm run build    # Production build
npm run start    # Start production server
npm run lint     # Run ESLint
```

## Environment Variables

| Variable | Required | Default | Description |
|---|---|---|---|
| `NEXT_PUBLIC_API_URL` | No | `https://localhost:7290` | Backend API base URL |

Create a `.env.local` file for local overrides. Never commit `.env*` files.

## What's Not Built Yet

- Dashboard, portfolio, trading, market data pages
- Token refresh logic (refreshToken stored but unused)
- Route protection / middleware
- GET/PUT/DELETE API methods
- Tests
- CI/CD and deployment config
