# Radial Select — Design Spec

**Date:** 2026-06-07
**Status:** Approved for implementation planning
**Target release:** v2.1 (headline feature of the "Advanced Inputs" minor)

---

## 1. Context

htmxRazor is at v2.0.1 — a mature, server-rendered Tag Helper component library (85+
components, custom `rhx-` design system, CSS Anchor Positioning with JS fallback, model
binding, 1,838 unit tests). This spec defines a single new compound control,
`<rhx-radial-select>`, which is the headline feature of the next minor.

### 2.X roadmap (agreed framing — context only, not part of this spec's build)

| Version | Theme | Highlights |
|---|---|---|
| **2.1 — Advanced Inputs** | New interactive form controls | **`rhx-radial-select`** (this spec), `rhx-date-picker` family, re-enable + stabilize Playwright E2E in CI |
| **2.2 — Playground & DX completion** | Finish what 2.0 started | Playground coverage for all form/action components, JetBrains plugin parity with VS Code snippets |
| **2.3 — Data & Visualization** | Grow Data Display | SVG bar/line/area charts, expandable/tree table rows, optional virtualized table body |
| **2.4 — A11y & i18n hardening** | Polish & reach | WCAG 2.2 contrast audit, RTL, localizable strings, clear infra-cleanup backlog (dead `EnableCssIsolation` / `IHtmxSupported`, `hx-replace-url` attr, `HtmlRenderer` adoption) |

All additions are additive (minor bumps); cleanup that removes dead options lands in 2.4
without breaking the public API.

---

## 2. The control

A compound form control composed of:

- A **rectangular trigger button** flush-left against a **dropdown**, forming one input
  group (shared border, left-only / right-only border radius — same visual idiom as a
  button-attached input).
- A **circular pie popup** opened from the trigger. Each pie **wedge is a category**
  rendered with a color + icon.
- Selecting a wedge:
  1. marks that category active and echoes its color + icon onto the trigger,
  2. sets the hidden **category** value,
  3. fires the wedge's `hx-get`; the server returns an `<rhx-option>` fragment that htmx
     swaps into the dropdown's listbox (the dropdown's *entire option set* changes),
  4. **auto-selects the first option** of the new set,
  5. closes the popup and moves focus to the dropdown.
- The dropdown then selects a value **within** the active category.

This is a cascading select where the parent selector is a visual pie. Data is loaded
**htmx-natively** (server fetch per wedge).

---

## 3. Public API

### 3.1 `<rhx-radial-select>` (wrapper)

Renders the whole input group, the pie popup, and the hidden inputs. Owns the dropdown
internally (the dropdown is not separately addressable by the author).

```razor
<rhx-radial-select rhx-for="FoodItem"
                   rhx-placeholder="Choose an item…"
                   rhx-category-name="Category"
                   aria-label="Food category">

    <rhx-radial-option rhx-value="fruit" rhx-label="Fruit"
                       rhx-icon="apple"  rhx-color="danger"
                       hx-get="/Menu?handler=Items&cat=fruit" />

    <rhx-radial-option rhx-value="meat"  rhx-label="Meat"
                       rhx-icon="drumstick" rhx-color="success"
                       hx-get="/Menu?handler=Items&cat=meat" />
</rhx-radial-select>
```

| Attribute | Type | Default | Purpose |
|---|---|---|---|
| `rhx-for` | `ModelExpression` | — | Model binding for the **dropdown value** (the in-category selection). Mutually exclusive with `name`. |
| `name` | string | — | Form field name for the dropdown value when not using `rhx-for`. |
| `rhx-placeholder` | string | — | Dropdown placeholder shown before any option is selected (and when a category has zero options). |
| `rhx-category-name` | string | — | Optional. When set, a second hidden input with this name submits the **active category value** (`rhx-value` of the selected wedge), enabling round-trip / model binding of the chosen category. |
| `rhx-default-category` | string | — | Optional. `rhx-value` of the wedge to activate on initial render (server should pre-render that category's options). |
| `rhx-size` | `small` \| `medium` \| `large` | `medium` | Sizing, consistent with other form controls. |
| `rhx-disabled` | bool | `false` | Disables the whole control. |
| `aria-label` | string | — | Accessible name for the category picker (the menu). |

### 3.2 `<rhx-radial-option>` (one wedge / category)

| Attribute | Type | Default | Purpose |
|---|---|---|---|
| `rhx-value` | string | **required** | Category identifier (submitted via `rhx-category-name`, sent to the server). |
| `rhx-label` | string | **required** | Accessible name + tooltip/visible label for the wedge. |
| `rhx-icon` | string | — | Icon name resolved through the existing `IconRegistry`. Rendered at the wedge centroid and echoed on the trigger when active. |
| `rhx-color` | variant token | `neutral` | One of the named design-system variants: `brand`, `success`, `warning`, `danger`, `neutral`. Maps to `--rhx-color-<variant>-500` tokens — theme- and dark-mode-correct. See §3.3. |
| `hx-get` (+ other `hx-*`) | string | — | Endpoint returning the dropdown's option fragment for this category. Any standard htmx attributes are honored; `hx-target`/`hx-swap` are supplied automatically by the component (see §5) but may be overridden. |
| `rhx-disabled` | bool | `false` | Renders the wedge dimmed and non-selectable (`aria-disabled`). |

### 3.3 Color palette and wedge-count behavior (per "variant tokens only")

- Allowed `rhx-color` values are the **named variants only**: `brand`, `success`,
  `warning`, `danger`, `neutral` (the five `--rhx-color-<variant>-500` families that exist
  in the token set). No raw hex / custom CSS color is accepted — this guarantees theming,
  dark-mode, and contrast correctness.
- If `rhx-color` is omitted on a wedge, the component assigns one **deterministically** by
  wedge index, cycling through a fixed ordered list of the named variants:
  `brand → success → warning → danger → neutral` (then repeats).
- Explicit `rhx-color` always wins over the auto-cycle.
- Wedge count is expected in the **2–12** range. Beyond the number of named variants,
  colors repeat per the cycle above (documented, deterministic — not an error). Adjacent
  identical colors are avoided where the cycle allows.

---

## 4. Rendering

### 4.1 Structure (server-rendered HTML)

```
<div class="rhx-radial-select" data-rhx-radial-select data-rhx-placement="top-start">
  <div class="rhx-radial-select__group">
    <button class="rhx-radial-select__trigger" type="button"
            aria-haspopup="menu" aria-expanded="false"
            aria-controls="<id>-pie" aria-label="<aria-label>">
      <!-- active category icon echoed here -->
    </button>

    <!-- internal dropdown: reuses rhx-select machinery -->
    <div class="rhx-radial-select__dropdown" ...>
      <button role="combobox" aria-haspopup="listbox" aria-expanded="false" ...></button>
      <div class="rhx-radial-select__listbox" id="<id>-listbox" role="listbox" hidden>
        <!-- option fragment swapped in here by htmx -->
      </div>
    </div>
  </div>

  <!-- pie popup -->
  <div class="rhx-radial-select__pie" id="<id>-pie" role="menu" aria-label="<aria-label>" hidden>
    <svg viewBox="0 0 200 200" class="rhx-radial-select__wheel">
      <!-- one <path> arc per wedge + icon at centroid; role on a wrapping <g> or button -->
    </svg>
  </div>

  <!-- form submission -->
  <input type="hidden" name="<value-name>"    data-rhx-radial-value    value="…">
  <input type="hidden" name="<category-name>" data-rhx-radial-category value="…">  <!-- only if rhx-category-name set -->
</div>
```

### 4.2 The pie (SVG, not conic-gradient)

- Each wedge is an SVG `<path>` describing a `360/n`-degree arc sector, filled with the
  resolved variant token (`fill: var(--rhx-color-<variant>-500)` or equivalent).
- SVG is chosen over `conic-gradient` because it provides **per-wedge hit targets**, crisp
  edges, and precise icon centroid placement.
- The wedge icon is rendered via `IconRegistry` at the sector centroid; the active
  category's icon also renders in a small center **hub** and on the trigger.
- Popup placement reuses the existing **CSS Anchor Positioning** path with **`rhx-position.js`**
  fallback, left-edge aligned to the control, flipping when it would overflow the viewport.
- Honors `prefers-reduced-motion` (no scale/rotate-in animation when set).
- Dark-theme correct via tokens; no hardcoded colors (project rule).

---

## 5. Behavior (JS: `Assets/js/components/rhx-radial-select.js`)

- **Open/close:** trigger toggles the pie; `aria-expanded` reflects state; click-outside
  and `Esc` dismiss; focus returns to the trigger on `Esc`.
- **Wedge select:**
  1. set active wedge (`aria-checked="true"`, others `false`),
  2. echo color + icon onto the trigger and the hub,
  3. write the category hidden input,
  4. `htmx.ajax('GET', wedge.hxGet, { target: '#<id>-listbox', swap: 'innerHTML' })`
     (mirrors the Kanban drag→`htmx.ajax` pattern already in the codebase),
  5. on swap completion, **auto-select the first option** in the listbox and reflect it in
     the dropdown trigger label + value hidden input,
  6. close the pie and move focus to the dropdown trigger.
- **Zero-result category:** if the returned fragment has no options, show the placeholder
  and leave the value hidden input empty.
- **Dropdown interaction:** delegated to the existing `rhx-select` behavior (listbox open,
  keyboard nav, value binding, hidden input) — not reimplemented.
- **Position:** reuse `rhx-position.js`; no new positioning engine.

---

## 6. Accessibility

- Visual is radial; the a11y tree is a **menu** (a known, supported pattern).
- Trigger: `aria-haspopup="menu"`, `aria-expanded`, accessible name from `aria-label`.
- Pie: `role="menu"`; wedges: `role="menuitemradio"` with `aria-checked` (single active),
  accessible name = `rhx-label`, `aria-disabled` for disabled wedges.
- Keyboard within the pie: Arrow keys move between wedges in rotational order, type-ahead
  by label, `Enter`/`Space` selects, `Esc` closes and returns focus to the trigger.
- After selection, focus lands on the dropdown trigger (logical next step).
- Respects `prefers-reduced-motion`.

---

## 7. Components, isolation & dependencies

| Unit | Responsibility | Depends on |
|---|---|---|
| `RadialSelectTagHelper` | Render group, pie, hidden inputs; collect child wedges via `context.Items`; resolve binding (`rhx-for`/`name`), category name, default category, size, disabled. | `htmxRazorTagHelperBase` / `FormControlTagHelperBase`, `SlotRenderer`, `IconRegistry`, `CssClassBuilder` |
| `RadialOptionTagHelper` | Contribute one wedge's data (value, label, icon, color, htmx attrs, disabled) to the parent via `context.Items`; render nothing on its own. | parent context, `IconRegistry` |
| `rhx-radial-select.css` | Pie/wedge/trigger/group styling via `--rhx-*` tokens; dark-theme + reduced-motion aware. | tokens, `@layer` |
| `rhx-radial-select.js` | Open/close, keyboard, wedge hit/selection, color/icon echo, htmx cascade, focus, positioning. | `rhx-position.js`, htmx |

Each unit has a single clear purpose and a well-defined interface; the wrapper can be
understood without reading the option helper's internals.

---

## 8. Testing

- **Unit (`htmxRazor.Tests`)** — per the existing per-component style:
  - wrapper renders group + pie + hidden input(s); `rhx-category-name` adds the 2nd input.
  - wedge color resolution: explicit variant honored; omitted color follows the documented
    cycle; invalid color rejected/falls back to `neutral`.
  - icon resolution through `IconRegistry`; unknown icon handled gracefully.
  - ARIA: `role="menu"`, `menuitemradio`, `aria-checked`, `aria-haspopup`/`aria-expanded`.
  - htmx wiring: each wedge carries its `hx-get`; component-supplied target/swap present.
  - `rhx-default-category` activates the right wedge and echoes its color/icon.
  - disabled wrapper and disabled wedge states.
- **E2E (Playwright)** — open pie → select wedge → assert listbox repopulates, first option
  auto-selected, trigger echoes color/icon, focus moves to dropdown. Lands as part of the
  2.1 effort to **re-enable Playwright in CI**.

---

## 9. Docs & ancillary

- README catalog: add **Radial Select** under **Forms**; add a Feature Highlights snippet.
- `CHANGELOG.md`: entry under the 2.1 section (Added + Components Added).
- VS Code snippet for `<rhx-radial-select>` (+ composition snippet with wedges).
- Demo site: a page showing the food-category cascade with a server handler returning the
  `<rhx-option>` fragment.

---

## 10. Out of scope (YAGNI)

- Custom/hex wedge colors (variant tokens only, by decision).
- Multi-select pie (single active category only).
- Pairing with an externally-declared dropdown by id (wrapper owns the dropdown).
- Client-side / inline option sets (htmx-native only, by decision).
- Nested / multi-ring radial menus.
