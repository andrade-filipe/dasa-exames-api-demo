---
name: dotnet-xunit-testing
description: |
  Writes and reviews xUnit tests for C#/.NET code, leaning on stack-agnostic test-design judgment
  and keeping .NET specifics light — no assumptions baked in about framework version, DI container,
  or project layout. Covers Fact/Theory, Arrange-Act-Assert, one-behavior-per-test naming,
  fixtures for shared setup, exception and async assertions, and deterministic tests (no clock,
  random, or network) — which matters when the code under test drives health/diagnostics decisions.

  Trigger phrases:
  - "write xUnit tests for this" / "add unit tests for this C# class"
  - "test this .NET service" / "cover this with tests"
  - "TDD this" / "write a failing test first"
  - "how do I test the exception / async path here?"
tier: technique
task_class: decision-coherent
targets_scenarios: []   # draft-incomplete — see composition.yaml [needs-architect-review]
---

# .NET xUnit Testing

A deliberately **thin** skill for writing C#/.NET tests in xUnit. It supplies the *how in xUnit* and
leans on stack-agnostic testing skills for the *what to test and why*. It intentionally hard-codes
nothing about your sub-stack: no framework version, no specific DI container, no fixed project
layout — read those from the codebase you're in.

For the harder judgment — what behavior is worth a test, where a test is telling you the design is
wrong, how to structure a suite — defer to the agnostic testing discipline the catalog already
carries: **testing-as-design-feedback** (a test that's hard to write is design feedback),
**beck-tdd-pattern-family** (red-green-refactor and the TDD rhythm), and
**fields-test-design-discipline** (what to test, boundaries, and coverage judgment). This skill does
not restate them — it composes with them and translates their judgment into xUnit mechanics.

---

## When this triggers

- "write xUnit tests", "add unit tests for this C# class", "test this .NET service"
- "cover this with tests", "TDD this", "write a failing test first"
- Questions about the xUnit mechanics of a case: the exception path, the async path, shared setup,
  or parameterized cases.

If the question is really *what* to test or whether a test is worth writing, that's design judgment
— reach for the agnostic testing skills named above; this skill handles the xUnit expression of the
answer.

---

## xUnit essentials

**`[Fact]` vs `[Theory]`.** A `[Fact]` is one case with no inputs. A `[Theory]` with `[InlineData(...)]`
runs the same assertion across several inputs — use it for boundaries and equivalence classes rather
than copy-pasting near-identical `[Fact]`s.

```csharp
[Theory]
[InlineData(-1)]
[InlineData(0)]
public void Withdraw_NonPositiveAmount_Throws(decimal amount)
{
    var account = new Account(balance: 100m);           // Arrange
    Action act = () => account.Withdraw(amount);        // Act
    Assert.Throws<ArgumentOutOfRangeException>(act);    // Assert
}
```

**Arrange-Act-Assert.** Keep the three phases visible and in order. One behavior per test: a test
that asserts two unrelated things fails for two reasons and stops telling you which.

**Naming: `Method_State_Expected`.** `Cancel_AlreadyShipped_Throws`, `Parse_EmptyInput_ReturnsNone`.
The name states the behavior so a red test reads like a sentence in the runner output.

**Shared setup: fixtures, not field soup.** Use `IClassFixture<T>` for setup shared across the tests
in one class (built once per class), and a collection fixture (`[CollectionDefinition]` +
`ICollectionFixture<T>`) for setup shared across multiple test classes. Reach for a fixture only when
setup is genuinely expensive or shared — a plain constructor is fine for per-test arrange, and xUnit
constructs the test class fresh for every test, so there's no cross-test state to reset.

**Exceptions.** `Assert.Throws<TException>(() => ...)` for the sync path; `await
Assert.ThrowsAsync<TException>(() => ...)` for the async path. Assert on the exception type (and
message/parameter only when it's part of the contract), not on incidental wording.

**Async.** Make the test `async Task` and `await` the call — never `.Result` or `.Wait()`, which can
deadlock and swallow the real exception. Assert on the awaited result.

**Don't over-mock.** Prefer testing observable behavior through the real object. Mock only true
external boundaries (network, clock, I/O) — mocking everything in between welds the test to the
implementation, so a harmless refactor turns the suite red without any behavior changing. Where the
logic can be shaped as a pure function over its inputs, a mockless test against that functional core
is both simpler and more trustworthy than a mock-heavy one — a fit worth reaching for when the code
drives a health/diagnostics decision that must be exactly reproducible.

**Determinism is non-negotiable here.** No wall-clock reads, no `Random` without a fixed seed, no
live network or database in a unit test. Inject an abstraction for "now", seed randomness, and stub
external calls — a test whose result depends on when or where it runs cannot protect correctness in a
diagnostics path, and a flaky test trains the team to ignore red.

---

## Anti-patterns

| Anti-pattern | Why it fails |
|---|---|
| **Nondeterministic test** — read `DateTime.Now`, unseeded `Random`, or hit a live network/DB in a unit test | The test passes or fails on timing or environment, not on the code. In a health/diagnostics path a flaky suite is worse than none — it trains the team to ignore red and hides real regressions. Inject "now", seed randomness, stub the boundary. |
| **Over-mocking** — mock every collaborator, including pure in-process logic | The suite asserts *how* the code is wired, not *what* it does. A behavior-preserving refactor turns it red, so the tests punish good changes and stop being trusted. Mock only real external boundaries; test behavior through the real object. |
| **Multiple behaviors per test** — assert several unrelated outcomes in one `[Fact]` | A failure names the test, not the behavior; you can't tell which contract broke, and the first failing assert hides the rest. One behavior per test keeps failures diagnostic. |
| **Copy-pasted cases instead of `[Theory]`** — three near-identical `[Fact]`s differing only by input | Duplication drifts out of sync and buries the boundary you're actually checking. A `[Theory]` with `[InlineData]` states the equivalence class in one place. |
| **`.Result` / `.Wait()` in async tests** — block on a task instead of awaiting it | Can deadlock and wraps the real exception in an `AggregateException`, so `Assert.ThrowsAsync` misses it and the failure reason is obscured. Make the test `async Task` and `await`. |
| **Testing the framework, not your code** — assert that the ORM/serializer/DI container does its own job | Spends effort on behavior you don't own and that already has its own tests. Test your logic and its contracts; trust the framework's. |

---

## Composition

- **testing-as-design-feedback** — when a test is hard to write, the design is talking; that judgment
  decides whether to restructure the code before forcing the test.
- **beck-tdd-pattern-family** — the red-green-refactor rhythm this skill expresses in xUnit ("TDD this"
  starts with a failing `[Fact]`).
- **fields-test-design-discipline** — what to test, which boundaries matter, and how much coverage is
  enough; this skill supplies the xUnit form for those decisions.
- **bernhardt-mockless-testing-via-functional-core** — the mockless / functional-core route behind the
  "don't over-mock" and determinism guidance.

This skill stays thin on purpose: the agnostic skills carry the test-design judgment, and this one
carries the xUnit mechanics. Read the sub-stack (framework version, DI, layout) from the codebase
rather than assuming it.
