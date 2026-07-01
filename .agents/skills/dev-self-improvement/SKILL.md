---
name: dev-self-improvement
description: |
  Turns each finished task into a concrete, human-approved improvement to the team's shared
  setup. After a non-trivial task — or whenever you say a retro is due — it captures the
  friction, corrections, and recurring mistakes from the session, distills them into specific
  proposed edits to AGENTS.md and/or a specific team skill, and presents those edits for your
  approval before writing anything. This is how the toolbox learns your stack over time.

  Trigger phrases:
  - "that went wrong / we hit a snag / that was painful"
  - "retro this / let's do a retro / capture this gotcha"
  - "update our AGENTS.md / propose AGENTS.md changes"
  - "improve our setup / make the toolbox smarter about this"
  - "/dev-self-improvement"
tier: technique
task_class: decision-coherent
targets_scenarios: []   # draft-incomplete — see composition.yaml [needs-architect-review]
---

# Dev Self-Improvement

This skill is how the team's shared toolbox **adapts itself in the field**. The exact services,
sub-stack, and day-to-day use-cases of your codebase are not known ahead of time — so instead of
guessing them up front, the toolbox learns them from real sessions. After a task, this skill turns
what just happened into a concrete proposal to improve `AGENTS.md` (the team's durable memory file)
and the team's own skills, and it never writes to those files without your explicit go-ahead.

Treat it as the "propose improvements" engine for the whole team. It is the broader companion to
the lean landmine list maintained by **curated-gotchas-retrospective-loop**: this skill proposes
improvements of any shape (a corrected instruction, a new gotcha, a sharper skill), while
curated-gotchas-retrospective-loop is the disciplined, bounded list of system-specific failure
conditions that this engine feeds when the improvement is a landmine.

---

## When this triggers

**Automatically**, at the end of a non-trivial task — anything where the assistant needed several
turns, hit a wrong path, or you had to correct it — or when you signal a retrospective is due:

- "that went wrong", "we hit a snag", "that took way too long"
- "retro this", "capture this gotcha", "note this for next time"
- "update our AGENTS.md", "we should write this down"

**Manually**, on demand:

- "/dev-self-improvement"
- "improve our setup", "propose AGENTS.md changes", "make the toolbox smarter about this service"

If nothing durable came out of the session — the task was routine and nothing surprised anyone —
say so and stop. Not every task produces an improvement, and inventing one just to have output is
its own anti-pattern (see below).

---

## Behavior

Three phases. The third is a hard gate.

### 1. Collect

Review what actually happened this session: where the assistant went down a wrong path, what you
had to correct, which assumptions about the codebase turned out to be false, and any mistake that
you recognize as *recurring* (you've corrected it before). Prefer specifics tied to real files,
services, and commands over vague sentiment. If the codebase itself can confirm a detail, check it
rather than guessing.

### 2. Distill

Turn the raw friction into **concrete proposed edits**, not observations. Each proposal is one of:

- a new or revised line in `AGENTS.md` (a durable, system-specific instruction future sessions
  should load) — keep it a system-specific failure condition the model would not otherwise infer,
  not general craft advice it already applies;
- an edit to a specific team skill (sharpen a trigger, fix a wrong step, add a missing constraint);
- a new landmine handed to the lean gotchas list (the curated-gotchas-retrospective-loop
  discipline) when the improvement is "next time, watch out for X."

Each proposal must name its target file and show the exact text to add or change, so it can be
approved and applied without further interpretation.

### 3. Approve, then write (gated)

**Present every proposal for approval before writing anything.** Show the target file, the exact
diff, and one line on why it earns its place. Wait for an explicit yes. Only after approval do you
edit `AGENTS.md` or the skill. Editing durable team memory silently is forbidden — a bad line in
`AGENTS.md` is read by every future session, so a human decides what lands there. If a proposal is
rejected, drop it; if it's revised, re-show the final text before writing.

Keep the durable memory lean. If `AGENTS.md` is growing without bound, propose graduating stable
entries (a rule that's now always-true belongs in durable docs or an automated check) or pruning
stale ones whose root cause was already fixed in code.

---

## Anti-patterns

| Anti-pattern | Why it fails |
|---|---|
| **Silent memory write** — edit `AGENTS.md` or a team skill directly without showing the proposal and getting a yes | Durable memory is read by every future session; one wrong or biased line quietly corrupts every downstream task. A human must approve what lands. Skipping the gate is exactly the failure this skill exists to prevent. |
| **Write-only retro** — record lessons but never fold them into `AGENTS.md` or a skill where the next session will actually load them | A lesson the next session never sees prevents nothing. The value is in the *consume* half — the improvement has to reach the file that future work reads, not just a scratch note that's discarded. |
| **Improvement theater** — manufacture a proposal after every task even when nothing surprising happened | Padding durable memory with generic advice ("write tests", "handle errors") the model already applies crowds out the rare, high-signal, stack-specific landmines. If the session held no real lesson, say so and write nothing. |
| **General-knowledge gotcha** — propose a line for something any competent .NET dev already knows | AGENTS.md earns its keep only with system-specific failure conditions unique to *this* codebase. Generic craft knowledge belongs in onboarding docs, not the hot memory the model reloads every session. |
| **Unbounded memory growth** — append every observation forever with no pruning or graduation | The memory file slowly bloats back into noise, and the model's attention degrades across it. Graduate stabilized rules out to durable docs or automated checks; prune entries whose cause is already fixed. |

---

## Composition

- Pairs with **curated-gotchas-retrospective-loop** — that skill is the lean, bounded landmine
  list; this one is the broader propose-and-approve engine that feeds it when the improvement is a
  gotcha, and also handles skill edits and AGENTS.md instructions that are not landmines.
- Complements **dev-grill-me** — grill-me sharpens a task *before* you act; this skill captures what
  you learned *after*.

The loop is deliberately gated: capture is cheap and automatic, but every durable write waits for a
human yes.
