# Date/Time Picker Family — Design Spec (v2.1)

**Date:** 2026-06-11
**Status:** Approved for implementation planning
**Target release:** v2.1 ("Advanced Inputs") — second feature of the minor, after Radial Select.

---

## 1. Context

htmxRazor is at v2.1.0-in-progress. The Radial Select component has shipped; this spec
defines the second v2.1 feature: a **family of four date/time input controls** built on a
shared, server-rendered calendar core with htmx-driven navigation. The library already has
display-only date helpers (`<rhx-format-date>`, `<rhx-relative-time>`); these are new
*interactive form inputs*, not formatters.

The family is designed together because all four share one foundation (calendar grid, popup,
value binding, the `/_rhx/calendar` endpoint). **Implementation is phased** (§9): each
component is an independently shippable milestone with its own implementation plan, built in
order on top of the shared core.

---

## 2. The components

| Component | Renders | Bound value |
|---|---|---|
| `rhx-date-picker` | text input + calendar popup | `DateOnly` / `DateTime` |
| `rhx-time-picker` | text input + time-list popup | `TimeOnly` / `DateTime` |
| `rhx-datetime-picker` | text input + calendar **and** time list | `DateTime` |
| `rhx-date-range-picker` | input(s) + **two** calendars + presets | start/end pair |

---

## 3. Architecture & shared foundation

### 3.1 Render & navigation model (server/htmx)

- Each picker server-renders the visible input, the trigger icon, the hidden value input(s),
  and the initial popup contents.
- The **calendar month grid is server-rendered**. Month navigation — prev/next arrows, and
  the clickable-label month-view and year-view — fire an **htmx GET** that swaps a freshly
  rendered grid into the popup body. No full-page round-trip; only the grid swaps.
- The **time list is static** (rendered once at popup render; no endpoint or navigation).

### 3.2 The `/_rhx/calendar` endpoint (built-in) + override

- `UsehtmxRazor()` registers a built-in GET endpoint at **`/_rhx/calendar`** (alongside the
  existing `/_rhx/*` asset middleware) that renders any calendar view from query parameters.
- **Query params:** `year`, `month`, `view` (`days`|`months`|`years`), `selected` (ISO date),
  `min`, `max` (ISO dates), `week-start` (`mon`|`sun`|…), `mode` (`single`|`range`|`datetime`),
  `start`, `end` (ISO, for range state), `months` (1 or 2 — range shows 2), and an `id` prefix
  so the returned markup carries stable element ids for the htmx swap.
- **App override:** authors set their own `hx-get` on the picker (e.g. to inject availability
  or app-specific disabled dates). Their handler must return the **same grid contract** (§3.4),
  which they produce with the exposed helper `RhxCalendar.RenderMonth(...)`.

### 3.3 Value, binding & format

- All four extend `FormControlTagHelperBase`: `rhx-for` (binds `DateOnly`/`DateTime`/`TimeOnly`),
  `name`, `value`, `rhx-label`, `rhx-hint`, `rhx-placeholder`, `rhx-size`, `rhx-disabled`,
  `rhx-readonly`, `rhx-required`, validation (`data-val-*`), `aria-label`.
- **On the wire:** hidden input(s) carry **ISO 8601, culture-invariant** so ASP.NET model
  binding round-trips correctly: date `yyyy-MM-dd`, time `HH:mm`, datetime `yyyy-MM-ddTHH:mm`.
- **Display:** the visible text input shows a formatted value via `rhx-format` (a .NET format
  string) or, if unset, the current culture's short date/time pattern.
- **Split of responsibilities:** **htmx** owns grid rendering/navigation; **JS** owns value
  commit — selecting a day/time updates the hidden input + visible input, sets selected state,
  closes the popup, and dispatches `input`/`change` (so forms and htmx see the value). This
  mirrors the existing `rhx-combobox` / `rhx-select` pattern.

### 3.4 Shared calendar-grid contract

Both the built-in endpoint and any app override return the **same** markup so the JS and CSS
work identically:

```html
<div class="rhx-calendar__grid" role="grid" aria-label="October 2026" data-rhx-calendar-grid>
  <div role="row">
    <span role="columnheader" abbr="Monday">Mo</span> … <span role="columnheader">Su</span>
  </div>
  <div role="row">
    <button role="gridcell" class="rhx-calendar__day rhx-calendar__day--muted"
            data-date="2026-09-29" tabindex="-1">29</button>
    …
    <button role="gridcell" class="rhx-calendar__day rhx-calendar__day--today"
            data-date="2026-10-09" tabindex="-1">9</button>
    <button role="gridcell" class="rhx-calendar__day rhx-calendar__day--selected"
            data-date="2026-10-15" aria-selected="true" tabindex="0">15</button>
    <button role="gridcell" class="rhx-calendar__day" data-date="2026-10-20"
            aria-disabled="true" disabled>20</button>
    …
  </div>
</div>
```

Day-state classes: `--muted` (adjacent month), `--today`, `--selected`, `--in-range`,
`--range-start`, `--range-end`; disabled cells use `disabled` + `aria-disabled`. The
month-view and year-view (`view=months|years`) return analogous grids of month/year buttons.
A single server-side **`CalendarRenderer`** produces all of this and backs both the endpoint
and the `RhxCalendar.RenderMonth(...)` helper.

### 3.5 Popup & positioning

Popup opens on input focus or trigger-icon click; positioned with **`rhx-position.js`**
(`placement: bottom-start`, flip/shift for viewport). Esc and click-outside close and return
focus to the trigger. `role="dialog"`, `aria-modal="false"`, accessible-named by the field
label.

---

## 4. `rhx-date-picker`

```razor
<rhx-date-picker rhx-for="DueDate" rhx-placeholder="Pick a date…"
                 rhx-min="2026-01-01" rhx-max="2026-12-31" rhx-week-start="mon" />
```

- Visible text input + calendar icon button; hidden `<input name="DueDate">` (ISO).
- **Popup header (Layout A):** `‹ October 2026 ›`. Arrows `hx-get …&view=days&month=±1`. The
  **label** is a button → `hx-get …&view=months`, and from there `view=years`, swapping the
  grid body. Footer: **Today** (selects today / navigates to it) and **Clear** (empties value).
- **Day select:** JS sets hidden ISO + display, `aria-selected`, closes, dispatches `change`.
- Out-of-range / disabled days rendered `disabled` server-side from `rhx-min`/`rhx-max`.

**Config attributes (shared by all calendar pickers):** `rhx-min`, `rhx-max` (ISO),
`rhx-week-start` (default **`mon`**), `rhx-format`, `rhx-disabled-dates` (comma ISO list and/or
weekday names), `rhx-show-today`/`rhx-show-clear` (default true).

---

## 5. `rhx-time-picker`

```razor
<rhx-time-picker rhx-for="StartTime" rhx-step="30" rhx-12hour="true" />
```

- Input + clock icon; hidden ISO `HH:mm`.
- Popup = **scrollable time list** from 00:00 to 23:30 at `rhx-step` minutes (default 30),
  bounded by `rhx-min`/`rhx-max` (time-of-day). Displayed 12-hour (`9:30 AM`) when
  `rhx-12hour` (default **true**), else 24-hour.
- Static markup, `role="listbox"` / `option`. JS selects, scrolls the selected item into view,
  closes; arrow keys + type-ahead navigate.

---

## 6. `rhx-datetime-picker`

```razor
<rhx-datetime-picker rhx-for="StartsAt" rhx-step="30" rhx-week-start="mon" />
```

- One control, one `DateTime`; hidden ISO `yyyy-MM-ddTHH:mm`.
- Popup = **calendar (left) + time list (right)** side by side, reusing §4 and §5 verbatim.
- Picking a day keeps the popup open; the value commits when **both** date and time are set
  (or via an explicit **Done** button). Until then the visible input shows the partial state.

---

## 7. `rhx-date-range-picker`

```razor
<rhx-date-range-picker rhx-start-name="From" rhx-end-name="To"
                       rhx-min="2026-01-01" rhx-week-start="mon"
                       rhx-presets="today,last7,thismonth,last30" />
```

- Visible input shows `From – To`; **two hidden inputs** (`From`, `To`, ISO). May also bind via
  `rhx-for-start` / `rhx-for-end`.
- Popup = **two calendars** (month *N* and *N+1*; arrows move the pair) + a **presets** row.
- **Interaction:** first click sets start, second sets end (auto-swap if the second is earlier);
  hovering between the two clicks shows a **live in-range preview** (JS). In-range days get
  `--in-range`; the endpoints get `--range-start` / `--range-end`. On navigation the server
  renders committed range state from `start`/`end` params; the in-flight hover preview is JS.
- **Presets** (`rhx-presets`): named ranges (`today`, `yesterday`, `last7`, `last30`,
  `thismonth`, `lastmonth`, …) set both dates client-side and re-render.

---

## 8. Accessibility (W3C APG)

- Trigger icon: `<button aria-haspopup="dialog" aria-expanded>`; input also opens on focus.
- Popup `role="dialog"` `aria-modal="false"`, accessible-named by the field label; a polite
  live region announces the visible month/year after navigation.
- Calendar `role="grid"` with weekday `columnheader`s; days are `gridcell` buttons with **roving
  tabindex** (focused day `tabindex=0`, others `-1`), `aria-selected` on the selection,
  `aria-disabled` on disabled days.
- **Keyboard:** Arrow keys = ±1 day / ±1 week; PageUp/PageDown = ±month; Shift+PageUp/Down =
  ±year; Home/End = start/end of week; Enter/Space selects; **Esc** closes and restores focus to
  the trigger. Crossing a month edge with arrows triggers an htmx grid swap, after which focus
  lands on the equivalent day (the base class's `rhx-focus-after-swap` supports this).
- Time list = `role="listbox"` / `option`, arrow + type-ahead, Enter selects, Esc closes.
- Range preview is purely visual; start/end are announced on commit. Honors
  `prefers-reduced-motion`.

---

## 9. Components, isolation & phased build

| Unit | Responsibility |
|---|---|
| `CalendarRenderer` (server) | Pure date math → grid/month/year view markup (the §3.4 contract). Backs the endpoint and the public `RhxCalendar.RenderMonth` helper. No HTTP/DI coupling. |
| `/_rhx/calendar` endpoint | Parse query params → call `CalendarRenderer` → return grid HTML. Registered in `UsehtmxRazor()`. |
| `DatePickerTagHelper` | Input + trigger + hidden value + initial calendar popup; wires htmx nav to the endpoint (or override). |
| `TimePickerTagHelper` | Input + trigger + static time-list popup. |
| `DateTimePickerTagHelper` | Composes calendar + time list in one popup. |
| `DateRangePickerTagHelper` | Two-calendar popup + presets + dual hidden inputs. |
| `rhx-calendar.css` + per-component CSS | Shared grid/popup styling + small per-component bits, `@layer rhx.components`, tokens only. |
| `rhx-datepicker.js` | One module (registered via `RHX.register`) for all four: open/close, day/time select, keyboard, range hover-preview, re-init after htmx grid swaps. |

**Build order (each an independent milestone + its own implementation plan):**
1. **Calendar core + `/_rhx/calendar` endpoint + `rhx-date-picker`** — the foundation.
2. **`rhx-time-picker`** (static time list).
3. **`rhx-datetime-picker`** (composes #1 + #2).
4. **`rhx-date-range-picker`** (two calendars + presets + range state).

---

## 10. Testing

- **Unit (`htmxRazor.Tests`, `TagHelperTestBase`):** each Tag Helper's markup — visible input,
  hidden ISO value(s), trigger, popup scaffold, `rhx-min/max/week-start/format/step/12hour`
  applied, disabled/readonly, model binding for `DateOnly`/`DateTime`/`TimeOnly`, range's two
  hidden inputs. **`CalendarRenderer`** gets focused tests: day layout for a given month +
  week-start, leading/trailing (muted) days, today/selected/in-range/disabled flags, month and
  year views, range state. Endpoint param parsing + `RhxCalendar.RenderMonth` helper output.
- **Playwright E2E (`PlaywrightTests`, Chromium locally / all browsers in CI):** open popup;
  navigate months via htmx swap; pick a day → hidden ISO + display update + close; keyboard grid
  navigation; time-list select; datetime day+time commit; range start→end with in-range
  highlight + a preset.

---

## 11. Docs & ancillary (per milestone)

- Demo page under `Pages/Docs/Components/` (`_ComponentPage` layout) + **Forms** sidebar entry.
- Register new CSS/JS in `_DocsLayout` and `_Layout`.
- README catalog (Forms row), `CHANGELOG.md` (2.1.0 section), VS Code snippet per component.

---

## 12. Out of scope (YAGNI)

- Multiple/non-Gregorian calendars, lunar/Hijri, fiscal calendars.
- Recurring-date / cron pickers.
- Time zones beyond the bound `DateTime`'s kind (no tz selector).
- Inline (always-open, non-popup) variants — popup only for now.
- Seconds precision in the time list (minute granularity via `rhx-step`).
