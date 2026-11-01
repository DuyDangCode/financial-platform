# Logging

## 1. Purpose

This document defines the logging architecture, log levels, structured logging conventions, sensitive data redaction rules, and the boundary between **Application Logs** (`ILogger<T>`) and **Business Audit Logs** (`AuditLog` entity) in the Financial Portfolio & Trading Operations Platform (FR-024, NFR-006).

---

## 2. Application Logs vs. Business Audit Logs

The platform maintains a strict separation between diagnostic application logs and persistent business audit logs:

| Dimension | Application Logs (`ILogger<T>`) | Business Audit Logs (`AuditLogs` Table) |
| --- | --- | --- |
| **Purpose** | Technical diagnostics, exception stack traces, performance timing, infrastructure debugging | Security traceability, regulatory/business compliance, user activity history (FR-024, US-013, US-016) |
| **Storage** | Standard output (`stdout`), console, or log aggregator (Seq / ELK / CloudWatch) | Authoritative PostgreSQL `AuditLogs` table (`src/FinancialPlatform.Domain/Entities/AuditLog.cs`) |
| **Transactionality** | Best-effort, asynchronous stream | Committed atomically in the same DB transaction as the business state change (FR-027) |
| **Access** | Engineers / DevOps only | Exposed to users (`GET /api/user/me/activity`) and administrators (`GET /api/dashboard/admin-summary`) |

---

## 3. Current Logging Configuration (`Implemented`)

### 3.1. `appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### 3.2. Centralized Exception Logging (`GlobalExceptionMiddleware.cs`)

`GlobalExceptionMiddleware` (`src/FinancialPlatform.Api/Middleware/GlobalExceptionMiddleware.cs`) wraps the entire HTTP pipeline, catches all unhandled exceptions, and logs them before translating them into `ErrorResponse`:

```csharp
_logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
```

---

## 4. Log Level Guidelines

When adding logging in handlers, infrastructure services, or middleware, use the following severity levels:

| Level | When to Use | Examples |
| --- | --- | --- |
| `Debug` / `Trace` | Verbose step-by-step developer diagnostics (disabled in Production) | Cache hit/miss details, EF Core SQL parameter inspection in local dev |
| `Information` | Normal, high-value lifecycle milestones | Startup seeding (`SeedData`), order filled (`OrderId`, `PortfolioId`), background price refresh completed |
| `Warning` | Handled domain/validation rejections, retryable conflicts, or degraded non-critical dependencies | `DomainException` thrown, `DbUpdateConcurrencyException` (`xmin` conflict), failed login attempt, Redis cache unavailable (falling back to DB) |
| `Error` | Unhandled exceptions or failed infrastructure operations requiring investigation | Database connection failure, unhandled `500` in `GlobalExceptionMiddleware` |
| `Critical` | System-wide outage or corrupted configuration preventing startup | Missing `JwtSettings` on startup, unrecoverable DB migration failure |

---

## 5. Structured Logging Conventions

Always use **message templates with named placeholders** rather than string interpolation (`$"..."`), so log aggregators can index properties:

```csharp
// Correct (Structured)
_logger.LogInformation(
    "Order {OrderId} filled for Portfolio {PortfolioId} with {ExecutedQuantity} {Symbol} at {ExecutionPrice}",
    order.Id, order.PortfolioId, execution.ExecutedQuantity, asset.Symbol, execution.ExecutionPrice);

// Incorrect (Unstructured string interpolation)
_logger.LogInformation($"Order {order.Id} filled for Portfolio {order.PortfolioId}");
```

### Correlation IDs (Planned Phase 7)

Add a lightweight `CorrelationIdMiddleware` that reads `X-Correlation-ID` from incoming request headers (or generates a new `Guid` if absent), echoes it on the HTTP response header, and pushes it into `ILogger.BeginScope` so every log line within a request shares `CorrelationId`.

---

## 6. Sensitive Data Redaction Rules (NFR-001)

**Never** log any of the following in `ILogger` messages or `AuditLog.Details` payloads:

1. Plaintext passwords (`password`, `currentPassword`, `newPassword`) or `PasswordHash`.
2. Raw JWT access tokens (`token`), `RefreshToken.Token` values, or `PasswordResetToken.Code` values (outside local `Development` seed notes).
3. Database connection strings containing credentials or `Jwt:Key` signing secrets.
4. Full request bodies on `/api/auth/*` endpoints.

---

## 7. Standard `AuditLog.Action` Catalog (FR-024)

When persisting business audit events via `AuditLog.Create(...)`, use consistent uppercase `SNAKE_CASE` action codes:

| Module | `Action` Code | `EntityType` | `EntityId` | Trigger |
| --- | --- | --- | --- | --- |
| **Identity** | `USER_REGISTERED` | `"User"` | `user.Id` | `RegisterCommandHandler` |
| **Identity** | `USER_LOGGED_IN` | `"User"` | `user.Id` | `LoginCommandHandler` |
| **Identity** | `USER_PASSWORD_CHANGED` | `"User"` | `user.Id` | `ChangePassword` / `ResetPassword` |
| **Identity** | `USER_STATUS_CHANGED` | `"User"` | `targetUser.Id` | Admin enables/disables user |
| **Identity** | `USER_ROLE_ASSIGNED` | `"UserRole"` | `"{userId}:{roleId}"` | Admin assigns role |
| **Portfolio** | `PORTFOLIO_CREATED` | `"Portfolio"` | `portfolio.Id` | Portfolio created |
| **Portfolio** | `PORTFOLIO_UPDATED` | `"Portfolio"` | `portfolio.Id` | Portfolio renamed or status toggled |
| **Trading** | `ORDER_CREATED` | `"Order"` | `order.Id` | Order placed (`PENDING`) |
| **Trading** | `ORDER_CANCELLED` | `"Order"` | `order.Id` | Order cancelled (`CANCELLED`) |
| **Trading** | `ORDER_REJECTED` | `"Order"` | `order.Id` | Order rejected (`REJECTED`) |
| **Trading** | `ORDER_FILLED` | `"Order"` | `order.Id` | Order executed (`FILLED` + `ExecutionId` + `TransactionId` in `Details` JSON) |
