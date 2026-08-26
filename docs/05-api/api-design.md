# API Design

## 1. Purpose

This document describes the HTTP API design of the Financial Platform.

It defines:

- General API conventions.
- The standard response envelope.
- Authentication requirements.
- The available endpoints.
- Planned endpoints for modules under development.

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

Controllers exist as placeholders and will be implemented incrementally:

| Controller   | Resource      | Purpose                                  |
| ------------ | ------------- | ---------------------------------------- |
| User         | `api/user`    | Profile management, user administration.|
| Role         | `api/role`    | Role management.                         |
| Permission   | `api/permission` | Permission management.               |
| Account      | `api/account` | Portfolio accounts.                      |
| Transaction  | `api/transaction` | Transactions history and operations. |
| Dashboard    | `api/dashboard` | Aggregated portfolio overview data.    |

---

# 6. Auth API Reference

## 6.1. Register

```text
POST /api/auth/register
```

Creates a user account. The response contains tokens, so the client is
signed in immediately after registration.

Request body (`RegisterRequest`):

| Field          | Type   | Required | Constraints                    |
| -------------- | ------ | -------- | ------------------------------ |
| `userName`     | string | Yes      | Max 256 chars.                 |
| `email`        | string | Yes      | Valid email, max 256 chars.    |
| `password`     | string | Yes      | Min 8, max 128 chars.          |
| `firstName`    | string | No       | Max 128 chars.                 |
| `lastName`     | string | No       | Max 128 chars.                 |
| `displayName`  | string | No       | Max 256 chars.                 |
| `phoneNumber`  | string | No       | Valid phone, max 32 chars.     |

Example:

```json
{
  "userName": "thanhduy",
  "email": "user@example.com",
  "password": "Str0ng!Passw0rd",
  "firstName": "Thanh",
  "lastName": "Duy",
  "displayName": "Thanh Duy",
  "phoneNumber": "+84901234567"
}
```

Responses:

| Status | Meaning                                   |
| ------ | ----------------------------------------- |
| 200    | Registration succeeded, tokens returned.  |
| 400    | Model validation failed or domain rule violated. |
| 409    | A user with this email already exists.    |

Returns: `LoginResponse`.

## 6.2. Login

```text
POST /api/auth/login
```

Request body (`LoginRequest`):

| Field      | Type   | Required |
| ---------- | ------ | -------- |
| `email`    | string | Yes      |
| `password` | string | Yes      |

Responses:

| Status | Meaning                      |
| ------ | ---------------------------- |
| 200    | Login succeeded.             |
| 401    | Invalid email or password.   |

Returns: `LoginResponse`.

Note: `LoginRequest` currently carries no validation attributes; missing or
empty credentials surface as `401 Invalid Credentials` from the handler
rather than a validation `400`.

`LoginResponse` fields:

| Field         | Type          | Description                              |
| ------------- | ------------- | ---------------------------------------- |
| `token`       | string        | JWT access token.                        |
| `expiresAt`   | datetime (UTC)| Access token expiration timestamp.       |
| `userId`      | guid          | The authenticated user id.               |
| `userName`    | string        | Username.                                |
| `email`       | string        | Email address.                           |
| `displayName` | string        | Display name.                            |
| `refreshToken`| string \| null| Opaque refresh token for token renewal.  |

## 6.3. Refresh

```text
POST /api/auth/refresh
```

Exchanges a valid refresh token for a new access token and a rotated
refresh token.

Request body (`RefreshRequest`):

| Field          | Type   | Required |
| -------------- | ------ | -------- |
| `refreshToken` | string | Yes      |

Responses:

| Status | Meaning                              |
| ------ | ------------------------------------ |
| 200    | New tokens issued.                   |
| 400    | Refresh token is invalid or expired. |

Returns: `LoginResponse`.

## 6.4. Logout

```text
POST /api/auth/logout
```

Revokes the supplied refresh token so it cannot be used again.

Request body (`LogoutRequest`):

| Field          | Type   | Required |
| -------------- | ------ | -------- |
| `refreshToken` | string | Yes      |

Responses:

| Status | Meaning                  |
| ------ | ------------------------ |
| 200    | Logout handled.          |
| 400    | Model validation failed. |

Returns: `MessageResponse` (`{ "message": "..." }`).

## 6.5. Change Password

```text
POST /api/auth/change-password
```

Requires authentication (`[Authorize]`). Changes the password of the user
identified by the access token.

Request body (`ChangePasswordRequest`):

| Field             | Type   | Required | Constraints           |
| ----------------- | ------ | -------- | --------------------- |
| `currentPassword` | string | Yes      | Current password.     |
| `newPassword`     | string | Yes      | Min 8, max 128 chars. |

Responses:

| Status | Meaning                                    |
| ------ | ------------------------------------------ |
| 200    | Password changed.                          |
| 400    | Current password wrong or validation failed.|
| 401    | Missing, invalid, or expired access token. |

Returns: `MessageResponse`.

## 6.6. Forgot Password

```text
POST /api/auth/forgot-password
```

Issues a single-use password reset code valid for 15 minutes.

Request body (`ForgotPasswordRequest`):

| Field   | Type   | Required | Constraints   |
| ------- | ------ | -------- | ------------- |
| `email` | string | Yes      | Valid email.  |

Response data (`ForgotPasswordResponse`):

| Field       | Type           | Description                                    |
| ----------- | -------------- | ---------------------------------------------- |
| `delivered` | boolean        | Whether a reset code was issued (`false` when the email is not registered). |
| `resetCode` | string \| null | Exposed **only** in Development until an email service exists; always `null` otherwise. |

> **Known limitation:** `delivered: false` reveals that an email is not
> registered, which contradicts the account-enumeration-resistance goal of
> the constant response message. This should be addressed before production
> (e.g. always return `true`, or drop the field).

Responses:

| Status | Meaning                  |
| ------ | ------------------------ |
| 200    | Request processed.       |
| 400    | Model validation failed. |

## 6.7. Reset Password

```text
POST /api/auth/reset-password
```

Consumes a reset code and sets a new password.

Request body (`ResetPasswordRequest`):

| Field         | Type   | Required | Constraints           |
| ------------- | ------ | -------- | --------------------- |
| `email`       | string | Yes      | Valid email.          |
| `code`        | string | Yes      | Reset code.           |
| `newPassword` | string | Yes      | Min 8, max 128 chars. |

Responses:

| Status | Meaning                                     |
| ------ | ------------------------------------------- |
| 200    | Password reset.                             |
| 400    | Code invalid/expired or validation failed.  |

Returns: `MessageResponse`.

---

# 7. Design Principles

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
