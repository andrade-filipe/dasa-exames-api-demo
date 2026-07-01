---
name: context-hygiene
version: "1.0.0"
description: |
  Helps a developer avoid context pollution while working with LLM agents: load only
  the context relevant to the task just-in-time, compact accumulated context when the
  window grows stale, and keep the working window minimal-sufficient at every step.

  Trigger phrases:
  - "my agent's context is getting polluted / bloated / stale"
  - "I need to manage what goes into the context window for this coding task"
  - "load only relevant files / docs for this task"
  - "compact / summarize accumulated context before the next phase"
  - "keep context clean between agent phases"
tier: technique
task_class: decision-coherent
targets_scenarios: []   # draft-incomplete — see composition.yaml [needs-architect-review]
---

# Context Hygiene

A developer-facing Skill that operationalizes **just-in-time context loading** (primary
source: `framework/techniques/just-in-time-context-loading.md`) composed with **context
compaction** (`framework/techniques/context-compaction.md`) to keep every LLM-agent coding
step's working window minimal-sufficient.

The Skill is instantiated per task or per agent phase. It answers four questions:

1. **What context do I load and when?** (`load_strategy` + `relevance_filter`)
2. **How do I represent what I haven't loaded yet?** (`pointer_metadata`)
3. **When do I compact the accumulated window?** (`compaction_trigger`)
4. **How large is my allowed window?** (`window_budget`)

---

## Slots

| Slot | Type | Required | Description |
|---|---|---|---|
| `load_strategy` | enum | yes | How context enters the step. One of `just-in-time` (load via a tool exactly when the content is needed — DEFAULT) or `upfront-bounded` (load a curated subset at step start, bounded by `window_budget`). `fill-the-window` and `all-history` are rejected. |
| `relevance_filter` | string | yes | How task-relevance is decided for each candidate context item before loading it. Example: `"load only files referenced in the current task description"`, `"load only the function being edited + its direct callers"`, `"retrieve top-3 docs matching the current error message"`. |
| `pointer_metadata` | string | yes | The lightweight reference format used to represent NOT-YET-LOADED content in context. Keeps identifiers cheap — loaded on demand via the `trigger_tool`. Example: `"file path + one-line summary"`, `"skill name + description"`, `"URL + title"`. |
| `compaction_trigger` | enum | yes | When to compact the accumulated window. One of: `threshold-based` (compact when window exceeds N% — composing with `context-compaction`'s `compaction_threshold` default of 80), `phase-boundary` (compact at the end of each agent phase / task), `explicit` (developer calls compaction manually). |
| `window_budget` | int | yes | Finite token ceiling for the working window at any single step. Sized to the model's effective-attention envelope, NOT to the nominal context window. Consult REF-0019 (Lost in the Middle): reader accuracy saturates well before the nominal window — operating points for a 200K-window model typically fall in the 20–40K range. |

---

## Pattern

### Load phase

At the start of each step, the agent holds only **pointer metadata** — lightweight identifiers
for all potentially relevant context (file paths, skill names, doc URLs, query handles). Nothing
is loaded into the token budget yet.

When reasoning requires actual content, the agent calls the designated `trigger_tool`
(`read_file`, `fetch_url`, `query_database`, etc.) to load **only** the item needed, exactly
at the moment of need. Content not yet needed stays as a pointer.

The `relevance_filter` decides which candidates are even admitted as pointers: items outside
the filter are not tracked — they do not consume pointer budget either.

### Window management

At every step, the live window is checked against `window_budget`. If loading a new item would
push the window past the budget, the oldest non-critical content is dropped or compacted first.

`compaction_trigger` governs when a full compaction call is made:

- **`threshold-based`**: the agent checks utilization before each load; when the window exceeds
  the declared percentage, it fires a compaction call (per the `context-compaction` technique's
  strategy and preserved-categories contract) before continuing.
- **`phase-boundary`**: at the end of each agent phase (e.g. after all edits for a sub-task are
  done), the agent compacts accumulated intermediate reasoning and tool outputs into a brief
  summary, then starts the next phase clean.
- **`explicit`**: the developer invokes compaction directly (useful for interactive coding loops
  where phase boundaries are not automatic).

### Compaction contract

When `compaction_trigger = threshold-based` or `phase-boundary`, compaction follows the
`context-compaction` technique's contract:

- **Threshold**: use the declared percentage or the technique's default (80% of `window_budget`).
- **Strategy**: `natural-language-summary` is the default (cheapest, weakest schema guarantees);
  upgrade to `fixed-schema-extraction` for long-running sessions where decisions and open
  questions must survive verbatim.
- **Preserved categories (mandatory whitelist)**:
  - `failure-traces` — errors, rejected approaches, tool failures (per `preserve-failure-clear-redundancy` constant; NON-NEGOTIABLE)
  - `decisions` — architectural choices, user preferences expressed in-session
  - `active-file-paths` — paths the agent is currently editing

---

## Usage

```
load_strategy: just-in-time
relevance_filter: "load only files referenced in the current task description and their direct callers"
pointer_metadata: "file path + last-modified + one-line purpose"
compaction_trigger: phase-boundary
window_budget: 32000
```

With this configuration, the agent begins each step holding only the file-path index.
Files are loaded on demand. At the end of each sub-task phase the window is compacted to a
summary that preserves failure traces, decisions, and active paths — the next phase starts
with a clean, minimal window.

---

## Anti-patterns

| Anti-pattern | Why it fails |
|---|---|
| **fill-the-window** — load every potentially relevant file/doc upfront because the model "can handle it" | Per REF-0019 (Lost in the Middle), reader accuracy degrades sharply past the effective-attention envelope. Filling a 200K window with code files induces >20pp accuracy drop and position-sensitivity bias. The model attends strongly only to document heads/tails; middle files are silently ignored. |
| **dump-all-docs** — pass an entire documentation corpus (README, API ref, CHANGELOG, guides) into context for every coding step | Context pollution (REF-0036): as volume grows past effective-attention, signal-to-noise collapses and the agent produces inconsistent decisions across nearly identical prompts. The correct pattern is `pointer_metadata` + JIT load on demand. |
| **skip-compaction-across-phases** — keep accumulated reasoning from phase 1 verbatim through phases 2, 3, 4 without compacting | Per REF-0032 (Manus lessons), this fills the window with redundant successful-step outputs while the agent's attention degrades. Worse, if `failure-traces` are not preserved through a compaction boundary, the agent repeats the same wrong approach in subsequent phases. |
| **upfront-bounded-without-relevance-filter** — set `load_strategy: upfront-bounded` but load all available context up to `window_budget` | Without a `relevance_filter`, upfront-bounded degenerates to fill-the-window at budget scale. The filter is what keeps loading bounded to *task-relevant* items, not merely *fitting* items. |
| **all-history** — carry the full conversation / task history without summarization | Grows the window monotonically. Per REF-0033, this turns budget exhaustion into a crash event with no recovery path. The `compaction_trigger` slot exists precisely to prevent this. |

---

## Composition

This Skill composes:

- **`just-in-time-context-loading`** (`framework/techniques/just-in-time-context-loading.md`) — primary pattern for the load phase: lightweight pointers in context, tool-based fetch on demand.
- **`context-compaction`** (`framework/techniques/context-compaction.md`) — governs the compact phase when `compaction_trigger = threshold-based`.

Primary constant: **`minimal-sufficient-context`** (`framework/constants/minimal-sufficient-context.md`) — the Skill IS this constant applied to the daily agent-coding loop. The `window_budget` slot realizes the `token_budget_per_step` the constant requires; the `load_strategy` realises the `select_strategy`; the `compaction_trigger` realizes the `budget_overflow_action`.

Secondary constants:
- `context-curation-discipline` — the `relevance_filter` slot is the Select-pillar realization.
- `preserve-failure-clear-redundancy` — `failure-traces` is the mandatory item in the compaction whitelist.
- `bounded-tool-surface` — the JIT load pattern keeps tool surface tight: only tools needed for the current task are active.

---

## Source references

- `framework/techniques/just-in-time-context-loading.md` (primary)
- `framework/techniques/context-compaction.md` (composed)
- `framework/constants/minimal-sufficient-context.md`
- REF-0001 (Anthropic, effective context engineering — compaction-prompt-tuning, curation-over-maximization)
- REF-0029 (LangChain, context engineering for agents — JIT loading, auto-compact at 80%)
- REF-0032 (Manus lessons — failure-trace preservation, restorable-compression)
- REF-0019 (Lost in the Middle — reader saturation, U-shaped position curve)
