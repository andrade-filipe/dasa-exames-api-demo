---
# Buyer-facing SKILL.md — lean 5-field frontmatter per ADR-0010 / ADR-0017
# Full framework-classification metadata lives in composition.yaml
name: verify-and-doc
tier: technique
task_class: decision-coherent
targets_scenarios: []   # [needs-architect-review] — no documentation-sync-loop scenario seeded yet;
                        # see stage5_observation in composition.yaml. Pending scenario seed by architect
                        # before B2 cadence can proceed. Artifact is draft-incomplete on this field only.

# Technique slots — adapted from curated-gotchas-retrospective-loop (post-task graduation phase)
# applied to documentation-sync. All four are operator-parameterized slots.
doc_root: "<operator-supplied: path to documentation directory; e.g. 'docs/llm/', 'docs/', 'docs/slices/'>"
ownership_convention: "<operator-supplied: how code paths map to doc sections; e.g. 'slice frontmatter owners-paths globs', 'module-to-doc MAP.md table', 'directory-mirrors-doc-hierarchy'>"
verify_trigger: post-implement   # enum: post-implement | post-commit | on-demand
graduate_prune_policy: graduate-to-durable-docs   # enum: graduate-to-durable-docs | prune-on-staleness | keep-hot

description: >
  Closes the R-P-I-V loop after an implement cycle: reads git diff, matches changed files against
  doc ownership conventions, updates drifted documentation, creates docs for undocumented areas,
  and refreshes last-verified markers.
  Triggers when: an implement cycle or commit introduces code changes and docs may have drifted;
  a git diff shows files changed but no corresponding doc update landed in the same pass;
  a new module, integration, or behavioral area appears in the diff that has no existing doc slice;
  the developer is finishing an RPI cycle and needs the V (Verify) phase to close the loop before
  the next research or planning pass. Does NOT trigger for pure refactors that preserve behavior
  (no observable behavioral change = no doc update needed). Does NOT trigger before implementation
  (planning docs belong in the plan artifact, not here). Does NOT write or edit source code.

source_refs:
  - REF-0193   # Nisi, "How I deleted 95% of my agent skills" — capture→consume→graduate loop shape
  - REF-0022   # Externalization in LLM agents — filesystem-as-experience-store substrate
  - REF-0020   # Meta-harness outer loop — cross-session improvement via durable external artifacts
  - REF-0002   # OpenAI Harness Engineering — graduate destination (golden principles, executable gates)
---

# Verify & Document

The "V" closer of an R-P-I-**V** loop. After an implement cycle, this Skill verifies that
existing documentation still matches the code, updates drifted sections, creates documentation
for newly-introduced areas, refreshes `last-verified` markers, and proposes graduation of any
documentation content stable enough to move from ephemeral tracking to durable institutional record.
Documentation is treated as a first-class loop artifact — code that changes without a verification
pass leaves the next session's context window polluted with stale facts.

## When this Skill triggers

This Skill applies **after an implement cycle** — the moment when code has changed and the doc layer
may be behind. Harness-level auto-trigger: surface whenever a git diff shows modified source files
but the same diff shows no documentation updates. Developer-level trigger: the developer consciously
closes the RPI loop (Research → Plan → Implement → **Verify**).

Concrete activation signals:

- An implement cycle or feature branch lands new code; documentation coverage is unclear.
- `git diff` shows modified files in `src/`, `lib/`, `app/`, or equivalent source trees with no
  matching change in `doc_root`.
- A new module, API integration, CLI command, or behavioral area appears in the diff and no doc
  slice or section exists for it yet.
- `last-verified` dates in existing doc slices are older than the code they describe.
- The operator explicitly invokes `/verify-and-doc` to close the loop after an implement phase.
- Gotchas graduation cue: a gotcha or known-issue entry has been stable for several sessions,
  suggesting it should be promoted to durable documentation and removed from the hot list.

This Skill does **NOT** trigger for:

- Pure refactors that preserve behavior with no observable changes for downstream consumers.
- Planning documents (those belong in the plan artifact, produced during the P phase).
- Pre-implementation verification (run the Skill on the post-implement diff, not the intent).
- Source code edits — this Skill never touches `src/`, `lib/`, or runtime code.

## Tool contract

This Skill is tool-using. The feedback loop is:

```
git diff (read) → file reads (doc + code comparison) → doc edits (Write/Edit) → structured report
```

| Tool | Schema | Feedback path |
|---|---|---|
| `Bash` — `git diff <base>...HEAD --name-only` | shell command; output = newline-separated paths | changed-files list feeds the verification loop; if output is empty, Skill exits with "no changes to verify" |
| `Bash` — `git diff <base>...HEAD -- <path>` | shell command; output = unified diff text | diff text fed to comparison step for each matched slice; identifies added/removed/changed lines |
| `Read` — source files | file path → file content | compared against doc slice content to determine if slice is accurate, needs update, or needs creation |
| `Read` / `Grep` — doc slices | file path / regex → matches | identifies which slices match changed paths via ownership convention; retrieves current doc content |
| `Edit` / `Write` — doc slices | file path + edit delta → acknowledgement | applies updates, bumps `version`, refreshes `last-verified`; failure surfaces as typed error, blocks run |
| `Grep` — ownership map | regex over doc_root | resolves which doc owns each changed source file; used when MAP.md or slice frontmatter carries ownership globs |

**Failure signal handling:** if a `git diff` command fails (non-zero exit), the Skill surfaces the error as a typed report entry and halts — it does not proceed on a partial diff. If a `Read` on a doc file fails (file missing), the Skill treats the area as undocumented and proceeds to the creation path. If an `Edit`/`Write` fails, the Skill reports the failure and does not silently continue.

## The verification loop (four passes)

### Pass 1 — Diff intake

```bash
git diff <diff_base>...HEAD --name-only
```

Collect the full list of changed files. Determine the `diff_base` from context (default: main/master;
use the branch's divergence point on feature branches). If the diff is empty, exit cleanly:
"no changes to verify — loop is already closed."

### Pass 2 — Ownership resolution

For each changed file, resolve which doc section(s) own it:

1. Check `doc_root/MAP.md` (or equivalent index) for a row mapping the file path or its parent
   directory to a doc slice.
2. Check `doc_root/**/*.md` frontmatter for `owners-paths:` globs that match the file.
3. If neither matches: the area is undocumented → proceed to Pass 4 (create).
4. If multiple slices match: update all of them (shared ownership is explicit, not implicit).

The `ownership_convention` slot parameterizes how resolution works. Three canonical strategies:

- `slice-frontmatter-owners-paths` — each doc slice carries an `owners-paths:` frontmatter list of
  globs; grep the frontmatter to find matches.
- `map-table` — a central `MAP.md` or `OVERVIEW.md` table maps source directories to doc sections;
  grep the table.
- `mirror-hierarchy` — documentation directory structure mirrors source structure; a changed file
  at `src/auth/login.js` maps to `docs/auth/login.md`.

### Pass 3 — Slice accuracy check and update

For each matched slice:

1. Read the slice and the changed source file(s) it covers.
2. Produce a plain-language accuracy verdict:
   - **Still accurate** — update `last-verified: <today>` in frontmatter only. Do NOT bump `version`.
   - **Needs section update** — edit the affected section with accurate content. Bump `version`.
     Update `last-verified`.
   - **Fundamentally wrong** — rewrite the affected section. Bump `version`. Update `last-verified`.
     Flag in report.

**Discipline:** update only what the diff requires. Do not rewrite unrelated sections. Do not
speculate about behavior the diff does not touch.

### Pass 4 — Slice creation for undocumented areas

If a changed area has no owner slice:

1. Assess whether the area warrants documentation: "would a future LLM working on this area benefit
   from knowing this exists?" The bar is architectural or behavioral significance — a one-line bug
   fix typically does not need a slice.
2. If yes: create a new slice in `doc_root/` following the project's slice convention
   (`ownership_convention`). At minimum: frontmatter (slice name, `owners-paths:`, `last-verified:`,
   `version: 1`), one-paragraph summary, key behavioral invariants.
3. Add a row to `MAP.md` (or equivalent index) so future passes resolve this area.
4. Report the new slice in the output.

### Pass 5 — Graduate or prune pass (documentation-layer CASE loop)

For each slice touched in this run, assess whether any content has stabilized enough to graduate:

- **Graduate to durable docs**: a section that has not changed across N consecutive verified passes,
  describes a stable architectural invariant, and is referenced by multiple slices — move it to
  authoritative documentation (e.g. `CLAUDE.md`, a top-level README section, an ADR). Remove the
  ephemeral tracking entry. Flag in report.
- **Prune on staleness**: a section that describes a behavior or module deleted in this diff — mark
  for deletion. Do not delete silently; flag in report and let the operator confirm.
- **Keep current**: section is accurate and active; `last-verified` refresh is sufficient.

This pass is the documentation-layer analog of the gotchas loop's `graduate_or_prune_policy`:
documentation earns its keep by remaining accurate and proportionate, not by accumulating forever.

## Disambiguation: this Skill vs. writing documentation during Planning

**Verify & Doc does not replace planning documentation.** The two phases are complementary:

| Phase | Artifact | What it captures |
|---|---|---|
| **Plan (P)** | `docs/plans/<feature>.md` | Intent — what we intend to build and why |
| **Implement (I)** | Source code | Behavior — what was actually built |
| **Verify & Doc (V)** ← this Skill | Doc slices, MAP.md | Ground truth — what the code actually does, verified post-implementation |

A plan doc describes intent; a verified slice describes what is. They are both needed. Running this
Skill before implementation closes a loop on stale intent, not on implemented behavior. The Skill
MUST run after implementation, on the real diff, not on a planned diff.

## Output format

```markdown
## Verify & Document run

**Diff base:** <base>..HEAD
**Files changed:** N
**Triggered by:** post-implement | post-commit | on-demand

### Slices touched
- `<slice-path>` — <updated section X / bumped version to N / last-verified only>

### Slices created
- `<slice-path>` — covers <topic>. Added row to MAP.md.

### Graduated / pruned
- `<entry>` graduated to `<destination>` — <one-line rationale>
- `<entry>` flagged for pruning — <one-line rationale; awaits operator confirmation>

### Sanity check findings (informational — do not block)
- Conventional commit format: pass / warn
- console.log / debug statements in diff: <count or "none">
- Markdown files added outside doc_root: <list or "none">

### Recommendations
- <optional next steps>
```

## Hard rules

- **Never edit source code.** This Skill writes ONLY to `doc_root` and to root-level convention
  files (`CLAUDE.md`, `README.md` at project root). Any path under `src/`, `lib/`, `app/`, or
  equivalent runtime trees is out of bounds.
- **Don't invent behavior.** If a slice says X and the code shows X, leave it (update `last-verified`
  only). Only update what the diff requires.
- **Don't create a slice for trivial changes.** A one-line bug fix does not need a slice unless it
  fixes a semantically significant behavioral edge case.
- **Don't delete slices.** If a slice's area was deleted from the code, flag in the report and let
  the operator confirm. Silent deletion of documentation is the `write-only-retrospective` failure
  applied to the removal path.
- **`last-verified` must be today, never a future date.**

## Anti-patterns

| Anti-pattern | What it looks like | Why it fails |
|---|---|---|
| `write-only-doc-update` | Updating docs without first reading and comparing the actual code diff; writing what you think the code does, not what it does | Produces plausible-but-wrong documentation; a future session will be misled by it; the verify step exists precisely to catch the delta between intent and reality |
| `implement-without-verify` | Merging code changes without running this Skill; treating documentation as optional | Leaves the next session's context window polluted with stale facts; `last-verified` dates become meaningless; the RPI loop degrades into RI with no feedback |
| `doc-for-every-line-change` | Creating new slices or bumping versions for every trivial diff (typos, comments, minor renames) | Inflates the documentation tree with low-signal entries; documentation-maintenance cost crowds out code work; this Skill should run on behavioral changes, not mechanical ones |
| `pre-emptive-doc-update` | Updating documentation before the code is written, then not updating it again after implementation | Produces intent-as-fact conflation; plan docs and verified docs are different artifacts with different epistemic status; this Skill runs POST-implement on the real diff, not on intended behavior |
| `over-broad-graduation` | Graduating gotchas or doc entries to "durable" status after one pass without stability evidence | Premature graduation pollutes authoritative docs with content that may still change; graduation requires N consecutive verified passes with no behavior change, not a single "looks stable" call |

## CASE cycle reference (documentation-sync instantiation)

The loop follows the **CASE** framing (REF-0193) adapted to documentation sync:

```
Collect  → git diff captures what changed in the implementation pass
Analyze  → ownership resolution identifies which doc sections are affected
Synthesize → accuracy verdict produces minimal-sufficient doc updates (section update, creation, or last-verified bump only)
Enforce  → updated slices are written and graduate-or-prune pass runs to keep the doc layer lean
```

Each transition is gated: Collect → Analyze requires a non-empty diff. Analyze → Synthesize requires
reading the actual current code (not from memory). Synthesize → Enforce requires the accuracy verdict
to be grounded in the diff, not speculated.

## Composition notes

- **With `curated-gotchas-retrospective-loop`:** the Gotchas loop's `graduate_or_prune_policy` can
  hand stable gotcha entries to this Skill's Pass 5 (graduation). A gotcha that has become a settled
  invariant graduates out of the hot list INTO a doc slice or `CLAUDE.md` section — this Skill
  completes that graduation with a verified write.
- **With `just-in-time-context-loading`:** the `retrieve-relevant-entries-by-task` strategy in the
  gotchas loop is the same grep-by-ownership-convention that this Skill uses for Pass 2. The two
  Skills share the same JIT-loading discipline applied to different artifact types (gotchas vs.
  doc slices).
- **With `observable-agent-traces`:** the git diff IS the implement cycle's observable trace for
  this Skill. The Skill's capture phase reads the diff; without a reviewable diff there is nothing
  to verify against.

## Cross-references

- Framework source: [`framework/techniques/curated-gotchas-retrospective-loop.md`](../../../framework/techniques/curated-gotchas-retrospective-loop.md) — the source Technique pattern this Skill realizes in the documentation-sync domain.
- Reference implementation: [`knowledge/zoetis-skills/gerar-web/.claude/agents/verify-and-document.md`](../../../knowledge/zoetis-skills/gerar-web/.claude/agents/verify-and-document.md) — the domain-specific original from which this portable Skill was extracted.
- Reference command: [`knowledge/zoetis-skills/gerar-web/.claude/commands/verify-and-doc.md`](../../../knowledge/zoetis-skills/gerar-web/.claude/commands/verify-and-doc.md) — the R-P-I-V chain command that dispatches this Skill's equivalent in the gerar-web context.
- Evidence: REF-0193 (capture→graduate loop shape), REF-0022 (externalization substrate), REF-0020 (cross-session improvement via durable external artifacts), REF-0002 (golden-principles graduate destination).
- Sibling Skill: [`generation/skills/curated-gotchas-retrospective-loop/`](../curated-gotchas-retrospective-loop/) — the gotchas-maintenance loop; this Skill handles documentation, that Skill handles the landmine list; both share the same CASE loop shape.
- Constants honored: `typed-tool-contracts-with-feedback-loop` (git-diff feedback loop grounds every doc decision; tool failures surface as typed errors); `observable-agent-traces` (the git diff is the implement cycle's trace; without it there is nothing to verify); `minimal-sufficient-context` (Pass 3 updates only what the diff requires; Pass 5 keeps the doc layer lean via graduation and pruning).
