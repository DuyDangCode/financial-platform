---
name: ui-design
description: Write a UI design document in markdown describing what a page/screen looks like — layout, components, colors, typography, states, responsive behavior. Use when asked to design a UI page, create a UI/mockup/wireframe spec, or describe how a screen should look. Does NOT write frontend code.
---

# UI Design

Produce a **markdown design document** describing the UI for a page or screen. This is a specification step only — do not write any application code.

## Output

- One file per screen: `docs/10-frontend/<page-or-feature>.md` (kebab-case, e.g. `login.md`, `portfolio-overview.md`). Create the folder if missing.
- Before designing, ground the design in real requirements: skim `docs/01-product/user-stories.md`, `docs/01-product/scope.md`, `docs/02-domain/*`, and any matching API spec in `docs/05-api/`. Reference them in the doc.
- Keep the app's visual language consistent with already-designed screens in `docs/10-frontend/` if any exist.

## Required document structure

```markdown
# <Screen Name>

## Purpose
What the screen is for; which user story/module it serves.

## Layout
ASCII wireframe + zones (header, sidebar, main content, footer).
Desktop-first sketch, then note mobile rearrangement.

## Components
Table or list: component | content/data shown | behavior (clickable? collapsible?).

## Visual Style
Colors (name roles: primary, surface, success, danger...), typography
(headings/body/numbers), spacing/density, iconography.
Follow financial conventions: right-aligned tabular numbers,
explicit currency formatting, gains green / losses red PLUS arrow icons.

## States
For every data view: loading skeleton, empty state, error state,
and interactive states (hover/focus/disabled). Auth screens: validation
and server-error presentation.

## Responsive Behavior
Breakpoint-by-breakpoint changes (mobile / tablet / desktop).

## Interactions & Flows
Navigation entry/exit points, what each action does, confirmations/modals.

## Data & API Mapping
Fields displayed → backing endpoint/response field (from docs/05-api),
or "TBD" if backend doesn't exist yet.

## Accessibility Notes
Focus order, labels, contrast, keyboard shortcuts if any.
```

## Style rules

1. Concrete over vague: "48px tall primary button, brand blue #2563EB, white label" beats "a nice button".
2. Never encode meaning by color alone — pair color with icons/text.
3. Use hex values or named tokens, but keep a single palette consistent across all screen docs.
4. Mark unknowns as `TBD` rather than inventing product decisions silently.

## Verification

Re-read the finished doc: every section above present, wireframe renders correctly in plain markdown (code block), and referenced user stories/endpoints actually exist in `docs/`.
