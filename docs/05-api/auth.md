# Auth API

Reference for the authentication endpoints under `api/auth`.

Authentication mechanics (JWT Bearer, token lifetimes) are described in
[api-design.md](./api-design.md#4-authentication).

## Endpoints

| Method | Path                       | Auth Required | Description                        |
| ------ | -------------------------- | ------------- | ---------------------------------- |
| POST   | `/api/auth/register`       | No            | Create an account and sign in.     |
| POST   | `/api/auth/login`          | No            | Exchange credentials for tokens.   |
| POST   | `/api/auth/refresh`        | No            | Rotate refresh token, reissue JWT. |
| POST   | `/api/auth/logout`         | No            | Revoke a refresh token.            |
| POST   | `/api/auth/change-password`| Yes           | Change the current user password.  |
| POST   | `/api/auth/forgot-password`| No            | Issue a password reset code.       |
| POST   | `/api/auth/reset-password` | No            | Reset password using a code.       |

---

## Register

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

## Login

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

## Refresh

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

## Logout

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

## Change Password

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

## Forgot Password

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

## Reset Password

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
