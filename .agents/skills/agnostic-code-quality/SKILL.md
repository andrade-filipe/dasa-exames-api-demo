---
name: agnostic-code-quality
description: >
  Apply KISS, YAGNI, and SOLID code quality principles to reduce complexity 
  and prevent over-engineering. Provides language-agnostic guidance 
  with examples in Python, Rust, and TypeScript. Use when refactoring, 
  reviewing code, or establishing architectural standards.
license: MIT
source_author: Gemini CLI
source_url: https://github.com/google-gemini/gemini-cli
---

> **Attribution:** Gemini CLI, MIT license.

# Code Quality Principles

Guidance on KISS, YAGNI, and SOLID principles with language-agnostic application.

## When To Use

- Improving code readability and maintainability
- Applying SOLID, KISS, YAGNI principles during refactoring
- Establishing team-wide coding standards
- Evaluating architectural decisions

## Core Principles

### KISS (Keep It Simple, Stupid)
**Principle**: Avoid unnecessary complexity. Prefer obvious solutions over clever ones.
- **Prefer**: Simple conditionals, explicit code, standard patterns, direct solutions.
- **Avoid**: Complex regex for simple checks, magic numbers, clever shortcuts, over-abstracted layers.

### YAGNI (You Aren't Gonna Need It)
**Principle**: Don't implement features until they are actually needed.
- **Do**: Solve current problems, add abstractions only when the 3rd use case appears, delete dead code.
- **Don't**: Build for hypothetical futures, create abstractions for 1 use case, keep "just in case" code.

### SOLID Principles

- **Single Responsibility Principle (SRP)**: Each module/class should have one reason to change.
- **Open/Closed Principle (OCP)**: Open for extension, closed for modification.
- **Liskov Substitution Principle (LSP)**: Subtypes must be substitutable for their base types.
- **Interface Segregation Principle (ISP)**: Clients shouldn't depend on interfaces they don't use.
- **Dependency Inversion Principle (DIP)**: Depend on abstractions, not concretions.

## Quick Reference

| Principle | Question to Ask | Red Flag |
|-----------|-----------------|----------|
| **KISS** | "Is there a simpler way?" | Complex solution for simple problem |
| **YAGNI** | "Do I need this right now?" | Building for hypothetical use cases |
| **SRP** | "What's the one reason to change?" | Class doing multiple jobs |
| **OCP** | "Can I extend without modifying?" | Switch statements for types |
| **LSP** | "Can subtypes replace base types?" | Overridden methods with side effects |
| **ISP** | "Does client need all methods?" | Empty method implementations |
| **DIP** | "Am I depending on abstractions?" | `new` keyword in business logic |

## Limitations

- **Principles are not laws.** Blindly following SOLID can lead to over-abstraction (violating KISS/YAGNI). Senior judgment is required to balance them.
- **Context matters.** What is "simple" depends on the team's expertise and the domain's inherent complexity.
- **Premature abstraction is a common trap.** It is often better to duplicate code twice than to create the wrong abstraction prematurely.

---

> Provenance + framework classification: see `composition.yaml` (sidecar).
> Compliance badges: see `badges-draft.yaml` (architect sign-off pending).
