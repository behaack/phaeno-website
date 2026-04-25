# AGENTS.md

## Decision order

- Prefer existing project patterns over new abstractions.
- Prefer small, focused diffs over broad cleanup.
- If a task crosses app boundaries or changes architecture, propose a short plan first.
- If instructions conflict, preserve current behavior and ask before making broader changes.
- In the UI projects, prefer smaller components over large monolithic components.
- Prefer one component per file unless the component is small and single-purpose to its parent file.

## Working rules
- Before changing architecture, propose a short plan
- After auth changes, verify sign-in, refresh, and logout flows
- Do not add dependencies unless necessary
- Prefer existing project patterns over introducing new abstractions
- Explain root cause briefly before large refactors
- After schema changes, do not modify migrations but prompt me to add a migration
- Do not stage or commit git changes
## Stop and ask first

- Architecture or folder structure changes
- New dependencies or replacing core libraries
- Auth, permissions, or session lifecycle changes
- Database schema or API contract changes that affect multiple apps
