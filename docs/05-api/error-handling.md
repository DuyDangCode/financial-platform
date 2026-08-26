# Error Handling

## 1. Purpose

This document describes how the Financial Platform API handles and reports
errors.

It defines:

- The central error handling pipeline.
- The standard error response format.
- The mapping between domain exceptions and HTTP status codes.
- Validation error reporting.

The goal is a single, predictable error contract for every client.

---

# 2. Error Handling Pipeline

All unhandled exceptions are caught by the `GlobalExceptionMiddleware`,
registered as the first middleware in the pipeline:

```text
Request
  ↓
GlobalExceptionMiddleware
  ↓
Authentication / Authorization
  ↓
Controller → Application → Domain
  ↓ (on exception)
GlobalExceptionMiddleware catches
  ↓
Log the exception
  ↓
Map exception type to HTTP status code
  ↓
Write ErrorResponse JSON
```

Behavior:

- Every unhandled exception is logged with `ILogger` at Error level.
- The client never receives stack traces or internal details.
- Exceptions of known domain types are translated into meaningful status
  codes and messages.
- Any other exception becomes a generic `500` response.

---

# 3. Error Response Format

All errors use the shared `ErrorResponse` shape:

| Field             | Type    | Description                                    |
| ----------------- | ------- | ---------------------------------------------- |
| `statusCode`      | integer | The HTTP status code.                          |
| `message`         | string  | Safe, human-readable error description.        |
| `validationErrors`| array \| null | Optional field-level validation errors.  |

Example:

```json
{
  "statusCode": 409,
  "message": "A user with this email already exists.",
  "validationErrors": null
}
```

Each entry in `validationErrors` is a `ValidationError`:

```json
{
  "field": "password",
  "message": "The Password field must be at least 8 characters long."
}
```

---

# 4. Exception to Status Code Mapping

`GlobalExceptionMiddleware` maps exception types to status codes in the
following order:

| Exception                     | Status Code | Body Message                       |
| ----------------------------- | ----------- | ---------------------------------- |
| `UserAlreadyExistsException`  | 409 Conflict | Exception message.                |
| `InvalidCredentialsException` | 401 Unauthorized | Exception message.            |
| `DomainException` (any other) | 400 Bad Request | Exception message.            |
| Any other exception           | 500 Internal Server Error | Generic message only. |

Notes:

- Mapping is evaluated from most specific to most general; specialized
  exceptions inherit from `DomainException`.
- For `500` responses the real exception message is hidden and replaced with:
  `"An unexpected error occurred. Please try again later."`
- All exception details are still written to the server log.

---

# 5. Domain Exceptions Reference

Current domain exceptions live in `FinancialPlatform.Domain.Exceptions`:

| Exception                       | Base              | Message                                  | Typical Cause                  |
| ------------------------------- | ----------------- | ---------------------------------------- | ------------------------------ |
| `InvalidCredentialsException`   | `DomainException` | "Invalid email or password."             | Login with wrong credentials.  |
| `UserAlreadyExistsException`    | `DomainException` | "A user with this email already exists." | Registration duplicate email.  |
| `InvalidOrExpiredTokenException`| `DomainException` | "The token is invalid or has expired."   | Refresh/reset token problems.  |
| `DomainException`               | `Exception`       | Varies.                                  | Any other business rule breach.|

New domain exceptions should inherit from `DomainException`; if they need a
specific HTTP status code, extend the mapping in
`GlobalExceptionMiddleware.HandleExceptionAsync`.

---

# 6. Model Validation Errors

Request DTOs are annotated with `System.ComponentModel.DataAnnotations`
attributes (`[Required]`, `[EmailAddress]`, `[MinLength]`, ...).

Because controllers use `[ApiController]`, ASP.NET Core automatically rejects
invalid models with HTTP `400 Bad Request` before an action executes.

- Current behavior: the automatic response uses ASP.NET Core's default
  validation problem format.
- Target contract: validation failures are represented by the optional
  `validationErrors` array on `ErrorResponse`, so clients can highlight
  individual fields.

Validation constraints currently enforced:

| Request                 | Key Constraints                                            |
| ----------------------- | ---------------------------------------------------------- |
| `RegisterRequest`       | Email format; password min 8/max 128; name length limits.  |
| `ChangePasswordRequest` | Current password required; new password min 8/max 128.     |
| `ResetPasswordRequest`  | Email format; code required; new password min 8/max 128.   |
| `ForgotPasswordRequest` | Email format.                                              |
| `RefreshRequest` / `LogoutRequest` | Refresh token required.                         |

`LoginRequest` intentionally has no annotations today; bad credentials are
reported as `401` by the domain instead of `400` by validation.

---

# 7. Authentication Errors

Authentication failures are handled by the JWT bearer middleware rather than
the exception middleware:

| Situation                              | Status Code | Result                          |
| -------------------------------------- | ----------- | ------------------------------- |
| No token on an `[Authorize]` endpoint. | 401         | Default bearer challenge.       |
| Invalid signature/issuer/audience.     | 401         | Default bearer challenge.       |
| Expired token (zero clock skew).       | 401         | Default bearer challenge.       |
| Authenticated but not permitted.       | 403         | Default forbidden response.     |

Clients should treat any `401` as "re-authenticate", typically by calling
`POST /api/auth/refresh`.

---

# 8. HTTP Status Usage Summary

| Status | Usage                                                             |
| ------ | ----------------------------------------------------------------- |
| 200    | Successful operation (including handled business outcomes).       |
| 400    | Validation failure or generic domain rule violation.              |
| 401    | Missing/invalid credentials, invalid JWT, failed login.           |
| 403    | Authenticated but not allowed to perform the action.              |
| 404    | Resource not found (as endpoints are added).                      |
| 409    | Conflicting state, e.g. registering an existing email.            |
| 500    | Unexpected server error; details are logged, never returned.      |

---

# 9. Guidelines

- Throw `DomainException` subclasses for business rule violations; do not
  return raw exceptions from handlers.
- Keep exception messages safe to expose; they are sent to clients verbatim.
- Never catch-and-swallow exceptions in controllers or handlers; let the
  global middleware produce the error response.
- Add new mappings to the middleware when introducing new exception types,
  instead of returning ad-hoc error JSON from actions.
