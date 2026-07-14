# Decision log

Dates are capture dates for decisions visible in current code and deployment docs.

## 2026-07-14: The public site is statically generated

Status: observed in current code.

Astro uses `output: 'static'`; content pages should remain static by default. Use React islands for targeted client interaction rather than turning whole pages into client applications.

## 2026-07-14: UI and API deploy independently

Status: observed in current deployment docs.

The Astro UI deploys to Vercel. The .NET API deploys to Hetzner and uses PostgreSQL PostgreSQL. A successful UI deployment is not proof that API, database, Mailgun, reCAPTCHA, or search indexing is healthy.

## 2026-07-14: Public HTML is the internal-search contract

Status: observed in current crawler, metadata helpers, and Markdown plugin.

The API crawler indexes generated HTML under `<main>`. Stable metadata, heading IDs, and search labels are therefore cross-application contracts.

## 2026-07-14: The design system is token-led

Status: observed in project guidance and design-system files.

Public UI changes should use semantic tokens and existing component patterns. New hard-coded brand colors or isolated styling systems require an explicit design-system decision.

## Open decisions

- Complete and close the active Astro 7 upgrade plan after its remaining verification criteria are met.
- Decide any future direct provider-calendar, portal, or commerce integration in a dedicated plan rather than embedding it in a page change.
