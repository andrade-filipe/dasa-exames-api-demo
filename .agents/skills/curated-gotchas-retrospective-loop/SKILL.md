---
# Buyer-facing SKILL.md — lean 5-field frontmatter per ADR-0010 / ADR-0017
# Full framework-classification metadata lives in composition.yaml
name: curated-gotchas-retrospective-loop
tier: technique
task_class: decision-coherent
targets_scenarios: []   # [needs-architect-review] — no harness-improvement-loop scenario seeded yet;
                        # see stage5_observation in composition.yaml. Pending scenario seed by architect
                        # before B2 cadence can proceed. Artifact is draft-incomplete on this field only.

# Technique slots (all five required, per framework/techniques/curated-gotchas-retrospective-loop.md)
gotchas_file_path: "<operator-supplied: path to the single, version-controlled, human-curated landmine file; e.g. 'docs/GOTCHAS.md' or 'AGENTS.md#gotchas'>"
entry_budget: 600       # operator-set ceiling (positive integer); reference operating point: 553 lines (REF-0193)
# capture_trigger allowed values: post-task-retrospective | recurring-failure-threshold | human-curation-on-demand
capture_trigger: post-task-retrospective
# consume_strategy allowed values: load-full-list-before-task | retrieve-relevant-entries-by-task | inject-on-matching-precondition
# legal to use load-full-list-before-task while list stays under entry_budget; switch to retrieve-relevant-entries-by-task as it approaches the ceiling
consume_strategy: load-full-list-before-task
# graduate_or_prune_policy allowed values: graduate-to-durable-docs | prune-on-staleness | keep-hot
graduate_or_prune_policy: graduate-to-durable-docs

description: >
  Harness improvement loop that keeps a lean, human-curated landmine list current across sessions.
  Triggers when: a recurring agent system is failing on the same product-specific conditions repeatedly
  and more documentation is not helping (or is actively hurting accuracy); an operator correction reveals
  a failure condition the base model would not infer from general training; a post-run retrospective
  identifies a candidate gotcha entry for human review; the harness needs a structured capture →
  consume → graduate/prune cycle to prevent context rot from re-accumulating. Does NOT trigger for
  within-task runtime self-reflection (that is reflexion); does NOT trigger for one-shot tasks with
  no cross-session surface to improve.

source_refs:
  - REF-0193   # Nisi, "How I deleted 95% of my agent skills", AI Engineer 2026 — anchor evidence
  - REF-0020   # Meta-Harness outer loop — automated sibling; supplies filesystem-as-experience-store substrate
  - REF-0002   # OpenAI Harness Engineering — graduate destination (golden principles, executable gates)
  - REF-0044   # Lopopolo, AI Engineer 2026 — concentrated-skill-leverage corroboration
---

# Curated Gotchas Retrospective Loop

A cross-session harness-improvement loop that keeps ONE lean, human-curated list of system-specific failure conditions current and feedforward. The list is bounded by `entry_budget` and cycles through three phases: **capture** (distill a recurring mistake into a single entry after a run), **consume** (load relevant entries into the next task's context before the agent can repeat the mistake), and **graduate or prune** (stable entries rise to durable docs or executable gates and leave the hot list; stale or fixed entries are deleted). The loop's load-bearing claim is subtractive: a 553-line curated list outperformed a 10,000-line auto-generated tree by +20 percentage points on task success (REF-0193: 77% → 97%). Every entry must be a failure condition the base model would NOT infer from general training.

## When this Skill triggers

This Skill applies to a **recurring agent system** — one that runs against the same codebase, workflow, or product context across multiple sessions — and specifically when **two conditions hold simultaneously**:

1. The same product-specific failure conditions are recurring across sessions (the agent trips on the same landmines repeatedly).
2. Adding more documentation, instructions, or skill files is not improving results — or has begun degrading them.

Concrete activation signals:
- An operator issues a correction that reveals a system-specific failure the base model would not derive from general knowledge (e.g. "this project uses `X` instead of the standard `Y` pattern — the agent keeps reaching for `Y`").
- A post-run trace review identifies a mistake that occurred more than once in recent sessions.
- The gotchas file approaches or exceeds `entry_budget` (triggering a mandatory graduate-or-prune pass).
- The capture-trigger condition fires: a completed run's trace is available for retrospective analysis.

This Skill does **NOT** trigger for:
- One-shot tasks where there is no cross-session surface to improve.
- Within-task runtime issues (use `reflexion` for those — see disambiguation below).
- General-purpose knowledge the base model already applies correctly.

## Disambiguation: this Skill vs. `reflexion`

**This Skill (`curated-gotchas-retrospective-loop`) and `reflexion` are not interchangeable.** They are complementary siblings operating at different granularities:

| Dimension | `curated-gotchas-retrospective-loop` (this Skill) | `reflexion` |
|---|---|---|
| Scope | **Cross-session** — improves the harness over time | **Within-task** — improves within a bounded trial budget |
| Human involvement | **Human in the loop** — a human (or an operator-approved analyzer) curates what enters the list | **Automated** — the agent generates its own reflection text and retries |
| Artifact | **Durable external file** — the gotchas list persists in version control across sessions | **Ephemeral in-context buffer** — reflection text lives in the current task's context and is discarded at task end |
| Loop timing | **Post-task** — the retrospective runs after a completed run; the insight feeds the NEXT session | **In-task** — reflection happens between trials of the SAME task |
| Failure target | **Recurring product-specific landmines** — conditions unique to the system the base model doesn't know | **Trial-level reasoning errors** — mid-task inference failures the model can self-correct |

A system may use both: `reflexion` improves individual task trials; this Skill graduates the recurring mistakes those trials surface into durable gotchas for the next session. The two loops compose without conflict.

**Critical:** a Skill that documents only a write-only retrospective (capture without consume) is the `write-only-retrospective` anti-pattern — it records lessons after a failure but never loads the relevant gotcha BEFORE the next task that would trip on it. The feedforward consume step is where the value is realized.

## The three phases

### Phase 1 — Capture

**What enters the list and how.** The `capture_trigger` governs admission:

- `post-task-retrospective` — after a completed run, a human or designated analyzer reviews the execution trace, identifies ONE recurring product-specific mistake, and proposes ONE lean entry in the format: **symptom → cause → fix → location + tag**. The entry is approved by a human before landing in the gotchas file. Bulk auto-generation from documentation is rejected — it is the failure mode this Skill reverses (REF-0193: auto-generated skills sent accuracy from 97% to 77%).
- `recurring-failure-threshold` — the same failure observed across N sessions auto-nominates itself as a candidate entry. A human reviews and approves the candidate before it is written.
- `human-curation-on-demand` — an operator explicitly promotes a landmine after observing it in production, without waiting for the threshold or post-run cycle.

**Entry format (lean, not verbose):**

```
## [TAG] Short symptom description
**Cause:** one sentence on why this happens in THIS system.
**Fix:** one sentence or code pattern.
**Location:** file path or module name where the issue manifests.
```

**Anti-bloat discipline:** if a proposed entry overlaps ~80% with an existing entry, edit the existing entry rather than adding a new one. If the entry describes general craft knowledge the base model already applies (write tests, handle errors, use meaningful names), reject it — it belongs in onboarding docs, not the hot gotchas list.

**Budget enforcement:** the gotchas file MUST NOT exceed `entry_budget` lines/entries. When the file approaches the ceiling, the **graduate-or-prune** phase is mandatory before new entries can be added.

### Phase 2 — Consume

**How entries reach the next task's context.** The `consume_strategy` governs this:

- `load-full-list-before-task` — the entire gotchas file is loaded into the context at task start. **Legal only while the file stays under `entry_budget`.** As the list approaches the budget ceiling, switch to `retrieve-relevant-entries-by-task`.
- `retrieve-relevant-entries-by-task` — grep or semantic search over the gotchas file keyed on the current task's description, file paths, or tag cloud. Load only the matching entries. This is the `just-in-time-context-loading` Technique applied to the gotchas list. Required once the list approaches the `entry_budget`.
- `inject-on-matching-precondition` — a harness hook checks the current step against each entry's precondition field and injects the entry only when the precondition matches. The most precise strategy; requires tooling but gives the tightest context budget.

**The feedforward obligation is non-negotiable:** the relevant entry MUST be loaded BEFORE the agent executes the step that would trip on the landmine. An entry that is only audited after the mistake has been repeated is a `write-only-retrospective` failure.

When `consume_strategy == load-full-list-before-task`: the file MUST stay under `entry_budget` — if the list exceeds the budget, this strategy is invalid and `retrieve-relevant-entries-by-task` becomes mandatory. This assertion is enforced by B1 structural check assertion 6.

### Phase 3 — Graduate or Prune

**What keeps the list lean over time.** The `graduate_or_prune_policy` governs the lifecycle of each entry:

- `graduate-to-durable-docs` — a gotcha that has become a settled, enforced invariant (the team always fixes it, every new team member learns it in onboarding, it has not recurred in 5+ sessions) **leaves the hot list** and rises to durable documentation, onboarding material, or — best of all — an executable gate (a linter rule, CI check, or state machine constraint that makes the failure structurally impossible). A graduated gotcha becomes a `framework/constants/`-style invariant rather than a `harness/gotchas/`-style warning. The `enforce-dont-instruct` principle (REF-0193) is the graduation destination: the failure becomes structurally impossible rather than merely warned-against.
- `prune-on-staleness` — an entry whose root cause was fixed in code, whose failure has not recurred across N sessions, or whose precondition no longer exists in the codebase is deleted from the hot list. A fixed bug is not a landmine anymore.
- `keep-hot` — the failure is still live and the root cause is not yet enforceable via a gate. The entry stays until it either graduates or is pruned.

**The graduate-or-prune policy is what separates a curated landmine list from a documentation tree.** A list that only grows becomes the very thing this Skill deletes. Per REF-0193's central finding: a 553-line list that regrows into a 10,000-line list has undone the experiment and context rot returns through the back door.

## Entry budget and leanness rationale

`entry_budget` is the structural bound that keeps the list lean and enforceable. The reference operating point from REF-0193 is **553 lines** — a list that replaced approximately 10,000 lines of auto-generated agent skills and raised task success from 77% to 97%. The +20 percentage point gain was not from adding MORE documentation; it was from **deleting 95%** of existing documentation and replacing it with a single curated slice.

The mechanism is `minimal-sufficient-context`: context rot — the degradation of task accuracy when the agent's context window is inflated with irrelevant documentation — was the root cause of the 97% → 77% regression. The curated gotchas list is the minimum-sufficient-context discipline applied to cross-session harness improvement.

Operators set `entry_budget` based on their system's characteristics. Suggested starting points:
- Small, stable codebase with few novel failure modes: 200–300 lines
- Medium production system with active development: 400–600 lines (REF-0193 reference range)
- Large system with many subsystems: 600–800 lines (approaching upper bound; consider splitting by subsystem)

A budget above 1,000 lines should be treated as a signal that the graduate-or-prune cycle is not running, not as a reason to raise the ceiling.

## Gotchas file format and constraints

The `gotchas_file_path` must point to a **single, version-controlled, human-curated file** (or one bounded section within a larger file, e.g. `AGENTS.md#gotchas`). Constraints:

1. **ONE file, not a tree.** The Technique's load-bearing claim is the deletion of the tree in favor of one lean file. An auto-generated directory of per-domain skill files fails this slot — that is the failure mode this Skill was designed to reverse.
2. **Human-curated, not auto-generated.** Entries are written and approved by a human. Auto-generated entries from documentation processing tools fail the slot.
3. **Version-controlled.** The file lives in a repository and its history is tracked. The trace substrate for the capture phase reads from this file; the capture phase writes back to it. Without version control, the retrospective loop cannot trace its own improvement over time.
4. **Tag-indexed.** Each entry carries at least one tag (e.g. `[auth]`, `[database]`, `[api-client]`) so the `retrieve-relevant-entries-by-task` consume strategy can grep efficiently.

## Anti-patterns

| Anti-pattern | What it looks like | Why it fails |
|---|---|---|
| `skill-bloat-context-rot` | Auto-generating large volumes of skill/doc files so the agent consults many irrelevant files | Inflates context with noise; REF-0193 measured 97% → 77% accuracy regression with the bloated skill loaded; leaving the tree in place defeats this Skill |
| `write-only-retrospective` | Running the analyze-the-logs half without the consume half; lessons are recorded after failures but never loaded before the next task that would trip on them | Produces audit material, not improvement; the feedforward read is where value is realized (CASE: Synthesize without Enforce = no loop) |
| `unbounded-gotchas-growth` | Appending every failure forever with no `entry_budget` and no graduate/prune policy | The list slowly regrows into the documentation tree it replaced; context rot returns through the back door; a 553-line list that becomes 10,000 lines has undone the experiment |
| `gotcha-for-knowledge-the-model-already-has` | Populating the list with general craft knowledge ("write tests", "handle errors", "use meaningful names") | Spends the entry budget on signal the base model already applies; crowds out system-specific landmines that are the entire point; per REF-0193 models already know how to code — the gap is product-specific failure conditions |

## CASE cycle reference

The loop follows the **CASE** framing from REF-0193 (Nisi):

```
Collect  → gather completed run logs, operator corrections, recurring-failure nominations
Analyze  → identify which failures are product-specific and recurring (not general knowledge)
Synthesize → distill ONE entry per failure in the lean symptom→cause→fix→location format
Enforce  → inject the entry into the next task's context before the mistake can recur
           OR graduate into an executable gate that makes the failure structurally impossible
```

Each transition is gated: Collect → Analyze requires a completed run with a structured trace. Analyze → Synthesize requires a human reviewer approving the candidate entry. Synthesize → Enforce requires the entry being written to the version-controlled gotchas file before the next session begins.

## Composition notes

- **With `reflexion`:** within-task trial reflection identifies errors in real time; this Skill graduates the recurring ones into durable cross-session gotchas. The loops compose without conflict.
- **With `just-in-time-context-loading`:** the `retrieve-relevant-entries-by-task` consume strategy IS just-in-time loading applied to the gotchas list. When the list approaches `entry_budget`, JIT loading is the mandatory graduation path for the consume strategy.
- **With `context-compaction`:** the retrospective phase (Analyze → Synthesize) is a form of cross-session compaction — many raw trace failures distilled into one lean entry in the gotchas file.

## Cross-references

- Framework source: [`framework/techniques/curated-gotchas-retrospective-loop.md`](../../../framework/techniques/curated-gotchas-retrospective-loop.md) — the source Technique pattern this Skill realizes.
- Anchor evidence: [`research/articles/catalog/fact-sheets/REF-0193_2026-harness-engineering-nisi-deleting-agent-skills-gotchas.md`](../../../research/articles/catalog/fact-sheets/REF-0193_2026-harness-engineering-nisi-deleting-agent-skills-gotchas.md)
- Disambiguation: [`generation/skills/chain-of-thought-wrapper/`](../chain-of-thought-wrapper/) — the `reflexion` Technique for within-task trial loops (the sibling this Skill is explicitly NOT).
- Constants honored: `minimal-sufficient-context` (the 97%→77% regression IS the fill-the-window anti-pattern, measured at production scale); `observable-agent-traces` (capture phase reads the completed run's trace — without replayable traces there is nothing to retrospect on); `bounded-tool-surface` (the saturation finding that the 95%-skill-deletion corroborates: a sprawling capability surface degrades accuracy).
