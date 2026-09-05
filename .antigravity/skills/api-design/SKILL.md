---
name: api-design
description: Design REST endpoints for one API domain and document them in a per-domain markdown file under docs/05-api/, listing each endpoint with method, path, auth, request/response schemas, and status codes. Use when asked to design endpoints, define an API for a module, or write/update an endpoint reference doc. Does NOT write controller code.
---

# API Design (endpoint docs)

Design endpoints for a domain and write them into the **per-domain API document**. This is a specification step — do not implement controllers.

## Output files

1. `docs/05-api/<module>.md` — the domain's endpoint reference (create if missing; kebab/lowercase name matching the controller resource, e.g. `account.md`, `dashboard.md`).
2. Update `docs/05-api/api-design.md` so the new/changed module is listed in **both** the module-reference table (§1) and the endpoint-overview tables (§5.1 Implemented / §5.2 Planned), replacing `TBD` with the file link.

## Before designing

- Read `docs/05-api/api-design.md` (conventions, envelope, auth) and `docs/05-api/error-handling.md` (error semantics).
- Ground the design in requirements: `docs/01-product/user-stories.md`, `docs/01-product/scope.md`, `docs/02-domain/*`.
- Match the format of existing references: `docs/05-api/auth.md` (implemented example) or `docs/05-api/transaction.md` (planned skeleton).

## Required per-domain document structure

```markdown
# <Domain> API

> **Status: Planned|Implemented.** <one line on implementation state,
> e.g. "Controller exists only as a placeholder".>

## Overview
Controller/resource table + auth note + link to api-design.md conventions.

## Endpoints
Summary table: | Method | Path | Auth Required | Description |

## <Endpoint Name>            (repeat per endpoint)
```text
POST /api/<resource>/<action>
```
Description of what it does.

Request body (`<Name>Request`):
| Field | Type | Required | Constraints |
+ JSON example.

Responses:
| Status | Meaning |
(only codes the endpoint can actually produce)

Returns: `<ResponseDto>` + field table.
```

## Design rules

- Follow global conventions from api-design.md: paths `api/{controller}/{action}`, camelCase JSON, Guid ids as strings, every success wrapped in the `ApiResponse<T>` envelope (`success`/`message`/`data`/`error`) — envelope itself not repeated per endpoint, just say what `data` contains.
- Mark protected endpoints Auth Required = Yes (`[Authorize]`); public ones No.
- Status codes must be consistent with `GlobalExceptionMiddleware` mapping (`DomainException` → 400, `UserAlreadyExistsException` → 409, `InvalidCredentialsException` → 401, else 500); a new exception needs a mapping before its code appears in docs.
- Resource naming: plural-free, lowercase resources (`api/account`, `api/dashboard`), POST-first style like the auth module unless GET is clearly natural for reads.
- Secure defaults: no passwords/tokens/reset codes echoed in responses; reset codes exposed only in Development.
- Unknown product decisions → mark `TBD`, don't invent silently.

## Verification

Re-read your diff: summary table rows match detailed sections one-to-one; links resolve; `api-design.md` tables updated; every factual claim traceable to code or other docs.
