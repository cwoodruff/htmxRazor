# htmxRazor Snippets for VS Code

Code snippets for all [htmxRazor](https://github.com/cwoodruff/htmxRazor) Tag Helper components in ASP.NET Core Razor views.

## Usage

Type `rhx-` in any `.cshtml`, `.razor`, or `.html` file to see the available snippets. Each snippet expands the full tag with commonly used attributes as interactive tabstops.

## Snippet Categories

| Category | Snippets | Examples |
|----------|----------|----------|
| Actions | 8 | `rhx-button`, `rhx-button-htmx`, `rhx-dropdown` |
| Forms | 17 | `rhx-input`, `rhx-select`, `rhx-htmx-form`, `rhx-combobox-server` |
| Feedback | 10 | `rhx-callout`, `rhx-toast`, `rhx-tooltip`, `rhx-spinner` |
| Navigation | 14 | `rhx-tab-group`, `rhx-pagination`, `rhx-wizard`, `rhx-tree` |
| Organization | 12 | `rhx-card-full`, `rhx-split-panel`, `rhx-timeline` |
| Overlays | 8 | `rhx-dialog`, `rhx-drawer`, `rhx-command-palette` |
| Imagery | 6 | `rhx-avatar`, `rhx-comparison`, `rhx-icon` |
| Formatting | 4 | `rhx-format-date`, `rhx-format-number`, `rhx-relative-time` |
| Data Display | 4 | `rhx-data-table`, `rhx-column`, `rhx-sparkline` |
| Utilities | 6 | `rhx-copy-button`, `rhx-qr-code`, `rhx-popover` |
| Patterns | 6 | `rhx-lazy-load`, `rhx-sse-stream`, `rhx-poll` |

## htmx Variants

Several components include htmx-specific snippet variants (suffixed with `-htmx`) that pre-fill `hx-get`, `hx-target`, and `hx-swap` attributes:

- `rhx-button-htmx` — Button with htmx request attributes
- `rhx-input-htmx` — Input with live validation or search

## Composition Snippets

Multi-component structures are available as single snippets:

- `rhx-card-full` — Card with header, body, and footer
- `rhx-card-image` — Card with image
- `rhx-breadcrumb-manual` — Breadcrumb with inline items
- `rhx-tab-group` — Complete tab group with panels
- `rhx-combobox-server` — Combobox with server-side filtering

## Installation

### From the Marketplace

Search for "htmxRazor Snippets" in the VS Code Extensions panel.

### Manual Installation

```bash
cd vscode-extension
npm install -g @vscode/vsce
vsce package
code --install-extension htmxrazor-snippets-2.0.0.vsix
```

## Requirements

- [htmxRazor](https://www.nuget.org/packages/htmxRazor) NuGet package installed in your ASP.NET Core project
- VS Code with C# / Razor language support
