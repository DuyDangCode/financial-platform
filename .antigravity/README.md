# Antigravity Agent Workflow & Customization System

This directory (`.antigravity/`) configures the specialized multi-agent workflow and domain skills for **FinancialPlatform**. It replicates and adapts the workflow originally designed in `.opencode/` for Google Antigravity (Antigravity CLI, IDE, and 2.0).

---

## 1. Architecture Overview

The system establishes two primary development tracks: **Backend** (.NET 8 Clean Architecture) and **Frontend** (Next.js 16 + React 19 + Tailwind v4).

Each track follows a 3-tier separation of duties:
1. **Orchestrator** (`backend-dev` / `frontend-dev`): Manages the high-level roadmap, registry, and quality loops. Never writes code directly.
2. **Builder Subagent** (`backend-builder` / `frontend-builder`): Executes design-first implementation, builds vertical slices, writes tests, and ensures green builds.
3. **Reviewer Subagent** (`backend-reviewer` / `frontend-reviewer`): Read-only critic evaluating diffs across 5 dimensions, running up to 5 review/fix rounds.

```
                    ┌─────────────────────────┐
                    │      User Request       │
                    └───────────┬─────────────┘
                                │
                 ┌──────────────┴──────────────┐
                 ▼                             ▼
        [ backend-dev ]                [ frontend-dev ]
         (Orchestrator)                 (Orchestrator)
                 │                             │
    ┌────────────┼────────────┐   ┌────────────┼────────────┐
    │                         │   │                         │
    ▼                         ▼   ▼                         ▼
[ backend-builder ]           │ [ frontend-builder ]        │
  1. api-design               │   1. ui-design              │
  2. backend-build            │   2. frontend-build         │
  3. test-design              │   3. lint & dev verify      │
    │                         │   │                         │
    └────────────┬────────────┘   └────────────┬────────────┘
                 ▼                             ▼
       [ backend-reviewer ]          [ frontend-reviewer ]
         - Logic                       - Design Conformance
         - Security                    - Next 16 Correctness
         - Performance                 - Tailwind v4 Styling
         - Code Style                  - Quality / Lint Clean
         - DDD Compliance              - A11y & UX
                 │                             │
           (Loop <= 5)                   (Loop <= 5)
                 │                             │
                 ▼                             ▼
        [ document-write ]            [ document-write ]
         (docs/ update)                (pages.md + docs/)
```

---

## 2. Directory Structure

```text
.antigravity/
├── agents/
│   ├── backend-dev.md           # Backend orchestrator
│   ├── backend-builder.md       # Backend builder (design -> build -> tests)
│   ├── backend-reviewer.md      # Read-only backend reviewer (5 dimensions)
│   ├── frontend-dev.md          # Frontend orchestrator & pages registry owner
│   ├── frontend-builder.md      # Frontend builder (UI spec -> Next.js 16 -> verify)
│   └── frontend-reviewer.md     # Read-only frontend reviewer
├── skills/
│   ├── api-design/SKILL.md      # REST endpoint specification in docs/05-api/
│   ├── backend-build/SKILL.md   # .NET 8 clean architecture implementation
│   ├── backend-review/SKILL.md  # 5-dimension code review guidelines
│   ├── db-design/SKILL.md       # EF Core PostgreSQL persistence & migrations
│   ├── document-write/SKILL.md  # General project docs (README, changelog, architecture)
│   ├── frontend-build/SKILL.md  # Next.js 16 & Tailwind v4 page implementation
│   ├── frontend-review/SKILL.md # Frontend code review checklist
│   ├── test-design/SKILL.md     # xUnit unit test design & fake repositories
│   └── ui-design/SKILL.md       # Markdown UI wireframing & component specs
└── rules/
    └── workflow-orchestration.md # Strict role boundaries & review gate rules
```

---

## 3. Agents Reference

| Agent | Type / Role | Tools | Primary Responsibilities |
| :--- | :--- | :--- | :--- |
| `backend-dev` | Orchestrator | Read, Write, Subagent | Scopes backend requirements, delegates build to `backend-builder`, manages review loop with `backend-reviewer`, runs `document-write`. |
| `backend-builder` | Subagent | Read, Write | Drives `api-design` → `backend-build` → `test-design`. Ensures unit tests pass before reporting. |
| `backend-reviewer` | Subagent | Read-only | Evaluates diffs on Logic, Security, Performance, Code Style, and DDD. Verdict: approve / request changes. |
| `frontend-dev` | Orchestrator | Read, Write, Subagent | Owns `docs/10-frontend/pages.md`, coordinates design+build with `frontend-builder`, loops review with `frontend-reviewer`. |
| `frontend-builder` | Subagent | Read, Write | Drives `ui-design` → `frontend-build`. Verifies with `npm run lint` and manual dev inspection. |
| `frontend-reviewer` | Subagent | Read-only | Reviews Next.js 16 API usage, Tailwind v4 styling, hydration safety, accessibility, and design conformance. |

---

## 4. Skills Reference

| Skill | Description | Location |
| :--- | :--- | :--- |
| `api-design` | Design REST endpoints in `docs/05-api/<module>.md` and update `docs/05-api/api-design.md`. | [api-design/SKILL.md](file:///home/thanhduy/Projects/FinancialPlatform/.antigravity/skills/api-design/SKILL.md) |
| `backend-build` | Implement clean architecture vertical slices (Domain → Application → Infrastructure → Api). | [backend-build/SKILL.md](file:///home/thanhduy/Projects/FinancialPlatform/.antigravity/skills/backend-build/SKILL.md) |
| `backend-review` | Multi-dimension review for logic, security, performance, clean code, and DDD. | [backend-review/SKILL.md](file:///home/thanhduy/Projects/FinancialPlatform/.antigravity/skills/backend-review/SKILL.md) |
| `db-design` | EF Core entity configurations, migrations, and repository implementations. | [db-design/SKILL.md](file:///home/thanhduy/Projects/FinancialPlatform/.antigravity/skills/db-design/SKILL.md) |
| `document-write` | Update changelog, user stories, architecture ADRs, and guides. | [document-write/SKILL.md](file:///home/thanhduy/Projects/FinancialPlatform/.antigravity/skills/document-write/SKILL.md) |
| `frontend-build` | Build Next.js 16 / React 19 pages with Tailwind v4 in `frontend/financial-platform-web`. | [frontend-build/SKILL.md](file:///home/thanhduy/Projects/FinancialPlatform/.antigravity/skills/frontend-build/SKILL.md) |
| `frontend-review` | Check Next 16 breaking changes, hydration safety, a11y, and lint. | [frontend-review/SKILL.md](file:///home/thanhduy/Projects/FinancialPlatform/.antigravity/skills/frontend-review/SKILL.md) |
| `test-design` | xUnit test design with hand-written fakes (`AuthFakes.cs`) in UnitTests. | [test-design/SKILL.md](file:///home/thanhduy/Projects/FinancialPlatform/.antigravity/skills/test-design/SKILL.md) |
| `ui-design` | Markdown UI specifications, ASCII wireframes, tokens, and responsive behavior. | [ui-design/SKILL.md](file:///home/thanhduy/Projects/FinancialPlatform/.antigravity/skills/ui-design/SKILL.md) |

---

## 5. How to Invoke in Antigravity

- **In Chat**: Mention `@backend-dev` or `@frontend-dev` to start an end-to-end orchestrated workflow.
- **Via Subagents**: Antigravity agents can invoke them using the `invoke_subagent` tool with `TypeName: "backend-dev"` or any of the subagent names.
- **Skills Activation**: Antigravity automatically progressively discloses skills based on the task description, or you can invoke them directly in context.
