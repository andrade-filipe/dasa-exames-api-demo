---
tier: workflow
task_class: mixed
pack_name: dasa-dev-toolbox
member_agents:
  - researcher-agent
  - planner-agent
  - implementer-agent
member_skills:
  - dev-grill-me
  - dev-self-improvement
  - dotnet-xunit-testing
  - agnostic-code-quality
  - code-review-discipline
  - adversarial-code-reviewer
  - context-hygiene
  - verify-and-doc
  - curated-gotchas-retrospective-loop
orchestration_substrate: filesystem
---

# DASA Dev Toolbox

Your default, batteries-included way to do real engineering work with an AI coding agent
(Codex CLI) on DASA product code. If your prior AI experience was pasting code into a browser
chat, this is the upgrade: a disciplined pipeline that reads your actual repository, plans a
surgical change, implements it with tests, and gets better at your specific stack over time.

It is built around one core idea: **stop doing everything in a single long chat.** A long,
unbroken session slowly fills the agent's working memory with noise until it starts ignoring
your instructions and inventing facts about your code. This toolbox prevents that by splitting
work into three short, focused phases with a clean start between each one.

## The three phases (Research -> Plan -> Implement)

Each phase runs as its own fresh session. Between phases the agent's memory is fully reset, and
the next phase starts by loading only a single handoff file the previous phase wrote to disk.

1. **Research** — The agent reads the affected C#/.NET code and its dependencies and writes down
   what it found, with a real file path behind every claim. It does not plan or edit anything.
   Output file: `RESEARCH.md`.
2. **Plan** — A fresh session reads `RESEARCH.md` and produces a surgical, testable plan: which
   files to touch, the exact changes, and how the change will be tested. No code is edited.
   Output file: `PLAN.md`.
3. **Implement** — A fresh session reads `PLAN.md`, makes exactly those changes, writes/updates
   xUnit tests, keeps docs in sync with the diff, and self-checks quality before declaring done.

Why the reset matters: a fresh session for each phase keeps the agent sharp and focused only on
the current step's input file, instead of dragging along everything it read three steps ago.

## What handles state (so you can walk away)

State does not live in the chat. It lives in the two handoff files on disk (`RESEARCH.md`,
`PLAN.md`). If your laptop sleeps, the session drops, or you come back tomorrow, you resume by
starting the next phase and pointing it at the last file that was written. Nothing is lost.

## Member roster

### Agents (the three phases)

| Agent | Phase | Stops when |
|---|---|---|
| `researcher-agent` | Research | `RESEARCH.md` is complete and every finding cites a real file |
| `planner-agent` | Plan | `PLAN.md` is complete with steps, files, and a test strategy |
| `implementer-agent` | Implement | every plan step is done AND the tests pass |

### Skills (the judgment that plugs into the phases)

| Skill | Attaches at | Mode | What it does |
|---|---|---|---|
| `dev-grill-me` | Before Research | AUTO | Blocks a vague request and interviews you until the task is specific enough to act on |
| `context-hygiene` | Research | AUTO | Loads only what's relevant, just-in-time — stops context bloat at its source |
| `dotnet-xunit-testing` | Implement | AUTO | Writes/updates xUnit tests (Fact/Theory, AAA, deterministic — no clock/random/network) |
| `agnostic-code-quality` | Implement | AUTO | Stack-agnostic quality bar the implementer self-checks against |
| `code-review-discipline` | Implement | AUTO | Self-review discipline before declaring done |
| `verify-and-doc` | Implement | AUTO | Keeps docs in sync with the diff and verifies the change actually works |
| `curated-gotchas-retrospective-loop` | After task | AUTO | Captures the landmines you hit so the next task avoids them |
| `dev-self-improvement` | After task (and on demand) | AUTO + MANUAL | Runs a retro and proposes improvements to `AGENTS.md` / the skills |
| `adversarial-code-reviewer` | Before merge | MANUAL | Hostile-persona review for senior devs — you invoke it deliberately |

`AGENTS.md` is where durable team memory lives (Codex reads it automatically). The retro and
gotchas skills propose additions to it; a human approves before anything is written there.

## How to run it

1. Have a task. If it's vague ("fix the flaky check", "make this faster"), `dev-grill-me` fires
   first and sharpens it into something with named files and a success test.
2. Start the **Research** session with your sharpened task. Let it write `RESEARCH.md`. Read it.
3. Start a **fresh** session for **Plan**. It reads `RESEARCH.md` and writes `PLAN.md`. Read the
   plan — this is your approval checkpoint before any code changes.
4. Start a **fresh** session for **Implement**. It reads `PLAN.md`, makes the changes, adds
   tests, and stops when the plan is done and tests pass.
5. Before merging, optionally run `adversarial-code-reviewer` for a hostile review pass.
6. After the task, capture landmines and run the retro so the toolbox learns your stack.

See `glue/README.md` for the exact install steps and the phase-by-phase attach points.

## Optional: adding a Verify phase later

The default shipped here is the lean **RPI** (Research -> Plan -> Implement) flow. Once your
team is comfortable, you can add a 4th **Verify** phase — a dedicated review/test-gate pass
(for example, driven by `adversarial-code-reviewer` plus `dotnet-xunit-testing`) that runs after
Implement. It is not included by default on purpose: start lean, add it when you feel the need.
