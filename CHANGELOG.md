# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [2.1.0] — Advanced Inputs

### Added
- **Radial Select** — `<rhx-radial-select>` + `<rhx-radial-option>`. A rectangular icon trigger flush-left against a dropdown opens a circular SVG pie of category wedges (color + icon). Selecting a wedge fires its `hx-get` (issued via `htmx.ajax`), swaps the dropdown's option set, echoes the category color + icon onto the trigger, and auto-selects the first option. Wedge colors use named variant tokens (`brand`/`success`/`warning`/`danger`/`neutral`) with a deterministic cycle (`brand → success → warning → danger → neutral`) when omitted, and an invalid color falls back to that cycle. Icons resolve through `IconRegistry`. Optional `rhx-category-name` submits the active category; `rhx-default-category` activates a wedge on initial render (checked + trigger echo). Accessible as a menu: trigger `aria-haspopup="menu"`/`aria-expanded`, wedges `role="menuitemradio"` + `aria-checked`, arrow-key + Home/End + type-ahead navigation, Enter/Space to select, Escape to close and restore focus, roving `tabindex`; honors `prefers-reduced-motion`. Pie placement reuses `rhx-position.js`. Demo page at `/Docs/Components/RadialSelect`.
- **Date Picker** — `<rhx-date-picker>`: a text input + popup calendar. The month grid is server-rendered; prev/next and the clickable month/year label navigate via htmx against a built-in `/_rhx/calendar` endpoint (registered by `UsehtmxRazor()`), overridable per app via `hx-get` + `RhxCalendar.RenderMonth(...)`. Day selection commits a hidden ISO `yyyy-MM-dd` value for model binding; the visible input shows the culture short date. Configurable `rhx-min`/`rhx-max`, `rhx-week-start` (default Monday), `rhx-format`, Today/Clear. APG grid accessibility (roving tabindex, arrow/PageUp-Down nav that skips disabled days, Esc).
- **Time Picker** — `<rhx-time-picker>`: a text input + popup list of selectable times (from `rhx-min` to `rhx-max`, stepping by `rhx-step` minutes, default 30). Commits a hidden ISO `HH:mm` value for model binding (`TimeOnly`/`DateTime`); the visible input shows 12-hour (`9:30 AM`, default) or 24-hour (`09:30`, `rhx-12hour="false"`) display, or a custom `rhx-format`. Listbox accessibility (`role="combobox"`/`listbox`/`option`, `aria-activedescendant`, Down/Up/Home/End/Enter/Escape + type-ahead). Static list — no server round-trip.
- **Date Range Picker** — `<rhx-date-range-picker>`: a two-date range control with two side-by-side months (synced prev/next via `/_rhx/calendar-range`), live in-range hover preview, and quick presets (today, last7, last30, thismonth, lastmonth, yesterday). Commits two hidden ISO `yyyy-MM-dd` inputs (`rhx-start-name`/`rhx-end-name`); first click sets the start, second sets the end (auto-swapped if earlier). Range highlighting is painted client-side over the reused calendar grids. Configurable `rhx-min`/`rhx-max`, `rhx-week-start`, `rhx-format`.

### Components Added
- **Forms**: Radial Select, Radial Option, Date Picker, Time Picker, Date Range Picker

### Tests
- 1,909 unit tests (18 new), all passing. New Playwright E2E coverage (`RadialSelectTests`) for open → select wedge → cascade → auto-select-first → focus restore.

## [2.0.0] — Platform & DX

### Added
- **Theme Builder** — Interactive page at `/ThemeBuilder` in the demo site with color pickers and text inputs for all `--rhx-*` design tokens. Real-time preview panel shows curated sample components updating as you change values. Smart HSL palette generation from a single color pick. Download a `rhx-theme-overrides.css` file containing only the tokens you changed.
- **Interactive Component Playground** — Property toggle panel on component demo pages. Users change variant, size, and state props via dropdowns, checkboxes, and text inputs; the server re-renders the component preview with updated markup via htmx. URL-driven state makes playground configurations shareable. Generated Razor markup shown below the preview. Button implemented as reference; remaining components follow the same pattern.
- **SignalR Hub Connector** — `<rhx-signalr>` Tag Helper wrapping SignalR hub connections. Attributes: `hub-url`, `rhx-method`, `rhx-swap`, `rhx-target`, `rhx-reconnect`, `rhx-transport` (websockets/sse/longpolling), `rhx-groups` (comma-separated group names), `rhx-connection-state` (visual indicator). Received HTML fragments are swapped using htmx's programmatic swap API. Connection state reflected via CSS classes (`rhx-signalr--connected`, `--disconnected`, `--reconnecting`) with optional pulse indicator. Server-side `HtmxSignalRExtensions` provides `SendHtmlAsync()`, `SendHtmlToGroupAsync()`, `SendHtmlToConnectionAsync()`, and `SendHtmlToOthersAsync()` on `IHubContext<THub>`.
- **CSS Anchor Positioning** — Tooltip, popover, popup, and dropdown components now use the CSS Anchor Positioning API (`anchor-name`, `position-anchor`, `position-area`, `position-try-fallbacks`) when the browser supports it, with automatic fallback to the existing JavaScript positioning engine (`rhx-position.js`). Feature detection via `RHX.supportsAnchorPositioning` and `@supports (anchor-name: --x)`. Dropdown gains full CSS `position-area` placement mappings. Reduces JS footprint and improves edge-of-viewport correctness in modern browsers.
- **VS Code Snippet Extension** — `vscode-extension/` directory with 97 snippets for all `<rhx-*>` tags, scoped to `html`, `razor`, and `aspnetcorerazor` languages. Each snippet includes curated tabstops with choice lists for common attribute values (e.g., variant, appearance, size). Includes htmx-specific variants (`rhx-button-htmx`, `rhx-input-htmx`) and multi-component composition snippets (`rhx-card-full`, `rhx-tab-group`).
- **Kanban Board** — `<rhx-kanban>`, `<rhx-kanban-column>`, and `<rhx-kanban-card>` Tag Helpers. Native HTML Drag and Drop with optimistic DOM moves. Card drops fire `hx-post` with `cardId`, `sourceColumn`, `targetColumn`, and `position` values via `htmx.ajax()`. Column features: `rhx-title` header, `rhx-max-cards` WIP limit with visual indicator (`rhx-kanban-column--wip-exceeded`), `rhx-droppable` flag. Card features: `rhx-card-id`, `rhx-variant` color coding (brand/success/warning/danger), `rhx-draggable`. Full keyboard navigation (Enter/Space to grab, Arrow keys to move between columns or reorder, Escape to cancel). Responsive layout stacks columns vertically on narrow screens.
- **Dialog Size Variants** — `rhx-size` attribute on `<rhx-dialog>` with preset sizes (`small` 24rem, `medium` 32rem, `large` 48rem, `full` 90vw) and custom CSS width support (e.g., `rhx-size="40rem"`). Dialog now expands to fit content up to 48rem by default (previously capped at 32rem).
- **`palette` icon** — Added to `IconRegistry` for design/theming contexts.

### Components Added
- **Patterns**: SignalR Connector
- **Organization**: Kanban, Kanban Column, Kanban Card

### Tests
- 1,838 total tests (36 new), all passing.

## [1.3.0] — Data Table, Accessibility & Modern CSS

### Added
- **Data Table**: `<rhx-data-table>`, `<rhx-column>`, `<rhx-data-table-pagination>` with server-driven sort, filter, and pagination. Sort headers emit `hx-get` with query parameters; server returns partials. Includes `DataTableRequest` model binder for easy handler integration, `DataTableRequestModelBinderProvider` auto-registered via `AddhtmxRazor()`.
- **Command Palette**: `<rhx-command-palette>` modal search overlay activated via Cmd+K / Ctrl+K. Fires debounced `hx-get` to a configurable search endpoint. Results grouped with `<rhx-command-group>` and `<rhx-command-item>` with keyboard navigation and ARIA combobox/listbox pattern.
- **Skip Nav & Landmarks**: `<rhx-skip-nav>` (visually hidden, visible on focus) and `<rhx-landmark>` for semantic landmark regions, addressing WCAG 2.4.1 Bypass Blocks.
- **Focus Management After Swaps**: `rhx-focus-after-swap` attribute on the base class to move focus after htmx swaps. On by default (`"first"`) for `<rhx-dialog>` and `<rhx-drawer>`, addressing WCAG 2.4.3 Focus Order.
- **Container Queries**: Card, dialog, split panel, and data table CSS updated to use `@container` queries for container-responsive layouts. New `.rhx-container` utility class.
- **Icons**: Added `cursor`, `grid`, `layers`, `table`, `settings` icons to `IconRegistry`.

### Changed
- **APG Keyboard Audit**: Tabs, tree, dropdown, and combobox audited against W3C ARIA Authoring Practices Guide keyboard patterns.
  - Tree: Added type-ahead search (single character) and `*` to expand all siblings.
  - Dropdown: `ArrowUp` on trigger now opens menu and focuses last item (per APG). Added type-ahead search.
  - Combobox: Added `Alt+ArrowDown` (open without moving focus) and `Alt+ArrowUp` (close).
  - Dropdown items now render `tabindex="-1"` per APG menu pattern.
- **Play/pause icons** in `IconRegistry` and animated image JS now use `fill="currentColor"` for proper visibility.
- **Animated image** CSS uses opacity-based toggling instead of display toggling to prevent duplicate image on pause.
- **Drawer** CSS adds transition delay on `visibility` for smooth close animation.
- **Dialog** CSS uses `position: fixed` with `top: 50%; left: 50%; transform: translate(-50%, -50%)` for reliable centering.

### Fixed
- Dialog not centering on page (CSS cascade layer conflict with reset `* { margin: 0 }`).
- Switch htmx demo checking `"on"` instead of `"true"` for checkbox value.
- Checkbox htmx demo had the same `"on"` vs `"true"` bug.
- Callout htmx demo returning empty HTML instead of dismissal confirmation.
- Progress bar htmx demo returning raw Tag Helper tags in `Content()` strings (Tag Helpers don't process outside Razor).
- Animated image showing duplicate on pause and not unpausing correctly.
- Drawer lacking smooth close animation.
- Example project tabs not showing tasks in Active/Completed tabs (panels not sharing `#todo-list` container).

### Components Added
- **Data Display**: Data Table, Column, Data Table Pagination
- **Overlays**: Command Palette, Command Group, Command Item
- **Navigation**: Skip Nav, Landmark

### Tests
- 1,652 total tests (216 new), all passing.

## [1.2.0] — Notifications, Pagination & Quick Wins

### Added
- **Toast Notification System**: `<rhx-toast-container>` and `<rhx-toast>` components with auto-dismiss, severity variants, stacking, and `aria-live` announcements. Server-side `HxToast()` and `HxToastOob()` extension methods for triggering toasts from handlers.
- **Pagination**: `<rhx-pagination>` component with htmx-powered page navigation, ellipsis for large page counts, first/last/prev/next buttons, size variants, and page info display.
- **ARIA Live Region Manager**: `<rhx-live-region>` wrapper component for screen reader announcements on htmx swaps. Supports politeness levels, atomic updates, and visually-hidden mode.
- **`hx-on:*` Dictionary Attribute**: Dictionary attribute on `htmxRazorTagHelperBase` for htmx 2.x event handler attributes (e.g., `hx-on:after-request`). Refactored `InfiniteScrollTagHelper` to use it.
- **View Transition Support**: `rhx-transition` and `rhx-transition-name` attributes on the base class for smooth animated page transitions via the View Transitions API.

### Changed
- **CSS Cascade Layers**: All component CSS wrapped in `@layer rhx.components { }`, core CSS in corresponding layers (`rhx.reset`, `rhx.tokens`, `rhx.core`, `rhx.utilities`, `rhx.theme`). Host app styles automatically override component styles without specificity wars.
- `htmxRazorTagHelperComponent` now injects `@layer` order declaration before CSS links.
- `FormControlTagHelperBase.BuildHtmxAttributeString()` now forwards `hx-on:*` attributes to inner elements.

### Components Added
- **Feedback**: Toast, Toast Container
- **Navigation**: Pagination
- **Utilities**: Live Region

## [1.1.0]

### Added
- Complete Tag Helper component library with 72 components across 10 categories
- CSS design system with light/dark themes via CSS custom properties
- First-class htmx attribute support on every component
- ASP.NET Core model binding integration for form controls
- Embedded CSS/JS assets served from `/_rhx/` middleware path
- Auto-injection of design tokens, reset CSS, and htmx script via `AddhtmxRazor()`
- 1,436 unit tests for Tag Helper rendering
- Documentation site with Getting Started guide and component reference pages
- GitHub Actions CI/CD workflows (build, release, demo deployment)

### Components by Category
- **Actions**: Button, Button Group, Dropdown
- **Forms**: Input, Textarea, Select, Combobox, Checkbox, Switch, Radio, Slider, Rating, Color Picker, File Input, Number Input
- **Feedback**: Callout, Badge, Tag, Spinner, Skeleton, Progress Bar, Progress Ring, Tooltip
- **Navigation**: Tabs, Breadcrumb, Tree, Carousel
- **Organization**: Card, Divider, Split Panel, Scroller
- **Overlays**: Dialog, Drawer, Details
- **Imagery**: Icon (43 built-in), Avatar, Animated Image, Comparison, Zoomable Frame
- **Formatting**: Format Bytes, Format Date, Format Number, Relative Time
- **Utilities**: Copy Button, QR Code, Animation, Popup, Popover
- **Patterns**: Active Search, Infinite Scroll, Lazy Load, Poll

## Milestones

### v0.1.0 — Minimum Viable Library
- Button, Input, Validation
- Core design system (tokens, reset, utilities)
- htmx integration infrastructure
- NuGet package with embedded assets

### v0.2.0 — Form Controls + Feedback
- All form components (Textarea, Select, Combobox, Checkbox, Switch, Radio, Slider, Rating, Color Picker, File Input, Number Input)
- Feedback components (Callout, Badge, Tag, Spinner, Skeleton, Progress Bar, Progress Ring, Tooltip)

### v0.3.0 — Navigation + Organization
- Tabs, Breadcrumb, Tree, Carousel
- Card, Divider, Split Panel, Scroller

### v0.4.0 — Imagery + Utilities + Data Viz
- Icon, Avatar, Animated Image, Comparison, Zoomable Frame
- Copy Button, QR Code, Animation, Popup, Popover
- Sparkline

### v0.5.0 — Composite Patterns
- Active Search, Infinite Scroll, Lazy Load, Poll

### v1.0.0 — Stable Release
- Full component catalog with stable API
- Comprehensive documentation site
- Theme customization system
- Accessibility audit complete
