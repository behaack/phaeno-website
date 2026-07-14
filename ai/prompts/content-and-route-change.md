# Content and route change prompt

```text
Inspect the current Phaeno Website implementation for this content or route request.

Before editing:
- read AGENTS.md, the Phaeno design-system documentation, and matching route/component/content files;
- identify every public URL, redirect, sitemap/RSS entry, metadata helper, and internal-search dependency affected;
- distinguish source-backed claims from marketing copy that needs approval;
- state whether the change is Astro-only or also affects the WebOps API/crawler.

Propose a narrow plan. Preserve stable URLs and heading anchors unless the requested behavior requires a migration.

After implementation, build the UI when generated output is affected, inspect the generated HTML and feeds, run the relevant crawler/search smoke when needed, and report exactly what was verified.
```
