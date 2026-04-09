# htmxRazor Live Templates for JetBrains Rider

Live templates (code snippets) for all [htmxRazor](https://github.com/cwoodruff/htmxRazor) Tag Helper components in ASP.NET Core Razor views.

## Usage

Type `rhx-` in any `.cshtml` or HTML file to see the available templates. Each template expands the full tag with commonly used attributes as interactive variables with dropdown selections.

## Template Categories

| Category | Templates | Examples |
|----------|-----------|----------|
| Actions | 8 | `rhx-button`, `rhx-button-htmx`, `rhx-dropdown` |
| Forms | 17 | `rhx-input`, `rhx-select`, `rhx-htmx-form`, `rhx-combobox-server` |
| Feedback | 10 | `rhx-callout`, `rhx-toast`, `rhx-tooltip`, `rhx-spinner` |
| Navigation | 14 | `rhx-tab-group`, `rhx-pagination`, `rhx-wizard`, `rhx-tree` |
| Organization | 7 | `rhx-card-full`, `rhx-split-panel`, `rhx-timeline` |
| Overlays | 5 | `rhx-dialog`, `rhx-drawer`, `rhx-command-palette` |
| Imagery | 4 | `rhx-avatar`, `rhx-comparison`, `rhx-icon` |
| Formatting | 4 | `rhx-format-date`, `rhx-format-number`, `rhx-relative-time` |
| Data Display | 2 | `rhx-data-table`, `rhx-sparkline` |
| Utilities | 6 | `rhx-copy-button`, `rhx-qr-code`, `rhx-popover` |
| Patterns | 6 | `rhx-lazy-load`, `rhx-sse-stream`, `rhx-poll` |

## Installation

### Option 1: Import Live Templates Directly

1. Open Rider > **Settings** > **Editor** > **Live Templates**
2. Click the gear icon > **Import...**
3. Select the `resources/liveTemplates/htmxRazor.xml` file
4. The templates appear under the **htmxRazor** group

### Option 2: Copy to Config Directory

Copy the template file to your JetBrains config directory:

```bash
# macOS
cp resources/liveTemplates/htmxRazor.xml ~/Library/Application\ Support/JetBrains/Rider2024.3/templates/

# Windows
copy resources\liveTemplates\htmxRazor.xml %APPDATA%\JetBrains\Rider2024.3\templates\

# Linux
cp resources/liveTemplates/htmxRazor.xml ~/.config/JetBrains/Rider2024.3/templates/
```

Adjust the `Rider2024.3` folder name to match your installed version. Restart Rider after copying.

### Option 3: Build as Plugin JAR

Package as a JetBrains plugin for distribution via the Marketplace:

```bash
cd jetbrains-plugin
jar cf htmxrazor-livetemplates-2.0.0.jar -C resources .
```

Install in Rider via **Settings** > **Plugins** > gear icon > **Install Plugin from Disk...**.

## Requirements

- JetBrains Rider 2023.2+ (or any IntelliJ-based IDE with HTML support)
- [htmxRazor](https://www.nuget.org/packages/htmxRazor) NuGet package in your project
