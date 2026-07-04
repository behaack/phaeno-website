# Astro v7 Upgrade Plan

Status: Implemented locally; browser click-smoke blocked by unavailable browser backend
Owner: UI
Created: 2026-07-04
Updated: 2026-07-04

## Goal

Upgrade the Phaeno public website UI from Astro v5 to Astro v7 while preserving the current static-site behavior, public routes, MDX article experiment, search metadata, RSS feeds, sitemap output, and Vercel deployment shape.

This plan should be executed as a controlled upgrade slice after the current MDX experiment is stable. Do not mix this upgrade with unrelated visual redesign, API work, database work, or production deployment changes.

## Current State

- The UI app lives under `ui/`.
- The site is static output with `output: 'static'`.
- The project currently uses Astro v5 and a pinned Astro 5-compatible MDX integration.
- The MDX experiment has introduced `.mdx` blog content and Astro components imported from MDX.
- Content collections are currently defined in `ui/src/content/config.ts`.
- Several routes still use legacy collection access patterns such as `entry.slug` and `entry.render()`.
- The project uses a custom rehype plugin, `ui/src/lib/rehypePhaenoHeadingSearch.js`, for heading search metadata.
- The build currently reports pre-existing warnings around the empty `jobs` collection and global CSS parsing.

## References

- Astro v7 upgrade guide: https://docs.astro.build/en/guides/upgrade-to/v7/
- Astro v6 upgrade guide: https://docs.astro.build/en/guides/upgrade-to/v6/
- Astro MDX integration: https://docs.astro.build/en/guides/integrations-guide/mdx/

## Scope

In scope:

- Upgrade Astro and official Astro integrations to v7-compatible versions.
- Migrate content collections to the current Content Layer API.
- Preserve existing Markdown and MDX rendering behavior.
- Keep the current custom heading search rehype behavior working.
- Preserve generated public routes, RSS feeds, sitemap output, and static deployment behavior.
- Remove or replace deprecated Astro packages only when the build proves they are unnecessary or incompatible.

Out of scope:

- API changes.
- Database schema changes.
- Auth, permissions, or session lifecycle changes.
- Marketing redesign.
- New CMS adoption.
- Production deployment unless requested after local verification.

## Known Risks

- Astro v6 removes legacy content collection APIs, so collection definitions and route usage must be migrated before v7 is clean.
- Astro v7 changes the default Markdown processor to Satteri. Because this repo depends on a custom rehype plugin, the upgrade should explicitly keep the unified/remark pipeline unless the plugin is intentionally ported later.
- Astro v7 uses a stricter Rust compiler. Previously tolerated invalid HTML or unclosed tags may become build errors.
- Astro v7 changes default whitespace handling to `compressHTML: 'jsx'`, which can affect inline spacing in templates.
- The latest `@astrojs/mdx` package targets Astro v7 and Node 22.12 or newer. Local Node is already on Node 22, but CI/Vercel Node settings should be verified before deployment.

## Slice 0: Baseline

- [x] Confirm current branch intentionally includes the MDX experiment and route movement from `_blog` to `blog`.
- [x] Run `pnpm build` from `ui/` and record the current warnings.
- [ ] Smoke the MDX article locally:
  - [x] `/blog`
  - [x] `/blog/an-introduction-to-phased-sequencing-part-1`
  - [x] `/blog.xml`
  - [x] `/sitemap-index.xml` in built output
- [x] Note any pre-existing warnings that should not block the Astro upgrade.

Notes:

- Baseline Astro v5 build passed before upgrade.
- Pre-existing warnings: empty/missing `jobs` collection and global CSS optimizer warning around `@media`.

## Slice 1: Dependency Upgrade

- [x] Upgrade Astro and official Astro integrations together.
- [x] Target current v7-compatible package lines:
  - [x] `astro`
  - [x] `@astrojs/mdx`
  - [x] `@astrojs/react`
  - [x] `@astrojs/rss`
  - [x] `@astrojs/sitemap`
- [x] Add `@astrojs/markdown-remark` as a direct dependency if needed to preserve the unified markdown pipeline.
- [x] Review whether these packages are obsolete or incompatible after the upgrade:
  - [x] `@astrojs/image`
  - [x] `@astrojs/tailwind`
  - [x] `astro-robots-txt`
  - [x] `astro-compress`
- [x] Keep `@tailwindcss/vite` unless the upgraded build indicates a better Astro-native path.

## Slice 2: Content Layer Migration

- [x] Move collection configuration from `ui/src/content/config.ts` to the current `ui/src/content.config.ts` shape.
- [x] Use `z` from `astro/zod`.
- [x] Define explicit loaders with `glob()` or `file()` from `astro/loaders`.
- [x] Ensure Markdown and MDX blog entries are included with a pattern such as `**/[^_]*.{md,mdx}`.
- [x] Preserve all existing collections:
  - [x] `blog`
  - [x] `events`
  - [x] `jobs`
  - [x] `news`
  - [x] `press`
  - [x] `scientific_papers`
  - [x] `white_papers`
- [x] Decide how to handle empty `jobs` content during the upgrade.

Note: `ui/src/content/jobs/.gitkeep` preserves the expected folder, but Astro still warns when no matching job entries exist. This warning is non-blocking and matches the current empty jobs state.

## Slice 3: Route API Migration

- [x] Replace legacy `entry.slug` usage with the current collection entry id behavior.
- [x] Replace `entry.render()` with `render(entry)` from `astro:content`.
- [x] Update dynamic routes that read collections:
  - [x] `ui/src/pages/blog/[slug].astro`
  - [x] `ui/src/pages/blog/index.astro`
  - [x] `ui/src/pages/blog.xml.ts`
  - [x] `ui/src/pages/press.xml.ts`
  - [x] `ui/src/pages/about/job-openings/[slug].astro`
  - [x] `ui/src/pages/about/job-openings/index.astro`
  - [x] `ui/src/pages/_media/**`
- [x] Confirm helper functions such as `getBlogPostPath()` still produce the same public URLs.

## Slice 4: Markdown and MDX Behavior

- [x] Preserve `rehypePhaenoHeadingSearch`.
- [x] Configure the Markdown processor to keep unified/remark/rehype support if the v7 default processor does not run the existing plugin.
- [x] Confirm `.md` and `.mdx` content both render.
- [x] Confirm MDX can import local Astro components.
- [x] Confirm generated heading ids and `data-phaeno-search` attributes remain present in rendered HTML.
- [x] Consider setting `compressHTML: true` during migration if inline spacing changes are found.

## Slice 5: Deprecated Package Cleanup

- [x] Remove `@astrojs/image` if all current image usage is through `astro:assets` and the package is not needed.
- [x] Remove `@astrojs/tailwind` if Tailwind is fully handled by `@tailwindcss/vite`.
- [x] Remove `@types/react-icons` if it remains unnecessary because `react-icons` ships its own types.
- [x] Keep cleanup scoped to packages directly affected by the Astro upgrade.

## Slice 6: Verification

- [x] Run `pnpm build` from `ui/`.
- [x] Confirm static routes are generated for:
  - [x] `/`
  - [x] `/blog`
  - [x] `/blog/an-introduction-to-phased-sequencing-part-1`
  - [x] `/privacy`
  - [x] `/data-policies`
  - [x] `/technology`
  - [x] `/about/about-us`
  - [x] `/contact`
- [x] Confirm generated feeds:
  - [x] `/blog.xml`
  - [x] `/press.xml`
  - [x] `/sitemap-index.xml`
- [ ] Start the local dev server and browser-smoke:
  - [x] home page HTTP smoke
  - [x] blog index HTTP smoke
  - [x] MDX article HTTP smoke
  - [x] search modal SSR trigger markup
  - [x] contact forms render
  - [x] mobile menu SSR trigger markup
  - [ ] click/tap browser smoke blocked because no in-app browser backend was available in this Codex session
- [x] Inspect the generated MDX article HTML for:
  - [x] imported component markup
  - [x] heading ids
  - [x] `data-phaeno-search`
  - [x] no broken image paths

## Rollback Plan

- Revert the dependency upgrade and any content-layer migration commits if the v7 build cannot be stabilized quickly.
- Keep the current Astro v5 + MDX implementation as the fallback branch state.
- Do not rewrite history or discard unrelated user changes.

## Completion Criteria

- [x] Astro v7 build passes locally.
- [x] MDX article renders through the public blog route.
- [x] Existing public route shape is preserved.
- [x] RSS and sitemap outputs are present.
- [x] Custom heading search metadata remains present.
- [x] No new unrelated dependencies or architecture changes were introduced.
- [ ] Plan is moved from `PLANS/active` to `PLANS/complete`.
