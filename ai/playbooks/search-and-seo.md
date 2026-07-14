# Search and SEO playbook

Use this for routes, page metadata, headings, Markdown/MDX rendering, sitemap/RSS, and crawler/index changes.

## Preserve the contract

- Use `SEOMeta.astro`, `ArticleSEOMeta.astro`, or `JobSEOMeta.astro` rather than hand-built metadata.
- Emit meaningful `<title>`, description, and `phaeno:document-type` values.
- Keep important heading IDs stable. Use clear `data-phaeno-search` labels for anchorable sections.
- Preserve `rehypePhaenoHeadingSearch.js` behavior through Astro/Markdown changes.
- Use `data-phaeno-search-ignore` for visible decorative/control copy that should not pollute snippets.
- Remember that `List` documents are filtered from the Lucene rebuild and section results use document type `section`.

## Verify the built artifact

From `ui/`:

```powershell
pnpm build
```

Then inspect `dist/` for:

- the expected route HTML;
- canonical title, description, and document type;
- stable heading IDs and search labels;
- expected sitemap entries;
- expected `blog.xml` or `press.xml` entries when applicable.

If the change affects extraction or indexing, run the API/crawler smoke appropriate to the environment and confirm search results, not just job completion. A source route alone does not prove it survived the build.
