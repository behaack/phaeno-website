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

## Planning documents

- Before large multi-slice changes create a planning document in /PLANS/active. Update the progress of the plan after each slice.
- When a plan is complete, move it to /PLANS/complete

## Internal search metadata

- Preserve the internal public-site search contract when changing UI pages, layouts, Markdown/MDX rendering, SEO helpers, or crawler/indexing code.
- Search indexing is driven by the API crawler in `api/phaeno.api/Infrastructure/WebcrawlerServices/WebcrawlerService.cs` and Lucene indexing in `api/phaeno.api/Infrastructure/WebSearchServices/WebSearchService.cs`.
- Every searchable page should emit a meaningful `<title>`, `<meta name="description">`, and `<meta name="phaeno:document-type">`. Prefer the existing helpers in `ui/src/components/meta-data-helpers/` (`SEOMeta.astro`, `ArticleSEOMeta.astro`, `JobSEOMeta.astro`) instead of hand-rolling head metadata.
- The crawler indexes `<main>` content only. Header, footer, nav, forms, scripts, styles, SVGs, hidden content, `.sr-only`, `.grecaptcha-badge`, and `[data-phaeno-search-ignore]` are intentionally excluded by `HtmlTextExtractor`.
- Search results are section-oriented when headings or anchors are present. Preserve stable `id` values and clear `data-phaeno-search` labels on important `h1`-`h6` headings or anchorable sections. These become result URLs, titles, and snippets.
- Markdown and MDX headings currently receive generated `id` and `data-phaeno-search` attributes through `ui/src/lib/rehypePhaenoHeadingSearch.js`. Astro/MDX upgrades must preserve that behavior or provide an equivalent before merging.
- Use `[data-phaeno-search-ignore]` for content that should render visually but not pollute search snippets, such as recaptcha notices, purely decorative text, or controls.
- Do not change document type labels casually. `List` pages are filtered out of the Lucene rebuild, while section results are emitted with `DocumentType = "section"`.
- After changes that affect routes, page metadata, headings, Markdown/MDX rendering, or search extraction, verify the generated HTML contains the expected metadata and anchor attributes, then run a build or crawler/search smoke appropriate to the change.
