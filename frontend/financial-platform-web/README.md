# Financial Platform — Frontend

Next.js 16 web application for the Financial Platform. Provides authentication (login/register) and a landing page. Connects to the .NET 8 backend API.

## Tech Stack

| Technology | Version | Notes |
|---|---|---|
| Next.js | 16.3.1 | App Router, TypeScript |
| React | 19.2.8 | Server Components by default |
| Tailwind CSS | v4 | Configured via `@theme` in CSS (no config file) |
| TypeScript | ^5 | Strict mode |
| ESLint | v9 | Flat config |

## Prerequisites

- Node.js (version managed via root `mise.toml`)
- npm

## Getting Started

Install dependencies:

```bash
npm install
```

Start the dev server:

```bash
npm run dev
```

Open [http://localhost:3000](http://localhost:3000) in your browser.

## Environment Variables

| Variable | Required | Default | Description |
|---|---|---|---|
| `NEXT_PUBLIC_API_URL` | No | `https://localhost:7290` | Backend API base URL |

Create a `.env.local` file to override defaults for local development.

## Project Structure

```text
frontend/financial-platform-web/
├── app/                  # App Router pages and layouts
│   ├── layout.tsx        # Root layout (AuthProvider, fonts, dark theme)
│   ├── page.tsx          # Landing page
│   ├── login/            # Login page + form
│   └── register/         # Register page + form
├── components/           # Shared UI components
├── lib/                  # Utilities and API client
│   ├── api.ts            # Fetch wrapper (envelope parsing, error handling)
│   └── auth.ts           # Auth functions (login/register/logout, localStorage)
└── public/               # Static assets
```

## Commands

```bash
npm run dev      # Start dev server
npm run build    # Production build
npm run start    # Start production server
npm run lint     # Run ESLint
```

## API Integration

All API calls go through `lib/api.ts`. The backend wraps responses in an envelope:

```typescript
interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
  error?: string;
}
```

Currently only `postJson` is implemented. The auth module (`lib/auth.ts`) handles login, registration, and logout against the backend's `/api/auth/*` endpoints.

## Authentication

- Auth state is managed via React Context (`components/auth-provider.tsx`).
- Session is persisted in `localStorage` under key `fp.auth.session`.
- Access auth state via the `useAuth()` hook.

## Current State

Only the authentication module (login, register, logout) and a landing page are implemented. Dashboard, portfolio, trading, and market data features are planned but not yet built.
