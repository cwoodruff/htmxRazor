# htmxRazor Copilot Instructions

## Build and test commands

Use .NET 10 for this repository.

```bash
# Build the full solution
dotnet build --configuration Release

# Run the fast unit-test suite used in CI
dotnet test htmxRazor.Tests/htmxRazor.Tests.csproj --configuration Release --no-build

# Run a single unit test
dotnet test htmxRazor.Tests/htmxRazor.Tests.csproj --configuration Release --no-build --filter "FullyQualifiedName~ButtonTagHelperTests.Renders_Button_Element_By_Default"

# Run the demo/docs site locally
dotnet run --project htmxRazor.Demo

# Run the example app locally
dotnet run --project htmxRazor.Example

# Run the browser-based Playwright suite locally
dotnet test PlaywrightTests/PlaywrightTests.csproj --configuration Release

# Run a single Playwright test
dotnet test PlaywrightTests/PlaywrightTests.csproj --configuration Release --filter "FullyQualifiedName~ButtonTests.Variants_render_five_buttons"
```

`dotnet build` is effectively the quality gate for compile-time issues because `Directory.Build.props` enables nullable reference types and treats warnings as errors for most projects. CI only runs `htmxRazor.Tests`; `PlaywrightTests` are intentionally excluded from CI because they require browser binaries and are much slower.

## High-level architecture

- `htmxRazor/` is the NuGet package. It contains:
  - `Components/`: Tag Helpers grouped by category (`Actions`, `Forms`, `Overlays`, `Patterns`, etc.)
  - `Assets/css/components` and `Assets/js/components`: per-component styling and behavior
  - `Infrastructure/`: shared base classes, htmx/server helpers, DI registration, static asset hosting
  - `Rendering/`: shared HTML/slot rendering helpers for composite components
- `AddhtmxRazor()` and `UsehtmxRazor()` are the integration seam for consuming apps. `AddhtmxRazor()` registers the tag-helper component and MVC/model-binding services. `UsehtmxRazor()` serves embedded assets from the package under `/_rhx/...`.
- `htmxRazorTagHelperComponent` auto-injects the foundation CSS (`rhx-tokens`, reset, core, utilities), the selected theme stylesheet, htmx, and `rhx-core.js` into `<head>`. Individual component CSS and JS are **not** auto-injected; apps/layouts explicitly include the per-component files they use.
- `htmxRazor.Demo/` is the living docs site and the main integration surface for interactive examples. `Pages/Docs/Components/*.cshtml` pages use shared layouts/partials such as `_ComponentPage`, `_PropertyTable`, and `_PlaygroundPanel`.
- `PlaywrightTests/` treats the demo site as the system-under-test. It boots `htmxRazor.Demo` through `WebApplicationFactory` + real Kestrel, then drives the docs pages in Chromium, Firefox, and WebKit. Stable IDs and panel names in docs pages are part of the test surface.
- `htmxRazor.Example/` and `htmxRazor.StyleExample/` are consumer applications that show package usage outside the docs site.

## Key conventions

- New or changed components usually span multiple surfaces:
  1. Tag Helper in `htmxRazor/Components/...`
  2. matching CSS in `Assets/css/components/rhx-{component}.css`
  3. matching JS in `Assets/js/components/rhx-{component}.js` if interactive
  4. demo/docs wiring in `htmxRazor.Demo` (docs page, layout asset includes, and often `_SidebarNav.cshtml`)
  5. unit tests in `htmxRazor.Tests`
  6. Playwright coverage in `PlaywrightTests` for interactive behavior
- Follow the established component pattern from `ButtonTagHelper` and the contributing guide:
  - tag names are `rhx-{component}`
  - component attributes use the `rhx-` prefix
  - components extend `htmxRazorTagHelperBase` (or `FormControlTagHelperBase` for form inputs)
  - each component overrides `BlockName`
  - CSS classes are built with `CssClassBuilder`
  - shared attributes/classes go through `ApplyBaseAttributes()`
  - htmx attributes are forwarded with `RenderHtmxAttributes()`
- CSS follows BEM naming with the `rhx-` prefix: `.rhx-{block}`, `.rhx-{block}__{element}`, `.rhx-{block}--{modifier}`. Use design tokens (`var(--rhx-*)`) instead of hardcoded theme values.
- Docs pages are not just documentation; they are executable fixtures. Playwright tests depend on predictable preview/result containers like `#panel-variants-preview`, `#panel-states-preview`, and handler-based Razor Page interactions on the docs routes.
- Unit tests are output-focused. Test classes follow `{ComponentName}TagHelperTests` and typically inherit from `TagHelperTestBase`, which provides test helpers for `TagHelperContext`, `TagHelperOutput`, and URL generation.
- Use the infrastructure helpers for htmx response patterns instead of ad hoc headers/status handling. Examples:
  - `HtmxValidationFailure()` returns the standard 422 partial response for invalid htmx form posts
  - `HtmxSuccess()` and `HxToast()` trigger toast notifications through htmx response headers
  - `HtmxSseExtensions` and `HtmxSignalRExtensions` support the SSE and SignalR components
- Layouts manually include component CSS and JS from `/_rhx/...`. If a component renders correctly but looks or behaves wrong in the demo/example apps, check the corresponding layout includes before changing the component itself.
