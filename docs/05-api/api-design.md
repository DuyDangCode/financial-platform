# API Design

## 1. Purpose

This document describes the HTTP API design of the Financial Platform.

It defines:

- General API conventions.
- The standard response envelope.
- Authentication requirements.

Module-specific endpoint references live in their own files:

| Module      | Reference                          | Status     |
| ----------- | ---------------------------------- | ---------- |
| Auth        | [auth.md](./auth.md)               | Implemented|
| User        | [user.md](./user.md)               | Planned    |
| Role        | [role.md](./role.md)               | Planned    |
| Permission  | [permission.md](./permission.md)   | Planned    |
| Account     | [account.md](./account.md)         | Planned    |
| Asset       | [asset.md](./asset.md)             | Planned    |
| Order       | [order.md](./order.md)             | Planned    |
| Transaction | [transaction.md](./transaction.md) | Planned    |
| Dashboard   | [dashboard.md](./dashboard.md)     | Planned    |

The API is implemented with ASP.NET Core Web API (.NET 8) and follows a
controller-based REST style.

---

# 2. General Conventions

## 2.1. Base URL

All endpoints are prefixed with `api/` and named after their controller.

```text
https://localhost:{port}/api/{controller}/{action}
```

Example:

```text
POST https://localhost:5150/api/auth/login
```

Controller routing uses the `[Route("api/[controller]")]` convention, which
produces lowercase-friendly resource names such as `auth`, `user`, `role`.

## 2.2. Content Type

- Requests and responses use `application/json`.
- Request bodies are validated with `System.ComponentModel.DataAnnotations`
  attributes before reaching application handlers.

## 2.3. Casing

JSON payloads use **camelCase** (ASP.NET Core default):

```json
{
  "success": true,
  "message": "Login successful.",
  "data": { "userId": "...", "expiresAt": "..." }
}
```

## 2.4. Identifiers

- Entities are identified by `Guid` values serialized as strings.

---

# 3. Response Envelope

Every successful response is wrapped in a standard envelope
(`ApiResponse<T>`):

| Field     | Type            | Description                          |
| --------- | --------------- | ------------------------------------ |
| `success` | boolean         | Always `true` on success.            |
| `message` | string \| null  | Human-readable result message.       |
| `data`    | T \| null       | The endpoint-specific payload.       |
| `error`   | object \| null  | Populated only on failures.          |

Success example:

```json
{
  "success": true,
  "message": "Login successful.",
  "data": {
    "token": "eyJhbGciOi...",
    "expiresAt": "2026-08-26T13:00:00Z",
    "userId": "3f6d...",
    "userName": "thanhduy",
    "email": "user@example.com",
    "displayName": "Thanh Duy",
    "refreshToken": "base64-encoded-opaque-token"
  },
  "error": null
}
```

Error responses are described in
[error-handling.md](./error-handling.md).

---

# 4. Authentication

The API uses **JWT Bearer authentication**.

- Access tokens are issued by the auth endpoints and sent as:

```text
Authorization: Bearer {access_token}
```

- Tokens are validated for issuer, audience, lifetime, and signature.
- Clock skew is zero (`ClockSkew = TimeSpan.Zero`); expired tokens are
  rejected immediately.
- Token lifetimes:

| Token             | Default Lifetime | Configured By              |
| ----------------- | ---------------- | -------------------------- |
| Access token      | 60 minutes       | `Jwt:ExpiryMinutes`        |
| Refresh token     | 7 days           | Hard-coded (current phase) |
| Password reset    | 15 minutes       | Hard-coded (current phase) |

Endpoints requiring authentication are marked with `[Authorize]`.

---

# 5. Endpoint Overview

## 5.1. Implemented

### Auth (`api/auth`)

See [auth.md](./auth.md) for the full endpoint reference.

| Method | Path                       | Auth Required | Description                        |
| ------ | -------------------------- | ------------- | ---------------------------------- |
| POST   | `/api/auth/register`       | No            | Create an account and sign in.     |
| POST   | `/api/auth/login`          | No            | Exchange credentials for tokens.   |
| POST   | `/api/auth/refresh`        | No            | Rotate refresh token, reissue JWT. |
| POST   | `/api/auth/logout`         | No            | Revoke a refresh token.            |
| POST   | `/api/auth/change-password`| Yes           | Change the current user password.  |
| POST   | `/api/auth/forgot-password`| No            | Issue a password reset code.       |
| POST   | `/api/auth/reset-password` | No            | Reset password using a code.       |

## 5.2. Planned

Controllers exist as placeholders (or are planned for upcoming phases) and will be implemented incrementally. Each module has a full specification document:

| Controller   | Resource          | Purpose                                                      | Reference                          |
| ------------ | ----------------- | ------------------------------------------------------------ | ---------------------------------- |
| User         | `api/user`        | Profile management, activity history, user administration.   | [user.md](./user.md)               |
| Role         | `api/role`        | Role management and user-role assignments.                   | [role.md](./role.md)               |
| Permission   | `api/permission`  | Effective user permissions and role-permission matrix.       | [permission.md](./permission.md)   |
| Account      | `api/account`     | Portfolio CRUD, positions, and portfolio valuation/P&L.      | [account.md](./account.md)         |
| Asset        | `api/asset`       | Asset catalog, search, market price refresh, status toggle.  | [asset.md](./asset.md)             |
| Order        | `api/order`       | Order placement, simulated execution, cancellation, history. | [order.md](./order.md)             |
| Transaction  | `api/transaction` | Immutable transaction history and inspection.                | [transaction.md](./transaction.md) |
| Dashboard    | `api/dashboard`   | Aggregated investor portfolio overview & admin audit summary.| [dashboard.md](./dashboard.md)     |

---

# 6. Design Principles

- Thin controllers: controllers translate HTTP requests into application
  commands and wrap results in the standard envelope. Business rules live in
  the Application and Domain layers, not in controllers.
- One envelope everywhere: all success responses share `ApiResponse<T>`;
  all errors share `ErrorResponse`.
- Explicit status codes: endpoints document their possible status codes;
  unexpected exceptions are centralized in the global exception middleware.
- Secure defaults: passwords are never returned, reset codes are not exposed
  outside Development, and enumeration-resistant responses are used for
  password recovery flows.
