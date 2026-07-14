# Business and content rules

## Public content

- Public pages represent Phaeno's approved scientific, product, investor, and company messaging.
- Scientific and commercial claims must be traceable to approved source material; use `ai/research/README.md` for new claims or material rewrites.
- Legal pages and content collections are published content, not implementation notes. Preserve their URL and localization behavior unless the task explicitly changes it.
- Redirects are part of the public URL contract. Confirm whether a change preserves, collapses, or remaps paths before altering redirects.

## Search and SEO

- Every searchable page needs a meaningful title, description, and `phaeno:document-type` value through the existing helpers.
- `List` document types are excluded from Lucene rebuild results; do not relabel document types casually.
- Section search results depend on stable heading IDs and `data-phaeno-search` labels.
- The crawler indexes `<main>` and intentionally excludes surrounding chrome and ignored content.
- Sitemap and RSS output must be verified from the production build, not inferred only from source routes.

## Contact and order operations

- Contact and order submissions go through the versioned WebOps API.
- Server-side validation and reCAPTCHA verification remain authoritative even when the UI validates first.
- Persistence uses PostgreSQL; notifications use configured Mailgun services.
- Secrets and service-account material must remain outside source control.
- `database-ping` is a harmless keepalive/read path, not a general health proof for every external dependency.

## Design system

- Reuse the Phaeno design tokens and component guidance before introducing a new visual primitive.
- Accessibility and responsive behavior are release requirements for public pages.
- Use Astro for static content and React islands only where client interactivity warrants the extra runtime.
