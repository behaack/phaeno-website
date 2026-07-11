# Phaeno Design System Plan

## Goal

Create a practical design system grounded in the supplied `Design.pptx`, the current `phaeno-website` implementation, and the live public website.

## Guardrails

- Preserve current rendering and route behavior.
- Preserve the public-site search metadata and heading-anchor contract.
- Add no dependencies and make no architecture changes.
- Introduce semantic tokens as aliases so existing component styles can migrate gradually.

## Slices

- [x] Audit all 35 slides in `Design.pptx`, including typography, color, imagery, and recurring compositions.
- [x] Audit the live homepage and current Astro/Tailwind/CSS implementation.
- [x] Define the brand foundations, semantic tokens, component guidance, accessibility rules, and implementation conventions.
- [x] Add shared CSS tokens and align existing global variables/Tailwind brand colors without changing page behavior.
- [x] Build the site and verify the generated homepage metadata and search anchors.
- [x] Move this plan to `PLANS/complete` after verification.

## Verification

- `npm run build` completed successfully on July 10, 2026.
- Generated homepage HTML retained its title, description, `phaeno:document-type`, `<main id="main">`, and the `pseq-intro` and `clear-signal-architecture` search anchors.
- Generated CSS contains the shared Phaeno token layer.
