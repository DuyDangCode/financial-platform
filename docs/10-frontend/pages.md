# Frontend Pages

| Route | Page / Purpose | Design Doc | Status |
| --- | --- | --- | --- |
| `/` | Home / marketing landing with auth CTAs | [home.md](./home.md) | Designed |
| `/login` | Login screen (`POST /api/auth/login`) | [login.md](./login.md) | Designed |
| `/register` | Registration screen (`POST /api/auth/register`) | [register.md](./register.md) | Designed |
| `/dashboard` | Investor portfolio overview & KPI summary (`GET /api/dashboard/overview`) | [dashboard.md](./dashboard.md) | Designed |
| `/portfolios` | Portfolio CRUD & positions breakdown (`api/account`) | [portfolios.md](./portfolios.md) | Designed |
| `/trading` | Asset lookup, order entry, execution & order history (`api/asset`, `api/order`) | [trading.md](./trading.md) | Designed |
| `/transactions` | Immutable financial transaction history (`api/transaction`) | [transactions.md](./transactions.md) | Designed |

Statuses: Planned → Designed (spec exists) → Implemented (code + lint green).
