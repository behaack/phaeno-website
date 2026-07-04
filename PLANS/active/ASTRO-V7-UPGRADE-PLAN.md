# Astro v7 Upgrade Plan

Status: Draft, planning only
Owner: UI
Created: 2026-07-04

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

- [ ] Confirm current branch intentionally includes the MDX experiment and route movement from `_blog` to `blog`.
- [ ] Run `pnpm build` from `ui/` and record the current warnings.
- [ ] Smoke the MDX article locally:
  - [ ] `/blog`
  - [ ] `/blog/an-introduction-to-phased-sequencing-part-1`
  - [ ] `/blog.xml`
  - [ ] `/sitemap-index.xml`
- [ ] Note any pre-existing warnings that should not block the Astro upgrade.

## Slice 1: Dependency Upgrade

- [ ] Upgrade Astro and official Astro integrations together.
- [ ] Target current v7-compatible package lines:
  - [ ] `astro`
  - [ ] `@astrojs/mdx`
  - [ ] `@astrojs/react`
  - [ ] `@astrojs/sitemap`
- [ ] Add `@astrojs/markdown-remark` as a direct dependency if needed to preserve the unified markdown pipeline.
- [ ] Review whether these packages are obsolete or incompatible after the upgrade:
  - [ ] `@astrojs/image`
  - [ ] `@astrojs/tailwind`
  - [ ] `astro-robots-txt`
  - [ ] `astro-compress`
- [ ] Keep `@tailwindcss/vite` unless the upgraded build indicates a better Astro-native path.

## Slice 2: Content Layer Migration

- [ ] Move collection configuration from `ui/src/content/config.ts` to the current `ui/src/content.config.ts` shape.
- [ ] Use `z` from `astro/zod`.
- [ ] Define explicit loaders with `glob()` or `file()` from `astro/loaders`.
- [ ] Ensure Markdown and MDX blog entries are included with a pattern such as `**/[^_]*.{md,mdx}`.
- [ ] Preserve all existing collections:
  - [ ] `blog`
  - [ ] `events`
  - [ ] `jobs`
  - [ ] `news`
  - [ ] `press`
  - [ ] `scientific_papers`
  - [ ] `white_papers`
- [ ] Decide how to handle empty `jobs` content without producing noisy build warnings.

## Slice 3: Route API Migration

- [ ] Replace legacy `entry.slug` usage with the current collection entry id behavior.
- [ ] Replace `entry.render()` with `render(entry)` from `astro:content`.
- [ ] Update dynamic routes that read collections:
  - [ ] `ui/src/pages/blog/[slug].astro`
  - [ ] `ui/src/pages/blog/index.astro`
  - [ ] `ui/src/pages/blog.xml.ts`
  - [ ] `ui/src/pages/press.xml.ts`
  - [ ] `ui/src/pages/about/job-openings/[slug].astro`
  - [ ] `ui/src/pages/about/job-openings/index.astro`
  - [ ] `ui/src/pages/_media/**`
- [ ] Confirm helper functions such as `getBlogPostPath()` still produce the same public URLs.

## Slice 4: Markdown and MDX Behavior

- [ ] Preserve `rehypePhaenoHeadingSearch`.
- [ ] Configure the Markdown processor to keep unified/remark/rehype support if the v7 default processor does not run the existing plugin.
- [ ] Confirm `.md` and `.mdx` content both render.
- [ ] Confirm MDX can import local Astro components.
- [ ] Confirm generated heading ids and `data-phaeno-search` attributes remain present in rendered HTML.
- [ ] Consider setting `compressHTML: true` during migration if inline spacing changes are found.

## Slice 5: Deprecated Package Cleanup

- [ ] Remove `@astrojs/image` if all current image usage is through `astro:assets` and the package is not needed.
- [ ] Remove `@astrojs/tailwind` if Tailwind is fully handled by `@tailwindcss/vite`.
- [ ] Remove `@types/react-icons` if it remains unnecessary because `react-icons` ships its own types.
- [ ] Keep cleanup scoped to packages directly affected by the Astro upgrade.

## Slice 6: Verification

- [ ] Run `pnpm build` from `ui/`.
- [ ] Confirm static routes are generated for:
  - [ ] `/`
  - [ ] `/blog`
  - [ ] `/blog/an-introduction-to-phased-sequencing-part-1`
  - [ ] `/privacy`
  - [ ] `/data-policies`
  - [ ] `/technology`
  - [ ] `/about/about-us`
  - [ ] `/contact`
- [ ] Confirm generated feeds:
  - [ ] `/blog.xml`
  - [ ] `/press.xml`
  - [ ] `/sitemap-index.xml`
- [ ] Start the local dev server and browser-smoke:
  - [ ] home page
  - [ ] blog index
  - [ ] MDX article
  - [ ] search modal
  - [ ] contact forms render
  - [ ] mobile menu
- [ ] Inspect the generated MDX article HTML for:
  - [ ] imported component markup
  - [ ] heading ids
  - [ ] `data-phaeno-search`
  - [ ] no broken image paths

## Rollback Plan

- Revert the dependency upgrade and any content-layer migration commits if the v7 build cannot be stabilized quickly.
- Keep the current Astro v5 + MDX implementation as the fallback branch state.
- Do not rewrite history or discard unrelated user changes.

## Completion Criteria

- [ ] Astro v7 build passes locally.
- [ ] MDX article renders through the public blog route.
- [ ] Existing public route shape is preserved.
- [ ] RSS and sitemap outputs are present.
- [ ] Custom heading search metadata remains present.
- [ ] No new unrelated dependencies or architecture changes were introduced.
- [ ] Plan is moved from `PLANS/active` to `PLANS/complete`.
