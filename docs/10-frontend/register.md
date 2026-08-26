# Register

Route: `/register` · Files: `app/register/page.tsx` (server shell) + `app/register/register-form.tsx` (client form)
Status: Designed (this document)

## Purpose

Account creation screen. Posts to `POST /api/auth/register`
(`docs/05-api/auth.md`). The backend returns a full `LoginResponse` on
success, so registration doubles as auto-login: the session is stored and the
user is redirected to `/`.

## Layout

Centered card, max-width 480px (slightly wider than login — more fields):

```
┌────────────────────────────────────────────────┐
│      ◆ Financial Platform       (wordmark → /) │
│  ┌──────────────────────────────────────────┐  │
│  │ H1: Create your account                  │  │
│  │ p: Start tracking portfolios in minutes. │  │
│  │                                          │  │
│  │ (API error banner — hidden by dflt)      │  │
│  │ USERNAME*                                │  │
│  │ [ thanhduy____________________ ]         │  │
│  │ EMAIL*                                   │  │
│  │ [ you@example.com_____________ ]         │  │
│  │ PASSWORD*   hint: 8–128 characters       │  │
│  │ [ ••••••••••••________________ ]         │  │
│  │ CONFIRM PASSWORD*                        │  │
│  │ [ ••••••••••••________________ ]         │  │
│  │                                          │  │
│  │ ▸ Add personal details (optional)        │  │
│  │   (collapsed <details> panel:)           │  │
│  │   FIRST NAME    [______]                 │  │
│  │   LAST NAME     [______]                 │  │
│  │   DISPLAY NAME  [______]                 │  │
│  │   PHONE NUMBER  [______]                 │  │
│  │                                          │  │
│  │ [     Create account      ] (primary)    │  │
│  │                                          │  │
│  │ Already have an account? [Sign in]       │  │
│  └──────────────────────────────────────────┘  │
└────────────────────────────────────────────────┘
```

## Components

| Component | File | Content / data | Behavior |
| --------- | ---- | -------------- | -------- |
| Register page shell | `app/register/page.tsx` | Metadata (`title: "Create account"`), centered wrapper, wordmark | Server Component |
| `RegisterForm` | `app/register/register-form.tsx` | Required fields + optional `<details>` section + submit | `"use client"`; controlled inputs |
| Optional details toggle | native `<details>/<summary>` inside form | First name, Last name, Display name, Phone number | Collapsible; values submit even when collapsed |
| Error banner + Field | shared with login (`components/field.tsx`) | Same behavior as login.md | |

## Visual Style

Same palette and input/button styling as [login.md](./login.md). Card is
`max-w-md` (28rem / 448px content box), padding `p-8`. The `<summary>` row:
`text-sm font-medium text-zinc-300 cursor-pointer` with a chevron that rotates
90° when open (`group-open:rotate-90`); its panel adds a subtle top border
(`border-t border-zinc-800 pt-6 mt-2`) separating it from required fields.
Password hint text under the field: `text-xs text-zinc-500`.

## States

| State | Presentation |
| ----- | ------------ |
| Idle | Required fields empty; optional section collapsed; banner hidden; button enabled ("Create account") |
| Client validation failure | Inline errors under each invalid field; no API call |
| Submitting | Button disabled + spinner + "Creating account…"; all inputs disabled |
| API error (409 duplicate email/username, 400 domain rule, network) | Banner shows server message ("A user with this email already exists." etc.); network → generic retry text |
| API validation errors (400, `error.validationErrors`) | Entries mapped by `field` onto matching inputs (`userName`, `email`, `password`, `phoneNumber`, …); unmatched → banner |
| Success | Session stored (auto-login) → `router.replace("/")` |

Client-side validation mirrors the backend contract in `docs/05-api/auth.md`:

| Field | Rule |
| ----- | ---- |
| Username | required, ≤ 256 chars |
| Email | required, valid format, ≤ 256 chars |
| Password | required, 8–128 chars |
| Confirm password | required, must equal Password |
| First name / Last name | optional, ≤ 128 chars each |
| Display name | optional, ≤ 256 chars |
| Phone number | optional, ≤ 32 chars |

Validation on submit; per-field re-validation after first failed attempt,
same as login.

## Responsive Behavior

Identical to login.md: single column at every breakpoint; card full-width
below 640px, fixed width centered above.

## Interactions & Flows

- Entry: from `/` primary CTA or login page link.
- Exit: success → `/` authed; wordmark → `/`; "Already have an account? Sign in" → `/login`.
- Optional section uses native `<details>` — keyboard-operable for free; no JS state needed. Its inputs participate in validation only when non-empty (all rules are length caps).
- Submit via native form semantics (Enter submits).
- Redirect uses `router.replace`.

## Data & API Mapping

| Element | Endpoint / field |
| ------- | ---------------- |
| Submit | `POST /api/auth/register` body `{ userName, email, password, firstName?, lastName?, displayName?, phoneNumber? }` (optional keys omitted when empty) |
| Response | envelope; success → `data: LoginResponse` → stored as session (same as login) |
| Errors | `error.message` → banner; `error.validationErrors[] { field, message }` → inline field errors keyed by camelCase field name |
| Base URL | `process.env.NEXT_PUBLIC_API_URL` else `https://localhost:7290` |

## Accessibility Notes

- `h1` = "Create your account"; card wrapped in `<main>`.
- Labels, `aria-invalid`, `aria-describedby`, `role="alert"` banner — identical mechanics to login.md.
- `<summary>` is natively focusable/activatable; add `aria-controls` not required (native semantics suffice).
- Focus order: username → email → password → confirm → summary → (optional fields when open) → create account → sign-in link → wordmark.
