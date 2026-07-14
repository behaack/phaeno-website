# AI context map

| Task | Read first |
| --- | --- |
| Public page or component | `AGENTS.md`, design-system docs, matching page/component, `docs/business-rules.md` |
| Content collection or article | `ui/src/content.config.ts`, matching collection route, `ai/research/README.md` |
| Route, SEO, heading, sitemap, RSS, or search change | `docs/architecture.md`, `ai/playbooks/search-and-seo.md` |
| Contact or order form | matching React/Astro form, WebOps controller/service, reCAPTCHA and notification code |
| API or database change | `api/README.md`, API feature code, persistence code, and active plan if one exists |
| Deployment | root deployment docs and the current generated artifact; do not infer success across UI/API boundaries |
| Astro upgrade | `PLANS/active/ASTRO-V7-UPGRADE-PLAN.md` |

Put durable architecture, rules, terms, and decisions in `docs/`. Put active implementation state in `PLANS/`. Keep source summaries for new public claims in `ai/research/` only when the research will be reused.
