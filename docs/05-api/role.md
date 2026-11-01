# Role API

> **Status: Planned.** `RoleController` (`src/FinancialPlatform.Api/Controllers/RoleController.cs`) currently exists as a 0-byte placeholder. The `Role` and `UserRole` domain entities are implemented in `FinancialPlatform.Domain`.

## Overview

| Controller | Resource | Purpose |
| --- | --- | --- |
| Role | `api/role` | Role catalog inspection and administrative user-role assignments (FR-003, BR-037). |

All endpoints require authentication (`[Authorize(Roles = "Administrator")]`) and wrap successful responses in the standard [`ApiResponse<T>`](./api-design.md#3-response-envelope) envelope. Error semantics follow [`error-handling.md`](./error-handling.md).

## Endpoints

| Method | Path | Auth Required | Description |
| --- | --- | --- | --- |
| GET | `/api/role/list` | Yes (`Administrator`) | List all defined roles (`Investor`, `Administrator`). |
| POST | `/api/role/create` | Yes (`Administrator`) | Create a new named role. |
| POST | `/api/role/assign` | Yes (`Administrator`) | Assign a role to a user (`UserRole.Create`). |
| POST | `/api/role/revoke` | Yes (`Administrator`) | Remove a role assignment from a user. |

---

## List Roles

```text
GET /api/role/list
```

Returns all roles defined in the system.

Responses:

| Status | Meaning |
| --- | --- |
| 200 | Role list returned. |
| 401 | Missing, invalid, or expired access token. |
| 403 | Authenticated user is not an `Administrator`. |

Returns: `IReadOnlyList<RoleResponse>`

| Field | Type | Description |
| --- | --- | --- |
| `id` | guid | Role ID. |
| `name` | string | Unique role name (e.g., `"Investor"`, `"Administrator"`). |
| `createdAt` | datetime (UTC) | Creation timestamp. |

---

## Create Role

```text
POST /api/role/create
```

Creates a new role (`Role.Create(name)`).

Request body (`CreateRoleRequest`):

| Field | Type | Required | Constraints |
| --- | --- | --- | --- |
| `name` | string | Yes | Non-empty, max 64 chars, unique. |

Example:

```json
{
  "name": "Auditor"
}
```

Responses:

| Status | Meaning |
| --- | --- |
| 200 | Role created. |
| 400 | Validation error or domain rule violation (`DomainException`). |
| 401 | Missing, invalid, or expired access token. |
| 403 | Authenticated user is not an `Administrator`. |
| 409 | Role name already exists. |

Returns: `RoleResponse`.

---

## Assign Role to User

```text
POST /api/role/assign
```

Assigns a role to a user (`UserRole.Create(userId, roleId)`) and records a `USER_ROLE_ASSIGNED` audit event.

Request body (`AssignRoleRequest`):

| Field | Type | Required | Constraints |
| --- | --- | --- | --- |
| `userId` | guid | Yes | Target user ID. |
| `roleName` | string | Yes | Existing role name (e.g., `"Administrator"`). |

Example:

```json
{
  "userId": "3f6d0a12-71c4-4c60-9a19-1c29b38b5d10",
  "roleName": "Administrator"
}
```

Responses:

| Status | Meaning |
| --- | --- |
| 200 | Role assigned (idempotent if already assigned). |
| 400 | User or role not found (`DomainException`). |
| 401 | Missing, invalid, or expired access token. |
| 403 | Authenticated user is not an `Administrator`. |

Returns: `UserRoleResponse` (`userId`, `roleId`, `roleName`, `assignedAt`).

---

## Revoke Role from User

```text
POST /api/role/revoke
```

Removes a role assignment from a user.

Request body (`RevokeRoleRequest`):

| Field | Type | Required | Constraints |
| --- | --- | --- | --- |
| `userId` | guid | Yes | Target user ID. |
| `roleName` | string | Yes | Role name to revoke. |

Responses:

| Status | Meaning |
| --- | --- |
| 200 | Role revoked. |
| 400 | User/role not found or attempt to remove the user's last role (`DomainException`). |
| 401 | Missing, invalid, or expired access token. |
| 403 | Authenticated user is not an `Administrator`. |

Returns: `MessageResponse` (`{ "message": "Role revoked." }`).
