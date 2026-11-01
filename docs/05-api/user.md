# User API

> **Status: Planned.** `UserController` (`src/FinancialPlatform.Api/Controllers/UserController.cs`) currently exists as a 0-byte placeholder. The `User` and `AuditLog` domain entities are implemented in `FinancialPlatform.Domain`.

## Overview

| Controller | Resource | Purpose |
| --- | --- | --- |
| User | `api/user` | Current user profile management, personal activity log, and administrative user management (FR-024, FR-025, US-013, US-014, US-015, BR-002, BR-037). |

All endpoints require authentication (`[Authorize]`) and wrap successful responses in the standard [`ApiResponse<T>`](./api-design.md#3-response-envelope) envelope. Error semantics follow [`error-handling.md`](./error-handling.md).

## Endpoints

| Method | Path | Auth Required | Description |
| --- | --- | --- | --- |
| GET | `/api/user/me` | Yes | Retrieve the authenticated user's profile and assigned roles. |
| POST | `/api/user/me/update` | Yes | Update the authenticated user's profile details. |
| GET | `/api/user/me/activity` | Yes | View audit activity records associated with the current user. |
| GET | `/api/user/list` | Yes (`Administrator`) | List all registered users for administration. |
| POST | `/api/user/{userId}/status` | Yes (`Administrator`) | Enable or disable a user account. |

---

## Get Current User Profile

```text
GET /api/user/me
```

Returns the profile information and assigned role names of the currently authenticated user. Never exposes `PasswordHash`.

Responses:

| Status | Meaning |
| --- | --- |
| 200 | Profile returned. |
| 401 | Missing, invalid, or expired access token. |

Returns: `UserProfileResponse`

| Field | Type | Description |
| --- | --- | --- |
| `id` | guid | User ID. |
| `userName` | string | Username. |
| `email` | string | Normalized lowercase email address. |
| `firstName` | string \| null | Given name. |
| `lastName` | string \| null | Family name. |
| `displayName` | string \| null | Public display name. |
| `phoneNumber` | string \| null | Contact phone number. |
| `isActive` | boolean | Whether the account is active (`true` / `false`). |
| `roles` | array of string | Assigned role names (e.g., `["Investor"]`). |
| `createdAt` | datetime (UTC) | Registration timestamp. |

---

## Update Current User Profile

```text
POST /api/user/me/update
```

Updates the optional profile fields of the authenticated user (`User.UpdateProfile(...)`).

Request body (`UpdateProfileRequest`):

| Field | Type | Required | Constraints |
| --- | --- | --- | --- |
| `firstName` | string | No | Max 128 chars. |
| `lastName` | string | No | Max 128 chars. |
| `displayName` | string | No | Max 256 chars. |
| `phoneNumber` | string | No | Valid phone format, max 32 chars. |

Example:

```json
{
  "firstName": "Thanh",
  "lastName": "Duy",
  "displayName": "Thanh Duy",
  "phoneNumber": "+84901234567"
}
```

Responses:

| Status | Meaning |
| --- | --- |
| 200 | Profile updated and returned. |
| 400 | Validation error. |
| 401 | Missing, invalid, or expired access token. |

Returns: `UserProfileResponse`.

---

## Get Current User Activity

```text
GET /api/user/me/activity?limit=50
```

Returns the most recent `AuditLog` entries where `ActorUserId` equals the authenticated user's ID (FR-024, US-013).

Query parameters:

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `limit` | integer | No | `50` | Max records to return (`1..100`). |

Responses:

| Status | Meaning |
| --- | --- |
| 200 | Activity records returned. |
| 401 | Missing, invalid, or expired access token. |

Returns: `IReadOnlyList<AuditLogDto>` (see [`dashboard.md`](./dashboard.md)).

---

## List Users (Admin)

```text
GET /api/user/list?isActive=true&search=duy
```

Allows an `Administrator` to view registered users and their account statuses without exposing password hashes or tokens (FR-025, US-014, BR-037).

Query parameters:

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `isActive` | boolean | No | Optional filter by active/disabled status. |
| `search` | string | No | Optional substring filter on `Email`, `UserName`, or `DisplayName`. |

Responses:

| Status | Meaning |
| --- | --- |
| 200 | User list returned. |
| 401 | Missing, invalid, or expired access token. |
| 403 | Authenticated user is not an `Administrator`. |

Returns: `IReadOnlyList<UserProfileResponse>`.

---

## Update User Status (Admin)

```text
POST /api/user/{userId}/status
```

Enables (`User.Activate()`) or disables (`User.Deactivate()`) a target user account (FR-025, US-015, BR-002, BR-037). When a user is disabled, all active refresh tokens for that user are revoked (`IRefreshTokenRepository.RevokeAllForUserAsync`) and an `USER_STATUS_CHANGED` audit event is recorded.

Request body (`UpdateUserStatusRequest`):

| Field | Type | Required | Constraints |
| --- | --- | --- | --- |
| `isActive` | boolean | Yes | `true` to enable, `false` to disable. |

Example:

```json
{
  "isActive": false
}
```

Responses:

| Status | Meaning |
| --- | --- |
| 200 | User status updated. |
| 400 | User not found or administrator attempted to disable their own account (`DomainException`). |
| 401 | Missing, invalid, or expired access token. |
| 403 | Authenticated user is not an `Administrator`. |

Returns: `UserProfileResponse`.
