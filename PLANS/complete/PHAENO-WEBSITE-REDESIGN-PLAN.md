# Phaeno Website Redesign Plan

## Goal

Apply the Phaeno design system across the existing public website so the visual change is clear, cohesive, responsive, and production-ready without changing the site's architecture, routes, API contracts, or search-indexing behavior.

## Guardrails

- Preserve the Astro architecture and existing dependencies.
- Preserve page content unless a small edit materially improves hierarchy or clarity.
- Preserve titles, descriptions, document types, stable heading IDs, and `data-phaeno-search` attributes.
- Keep the supplied Phaeno imagery and wordmark as the authentic visual source.
- Meet WCAG 2.2 AA for interaction and body-text contrast.
- Verify actual desktop and mobile rendering, not only source CSS.

## Completed slices

### 1. Shared foundations and shell

- [x] Refined global typography, spacing, surfaces, links, focus states, and reusable utility patterns.
- [x] Redesigned desktop and mobile header/navigation.
- [x] Redesigned the footer.
- [x] Updated shared buttons, cards, tables, forms, media cards, team cards, search, and page banners.

### 2. Homepage

- [x] Recomposed the homepage hero around discovery mode.
- [x] Rebuilt the platform narrative, feature hierarchy, calls to action, and performance presentation.
- [x] Preserved homepage search metadata and anchors.

### 3. Core scientific pages

- [x] Applied the system to Technology.
- [x] Applied the system to Why Isoforms Matter.
- [x] Applied the system to Multi-omics.

### 4. Supporting pages

- [x] Applied shared banners, typography, spacing, footer, cards, media patterns, and controls across the site.
- [x] Added focused treatments for About, Contact, team, media, jobs, articles, legal/data pages, and error states through shared components.

### 5. Verification

- [x] Production build passed on July 10, 2026.
- [x] Generated titles, descriptions, document types, main landmarks, and homepage search anchors were verified.
- [x] Homepage, mobile navigation, Technology, and Contact were verified in the browser at desktop and phone widths.
- [x] Browser console showed no errors or warnings on the verified pages.

## Outcome

The website now uses the Phaeno discovery/evidence visual modes: an RNA-midnight scientific hero and footer, accessible green actions, editorial typography, asymmetrical platform storytelling, pale evidence surfaces, and consistent responsive component treatments. Architecture, routes, APIs, and crawler contracts remain unchanged.
