# DASA Dev Toolbox — Orchestration & Install Guide

This is the wiring behind the pack: how the three phases hand off to each other, where each
skill plugs in, and how to install everything into Codex CLI.

## How the phases hand off (filesystem, not chat)

The pipeline coordinates through **two files on disk**, not through one long conversation:

```
Research  --writes-->  RESEARCH.md
                          |
                     (context reset)
                          |
Plan      --reads RESEARCH.md, writes-->  PLAN.md
                          |
                     (context reset)
                          |
Implement --reads PLAN.md, makes the change + tests
```

- The **researcher** writes `RESEARCH.md` and ends its session.
- The **planner** starts a fresh session, reads `RESEARCH.md`, writes `PLAN.md`, ends.
- The **implementer** starts a fresh session, reads `PLAN.md`, and does the work.

### The context reset is mandatory

Between each phase you start a **new session** so the agent's working memory is cleared. Each
phase then loads only the previous phase's output file. This is the whole point of the toolbox:
it keeps the agent sharp instead of letting a single bloated session drift into ignoring your
instructions and hallucinating about the code. Do not run all three phases in one continuous
chat — that defeats the design.

Because state lives in `RESEARCH.md` and `PLAN.md`, you can stop after any phase and resume
later: just start the next phase and point it at the last file that was written.

## Where each skill attaches

| Skill | Attaches at | Mode |
|---|---|---|
| `dev-grill-me` | Before Research (sharpen a vague task first) | AUTO |
| `context-hygiene` | Research (load only what's relevant, just-in-time) | AUTO |
| `dotnet-xunit-testing` | Implement (write/update xUnit tests) | AUTO |
| `agnostic-code-quality` | Implement (self-check quality bar) | AUTO |
| `code-review-discipline` | Implement (self-review before done) | AUTO |
| `verify-and-doc` | Implement (keep docs synced to the diff; verify it works) | AUTO |
| `adversarial-code-reviewer` | Before merge (hostile-persona review) | MANUAL |
| `curated-gotchas-retrospective-loop` | After task (capture landmines) | AUTO |
| `dev-self-improvement` | After task, and on demand | AUTO + MANUAL |

**AUTO** skills fire on their own when the situation matches (a vague request, entering a phase,
finishing a task). **MANUAL** skills you invoke deliberately: `adversarial-code-reviewer` is the
pre-merge hostile review meant for senior devs who want their change stress-tested;
`dev-self-improvement` also runs on demand whenever you want a retro, in addition to firing
automatically after non-trivial tasks.

## Install into Codex CLI

The toolbox is a set of skill folders plus three agent files. Copy them into place:

1. **Skills** — copy each skill folder into your Codex skills directory `.agents/skills/`:

   In-node (this DASA project):
   - `dev-grill-me`  — from `catalog/projects/dasa/skills/dev-grill-me/`
   - `dev-self-improvement` — from `catalog/projects/dasa/skills/dev-self-improvement/`
   - `dotnet-xunit-testing` — from `catalog/projects/dasa/skills/dotnet-xunit-testing/`

   Reused from the shared catalog:
   - `agnostic-code-quality` — from `catalog/areas/development/craft-principles/skills/agnostic-code-quality/`
   - `code-review-discipline` — from `catalog/areas/development/code-review/skills/code-review-discipline/`
   - `adversarial-code-reviewer` — from `catalog/areas/development/code-review/skills/adversarial-code-reviewer/`
   - `context-hygiene` — from `catalog/universal/skills/context-hygiene/`
   - `verify-and-doc` — from `catalog/areas/development/dev-workflow/skills/verify-and-doc/`
   - `curated-gotchas-retrospective-loop` — from `catalog/universal/skills/curated-gotchas-retrospective-loop/`

2. **Agents** — copy the three agent files into your Codex agents location:
   - `researcher-agent.md` — from this pack's `agents/researcher-agent.md`
   - `planner-agent.md` — from this pack's `agents/planner-agent.md`
   - `implementer-agent.md` — from this pack's `agents/implementer-agent.md`

3. **Team memory** — make sure your repo has an `AGENTS.md` at its root. Codex reads it
   automatically as durable team memory. This is where the retro and gotchas skills propose
   additions over time (see below). It is `AGENTS.md`, not any other memory file.

After copying, the AUTO skills will engage on their own during the matching phase; the two
MANUAL entries you call when you want them.

## How the toolbox learns your stack (AGENTS.md)

Your exact DASA sub-stack is not baked into these skills on purpose. Instead, the toolbox adapts:

- After a task, `curated-gotchas-retrospective-loop` captures the landmines you hit.
- `dev-self-improvement` runs a retro and **proposes** improvements — new gotchas, sharper
  conventions, tweaks to a skill — targeted at `AGENTS.md`.
- **A human approves** those proposals before anything is written to `AGENTS.md`. Nothing edits
  your durable team memory silently.

Over many tasks, `AGENTS.md` accumulates your real conventions and traps, and every future run
starts already knowing them.

## Optional: a 4th Verify phase

The default is the lean **RPI** flow (Research -> Plan -> Implement). When your team wants a
stronger gate, you can add a **Verify** phase after Implement: a dedicated review/test-gate pass,
for example `adversarial-code-reviewer` plus `dotnet-xunit-testing` run as their own step before
merge. It ships off by default — add it when the need is real, and keep the base flow lean until
then.
