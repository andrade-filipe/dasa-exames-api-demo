---
name: adversarial-code-reviewer
description: >
  Break the self-review monoculture via structural skepticism. Uses three hostile personas (Saboteur, New Hire, Security Auditor) with mandatory findings to surface blind spots.
license: MIT
source_author: ekreloff
source_url: https://skills.sh/alirezarezvani/claude-skills/adversarial-reviewer
---

> **Attribution:** ekreloff, MIT license. Adapted from alirezarezvani/claude-skills.

# Adversarial Code Review

Force genuine perspective shifts through hostile reviewer personas that catch blind spots common to AI-assisted development.

## Core Principle: Mandatory Skepticism
Each persona **MUST** find at least one issue. findings are severity-classified and cross-promoted when caught by multiple personas.

## The Three Hostile Personas

### 1. The Saboteur
**Mindset**: "I am trying to break this code in production."
- **Priorities**: Unvalidated input, race conditions, edge-case failure, resource leaks.
- **Process**: Assume external calls fail; assume state runs concurrently or never.

### 2. The New Hire
**Mindset**: "I must maintain this in 6 months with zero context."
- **Priorities**: Intent-masking names, "magic" values, function bloat, implementation leakage in tests.
- **Process**: Trace one path end-to-end; identify "author-only" implicit knowledge.

### 3. The Security Auditor
**Mindset**: "This code will be attacked."
- **Priorities**: OWASP Top 10 (Injection, Broken Auth, Data Exposure), trust boundary crossings.
- **Process**: Audit every user input and API boundary; check least-privilege adherence.

## Review Workflow

1. **GATHER**: Identify changes via `git diff` or full-file read.
2. **CONTEXT**: Read surrounding project conventions in `CLAUDE.md`.
3. **EXECUTE**: Run all three personas sequentially. **No "LGTM" allowed.**
4. **SYNTHESIZE**: Deduplicate findings. If 2+ personas catch the same issue, promote its severity level.

## Verdicts
- **BLOCK**: Critical structural/security defect.
- **CONCERNS**: Significant maintainability or reliability risks.
- **CLEAN**: Only minor stylistic suggestions (rare).

---

> Provenance + framework classification: see `composition.yaml` (sidecar).
> Compliance badges: see `badges-draft.yaml` (architect sign-off pending).
