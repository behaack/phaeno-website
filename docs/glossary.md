# Glossary

| Term | Meaning in this repository |
| --- | --- |
| UI | The Astro public website in `ui/`, deployed to Vercel. |
| WebOps API | Versioned public endpoints for search, contact, order, and database keepalive operations. |
| Content collection | Typed Markdown/MDX content loaded through Astro's content configuration. |
| React island | An interactive React component hydrated inside an otherwise static Astro page. |
| Document type | Search metadata classification used by the crawler and Lucene rebuild. |
| Section result | A search result anchored to a labeled heading within a page. |
| Search-ignore marker | `data-phaeno-search-ignore`, used for visible content that should not enter snippets. |
| Crawler | The API service that reads sitemap/robots guidance and extracts public page content. |
| Lucene index | The API-owned internal search index built from crawled content. |
| PSEQ | Phaeno's phased-sequencing product/technology term; scientific claims about it require approved evidence. |
| PostgreSQL | The hosted PostgreSQL provider used by the website API. |
| Keepalive | The scheduled `database-ping` request used to touch the database without changing business data. |
