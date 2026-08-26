# Login

Route: `/login` · Files: `app/login/page.tsx` (server shell) + `app/login/login-form.tsx` (client form)
Status: Designed (this document)

## Purpose

Sign-in screen for the Financial Platform. Exchanges email + password for a
`LoginResponse` via `POST /api/auth/login` (`docs/05-api/auth.md`), stores the
session client-side (`lib/auth.ts`), and redirects to `/`.

## Layout

Centered card on dark page background, max-width 420px:

```
┌────────────────────────────────────────────┐
│ (page bg zinc-950)                         │
│                                            │
│      ◆ Financial Platform   (wordmark → /) │
│  ┌──────────────────────────────────────┐  │
│  │ H1: Sign in                          │  │
│  │ p: Welcome back. Enter your          │  │
│  │    credentials to continue.          │  │
│  │                                      │  │
│  │ (API error banner — hidden by dflt)  │  │
│  │                                      │  │
│  │ EMAIL*                               │  │
│  │ [ you@example.com_____________ ]     │  │
│  │ (field error line)                   │  │
│  │ PASSWORD*                            │  │
│  │ [ ••••••••••••________________ ]     │  │
│  │ (field error line)                   │  │
│  │                                      │  │
│  │ [        Sign in        ] (primary)  │  │
│  │                                      │  │
│  │ Don't have an account? [Create one]  │  │
│  └──────────────────────────────────────┘  │
│         footer disclaimer (small)          │
└────────────────────────────────────────────┘
```

## Components

| Component | File | Content / data | Behavior |
| --------- | ---- | -------------- | -------- |
| Login page shell | `app/login/page.tsx` | Metadata (`title: "Sign in"`), centered wrapper, wordmark link | Server Component |
| `LoginForm` | `app/login/login-form.tsx` | Email + password inputs, submit button, banner | `"use client"`; controlled inputs |
| Error banner | inside `LoginForm` | `error.message` from API or generic network text | Rendered only when present; `role="alert"` |
| Field | `components/field.tsx` (shared) | Label + input + inline error `<p>` | Wired with `htmlFor`, `aria-invalid`, `aria-describedby` |

## Visual Style

Identical palette to [home.md](./home.md): page `bg-zinc-950`, card
`bg-zinc-900 border border-zinc-800 rounded-2xl p-8 shadow-xl`,
headings `text-zinc-50`, body `text-zinc-400`.

Inputs: `h-11 w-full rounded-lg border border-zinc-700 bg-zinc-950 px-3
text-sm text-zinc-100 placeholder:text-zinc-600 focus:outline-none
focus-visible:ring-2 ring-emerald-400`; invalid state adds
`border-red-500/60`. Labels: `text-sm font-medium text-zinc-300`.
Field errors: `text-xs text-red-400` prefixed with no icon but explicit
text. Submit button: primary emerald style as home (`h-11 w-full rounded-lg
bg-emerald-500 hover:bg-emerald-400 text-white font-medium`). Banner:
`bg-red-500/10 border border-red-500/40 text-red-300 rounded-lg px-4 py-3
text-sm`.

## States

| State | Presentation |
| ----- | ------------ |
| Idle | Empty fields; banner hidden; button enabled ("Sign in") |
| Client validation failure | Inline field errors under each invalid input (`aria-invalid`, `aria-describedby`); no API call; banner hidden unless a previous API error is still showing (cleared on new submit) |
| Submitting | Button disabled + inline spinner + "Signing in…"; both inputs disabled |
| API error (401 invalid credentials, network down, 5xx) | Red banner above the form with server-safe message (`InvalidCredentialsException` → "Invalid email or password."); unknown/network → "Unable to reach the server. Please try again." Button returns to enabled |
| API validation errors (400, `error.validationErrors`) | Each entry mapped onto its matching input as an inline field error (field names `email`, `password`); unmatched entries fall back to the banner |
| Success | Session persisted via auth context → `router.replace("/")` |

Client-side rules (mirror backend intent):

| Field | Rule |
| ----- | ---- |
| Email | required, valid email format |
| Password | required (backend has no length rule on login; any non-empty accepted) |

Validation runs on submit; a field re-validates on change after its first
failed submission.

## Responsive Behavior

| Breakpoint | Changes |
| ---------- | ------- |
| `< 640px` | Card full-width with `px-4` gutter, padding `p-6`, vertically centered content |
| `≥ 640px` | Fixed 420px card, centered horizontally and vertically (`min-h-dvh grid place-items-center`) |

No other responsive differences.

## Interactions & Flows

- Entry: from `/` header/hero CTAs.
- Exit: success → `/` (authed state); wordmark → `/`; "Don't have an account? Create one" → `/register`.
- Submit = HTML form submit handler (`onSubmit` + `preventDefault`), so Enter key works natively.
- No confirmation modals. Redirect uses `router.replace` (login page not kept in history).

## Data & API Mapping

| Element | Endpoint / field |
| ------- | ---------------- |
| Submit | `POST /api/auth/login` body `{ email, password }` |
| Response | envelope `{ success, message?, data?, error? }`; success → `data: LoginResponse` |
| Stored session | `token`, `expiresAt`, `refreshToken`, user info (`userId`, `userName`, `email`, `displayName`) ← `LoginResponse` |
| Base URL | `process.env.NEXT_PUBLIC_API_URL` else `https://localhost:7290` |

## Accessibility Notes

- `h1` = "Sign in"; card wrapped in `<main>`.
- Every input has a real `<label htmlFor>`; errors linked via `aria-describedby` and flagged with `aria-invalid="true"`.
- Banner has `role="alert"` (assertive) so failures are announced; it receives no automatic focus (fields remain context).
- Submit button disabled state also sets `aria-busy="true"` while submitting.
- Visible `focus-visible` rings on inputs, links, and buttons; logical tab order: email → password → sign in → register link → wordmark.
