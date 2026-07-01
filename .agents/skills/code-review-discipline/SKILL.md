---
name: code-review-discipline
description: >
  Master effective code review practices to provide constructive feedback, 
  catch bugs early, and foster knowledge sharing while maintaining team 
  morale. Use when reviewing pull requests, establishing review standards, 
  or mentoring developers. Emphasizes specific, actionable feedback and 
  differentiates between critical and non-blocking issues.
license: MIT
source_author: Gemini CLI
source_url: https://github.com/google-gemini/gemini-cli
---

> **Attribution:** Gemini CLI, MIT license.

# Code Review Discipline

Transform code reviews from gatekeeping to knowledge sharing through constructive feedback, systematic analysis, and collaborative improvement.

## When to Use This Skill

- Reviewing pull requests and code changes
- Establishing code review standards for teams
- Mentoring junior developers through reviews
- Conducting architecture reviews
- Creating review checklists and guidelines
- Improving team collaboration
- Maintaining code quality standards

## Core Principles

### 1. The Review Mindset

**Goals of Code Review:**
- Catch bugs and edge cases
- Ensure code maintainability
- Share knowledge across team
- Enforce coding standards
- Improve design and architecture

**Not the Goals:**
- Show off knowledge
- Nitpick formatting (use linters)
- Block progress unnecessarily
- Rewrite to your preference

### 2. Effective Feedback

**Good Feedback is:**
- Specific and actionable
- Educational, not judgmental
- Focused on the code, not the person
- Balanced (praise good work too)
- Prioritized (critical vs nice-to-have)

### 3. Review Scope

**What to Review:**
- Logic correctness and edge cases
- Security vulnerabilities
- Performance implications
- Test coverage and quality
- Error handling
- API design and naming

**What Not to Review Manually:**
- Code formatting (use Prettier, Black, etc.)
- Import organization
- Linting violations

## Review Process

### Phase 1: Context Gathering
1. Read PR description and linked issue
2. Check PR size
3. Review CI/CD status
4. Understand the business requirement

### Phase 2: High-Level Review
1. **Architecture & Design**: Does it fit? Is it consistent?
2. **File Organization**: Is code grouped logically?
3. **Testing Strategy**: Are there tests covering edge cases?

### Phase 3: Line-by-Line Review
1. **Logic & Correctness**: Handles edge cases? Race conditions?
2. **Security**: Input validation? Sensitive data exposure?
3. **Performance**: N+1 queries? Unnecessary loops?
4. **Maintainability**: Clear naming? Single responsibility?

### Phase 4: Summary & Decision
1. Summarize key concerns
2. Highlight strengths
3. Make clear decision: Approve, Comment, or Request Changes

## Best Practices

- **Use Collaborative Language**: Ask "What happens if...?" instead of "This will fail."
- **Differentiate Severity**: Use labels like `[blocking]`, `[important]`, `[nit]`, `[suggestion]`.
- **Suggest, Don't Command**: Provide alternatives and explain why.
- **Review Promptly**: Aim for same-day feedback to maintain momentum.

## Limitations

- **Social friction.** Code review is a social activity. Technical correctness can be overshadowed by poor communication style.
- **Review fatigue.** Reviewing large PRs (>400 lines) significantly reduces the quality of the review.
- **Automation gap.** Reviewers often waste time on things that should be handled by linters or automated tests.

---

> Provenance + framework classification: see `composition.yaml` (sidecar).
> Compliance badges: see `badges-draft.yaml` (architect sign-off pending).
