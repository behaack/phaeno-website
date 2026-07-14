# Phaeno Website agent guide

## Start here

- Read `ai/README.md` and the task-specific files it points to.
- Use `PLANS/active/` for active multi-slice work and `PLANS/complete/` for historical decisions.
- Prefer current source and generated output over stale prose. Record conflicts instead of silently changing direction.

## Current architecture

- `ui`: Astro 7 static site deployed on Vercel, with React 19 islands, Tailwind 4, content collections, SEO helpers, and generated sitemap/RSS routes.
- `api/phaeno.api`: .NET 10 API deployed on Hetzner, with versioned `/api/v1` website operations, PostgreSQL PostgreSQL through EF Core, Mailgun, reCAPTCHA Enterprise, Quartz crawling, and Lucene search.
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
