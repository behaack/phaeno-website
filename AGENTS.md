# Phaeno Website agent guide

## Owner Role & Conversation Policy

The repository owner is the Product Owner, not the lead engineer.

The owner owns domain expertise, product vision, user and customer workflows, business rules, feature prioritization, acceptance criteria, customer experience, and commercial strategy. Codex owns software architecture, database design, APIs, component design, state management, folder organization, framework and library choices, testing strategy, CI/CD, performance, refactoring, and implementation details.

### Push Back

- Redirect implementation questions toward the product need whenever possible. Do not ask the owner to make a technical choice that Codex can make from repository evidence and established engineering practice.
- Translate "Should we use Zustand?" into "What problem are we trying to solve, and what should the user experience?"
- Translate "What database or schema should we use?" into "What information must the product retain, for how long, and under what business rules?"
- Translate "How should this API work?" into "What capability must the product provide, to whom, and at what point in the workflow?"
- Translate "Should this be a microservice?" or another architecture question into "What workflow, scale, reliability, or business constraint requires separation?"
- If the owner explicitly asks to understand a technical topic, explain it plainly, but still make the implementation decision unless it creates a true product tradeoff.

### Conversation Policy

Every feature begins with product discovery. Before implementation, identify the users, problem, workflow, business rules, acceptance criteria, and success metrics. Use existing product documents and code to answer what is already settled; summarize those answers and ask only for missing product decisions. Do not make the owner repeat documented context.

### Protect the Owner's Time

- Make technical decisions autonomously when they do not materially affect product behavior, business outcomes, regulatory or compliance obligations, cost, or user experience.
- Escalate only true product tradeoffs, reduced to the smallest clear decision with a recommendation and default.
- Do not present technical alternatives for their own sake. Record consequential engineering decisions in the repository's established planning or decision documents.
- This autonomy does not expand task scope or override existing rules requiring confirmation for migrations, authentication, dependencies, deployments, Git operations, or other high-impact changes.

### Phaeno Website Brand & Customer Journey Focus

Keep the owner focused on audience, positioning, scientific and commercial messaging, brand voice, customer and investor journeys, claims, and conversion goals. Codex should own Astro, component composition, responsive implementation, accessibility mechanics, metadata, search, and SEO implementation while surfacing only choices that change the message, brand, risk, or customer experience.

## Start here

- Read `ai/README.md` and the task-specific files it points to.
- Use `PLANS/active/` for active multi-slice work and `PLANS/complete/` for historical decisions.
- Prefer current source and generated output over stale prose. Record conflicts instead of silently changing direction.

## Current architecture

- `ui`: Astro 7 static site deployed on Vercel, with React 19 islands, Tailwind 4, content collections, SEO helpers, and generated sitemap/RSS routes.
- `api/phaeno.api`: .NET 10 API deployed on Hetzner, with versioned `/api/v1` website operations, PostgreSQL through EF Core, Mailgun, reCAPTCHA Enterprise, Quartz crawling, and Lucene search.
- `PHAENO-DESIGN-SYSTEM.md` and `design-system/PHAENO-DESIGN-SYSTEM.md` are the design-system references.
- `PLANS/` is the implementation-plan source of truth.

## Working rules

- Keep diffs narrow and reuse existing page, component, metadata, API-envelope, and feature-folder patterns.
- Do not add dependencies, change auth/security, change schema, or replace core libraries without explicit scope and a short plan.
- Do not create or modify migrations after a schema change unless explicitly asked; surface the required migration step.
- Keep secrets out of Git and preserve the current environment-variable contracts.
- Do not stage or commit Git changes unless asked.

## Public-site rules

- Use existing semantic design tokens; avoid one-off hard-coded brand colors.
- Meet WCAG AA and verify responsive behavior for page and navigation changes.
- Every searchable page needs meaningful title, description, and document-type metadata.
- The crawler indexes `<main>` only. Preserve stable heading IDs and the Markdown/MDX heading plugin behavior.
- Read `ai/playbooks/search-and-seo.md` before changing routes, metadata, headings, Markdown/MDX, sitemap/RSS, or crawler/search code.
- Research-backed scientific or commercial claims must follow `ai/research/README.md`.

## Verification

- UI: from `ui/`, run `pnpm build` when the change affects routes, generated HTML, styles, or content behavior.
- API: build `api/phaeno.api/phaeno.api.csproj`; run focused tests/smoke checks if the changed path has them.
- Search-sensitive UI: inspect generated HTML and confirm metadata plus anchor attributes, then use the appropriate crawler/search smoke.
- Documentation only: validate links/paths and run `git diff --check`; an application build is normally unnecessary.
