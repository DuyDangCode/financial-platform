# Permission API

> **Status: Planned.** `PermissionController` (`src/FinancialPlatform.Api/Controllers/PermissionController.cs`) currently exists as a 0-byte placeholder. Per [`data-model.md`](../04-database/data-model.md) §4.4 and §10, the MVP uses two-role RBAC (`Investor`, `Administrator`) and exposes static capability mappings until database-backed `Permission` / `RolePermission` tables are introduced post-MVP.

## Overview

| Controller | Resource | Purpose |
| --- | --- | --- |
| Permission | `api/permission` | Inspect platform permissions and effective capabilities for the current user or a specified role (FR-003). |

All endpoints require authentication (`[Authorize]`) and wrap successful responses in the standard [`ApiResponse<T>`](./api-design.md#3-response-envelope) envelope.

## Endpoints

| Method | Path | Auth Required | Description |
| --- | --- | --- | --- |
| GET | `/api/permission/me` | Yes | Return the effective permissions granted to the current authenticated user based on their roles. |
| GET | `/api/permission/matrix` | Yes (`Administrator`) | Return the complete role-to-permission matrix. |

---

## Get Current User Permissions

```text
GET /api/permission/me
```

Returns the list of logical permission codes granted to the caller's active roles so the frontend can conditionally render investor vs. administrative UI elements.

Responses:

| Status | Meaning |
| --- | --- |
| 200 | Effective permissions returned. |
| 401 | Missing, invalid, or expired access token. |

Returns: `UserPermissionsResponse`

| Field | Type | Description |
| --- | --- | --- |
| `userId` | guid | Current user ID. |
| `roles` | array of string | Assigned roles (e.g., `["Investor"]`). |
| `permissions` | array of string | Granted permission codes (e.g., `["portfolios:read", "portfolios:write", "orders:write", "transactions:read"]`). |

---

## Get Role-Permission Matrix (Admin)

```text
GET /api/permission/matrix
```

Returns all logical permissions grouped by role (`Investor` vs `Administrator`).

Standard permission catalog:

| Permission Code | Description | Granted To |
| --- | --- | --- |
| `portfolios:read` | View own portfolios, positions, and performance | `Investor`, `Administrator` |
| `portfolios:write` | Create, update, and toggle own portfolios | `Investor`, `Administrator` |
| `assets:read` | Browse and search tradable assets and prices | `Investor`, `Administrator` |
| `assets:write` | Create assets, refresh prices, toggle asset status | `Administrator` |
| `orders:read` | View own order history | `Investor`, `Administrator` |
| `orders:write` | Place, execute, and cancel own orders | `Investor`, `Administrator` |
| `transactions:read` | View own transaction history | `Investor`, `Administrator` |
| `users:manage` | View all users and enable/disable accounts | `Administrator` |
| `roles:manage` | Create roles and assign/revoke user roles | `Administrator` |
| `audit:read` | View platform-wide audit logs and admin dashboard | `Administrator` |

Responses:

| Status | Meaning |
| --- | --- |
| 200 | Role-permission matrix returned. |
| 401 | Missing, invalid, or expired access token. |
| 403 | Authenticated user is not an `Administrator`. |

Returns: `IReadOnlyList<RolePermissionGroupDto>` (`roleName`, `permissions[]`).
