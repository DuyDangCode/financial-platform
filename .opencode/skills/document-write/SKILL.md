---
name: document-write
description: Write and update general project documentation — README, project overview, product/domain docs, architecture and ADRs, security, testing, infrastructure, operations, changelog. Use when asked to document features, update docs, fill skeleton files, or write ADRs/changelogs. EXCLUDES docs/05-api (api-design), docs/04-database (db-design), and docs/10-frontend (ui-design) which are owned by other skills.
---

# Document Writing

## Scope

**In scope:**

- Root `README.md` (and `frontend/financial-platform-web/README.md` for frontend-facing facts).
- `docs/00-project/` — overview.md, changelog.md, roadmap.md
- `docs/01-product/` — user-stories, scope, requirements
- `docs/02-domain/` — domain-overview, domain-model, business-rules
- `docs/03-architecture/` — architecture-overview, system-architecture, decisions/ADR-00N.md
- `docs/06-security/` — authentication, authorization
- `docs/07-testing/` — testing-strategy
- `docs/08-infrastructure/` — deployment, docker, local-development
- `docs/09-operations/` — logging, monitoring, troubleshooting

**Out of scope — owned by other skills, never write these:**

| Folder | Owner |
|---|---|
| `docs/05-api/` | api-design skill |
| `docs/04-database/` | db-design skill |
| `docs/10-frontend/` | ui-design skill |

When a change affects those areas, note it in your summary instead of editing them.

## Ground rules

1. **Trust code over prose.** Many files are 0-byte skeletons (e.g. `ADR-001.md`, all of `06-security/`, `07-testing/testing-strategy.md`, `08-infrastructure/*`, `09-operations/*`) while siblings have substantial content. Read the target file first; match the tone/depth of filled sibling files in the same folder.
2. Document what IS implemented and mark planned work explicitly. Currently implemented: Identity/Auth only (register, login, refresh, logout, password reset/change). Role/Permission/Account/Dashboard/Transaction controllers are placeholders; Portfolio/Trading/Market Data do not exist.
3. ADRs: number sequentially (`ADR-002.md`, ...) since ADR-001 is an empty stub; sections = Context / Decision / Status / Consequences.
4. Changelog (`00-project/changelog.md`): newest entry first, one section per version/date with Added/Changed/Fixed subsections; only record real merged changes.
5. Keep root `README.md` edits minimal and factual — commands and layout only, no marketing.
6. Reflect actual conventions from code (not memory): `ApiResponse<T>` envelope, ports 7290/5150, Postgres 16 via docker-compose, InMemory fallback when connection string missing, no MediatR, manual handler registration.

## Verification

Re-read your diff: every factual claim traceable to a real file/config value in the repo, and no out-of-scope folder was touched.
