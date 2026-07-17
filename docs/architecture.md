# Architecture

## System shape

```text
Public browser
  -> Astro static site on Vercel (`ui`)
       -> React islands for interactive forms/components
       -> content collections for articles and jobs
       -> generated sitemap and RSS routes
  -> versioned ASP.NET Core API on Hetzner (`api/phaeno.api`)
       -> PostgreSQL via EF Core
       -> Mailgun notifications
       -> Google reCAPTCHA Enterprise
       -> Quartz website crawl -> Lucene index
```

## UI

The UI is a static Astro 7 site. Routes live under `ui/src/pages`, shared page structure under `ui/src/layouts` and `ui/src/components`, and content under `ui/src/content` with schemas in `ui/src/content.config.ts`.

Current content collections include jobs, blog, events, news, press, scientific papers, and white papers. Dynamic collection pages use slug routes. React is used as islands where client-side interaction is needed; normal marketing content remains Astro-rendered.

SEO helpers live under `ui/src/components/meta-data-helpers`. Markdown/MDX headings are processed by `ui/src/lib/rehypePhaenoHeadingSearch.js` so the internal crawler can emit section-level search results.

## API

The .NET 10 API uses feature and infrastructure folders:

- `Features/WebOps`: contact requests, order requests, database keepalive, and public search endpoints.
- `Infrastructure/WebcrawlerServices`: sitemap/robots-aware crawling and `<main>` text extraction.
- `Infrastructure/WebSearchServices`: Lucene indexing and querying.
- `Infrastructure/ChronJobs`: scheduled website indexing.
- `Infrastructure/NotificationServices`: Mailgun-backed notification delivery.
- `Infrastructure/RecatchaServices`: reCAPTCHA Enterprise validation.
- `Infrastructure/Db`: EF Core PostgreSQL access.

Public routes are versioned beneath `/api/v1/web-ops` and use the shared API envelope.

## Search flow

1. Astro emits public HTML, sitemap entries, document metadata, and heading anchors.
2. Quartz runs `IndexWebsiteJob`.
3. `WebcrawlerService` follows the configured public sitemap/robots rules.
4. `HtmlTextExtractor` indexes meaningful `<main>` content and skips navigation, forms, scripts, hidden/decorative elements, and `[data-phaeno-search-ignore]`.
5. `WebSearchService` writes and queries the Lucene index.

Route, heading, or metadata changes can therefore affect both public SEO and internal search.

## Deployment boundaries

- UI: Vercel, configured from `ui/vercel.json`.
- API: Hetzner at the deployment path documented in `README.md` and `api/README.md`.
- Database: PostgreSQL; production connection details remain environment-owned.
- Deployment procedures remain in `DEPLOY-ME.md`, `DEPLOY-ME-INSTRUCTIONS.md`, and `api/README.md`.
