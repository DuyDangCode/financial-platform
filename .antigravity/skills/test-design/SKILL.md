---
name: test-design
description: Design and write tests for the .NET backend using xUnit — test case scenarios, coverage plans, and fake/stub strategy. Use when asked to write tests, improve coverage, or plan test scenarios for handlers, entities, or controllers.
---

# Test Design (.NET / xUnit)

## Hard constraint

`tests/FinancialPlatform.UnitTests` references **Api/Application/Domain only — NOT Infrastructure**. Unit tests cannot touch `AppDbContext` or real repositories; dependencies are replaced with hand-written fakes in `tests/FinancialPlatform.UnitTests/TestSupport/AuthFakes.cs` (`Fake*`/`Stub*` prefix, dictionary-backed, e.g. `FakeUserRepository` with a `Seed()` helper). Extend that file with fakes for new interfaces. `tests/FinancialPlatform.IntegrationTests` is an empty stub — do not promise integration coverage without setting up Testcontainers first.

## Naming & style

- Method names: `MethodUnderTest_WithCondition_ExpectedResult` (see `Domain/Entities/UserTests.cs`, `Api/Controllers/AuthControllerTests.cs`).
- One arrange–act–assert block per test. `[Fact]` for single cases, `[Theory]` + `[InlineData]` for input sets. Plain `Assert.*`; no mocking library — don't introduce one without asking.

## Scenario design

For each handler/entity/controller cover:
1. Happy path (assert returned DTO/state).
2. Each validation rule → `DomainException` (or specific subclass like `UserAlreadyExistsException`).
3. Boundaries: empty/whitespace strings, zero/negative amounts, expired tokens, duplicate keys (existing email).
4. Auth/security paths: wrong password → `InvalidCredentialsException`; invalid/expired refresh or reset tokens.
5. Controller tests assert the `ApiResponse<T>` envelope (`Success`, `Message`, `Data`, `Error.StatusCode`).

## Minimum bar per feature

Every new command handler ships with: happy-path test + one test per thrown exception type + a fake for each newly injected abstraction.

## Run

- All: `dotnet test`
- Unit only: `dotnet test tests/FinancialPlatform.UnitTests`
- Single: `dotnet test <project> --filter FullyQualifiedName~<TestName>`
