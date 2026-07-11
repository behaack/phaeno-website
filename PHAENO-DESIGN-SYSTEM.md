# Phaeno Design System

Version 1.1 — July 2026

This system translates Phaeno's investor presentation language into a durable website vocabulary. It is grounded in the supplied `Design.pptx`, the public website, and the Astro implementation.

## Brand idea

Phaeno makes hidden molecular information legible. The experience should feel precise, revelatory, and credible: complex science made clear without making it simplistic.

1. **Reveal the signal.** Give one scientific claim, metric, or action obvious priority.
2. **Evidence over decoration.** Use data, molecular imagery, diagrams, and citations with a purpose.
3. **Technical, not clinical.** The visual voice is research-grade, not hospital-like or consumer-wellness oriented.
4. **Depth with restraint.** RNA blue creates discovery; white surfaces create analytical clarity.
5. **Explain before persuading.** Headings state the conclusion; supporting content shows why it is true.

## Visual modes

- **Discovery mode:** deep blue molecular imagery, white type, restrained green accents, and one dominant statement. Use for heroes, vision, and platform positioning.
- **Evidence mode:** white or pale green surfaces, dark RNA text, structured tables, and restrained accents. Use for methods, comparisons, team, forms, and detailed content.

Alternating these modes creates the primary visual rhythm. Do not place every section on molecular imagery and do not turn every concept into an equal card.

## Logo

Use the existing lowercase Phaeno wordmark as the primary mark. Use the standalone `p` only when the full brand is already established or space is genuinely constrained.

- Preserve aspect ratio and clear space.
- Use green on white or white on a dark RNA field.
- Never recreate the wordmark with live text, stretch it, or add effects.
- Prefer original vector artwork when it becomes available; repository assets are currently raster.

## Color

| Role | Token | Value |
| --- | --- | --- |
| Phaeno green | `--color-brand` | `#789946` |
| Accessible green action | `--color-action-primary` | `#627430` |
| Deep green | `--color-brand-strong` | `#526832` |
| RNA blue | `--phaeno-rna-600` | `#156082` |
| RNA midnight | `--color-surface-rna` | `#0E2841` |
| Evidence amber | `--color-evidence` | `#FEC950` |
| Action orange | `--color-link` | `#B35D0C` |
| Ink | `--color-text` | `#1D1D1D` |
| Muted ink | `--color-text-muted` | `#595959` |
| Canvas | `--color-canvas` | `#FFFFFF` |

Phaeno green `#789946` is a recognition color, not normal small text on white. Use `#627430` or darker behind white labels and `#526832` or darker for green text on white. Amber pairs with dark ink, not white. Never communicate scientific quality or state through color alone.

## Typography

Inter is the website typeface. Use the `--font-family-sans` stack. The reference deck contains legacy use of Lucida Sans, Arial, Arial Black, Trebuchet MS, and Open Sans; these are source-material evidence, not the web specification.

- Heroes: 48–104px, medium weight, tight line height, sentence case.
- H1: one per page and outcome-oriented.
- H2: major scientific or narrative sections.
- H3: evidence blocks, mechanisms, and component groups.
- Body: 16–18px with 1.4–1.6 line height and a maximum sustained-reading width of 44rem.
- Labels and evidence markers: 12–14px, semibold, carefully spaced uppercase.

Reserve bold for key figures and argument-carrying phrases. Use tabular numerals for metrics and comparisons.

## Layout

- Use the 4px-based `--space-*` scale.
- Default content width is 80rem; sustained prose stays within 44rem.
- Major sections use `--section-space` and fluid `--page-gutter` values.
- Prefer asymmetrical editorial compositions to grids of identical cards.
- On mobile, preserve reading order and make primary actions full width.

## Imagery

- Favor authentic molecular, sequencing, laboratory, and data imagery.
- Use navy/teal scrims behind type and validate contrast against the final crop.
- Use macro imagery for discovery mode and diagrams/tables for evidence mode.
- Avoid generic healthcare imagery, stock doctors, abstract AI brains, and decorative DNA unrelated to the claim.

## Components

### Header and navigation

Use a translucent white editorial header, full green wordmark, restrained selected-state pill, circular search/menu controls, and one accessible green action. Mobile navigation opens as a high-contrast white overlay with 44px targets.

### Buttons

- Primary: accessible green background, white label, darker hover.
- Secondary: transparent or white surface, strong border, clear label.
- Tertiary: text with a directional arrow.
- Use one primary action per region and label the outcome directly.

### Cards

Cards are for peer concepts or repeatable records. Use pale evidence surfaces and small borders; reserve the dark RNA card for the leading claim. A card has one primary action at most.

### Forms

Labels remain visible, required state is textual, focus uses `--color-focus`, and error messages are programmatically associated. Group fields by user intent.

### Tables and data visualization

Lead with the conclusion. Highlight Phaeno in green or RNA blue and render comparators in neutral gray. Include units, sample size, method, source, and validation status. Avoid 3D charts, dual axes, and color-only meaning.

### Footer

Use RNA midnight with green section labels and subdued white links. The footer is a strong closing field, not another pale navigation panel.

## Motion

Motion clarifies state change: 120ms for feedback, 200ms for standard transitions, and 320ms for deliberate reveal. Prefer opacity and transform and honor `prefers-reduced-motion`.

## Accessibility

- Target WCAG 2.2 AA.
- Body text contrast is at least 4.5:1; large text and meaningful boundaries are at least 3:1.
- Keyboard focus is always visible.
- Preserve semantic landmarks, heading order, labels, table headers, and meaningful alt text.
- Keep RUO, preliminary, and validation language readable and adjacent to the claim it qualifies.

## Content voice

Phaeno's voice is direct, exact, and evidence-led. State the result first, use concrete biological nouns and measurable verbs, define acronyms on first use, and distinguish measured, inferred, preliminary, planned, and validated claims.

## Search contract

Every searchable page includes a meaningful title, description, and `phaeno:document-type` through the existing SEO helpers. The crawler indexes `<main>` only.

- Preserve stable heading IDs and `data-phaeno-search` labels.
- Put the conclusion in the visible heading and synonyms in search metadata.
- Use `data-phaeno-search-ignore` for decorative content and controls.
- Do not casually change document-type labels.

## Implementation

The canonical token source is `ui/src/styles/design-system.css`, imported by `global.css`. New UI uses semantic roles such as `--color-text` and `--color-action-primary`. Existing compatibility aliases can migrate incrementally.

Before merging a design change, verify desktop and mobile rendering, interactive states, the production Astro build, generated metadata, stable search anchors, contrast, and reduced-motion behavior.
