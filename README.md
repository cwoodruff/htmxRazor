# htmxRazor

[![NuGet](https://img.shields.io/nuget/v/htmxRazor.svg)](https://www.nuget.org/packages/htmxRazor)
[![CI](https://github.com/cwoodruff/htmxRazor/actions/workflows/ci.yml/badge.svg)](https://github.com/cwoodruff/htmxRazor/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.
microsoft.com/)

<a href="https://www.producthunt.com/products/htmxrazor?embed=true&amp;utm_source=badge-featured&amp;utm_medium=badge&amp;utm_campaign=badge-htmxrazor" target="_blank" rel="noopener noreferrer"><img alt="htmxRazor - Server-rendered UI components for ASP.NET Core with htmx | Product Hunt" width="250" height="54" src="https://api.producthunt.com/widgets/embed-image/v1/featured.svg?post_id=1089741&amp;theme=light&amp;t=1774441690528"></a>

**Server-rendered UI components for ASP.NET Core with first-class htmx integration.**

htmxRazor is a complete UI component library implemented as Razor Tag Helpers. Every component renders clean, semantic HTML styled by its own CSS design system. No client-side framework required — just add htmx attributes to any component for dynamic behavior.

## Why htmxRazor?

- **Server-rendered** — No JavaScript framework. Components are Tag Helpers that render HTML on the server.
- **htmx-native** — Every component supports `hx-get`, `hx-post`, `hx-target`, `hx-swap`, and all htmx attributes directly.
- **85+ components** — Buttons, forms, data tables, dialogs, command palette, kanban board, tabs, trees, carousels, toasts, pagination, and more across 11 categories.
- **Design tokens** — Light/dark themes via CSS custom properties. Toggle with `data-rhx-theme` or `RHX.toggleTheme()`.
- **Accessible** — Semantic HTML, ARIA attributes, keyboard navigation, and screen reader support built in.
- **Model binding** — Form components integrate with ASP.NET Core model binding, validation, and `ModelExpression`.
- **Zero config** — One NuGet package. Two lines of setup. Start using components immediately.

## Quick Start

### 1. Install

```bash
dotnet add package htmxRazor
```

### 2. Configure

```csharp
// Program.cs
builder.Services.AddRazorPages();
builder.Services.AddhtmxRazor();

var app = builder.Build();
app.UseStaticFiles();
app.UsehtmxRazor();       // Serves component CSS, JS, and htmx from /_rhx/
app.MapRazorPages();
app.Run();
```

### 3. Register Tag Helpers

```razor
@* _ViewImports.cshtml *@
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, htmxRazor
```

### 4. Import Component CSS

`AddhtmxRazor()` auto-injects the foundation stylesheets (design tokens, reset, core utilities, and theme) into `<head>`. However, **each component has its own CSS file** that you must import separately. Add `<link>` tags for the components you use in your `_Layout.cshtml`:

```html
<head>
    @* Foundation CSS is auto-injected by AddhtmxRazor() *@

    @* Import CSS for each component you use *@
    <link rel="stylesheet" href="/_rhx/css/components/rhx-button.css" />
    <link rel="stylesheet" href="/_rhx/css/components/rhx-input.css" />
    <link rel="stylesheet" href="/_rhx/css/components/rhx-dialog.css" />
    <!-- Add more as needed -->
</head>
```

The naming convention is `/_rhx/css/components/rhx-{component}.css`. For example:
- `<rhx-button>` → `/_rhx/css/components/rhx-button.css`
- `<rhx-data-table>` → `/_rhx/css/components/rhx-data-table.css`
- `<rhx-command-palette>` → `/_rhx/css/components/rhx-command-palette.css`

> **Tip:** Without component CSS, your components will render correctly but appear unstyled. If a component looks generic, check that you've imported its CSS file.

Some components with interactive behavior also need their JavaScript file:

```html
<script src="/_rhx/js/components/rhx-dialog.js" defer></script>
<script src="/_rhx/js/components/rhx-tabs.js" defer></script>
```

### 5. Use Components

```html
<rhx-button rhx-variant="brand" rhx-size="large">
    Save Changes
</rhx-button>
```

## Feature Highlights

### Buttons with Every Variant

```html
<rhx-button rhx-variant="brand">Brand</rhx-button>
<rhx-button rhx-variant="success" rhx-appearance="outlined">Success</rhx-button>
<rhx-button rhx-variant="danger" rhx-appearance="plain">Delete</rhx-button>
<rhx-button rhx-variant="brand" rhx-pill="true">Pill Shape</rhx-button>
<rhx-button rhx-variant="brand" rhx-loading="true">Saving...</rhx-button>
```

### Form Controls with Model Binding

```html
<!-- Auto-detects type, label, validation from model metadata -->
<rhx-input rhx-for="Email" rhx-with-clear="true" />

<rhx-input rhx-label="Password"
           rhx-type="password"
           rhx-password-toggle="true"
           name="password" />
```

### htmx on Any Component

```html
<!-- Live search with debounce -->
<rhx-input rhx-label="Search"
           rhx-placeholder="Type to search..."
           hx-get="/search"
           hx-trigger="input changed delay:300ms"
           hx-target="#results" />

<!-- Delete with confirmation -->
<rhx-button rhx-variant="danger"
            hx-delete="/api/items/42"
            hx-confirm="Are you sure?"
            hx-target="#item-list"
            hx-swap="outerHTML">
    Delete
</rhx-button>
```

### Modal Dialogs

```html
<rhx-button rhx-variant="brand" data-rhx-dialog-open="edit-dialog">
    Edit Profile
</rhx-button>

<!-- rhx-size: small | medium | large | full | any CSS width -->
<rhx-dialog id="edit-dialog" rhx-label="Edit Profile" rhx-size="large">
    <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 1rem;">
        <rhx-input rhx-for="FirstName" />
        <rhx-input rhx-for="LastName" />
    </div>
    <rhx-dialog-footer>
        <rhx-button rhx-variant="neutral" rhx-appearance="outlined"
                    onclick="this.closest('dialog').close()">Cancel</rhx-button>
        <rhx-button rhx-variant="brand" type="submit">Save</rhx-button>
    </rhx-dialog-footer>
</rhx-dialog>
```

### Data Tables with Server-Driven Sort & Pagination

```html
<rhx-data-table id="products" rhx-striped="true" rhx-hoverable="true"
                rhx-sort-url="/Products?handler=TableData"
                rhx-label="Product list">
    <rhx-column rhx-field="name" rhx-header="Name" rhx-sortable="true" />
    <rhx-column rhx-field="price" rhx-header="Price" rhx-sortable="true" />
    <rhx-column rhx-field="stock" rhx-header="Stock" />
    @foreach (var p in Model.Items)
    {
        <tr><td>@p.Name</td><td>@p.Price</td><td>@p.Stock</td></tr>
    }
    <rhx-data-table-pagination rhx-page="1" rhx-page-size="10"
        rhx-total-items="@Model.TotalItems"
        rhx-url="/Products?handler=TableData"
        rhx-target="#products-body" />
</rhx-data-table>
```

### Command Palette (Cmd+K Search)

```html
<!-- Press Cmd+K / Ctrl+K to open -->
<rhx-command-palette id="search"
    rhx-placeholder="Search components..."
    hx-get="/Search"
    rhx-debounce="200">
</rhx-command-palette>

<!-- Server returns grouped results -->
<rhx-command-group rhx-heading="Pages">
    <rhx-command-item rhx-value="home" rhx-href="/" rhx-icon="home">
        Home
    </rhx-command-item>
</rhx-command-group>
```

### Theming

```html
<!-- Set theme on <html> -->
<html data-rhx-theme="light">

<!-- Toggle programmatically -->
<rhx-button onclick="RHX.toggleTheme()">Toggle Theme</rhx-button>

<!-- Override any design token -->
<style>
  :root {
    --rhx-color-brand-500: #6366f1;
    --rhx-radius-md: 0.75rem;
  }
</style>
```

### Real-time SignalR

```html
<!-- Connect to a SignalR hub and swap received HTML -->
<rhx-signalr hub-url="/chatHub" rhx-method="ReceiveMessage"
             rhx-swap="beforeend" rhx-target="#messages"
             rhx-connection-state="true" rhx-groups="room1">
    <rhx-spinner />
</rhx-signalr>
```

```csharp
// Server: send HTML fragments from any hub or controller
await hubContext.SendHtmlAsync("ReceiveMessage",
    "<div class='message'>Hello!</div>");

// Or to a specific group
await hubContext.SendHtmlToGroupAsync("room1",
    "ReceiveMessage", "<div>Group message</div>");
```

### Kanban Board

```html
<rhx-kanban>
    <rhx-kanban-column rhx-column-id="todo" rhx-title="To Do" rhx-max-cards="5">
        <rhx-kanban-card rhx-card-id="1" rhx-variant="brand"
                         hx-post="/Board?handler=Move"
                         hx-target="#board" hx-swap="outerHTML">
            Design homepage
        </rhx-kanban-card>
    </rhx-kanban-column>
    <rhx-kanban-column rhx-column-id="doing" rhx-title="In Progress" />
    <rhx-kanban-column rhx-column-id="done" rhx-title="Done" />
</rhx-kanban>
```

### Real-time SSE Streaming

```html
<!-- Client: declarative SSE container -->
<rhx-sse-stream rhx-url="/dashboard?handler=StatusStream"
                rhx-event="status-update">
    <rhx-spinner />
</rhx-sse-stream>
```

```csharp
// Server: stream events from an IAsyncEnumerable
public async Task<IActionResult> OnGetStatusStream(CancellationToken ct)
{
    await Response.WriteSseStreamAsync(GetUpdatesAsync(), "status-update", ct);
    return new EmptyResult();
}
```

### Multi-step Wizard

```html
<rhx-wizard rhx-current-step="2" page="/Checkout">
    <rhx-wizard-step rhx-title="Account" rhx-status="complete">
        ...
    </rhx-wizard-step>
    <rhx-wizard-step rhx-title="Shipping" rhx-status="current">
        <rhx-input rhx-for="Address" />
    </rhx-wizard-step>
    <rhx-wizard-step rhx-title="Payment">
        ...
    </rhx-wizard-step>
</rhx-wizard>
```

### Response-Aware Form

```html
<!-- Auto-configures response-targets extension and error handling -->
<rhx-htmx-form page="/Contact" page-handler="Submit"
                rhx-error-target="#errors"
                rhx-reset-on-success="true">
    <rhx-input rhx-for="Email" />
    <rhx-button type="submit" rhx-variant="brand">Send</rhx-button>
</rhx-htmx-form>
```

### Timeline

```html
<rhx-timeline>
    <rhx-timeline-item rhx-variant="success" rhx-label="March 10">
        Order placed
    </rhx-timeline-item>
    <rhx-timeline-item rhx-variant="brand" rhx-label="March 14" rhx-active="true">
        Shipped
    </rhx-timeline-item>
    <rhx-timeline-item rhx-label="Pending">
        Delivered
    </rhx-timeline-item>
</rhx-timeline>
```

### Optimistic UI

```html
<!-- Toggle flips immediately; reverts on server error -->
<rhx-switch name="darkMode" rhx-label="Dark mode"
            rhx-optimistic="true"
            hx-post="/settings/darkMode"
            hx-trigger="change" hx-swap="none" />
```

## Component Catalog

| Category | Components |
|----------|-----------|
| **Actions** | Button, Button Group, Dropdown |
| **Forms** | Input, Textarea, Select, Combobox, Radial Select, Date Picker, Time Picker, Date & Time Picker, Checkbox, Switch, Radio, Slider, Rating, Color Picker, File Input, Number Input, htmx Form |
| **Feedback** | Callout, Badge, Tag, Spinner, Skeleton, Progress Bar, Progress Ring, Tooltip, Toast, Toast Container |
| **Navigation** | Tabs, Breadcrumb, Tree, Carousel, Pagination, Skip Nav, Landmark, Wizard |
| **Organization** | Card, Divider, Split Panel, Scroller, Timeline, Kanban Board |
| **Overlays** | Dialog, Drawer, Details, Command Palette |
| **Data Display** | Data Table, Sparkline |
| **Imagery** | Icon (48 built-in), Avatar, Animated Image, Comparison, Zoomable Frame |
| **Formatting** | Format Bytes, Format Date, Format Number, Relative Time |
| **Utilities** | Copy Button, QR Code, Animation, Popup, Popover, Live Region |
| **Patterns** | Active Search, Infinite Scroll, Lazy Load, Poll, Load More, SSE Stream, SignalR Connector, Optimistic UI |

## How It Compares

| | htmxRazor | Bootstrap | Blazor | Web Awesome |
|---|---------|-----------|--------|-------------|
| **Rendering** | Server (Tag Helpers) | Client (jQuery/JS) | Client (WebAssembly/SignalR) | Client (Web Components) |
| **htmx support** | Native on every component | Manual attribute wiring | N/A (own reactivity) | Manual attribute wiring |
| **Bundle size** | CSS + htmx (~14 KB gzip) | ~24 KB JS + ~22 KB CSS | ~2 MB WebAssembly runtime | ~80 KB JS |
| **Model binding** | ASP.NET Core `ModelExpression` | None | Blazor `@bind` | None |
| **Dependencies** | ASP.NET Core | jQuery | .NET runtime in browser | Lit |
| **Theming** | CSS custom properties | Sass variables | CSS isolation | CSS custom properties |

## Solution Structure

| Project | Description |
|---------|-------------|
| `htmxRazor` | Core Tag Helper library with embedded CSS/JS assets |
| `htmxRazor.Demo` | Documentation site showcasing all components |
| `htmxRazor.Tests` | 1,838 unit tests for Tag Helper rendering |

## Development

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run the demo site
dotnet run --project htmxRazor.Demo

# Pack for NuGet
dotnet pack htmxRazor/htmxRazor.csproj --configuration Release
```

## Design System

htmxRazor owns its entire rendering and styling stack:

- **CSS Custom Properties** for design tokens (colors, spacing, typography, borders, shadows)
- **BEM naming** with `rhx-` prefix (`rhx-button`, `rhx-button--brand`, `rhx-button__label`)
- **Light/dark themes** via `data-rhx-theme` attribute on `<html>`
- **Utility classes** for layout and spacing (`rhx-flex`, `rhx-gap-md`, `rhx-p-lg`)
- **Auto-injected foundation** — `AddhtmxRazor()` injects tokens, reset, core, and utilities CSS plus the htmx script into `<head>`. Component-specific CSS (e.g., `rhx-button.css`) must be imported separately — see [Quick Start step 4](#4-import-component-css)

The project uses a custom CSS design system — not Bootstrap or any other CSS framework.

Key details from the codebase instructions:
- BEM naming convention with an rhx- prefix (e.g., .rhx-button, .rhx-button--loading, .rhx-button__icon)
- CSS custom properties (design tokens) defined in rhx-tokens.css — things like --rhx-color-brand-500, --rhx-radius-md, --rhx-font-size-base
- Light/dark theme support via a data-rhx-theme attribute on <html>
- Each component has its own scoped CSS file in Assets/css/components/
- All CSS is embedded and served at /_rhx/ — no external dependencies
- Hardcoded values are not allowed; everything references var(--rhx-*) tokens

It's a fully self-contained design system built specifically for this component library.

## Roadmap

Features ranked by **user impact** and **implementation effort**. The library is at v2.0.1 with 85+ components. The **Planned** releases below (2.1–2.4) are the next 2.X updates; the **Shipped** sections record what's already landed. Each planned release maps to a [GitHub milestone](https://github.com/cwoodruff/htmxRazor/milestones).

### Planned

#### v2.1 — Advanced Inputs

- **Radial Select** — `<rhx-radial-select>` + `<rhx-radial-option>`. A rectangular trigger button sits flush-left against a dropdown; clicking it opens a circular SVG **pie** of category wedges (each with a color + icon). Selecting a wedge fires `hx-get`, swaps the dropdown's option set to that category, echoes the category's color + icon onto the trigger, and auto-selects the first option. Wedge colors use named variant tokens (`brand`/`success`/`warning`/`danger`/`neutral`) with a deterministic cycle when omitted. Accessible as a menu (`menuitemradio`, arrow-key + type-ahead navigation).
- **Date / Time Picker** — `<rhx-date-picker>` family (date, time, range). The most-requested missing input — the catalog has date *formatting* but no date *input*. Full model binding, validation, design tokens, and keyboard accessibility.
- **Playwright E2E in CI** — re-enable and stabilize the end-to-end suite so PRs are gated on real browser regression tests.

#### v2.2 — Playground & DX Completion

- **Playground coverage for all components** — extend the 2.0 Component Playground (Button reference) to every Forms and Actions component: live property toggles, shareable URL state, generated Razor markup.
- **JetBrains plugin parity** — bring the JetBrains plugin to feature parity with the 97-snippet VS Code extension.

#### v2.3 — Data & Visualization

- **SVG Charts** — server-rendered `bar`, `line`, and `area` chart components built on the design-token system (joining the existing Sparkline).
- **Expandable / tree table rows** — detail rows and hierarchical rows for `<rhx-data-table>` with htmx-driven lazy expansion.
- **Virtualized table body** — optional windowed rendering for large datasets, compatible with server-driven sort/filter/pagination.

#### v2.4 — Accessibility & Internationalization

- **WCAG 2.2 contrast audit** — verify every foreground/background token pair (light + dark) meets AA.
- **RTL support** — logical CSS properties and mirrored directional affordances across components.
- **Localizable strings** — externalize built-in UI text for ASP.NET Core localization.
- **Infrastructure cleanup** — clear the backlog below (dead `EnableCssIsolation` / `IHtmxSupported`, `hx-replace-url` attribute, `HtmlRenderer` adoption).

### Shipped

#### v1.2 — Notifications, Pagination & Quick Wins

- **Toast Notification System** — `<rhx-toast-container>` + `<rhx-toast>` with auto-dismiss, severity variants, stacking, and `aria-live` announcements.
- **Pagination** — `<rhx-pagination>` with htmx-powered page navigation, ellipsis, first/last/prev/next buttons, and size variants.
- **ARIA Live Region Manager** — `<rhx-live-region>` for screen reader announcements on htmx swaps.
- **CSS Cascade Layers** — All component CSS wrapped in `@layer` for easy host app overrides without specificity wars.
- **View Transition Support** — `rhx-transition` and `rhx-transition-name` attributes for the View Transitions API.

#### v1.3 — Data Table, Accessibility & Modern CSS

- **Data Table** — `<rhx-data-table>`, `<rhx-column>`, `<rhx-data-table-pagination>` with server-driven sort, filter, and pagination. Includes `DataTableRequest` model binder auto-registered via `AddhtmxRazor()`.
- **Command Palette** — `<rhx-command-palette>` modal search overlay activated via Cmd+K / Ctrl+K with debounced `hx-get`, grouped results via `<rhx-command-group>` and `<rhx-command-item>`, and full keyboard navigation.
- **Focus Management After Swaps** — `rhx-focus-after-swap` attribute on the base class. On by default for `<rhx-dialog>` and `<rhx-drawer>` (WCAG 2.4.3).
- **Skip Nav & Landmarks** — `<rhx-skip-nav>` and `<rhx-landmark>` for semantic page regions (WCAG 2.4.1).
- **Container Queries** — Card, dialog, split panel, and data table CSS use `@container` queries for container-responsive layouts.
- **APG Keyboard Audit** — Tabs, tree, dropdown, and combobox audited against W3C ARIA Authoring Practices Guide. Added type-ahead, `*` expand siblings, `Alt+Arrow` patterns.

#### v1.4 — Real-time, Wizard & Patterns

- **SSE Stream Container** — `<rhx-sse-stream>` wraps `hx-ext="sse"` + `sse-connect` into a declarative Tag Helper. Companion `HtmxSseExtensions` provides `PrepareSseResponse()`, `WriteSseEventAsync()`, and `WriteSseStreamAsync()` to format `IAsyncEnumerable<string>` as `text/event-stream`.
- **Multi-step Wizard** — `<rhx-wizard>` and `<rhx-wizard-step>` with visual stepper indicator, auto-generated Previous/Next navigation buttons (`hx-get`/`hx-post`), step status tracking (incomplete, current, complete, error), horizontal/vertical layouts, and linear/non-linear navigation. Server-side `WizardState` and `WizardSessionExtensions` for TempData-based step tracking.
- **Response-Aware Form** — `<rhx-htmx-form>` auto-configures the htmx `response-targets` extension with `hx-target-422`, `hx-target-4*`, and `hx-target-5*`. Built-in error container with `aria-live`, submit-button disabling, submitting state, and reset-on-success. Pairs with existing `HtmxValidationFailure()` for 422 responses.
- **Timeline** — `<rhx-timeline>` and `<rhx-timeline-item>` with variant-colored connectors (brand, success, warning, danger), icon slots via `<rhx-timeline-icon>`, labels, active state highlighting, and vertical/horizontal/alternating layouts. Pairs with `<rhx-infinite-scroll>` or `<rhx-load-more>` for loading additional events.
- **Optimistic UI** — `rhx-optimistic` attribute on `<rhx-switch>`, `<rhx-rating>`, and `<rhx-button>`. Immediately reflects visual state on click before the server responds; reverts automatically on error via `htmx:responseError` with a brief flash animation. Respects `prefers-reduced-motion`.
- **Load More Pattern** — `<rhx-load-more>` button-triggered pagination. Renders a styled button with `hx-get`, loading spinner, and auto-removal after fetch. Configurable variant, target, swap strategy, and loading text.

#### v2.0 — Platform & DX

- **Theme Builder** — Interactive page at `/ThemeBuilder` with color pickers for all `--rhx-*` tokens. Real-time preview, smart HSL palette generation, and downloadable `rhx-theme-overrides.css` export.
- **Interactive Component Playground** — Property toggle panel on demo pages. Change variant, size, and state via dropdowns and checkboxes; server re-renders via htmx. URL-driven state for shareable configurations. Generated Razor markup displayed below preview.
- **SignalR Hub Connector** — `<rhx-signalr>` Tag Helper with `hub-url`, `rhx-method`, `rhx-swap`, `rhx-target`, `rhx-reconnect`, `rhx-transport`, `rhx-groups`, and `rhx-connection-state`. Server-side `HtmxSignalRExtensions` for `SendHtmlAsync()`, `SendHtmlToGroupAsync()`, `SendHtmlToConnectionAsync()`.
- **CSS Anchor Positioning** — Tooltip, popover, popup, and dropdown use the CSS Anchor Positioning API when supported, with automatic JS fallback. Reduces JS footprint and improves viewport correctness.
- **VS Code Snippet Extension** — 97 snippets in `vscode-extension/` for all `<rhx-*>` tags with curated tabstops, htmx variants, and composition snippets.
- **Kanban Board** — `<rhx-kanban>`, `<rhx-kanban-column>`, `<rhx-kanban-card>` with native Drag and Drop, keyboard navigation, WIP limits, card variants, and htmx server persistence.

### Infrastructure Cleanup (backlog — targeted for v2.4)

| Item | Description |
|------|-------------|
| Implement or remove `EnableCssIsolation` | Option is defined in `htmxRazorOptions` but has no implementation |
| Remove `IHtmxSupported` marker interface | Dead code — nothing checks for it |
| Add `hx-replace-url` to base class | Already exists as a response header helper but missing from tag helper attributes |
| Adopt `HtmlRenderer` in components | Replace raw `StringBuilder` HTML construction with the fluent `HtmlRenderer` for better escaping and readability |
| WCAG 2.2 contrast audit | Verify all foreground/background token pairs in `rhx-tokens.css` meet AA contrast ratios (4.5:1 normal text, 3:1 large text) |

## Project Template

A .NET project template for htmxRazor is available in the `templates/htmxRazor.Template` directory. This template provides a clean starting point with:
- No jQuery or Bootstrap.
- Pre-configured `Program.cs` for htmxRazor.
- A base `_Layout.cshtml` including htmxRazor assets.
- A sample `Index.cshtml` demonstrating htmx and htmxRazor components.

### Installation

You can install the template locally from the source or from a NuGet package.

To install the template locally from source, run the following command from the project root:

```bash
dotnet new install ./templates/htmxRazor.Template
```

Alternatively, you can pack and install from the template package:

```bash
dotnet pack templates/htmxRazor.Templates.csproj -o nupkg
dotnet new install nupkg/htmxRazor.Templates.1.3.0.nupkg
```

### Usage

To create a new project using the template, run:

```bash
dotnet new htmxrazor -n YourProjectName
```

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for development setup, PR guidelines, and coding standards.

## License

[MIT](LICENSE) - Copyright (c) 2026 Chris Woody Woodruff
