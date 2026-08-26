---
name: backend-review
description: Review .NET 8 backend changes for business logic correctness, security vulnerabilities, performance issues, code style (naming, clean code), and DDD principle compliance. Use when asked to review backend code, diffs, or PRs in src/ or tests/.
---

# Backend Review (.NET)

Review target: `src/**` and `tests/**`. Read-only unless asked to fix findings.

Read the full diff plus enough surrounding context (handler + entity + configuration) to judge each finding; never review from names alone. Check `docs/02-domain/business-rules.md` when judging logic.

Review along five dimensions — a violation in any is reportable:

## 1. Logic

- Business rules match the domain docs and entity invariants; validation happens once, at the right layer (in the entity/factory, not re-implemented in handlers or controllers).
- Edge cases: null/empty/whitespace, zero/negative amounts, expired tokens, duplicate keys, timezone handling (store UTC), money as `decimal`.
- Race conditions on multi-step flows (e.g. refresh-token rotation/reuse, concurrent email registration), correct idempotency where required.
- Error paths actually propagate: no swallowed exceptions, no empty catch, results of async ops awaited.
- DI trap: every new handler registered in `src/FinancialPlatform.Application/DependencyInjection.cs` (missing = runtime failure).

## 2. Security

- Authorization: `[Authorize]` present wherever user-specific data is touched; no IDOR (ownership checked before returning/mutating another user's resource).
- Secrets: none hardcoded beyond existing dev-only values in appsettings.json/docker-compose; no secrets/tokens/passwords logged or echoed; reset codes exposed only in Development.
- JWT: issuer/audience/lifetime validated (`ClockSkew.Zero`); refresh tokens rotated and revoked on logout/reset.
- Input: DataAnnotations on request models + domain-level validation; EF parameterization kept intact (no raw SQL string concat).
- Enumeration resistance preserved in forgot-password-style flows; 500 responses never leak exception messages/stack traces (check `GlobalExceptionMiddleware` switch covers any new exception type — unmapped ⇒ accidental 500 with hidden message).

## 3. Performance

- Fully async chain: no `.Result`/`.Wait()`/sync-over-async; `CancellationToken` accepted and passed down.
- No N+1: reads use projection/`Include` deliberately; list endpoints paginate (never unbounded `ToList()`).
- `AsNoTracking()` for read-only queries; beware the repo pattern calling `SaveChangesAsync` per method — flag handlers causing redundant saves.
- Indexes match query filters (unique index on Email exists; new query patterns need supporting indexes/configurations).
- No obvious per-request allocations of heavy resources; singletons vs scoped lifetimes used correctly (`IPasswordHasher` singleton OK; DbContext-scoped things must stay scoped).

## 4. Code style (naming, clean code)

- Naming matches conventions: files/types PascalCase matching type name; namespaces mirror folders; `<X>Command` sealed record + `<X>CommandHandler`; `I<X>Repository` in Domain / `<X>Repository` in Infrastructure; fakes `Fake<X>`/`Stub<X>`; slices at `Application/Features/<Area>/Commands/<Name>/`.
- Clean code: small focused methods, guard clauses over nesting, no duplication (extract shared logic), no dead code/commented-out blocks, no pointless comments restating code.
- Layer hygiene: Application has no HTTP concerns; Api has no business rules; entities never reference Infrastructure types.

## 5. DDD compliance

- Rich domain model: private setters + private ctor + `static Create(...)` factory; invariants enforced inside the entity via `DomainException` — anemic models (public setters, validation outside) are violations.
- Behavior methods on aggregates (`Activate()`, `UpdateProfile(...)`) instead of handlers mutating fields directly through setters.
- Ubiquitous language: class/method names use domain terms consistent with `docs/02-domain/*`.
- Dependency rule: Domain references nothing; Application references only Domain; Infrastructure/Application.Api depend inward. Repository interfaces live in Domain (`Domain/Interfaces/`), implementations in Infrastructure — never the reverse.
- Persistence ignorance: no EF attributes/types leaking into entities; mapping belongs in `IEntityTypeConfiguration<T>`.
- One aggregate = one repository; cross-aggregate coordination stays in application handlers.

## Output format

Group findings by dimension (Logic / Security / Performance / Code style / DDD), each tagged Blocker / Should fix / Nit, with `file:line` and a concrete suggestion. End with a one-line verdict: approve or request changes.
