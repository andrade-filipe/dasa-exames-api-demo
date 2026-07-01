---
name: dev-grill-me
description: |
  Blocks action on a vague coding request and interviews you into a specific, context-rich task
  before any code is written. When a request names no files, no concrete goal, or no way to verify
  it's done, this asks focused questions ONE AT A TIME — each paired with a recommended answer you
  can accept — until the task names the service/project, the exact files, the expected behavior,
  the constraints, and how success will be checked.

  Trigger phrases:
  - "fix the bug", "improve this", "make it faster", "add validation" (no file or symptom named)
  - "refactor this", "clean this up", "just make it work" (no acceptance criteria)
  - "it / that / the thing is broken" (no clear referent)
  - "grill me", "stress-test this", "are you sure this is enough to start?"
tier: technique
task_class: decision-coherent
targets_scenarios: []   # draft-incomplete — see composition.yaml [needs-architect-review]
---

# Dev Grill-Me

A lazy prompt produces lazy code. When a coding request is underspecified — no files, no concrete
goal, no acceptance criteria — this skill **stops and interviews you** until the task is dense
enough to act on with confidence. The point is to build the habit of naming what actually matters:
which service and project, which files, the exact behavior expected, the constraints that bind, and
how you'll both know it worked.

It runs one focused question at a time, and every question comes with the assistant's own
recommended answer — so the fast path is to skim the recommendation and say "yes" rather than to
compose a paragraph. The friction is the feature: a minute of grilling up front beats an hour
undoing a confident guess at the wrong file.

---

## When this triggers

Fires automatically on an underspecified coding request, including:

- **One-line directives with no target** — "fix the bug", "make it faster", "add validation",
  "improve this" with no file, path, service, or symptom named.
- **Imprecise referents** — "it", "that", "the thing", "this endpoint" with no clear antecedent the
  codebase or conversation pins down.
- **Plan / refactor / multi-file changes with no acceptance criteria** — any non-trivial change
  where "done" and "correct" are not defined.
- **Explicit requests** — "grill me", "stress-test this", "are you sure that's enough to start?"

It does **not** fire when the task already names the files, the concrete goal, the constraints, and
a way to verify — a specific request should proceed, not get interrogated.

---

## Behavior

**One focused question at a time.** Do not dump a checklist. Ask the single most load-bearing
unknown, wait, then ask the next. Each question carries the assistant's **recommended answer**, so
accepting the default is one word.

Prefer to answer questions from the codebase rather than asking. If the repo can tell you which
project owns a class, which callers a method has, or what the current behavior is, go read it — only
ask when the answer genuinely lives in your head, not the code.

Grill along these axes, framed for a .NET/C# codebase:

1. **Which code?** Which service / project and which specific files or types?
   *e.g. "Is this in the OrdersApi project — specifically `OrderService.Cancel`? (recommended: yes,
   that's the only Cancel path)"*
2. **What's the concrete goal?** The exact behavior change, not the vibe.
   *e.g. "Should a cancel on an already-shipped order throw, or return a no-op result? (recommended:
   throw `InvalidOperationException` — a silent no-op hides data bugs in a diagnostics flow)"*
3. **What constraints bind?** Backward compatibility, public contracts, performance, determinism,
   patterns already in the codebase.
   *e.g. "Must the public method signature stay unchanged for existing callers? (recommended: yes)"*
4. **How will we verify?** The acceptance criteria and the check that proves it.
   *e.g. "Is a passing xUnit test that asserts the throw on a shipped order the definition of done?
   (recommended: yes — plus one for the happy path)"*

**Stop when the prompt is dense enough to act on** — files named, goal explicit, constraints stated,
and a verification path agreed. Then summarize the now-specific task in a line or two and proceed.
Do not keep grilling past sufficiency; over-interrogation is its own anti-pattern.

---

## Anti-patterns

| Anti-pattern | Why it fails |
|---|---|
| **Guess and go** — accept "fix the bug" and start editing the file you assume is meant | The confident guess lands on the wrong file, the wrong service, or the wrong definition of "fixed," and the wasted work compounds. Naming the target first is cheaper than undoing a wrong change. |
| **Checklist dump** — fire ten questions at once | The developer skims, answers three, and the task is still underspecified. One question at a time keeps each answer considered and the momentum forward. |
| **Question without a recommendation** — ask "what should happen?" with no proposed answer | Puts all the drafting load on the developer and slows the loop. Every question must carry the assistant's own recommended answer so accepting the default is one word. |
| **Ask what the code already answers** — interrogate the developer for facts the repo states plainly | Which project owns a type, who calls a method, what the current behavior is — read it. Asking for retrievable facts wastes the developer's turn and signals you didn't look. |
| **Grill past sufficiency** — keep interrogating after files, goal, constraints, and verification are all pinned | The friction has already paid off; more questions now just annoy. Stop, summarize the specific task, and start. |

---

## Composition

- Complements **dev-self-improvement** — grill-me sharpens the task *before* you act; self-improvement
  captures what you learned *after*.
- The recommended-answer-per-question shape keeps the interview fast: the developer confirms rather
  than composes.
