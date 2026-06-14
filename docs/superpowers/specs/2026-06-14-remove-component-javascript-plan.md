# Plan: Remove all component JavaScript (htmx + Razor only)

**Date:** 2026-06-14
**Goal:** Eliminate every file under `htmxRazor/Assets/js/components/` (41 files, ~6.9k LOC)
and ideally `rhx-core.js` + `rhx-position.js` too, so the solution ships **no custom
JavaScript** — only htmx (and the platform: HTML, CSS, ASP.NET Core Razor Pages).
JavaScript is allowed only where the platform offers no alternative, and each such case
must be justified here.

## Guiding principle — a strict capability ladder

For every component, use the **highest** rung that delivers the behavior:

1. **Native HTML element** — `<dialog>`, `<details>`, `<select>`, `<input type=date|time|color|range|file|number|search>`, `<datalist>`, `<progress>`, `<meter>`, form validation.
2. **CSS only** — `:target`, `:checked`, `:focus-within`, `:has()`, sibling combinators, the **Popover API** (`popover` + `popovertarget`), **CSS Anchor Positioning**, `field-sizing: content`, `accent-color`, scroll-snap carousels (`::scroll-button`, `::scroll-marker`), `resize`, `@starting-style` + `transition-behavior: allow-discrete` for enter/exit animation, declarative **invoker commands** (`command` / `commandfor`).
3. **htmx** — server round-trips for anything that needs server state, lists, filtering, validation, pushed updates (SSE).
4. **JavaScript** — only the irreducible cases below, each with a written justification.

Modern baseline features (Popover API, invoker commands, CSS anchor positioning,
`field-sizing`, scroll-snap pseudo-elements) are what make this feasible without JS. Their
browser support is the central risk — see *Risks*.

## Why this is realistic — and where it bites

A large majority of the library is either already static or maps cleanly onto native
elements / CSS / htmx. The honest costs concentrate in a few places:

- **Custom-styled form widgets** (`select`, `combobox`, the four date/time pickers,
  `color-picker`) — replacing them with **native controls** removes 100% of their JS and
  gives perfect built-in accessibility, but changes their **appearance** to the browser's
  native rendering and drops some custom features (server-rendered calendar, multi-column
  range picker, in-cell range painting).
- **APG keyboard niceties** on CSS-only `tabs`/`dropdown`/`tree` (roving tabindex, type-ahead,
  arrow-key wrapping) are reduced to what native focus + `:checked`/`<details>` provide.
- A handful of components are **genuinely JS** (clipboard, drag-and-drop, realtime sockets,
  free-form pan/zoom). For each we either keep a tiny justified script or redesign to an
  htmx equivalent.

## Component disposition

Legend for **Strategy**: `Native` = native element, `CSS` = CSS-only, `htmx` = server
round-trip, `Server` = render output on the server (no client work), `JS*` = JavaScript
that must remain (justified below), `Remove` = behavior disappears with no replacement needed.

| Component | Today (JS does…) | Strategy | Replacement |
|---|---|---|---|
| dialog | open/close, backdrop, htmx open | **Native + CSS** | `<dialog>` + invoker `command="show-modal"`/`commandfor`; close via `<form method="dialog">` or `command="close"`; backdrop-close via CSS. *Server-initiated modal open* → see JS\* #1 |
| drawer | open/close, focus trap | **CSS** | `popover` + `popovertarget`, or `:has(:checked)`; `@starting-style` for slide-in |
| details | (none) | **Native** | already `<details>`/`<summary>` |
| dropdown | toggle, kbd nav, dismiss, flip | **CSS (Popover)** | `popover` + `popovertarget` + CSS anchor positioning. Arrow-key menu nav lost (Tab works) |
| popover | click/hover/focus open, position | **CSS (Popover)** | `popover` API + anchor positioning. Hover trigger dropped (click/focus only) |
| popup | positioning engine | **CSS** | CSS anchor positioning; delete `rhx-position.js` |
| tooltip | show/hide, position | **CSS** | `:hover`/`:focus-visible` + anchor positioning (or `popover="hint"`) |
| tabs | activate, ARIA, kbd | **CSS or htmx** | radio-input `:checked` tabs (CSS) **or** htmx panel swap (already used in example) |
| tree | expand/collapse, kbd, select | **Native + htmx** | nested `<details>` for expand/collapse; htmx for lazy children/selection |
| select | listbox, kbd, type-ahead, multi | **Native** | `<select>` / `<select multiple>` (zero JS, full a11y); style via `appearance: base-select` where supported |
| combobox | filter, kbd, select | **Native or htmx** | `<input list>` + `<datalist>` (native autocomplete) **or** htmx active-search list |
| date/daterange/datetime/time pickers | calendar popup, selection, commit | **Native (recommended) or htmx** | `<input type=date\|datetime-local\|time>` (zero JS) **or** keep server calendar driven entirely by htmx (round-trip per click). Range = two date inputs |
| color-picker | HSV math, drag | **Native** | `<input type=color>` (rich drag UI dropped) |
| slider | fill, value tooltip | **Native + CSS** | `<input type=range>` + `accent-color`; live value bubble dropped (or `<output>` — see JS\* note) |
| rating | click/hover/kbd | **CSS** | reversed radio-input stars with `:checked ~`/`:hover ~` |
| input | clear, password toggle, autosize, steppers | **CSS/Native + JS\*** | steppers → `type=number`; clear → `type=search`; **password toggle** → JS\* #2; **textarea autosize** → `field-sizing: content` + JS\* #9 fallback (temporary, Firefox) |
| file-input | DnD highlight, preview, size check | **Native** | `<input type=file>` + `accept`; DnD/preview dropped (or JS\* if wanted) |
| validation | client error display | **Native + htmx** | HTML5 constraints (`required`, `pattern`, `:user-invalid`) + server validation via htmx partials |
| callout | dismiss, auto-dismiss | **htmx + CSS** | dismiss → `hx-delete`+`hx-swap="delete"` (or CSS `:has(:checked)`); auto-dismiss → CSS timed animation |
| toast | create from event, auto-dismiss, stack | **htmx + CSS** | server pushes OOB toast markup; self-remove via `hx-trigger="load delay:Ns"` + empty swap; stack via CSS |
| carousel | nav, pagination, autoplay, drag | **CSS + JS\*** | scroll-snap + `::scroll-marker` (cross-browser); **prev/next buttons** → JS\* #10 (temporary) until `::scroll-button()` is cross-browser. **Autoplay dropped** (or JS\* if required) |
| comparison | drag handle | **CSS** | range-input overlay technique (CSS-only before/after) |
| split-panel | drag divider | **CSS** | `resize: horizontal`/`vertical` + `min/max` |
| scroller | scroll buttons | **CSS + JS\*** | overflow scroll (cross-browser); **scroll buttons** → JS\* #10 (temporary) until `::scroll-button()` is cross-browser |
| zoomable-frame | pan/zoom | **CSS (limited) or JS\*** | `overflow:auto` + pinch; smooth pan/zoom → JS\* if required |
| animated-image | play/pause toggle | **CSS** | play on `:hover`/`:focus`, or `<video>`/animated source |
| animation | apply CSS anim from attrs | **CSS** | plain CSS animations + htmx swap classes |
| qr-code | client canvas QR generation | **Server** | generate QR as inline **SVG/`<img>` in C#** (no client work). Re-render on change via htmx |
| relative-time | periodic refresh | **Server (+ htmx)** | render on server; optional `hx-trigger="every 60s"` refresh |
| copy-button | clipboard write | **JS\*** | JS\* #3 — no HTML/CSS/htmx clipboard write exists |
| kanban | drag-and-drop + htmx | **htmx (redesign) or JS\*** | replace DnD with htmx **move buttons** (←/→/↑/↓ → `hx-post`) = zero JS; keep DnD only as JS\* #4 if drag is required |
| radial-select | pie kbd nav, cascade | **htmx (redesign) or JS\*** | cascade already htmx; the pie/keyboard interaction is JS\* #5 unless redesigned to nested native `<select>`s |
| command-palette | Cmd+K, search | **htmx + JS\*** | `<dialog>`/popover + htmx search; global **Cmd+K** shortcut → JS\* #6 (or drop the shortcut → zero JS) |
| signalr | hub connection, swap | **htmx (SSE) or JS\*** | replace with **htmx SSE extension** for server→client push (zero custom JS) **or** keep as JS\* #7 if SignalR groups/transports are required |
| optimistic | optimistic toggle | **Remove** | htmx swap is the source of truth |
| htmx-form / wizard | form orchestration | **htmx** | htmx attributes + server step state |
| theme (in core) | toggle dark mode | **CSS or JS\*** | CSS-only via `<input>` + `:has()` + `prefers-color-scheme`; persistence needs JS\* #8 (else session/cookie via server) |

## The JavaScript that must remain (the case for each)

If we take the recommended path, **custom JS collapses to a tiny, opt-in surface**. Ranked by
how unavoidable it is:

1. **Server-initiated modal open** — `dialog.showModal()` cannot be triggered declaratively
   from a server response; `<dialog open>` is non-modal. *Options:* (a) ~10-line shim that
   listens for an `HX-Trigger` and calls `showModal()`; (b) only support user-click opens
   (invoker commands, zero JS) and render server-driven dialogs inline. **Recommend (b)** to
   stay JS-free; offer (a) as an opt-in snippet.
2. **Password visibility toggle** — toggling `<input type>` between password/text needs JS.
   *Tiny (~5 lines).* Alternative: drop the toggle.
3. **Copy-to-clipboard** — `navigator.clipboard.writeText` has no HTML/CSS/htmx equivalent.
   *Irreducible if the feature exists.* Alternative: drop the component.
4. **Kanban drag-and-drop** — HTML Drag and Drop requires JS handlers to fire htmx.
   **Recommend redesign to htmx move buttons (zero JS)**; keep DnD only if drag is a hard
   requirement.
5. **Radial-select pie** — radial keyboard/pointer menu can't be expressed in HTML/CSS.
   **Recommend redesign** (category `<select>` → cascading htmx) to remove it; otherwise JS.
6. **Command-palette Cmd+K** — a global key shortcut needs JS. **Recommend button-triggered
   open (zero JS)**; keep the shortcut only as an opt-in.
7. **SignalR** — the SignalR client is JS by nature. **Recommend the htmx SSE extension** for
   push (covered by "htmx ecosystem"); keep `rhx-signalr` only for apps that need SignalR
   groups/WebSocket transports.
8. **Theme persistence** — remembering dark mode across visits needs storage. **Recommend a
   server cookie** (zero client JS) set via htmx; the toggle itself is CSS (`:has`).
9. **Textarea autosize (TEMPORARY)** — `field-sizing: content` grows a textarea to its content
   with zero JS, but Firefox doesn't support it yet (spike, 2026-06-14). *Sanctioned temporary
   JS* (a tiny input-listener that sets height to scrollHeight) until `field-sizing` is
   cross-browser, at which point the CSS replaces it and the script is deleted. The CSS is
   shipped now so supporting browsers already get the no-JS path; the script is the fallback.
10. **Carousel / scroller prev-next buttons (TEMPORARY)** — scroll-snap scrolling is fully
    cross-browser, but the declarative `::scroll-button()` prev/next controls are Chromium-only
    (spike, 2026-06-14). *Sanctioned temporary JS* (click handlers that call
    `element.scrollBy()`) until `::scroll-button()` is cross-browser, then the CSS replaces it.
    Scrolling/swiping works without JS regardless; only the buttons need it.

> **Net:** with the recommended choices, the *always-on* custom JS is the **copy-button** (3)
> and the **password toggle** (2) — well under ~20 lines — plus two **temporary** platform-gap
> shims for **textarea autosize** (9) and **carousel/scroller buttons** (10), and whatever
> opt-in shims an app explicitly includes (1, 6). The temporary shims are isolated and tracked
> for removal once `field-sizing` / `::scroll-button()` ship cross-browser. Everything else
> becomes native HTML, CSS, htmx, or a server-side render. `rhx-position.js` and `rhx-core.js`
> are deleted (the two temporary shims ship as self-contained, narrowly-scoped files).

## Decisions (locked 2026-06-14)

1. **Native rendering accepted** for `select`, date/time/color inputs etc. (zero JS, native look).
2. **All four htmx redesigns approved:** kanban → htmx move buttons; command-palette → button-open (no Cmd+K); signalr → htmx SSE; radial-select → cascading native `<select>`s.
3. **htmx extensions count as "htmx"** (SSE/WS extensions allowed).
4. **Proceed via a Phase 0 spike** before the full migration.
5. **Two temporary JS holdouts are sanctioned** for the platform gaps the spike found —
   **textarea autosize** and **carousel/scroller prev-next buttons** — until a CSS/native
   solution lands cross-browser (`field-sizing` in Firefox; `::scroll-button()` in
   Firefox/WebKit). These are explicitly *temporary*: tracked, isolated, and removed once the
   platform catches up. (See JS\* #9 and #10.)

With these, the *always-on* custom JS target is **copy-button** + **password toggle**, plus
the two temporary holdouts (**textarea autosize**, **carousel/scroller buttons**) and optional
per-app shims. `rhx-core.js`, `rhx-position.js`, `rhx-signalr.js`, `rhx-kanban.js`,
`rhx-radial-select.js`, `rhx-command-palette.js` all go away.

## Original open decisions (now resolved above)

1. **Native vs custom appearance** — Accept browser-native rendering for `select`,
   date/time/color inputs (zero JS, perfect a11y) instead of custom-styled widgets? This is
   the single biggest visual change. *(If "no", those components stay htmx round-trip or
   remain JS.)*
2. **Accessibility/feature trade-offs** — OK to lose APG keyboard extras on CSS-only
   tabs/dropdown/tree, the rich color picker, carousel autoplay, file DnD, and the
   server-rendered calendar?
3. **Kanban** — redesign to htmx move buttons (zero JS) or keep drag-and-drop (JS)?
4. **Realtime** — replace SignalR with the htmx SSE extension, or keep `rhx-signalr` as a
   sanctioned JS integration?
5. **"htmx only" boundary** — do **htmx extensions** (SSE, etc.) count as allowed "htmx", or
   must even those be avoided?

## Phase 0 results (spike, 2026-06-14)

Ran a platform-feature probe across **Chromium, Firefox, WebKit** (Playwright's current
engines — note these are bleeding-edge/Nightly, so they read *optimistically* vs. stable
releases; confirm against Baseline/caniuse for the chosen floor).

| Feature | Chromium | Firefox | WebKit | Verdict |
|---|---|---|---|---|
| `<dialog>` | ✅ | ✅ | ✅ | safe |
| invoker `command`/`commandfor` (opens modal, no JS) | ✅ | ✅ | ✅ | safe |
| Popover API (`popover`/`popovertarget`) | ✅ | ✅ | ✅ | safe |
| CSS anchor positioning | ✅ | ✅* | ✅ | safe (*FF Nightly; verify on stable) |
| `:has()` | ✅ | ✅ | ✅ | safe |
| `accent-color` | ✅ | ✅ | ✅ | safe |
| `field-sizing: content` (textarea autosize) | ✅ | ❌ | ✅ | **gap: Firefox** |
| `::scroll-button()` (carousel/scroller nav) | ✅ | ❌ | ❌ | **gap: FF + WebKit** |

**Conclusion:** the core of the plan (native dialog via invoker commands, Popover-API
dropdowns/menus, native form controls, anchor positioning, `:has()` CSS state) is viable on
current evergreen engines. Two specific behaviors are **not** cross-browser CSS-only yet, and per Decision #5 each gets a
**sanctioned, temporary JS shim** (removed when the platform catches up):

- **Textarea autosize** — `field-sizing` missing in Firefox → JS\* #9 fallback (CSS still ships
  for supporting browsers).
- **Carousel / scroller prev-next buttons** — `::scroll-button()` Chromium-only → JS\* #10 for
  the buttons (scroll-snap scrolling/swiping itself needs no JS).

**Support floor:** target recent evergreen (roughly the last ~12-18 months of Chrome/Edge,
Firefox, Safari). Older browsers degrade (non-positioned popovers, inert dialog buttons).
This must be stated in the library README and is a prerequisite for Phases 2-3.

## Execution phases (after decisions)

- **Phase 0 — Spike & guardrails.** Add a CI grep/test that fails if any `.js` appears
  under `Assets/js/` **outside a small, explicit allowlist** — the sanctioned scripts only
  (copy-button, password toggle, and the two *temporary* holdouts: textarea autosize,
  carousel/scroller buttons). Each allowlisted file carries a header comment stating why it
  exists and (for the temporary ones) the platform feature whose cross-browser support retires
  it. Stand up a feature-detection note for the modern CSS/HTML features. Pick the 3 hardest
  components and prototype them end-to-end to validate the approach before committing.
- **Phase 1 — Free wins (Remove/Server/Native-static). ✅ DONE (2026-06-14).** Removed JS for
  all 7: animation (server-emitted CSS `animation`), relative-time (server text), optimistic
  (htmx `.htmx-request` CSS), htmx-form (`.htmx-request` + CSS `:empty` + `hx-on` reset),
  wizard, animated-image (CSS poster/hover model), and qr-code (ported the client encoder to a
  dependency-free C# `QrCodeGenerator` rendering inline SVG, proven byte-for-byte equivalent to
  the JS across 5 inputs × 4 EC levels). Component scripts: 39 → 32. Verified: solution builds,
  1921 unit tests pass, QR + AnimatedImage E2E (24) pass on chromium/firefox/webkit.
- **Phase 2 — CSS-only.** tooltip, drawer, dropdown, popover, popup, tabs, rating,
  comparison, split-panel, scroller, callout, slider, carousel. Delete JS + `rhx-position.js`.
- **Phase 3 — Native form controls.** select, combobox, date/time/datetime/range, color,
  file, input, validation. (Gated by Decision #1.)
- **Phase 4 — htmx server interactions.** tree, toast, command-palette, kanban (per
  Decision #3), radial-select (per redesign), signalr (per Decision #4).
- **Phase 5 — Core teardown.** Remove the registration/init machinery in `rhx-core.js`;
  fold the theme toggle into CSS + cookie. Delete the per-component CSS/JS includes from the
  asset pipeline and demo/example layouts. Update docs, the Tag Helpers that emit
  `data-rhx-*` JS hooks, and the VS Code snippets.
- **Each phase:** update the affected Tag Helpers (stop emitting JS-hook attributes, emit the
  native/CSS/htmx markup instead), update demo + example pages, and **verify in a browser**
  (the project's Playwright suite is the regression net — many E2E tests assert current
  JS-driven behavior and will need rewriting to the new interaction model).

## Risks & realities

- **Browser support is the gating risk.** Popover API, invoker commands (`command`/
  `commandfor`), CSS anchor positioning, `field-sizing`, and scroll-snap pseudo-elements are
  recent. On older browsers these degrade (e.g., a popover that doesn't position, a dialog
  button that does nothing). Decide a support floor before Phase 2.
- **This is a library API change, not just an internal refactor.** Tag Helpers currently emit
  `data-rhx-*` hooks and rich markup; consumers' pages and the 1,900-test suite assume the
  current behavior. Expect broad churn in Tag Helpers, CSS, demo, example, snippets, and
  tests. Consider whether this is a **major (3.0) breaking release**.
- **Some components lose capability**, not just implementation (autoplay, hover-popover,
  rich color picker, drag kanban, server calendar, Cmd+K). Each is called out above.
- **Accessibility can improve or regress** depending on the rung: native controls *improve*
  a11y; CSS-only `:checked` tabs/menus *regress* it versus the current APG implementations.

## Recommendation

The goal is achievable to **near-zero custom JS** — only clipboard + password toggle as
permanent shims, plus two **temporary** platform-gap shims (textarea autosize,
carousel/scroller buttons; Decision #5) that retire when `field-sizing` / `::scroll-button()`
ship cross-browser — **if** you accept native rendering for form controls (Decision #1) and
the htmx-redesign of kanban/radial/command-palette/signalr. If native appearance is unacceptable, the realistic
floor is higher because custom listbox/picker widgets can't keep their look *and* drop JS.
Treat this as a **major, breaking release** and validate the modern-platform features against
a defined browser floor in Phase 0 before committing.
