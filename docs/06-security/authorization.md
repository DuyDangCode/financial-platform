# Authorization

## 1. Purpose

This document defines the authorization model, role-based access control (RBAC), and resource ownership rules of the Financial Portfolio & Trading Operations Platform (FR-003, FR-025, BR-002, BR-036, BR-037).

---

## 2. Authorization Layers

Access control is enforced across three complementary layers:

```text
HTTP Request
  │
  ▼
1. Endpoint Authentication & Role Gate (API Layer)
   [Authorize] / [Authorize(Roles = "Administrator")]
  │
  ▼
2. Account Status Gate (Application Layer)
   Verify User.IsActive == true (BR-002)
  │
  ▼
3. Resource Ownership & Domain Invariants (Application & Domain Layers)
   Verify Portfolio.UserId == currentUserId (BR-036)
```

---

## 3. Role-Based Access Control (RBAC)

Per [`user-stories.md`](../01-product/user-stories.md) §2 and [`data-model.md`](../04-database/data-model.md) §4.4–4.5, the platform defines two primary roles stored in `Roles` and assigned via `UserRoles`:

| Role | Default Assignment | Capabilities |
| --- | --- | --- |
| `Investor` | Assigned automatically to every newly registered user (`RegisterCommandHandler`). | Create and manage own portfolios; view assets and market prices; place, execute, and cancel own orders; view own positions, transactions, performance, and personal activity logs. |
| `Administrator` | Seeded / assigned explicitly by an administrator. | Inspect user list, enable/disable user accounts (FR-025, US-015), manage roles, create/update tradable assets and refresh simulated market prices, and review platform-wide audit logs (FR-024, US-016). |

### JWT Role Claims Integration (Planned Phase 1 Completion)

When generating a JWT in `JwtTokenGenerator.GenerateToken`, each role name assigned to the user via `UserRoles` is emitted as a `ClaimTypes.Role` (`"role"`) claim:

```csharp
new Claim(ClaimTypes.Role, roleName)
```

Controllers and actions restricted to administrators apply:

```csharp
[Authorize(Roles = "Administrator")]
```

---

## 4. Resource Ownership Rules (BR-036)

Role checks alone are insufficient for investor operations because every investor holds the `Investor` role. Every command or query operating on a user-owned resource **must** verify ownership in the Application handler using the caller's `userId` extracted from `ClaimTypes.NameIdentifier` / `JwtRegisteredClaimNames.Sub`:

| Resource | Ownership Verification Rule |
| --- | --- |
| **Portfolio** | `portfolio.UserId == currentUserId` (or caller has `Administrator` role for read-only support). |
| **Position** | Resolved via parent `Portfolio`: `portfolio.UserId == currentUserId`. |
| **Order** | Resolved via parent `Portfolio`: `portfolio.UserId == currentUserId`. |
| **Transaction** | Resolved via `transaction.PortfolioId` → `portfolio.UserId == currentUserId`. |
| **User Profile / Password** | `currentUserId` is read directly from the validated JWT `sub` claim (never trusted from request body, matching `AuthController.ChangePassword`). |

### Security Response on Ownership Violation

When an authenticated investor requests or attempts to mutate a portfolio, order, or transaction belonging to another user:

- Do **not** leak whether the foreign GUID exists.
- Throw `DomainException("Portfolio not found.")` (or return `403`/`404` consistently) so attackers cannot enumerate other users' resource identifiers.

---

## 5. Disabled Account Enforcement (BR-002)

When an administrator disables a user (`User.Deactivate()` → `IsActive = false`):

1. **Login blocked:** `LoginCommandHandler` checks `if (!user.IsActive) throw new InvalidCredentialsException();`.
2. **Refresh blocked:** Active refresh tokens for the disabled user are revoked (`RevokeAllForUserAsync`), and `RefreshCommandHandler` rejects disabled users.
3. **Password recovery blocked:** `ForgotPasswordCommandHandler` and `ResetPasswordCommandHandler` check `user.IsActive`.
4. **Protected mutations blocked:** Application handlers that load the user or portfolio verify active status before mutating financial state.
