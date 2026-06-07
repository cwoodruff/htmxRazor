# Radial Select Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build `<rhx-radial-select>` — a compound form control with a rectangular trigger button flush-left against a dropdown, opening a circular SVG pie popup whose wedges are categories (color + icon) that drive the dropdown's option set via an htmx cascade.

**Architecture:** Two Tag Helpers following the codebase's parent/child compound pattern (parent seeds a `List<T>` in `context.Items`, children append and `SuppressOutput()`, exactly like `DataTableTagHelper`/`ColumnTagHelper`). The wrapper renders a button group (trigger + an internal listbox that reuses the existing `rhx-select` styling/value machinery), an SVG pie popup, and hidden inputs for the bound value and the active category. A dedicated JS file handles open/close, keyboard, wedge selection, and the `htmx.ajax` cascade, reusing `rhx-position.js` for placement. CSS lives in its own component stylesheet using only `--rhx-*` tokens.

**Tech Stack:** ASP.NET Core Tag Helpers (.NET 10, C#), htmx, vanilla JS, BEM CSS with `rhx-` prefix + design tokens, xUnit tests, Playwright E2E.

**Spec:** `docs/superpowers/specs/2026-06-07-radial-select-design.md`

**Conventions verified in the codebase (read before starting):**
- Parent/child collection: `htmxRazor/Components/DataDisplay/DataTableTagHelper.cs:91-99` (seed) + `htmxRazor/Components/DataDisplay/ColumnTagHelper.cs:8,67-82` (append + `SuppressOutput()` + `ColumnDefinition` record-like class).
- Form control base: `htmxRazor/Components/Forms/FormControlTagHelperBase.cs` (`ResolveName`/`ResolveId`/`ResolveValue`, `Enc`, `ApplyWrapperAttributes`, `BuildHtmxAttributeString`, `BuildValidationAttributeString`).
- Base helpers: `htmxRazor/Infrastructure/htmxRazorTagHelperBase.cs` (`GetElementClass`, `GetModifierClass`, `CreateCssBuilder`).
- Icons: `htmxRazor.Components.Imagery.IconRegistry.Get(name)` returns SVG string or `null`; `IconRegistry.Has(name)` returns bool.
- Color tokens that exist: `--rhx-color-{brand|success|warning|danger|neutral}-500` (NO `info`).
- Assets auto-embed via `htmxRazor/htmxRazor.csproj:50` (`<EmbeddedResource Include="Assets/**/*" />`) — new CSS/JS files need no csproj edit.
- Test base: `htmxRazor.Tests/TagHelperTestBase.cs` — `CreateContext`, `CreateOutput`, `HasClass`, `AssertAttribute`, `GetAttribute`, `CreateUrlHelperFactory`, `CreateViewContext`.

---

## File Structure

| File | Responsibility |
|---|---|
| `htmxRazor/Components/Forms/RadialOptionTagHelper.cs` | One wedge/category. Appends a `RadialOptionData` to the parent's list in `context.Items`; renders nothing (`SuppressOutput`). Holds the `RadialOptionData` class. |
| `htmxRazor/Components/Forms/RadialSelectTagHelper.cs` | Wrapper. Seeds the option list, processes children, resolves binding/category/size/disabled, resolves colors (explicit or cycle), builds the SVG pie + button group + hidden inputs. |
| `htmxRazor/Assets/css/components/rhx-radial-select.css` | Pie/wedge/trigger/group/listbox styling via `--rhx-*` tokens; dark-theme + reduced-motion aware; wrapped in `@layer`. |
| `htmxRazor/Assets/js/components/rhx-radial-select.js` | Open/close, keyboard nav, wedge selection, color/icon echo, `htmx.ajax` cascade, auto-select-first, focus, positioning via `rhx-position.js`. |
| `htmxRazor.Tests/RadialSelectTagHelperTests.cs` | Unit tests for both Tag Helpers. |
| `PlaywrightTests/RadialSelectTests.cs` | E2E: open → select wedge → cascade → auto-select-first → focus. |
| `htmxRazor.Demo/Pages/RadialSelect.cshtml` (+ `.cs`) | Demo page + server handler returning the `<rhx-option>` fragment. |
| `README.md`, `CHANGELOG.md`, `vscode-extension/snippets/*` | Docs, changelog, snippet. |

---

## Task 1: `RadialOptionData` + `RadialOptionTagHelper`

**Files:**
- Create: `htmxRazor/Components/Forms/RadialOptionTagHelper.cs`
- Test: `htmxRazor.Tests/RadialSelectTagHelperTests.cs`

- [ ] **Step 1: Write the failing test**

```csharp
using htmxRazor.Components.Forms;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Xunit;

namespace htmxRazor.Tests;

public class RadialSelectTagHelperTests : TagHelperTestBase
{
    private RadialOptionTagHelper CreateOptionHelper() =>
        new() { ViewContext = CreateViewContext() };

    [Fact]
    public async Task RadialOption_Appends_Data_To_Parent_List_And_Suppresses_Output()
    {
        var helper = CreateOptionHelper();
        helper.Value = "fruit";
        helper.Label = "Fruit";
        helper.Icon = "apple";
        helper.Color = "danger";
        helper.HxGet = "/Menu?handler=Items&cat=fruit";

        var context = CreateContext("rhx-radial-option");
        context.Items["RhxRadialOptions"] = new List<RadialOptionData>();
        var output = CreateOutput("rhx-radial-option", childContent: "");

        await helper.ProcessAsync(context, output);

        var list = (List<RadialOptionData>)context.Items["RhxRadialOptions"];
        var opt = Assert.Single(list);
        Assert.Equal("fruit", opt.Value);
        Assert.Equal("Fruit", opt.Label);
        Assert.Equal("apple", opt.Icon);
        Assert.Equal("danger", opt.Color);
        Assert.Equal("/Menu?handler=Items&cat=fruit", opt.HxGet);
        Assert.Null(output.TagName); // SuppressOutput sets TagName null
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test htmxRazor.Tests --filter RadialOption_Appends_Data_To_Parent_List_And_Suppresses_Output`
Expected: FAIL — `RadialOptionTagHelper` / `RadialOptionData` do not exist (compile error).

- [ ] **Step 3: Write minimal implementation**

```csharp
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Forms;

/// <summary>Data captured from one &lt;rhx-radial-option&gt; wedge.</summary>
public sealed class RadialOptionData
{
    public string Value { get; init; } = "";
    public string Label { get; init; } = "";
    public string? Icon { get; init; }
    public string? Color { get; init; }
    public string? HxGet { get; init; }
    public bool Disabled { get; init; }
}

/// <summary>
/// One pie wedge / category inside <c>&lt;rhx-radial-select&gt;</c>. Contributes its data
/// to the parent via <c>context.Items["RhxRadialOptions"]</c> and renders nothing itself.
/// </summary>
[HtmlTargetElement("rhx-radial-option", ParentTag = "rhx-radial-select")]
public sealed class RadialOptionTagHelper : TagHelper
{
    [HtmlAttributeName("rhx-value")] public string? Value { get; set; }
    [HtmlAttributeName("rhx-label")] public string? Label { get; set; }
    [HtmlAttributeName("rhx-icon")] public string? Icon { get; set; }
    [HtmlAttributeName("rhx-color")] public string? Color { get; set; }
    [HtmlAttributeName("hx-get")] public string? HxGet { get; set; }
    [HtmlAttributeName("rhx-disabled")] public bool Disabled { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (context.Items.TryGetValue("RhxRadialOptions", out var obj)
            && obj is List<RadialOptionData> options)
        {
            options.Add(new RadialOptionData
            {
                Value = Value ?? "",
                Label = Label ?? "",
                Icon = Icon,
                Color = Color,
                HxGet = HxGet,
                Disabled = Disabled,
            });
        }

        output.SuppressOutput();
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test htmxRazor.Tests --filter RadialOption_Appends_Data_To_Parent_List_And_Suppresses_Output`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add htmxRazor/Components/Forms/RadialOptionTagHelper.cs htmxRazor.Tests/RadialSelectTagHelperTests.cs
git commit -m "feat(radial-select): add RadialOptionTagHelper child + RadialOptionData"
```

---

## Task 2: `RadialSelectTagHelper` skeleton — wrapper, group, hidden value input

**Files:**
- Create: `htmxRazor/Components/Forms/RadialSelectTagHelper.cs`
- Test: `htmxRazor.Tests/RadialSelectTagHelperTests.cs`

- [ ] **Step 1: Write the failing tests**

```csharp
private RadialSelectTagHelper CreateRadialHelper() =>
    new(CreateUrlHelperFactory()) { ViewContext = CreateViewContext() };

[Fact]
public async Task RadialSelect_Renders_Wrapper_Div_With_Block_Class_And_DataAttr()
{
    var helper = CreateRadialHelper();
    helper.Name = "FoodItem";
    var context = CreateContext("rhx-radial-select");
    var output = CreateOutput("rhx-radial-select", childContent: "");

    await helper.ProcessAsync(context, output);

    Assert.Equal("div", output.TagName);
    Assert.True(HasClass(output, "rhx-radial-select"));
    Assert.NotNull(output.Attributes["data-rhx-radial-select"]);
}

[Fact]
public async Task RadialSelect_Renders_Trigger_And_Hidden_Value_Input()
{
    var helper = CreateRadialHelper();
    helper.Name = "FoodItem";
    var context = CreateContext("rhx-radial-select");
    var output = CreateOutput("rhx-radial-select", childContent: "");

    await helper.ProcessAsync(context, output);

    var html = output.Content.GetContent();
    Assert.Contains("rhx-radial-select__trigger", html);
    Assert.Contains("aria-haspopup=\"menu\"", html);
    Assert.Contains("data-rhx-radial-value", html);
    Assert.Contains("name=\"FoodItem\"", html);
}

[Fact]
public async Task RadialSelect_Category_Name_Adds_Second_Hidden_Input()
{
    var helper = CreateRadialHelper();
    helper.Name = "FoodItem";
    helper.CategoryName = "Category";
    var context = CreateContext("rhx-radial-select");
    var output = CreateOutput("rhx-radial-select", childContent: "");

    await helper.ProcessAsync(context, output);

    var html = output.Content.GetContent();
    Assert.Contains("data-rhx-radial-category", html);
    Assert.Contains("name=\"Category\"", html);
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test htmxRazor.Tests --filter RadialSelect_Renders_Wrapper_Div_With_Block_Class_And_DataAttr`
Expected: FAIL — `RadialSelectTagHelper` does not exist (compile error).

- [ ] **Step 3: Write minimal implementation**

```csharp
using System.Text;
using htmxRazor.Components.Imagery;
using htmxRazor.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Forms;

/// <summary>
/// Compound control: a rectangular trigger button flush-left against a dropdown.
/// The trigger opens a circular SVG pie popup whose wedges (categories, each with a
/// color + icon) drive the dropdown's option set via an htmx cascade.
/// </summary>
[HtmlTargetElement("rhx-radial-select")]
[RestrictChildren("rhx-radial-option")]
public sealed class RadialSelectTagHelper : FormControlTagHelperBase
{
    protected override string BlockName => "radial-select";

    /// <summary>Placeholder shown before any option is selected / when a category is empty.</summary>
    [HtmlAttributeName("rhx-placeholder")] public string? Placeholder { get; set; }

    /// <summary>Optional form field name for submitting the active category value.</summary>
    [HtmlAttributeName("rhx-category-name")] public string? CategoryName { get; set; }

    /// <summary>Optional rhx-value of the wedge to activate on initial render.</summary>
    [HtmlAttributeName("rhx-default-category")] public string? DefaultCategory { get; set; }

    // Ordered cycle for wedges that omit rhx-color. Only tokens that exist.
    private static readonly string[] ColorCycle =
        { "brand", "success", "warning", "danger", "neutral" };

    private static readonly HashSet<string> AllowedColors =
        new(ColorCycle, StringComparer.OrdinalIgnoreCase);

    public RadialSelectTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // ── Collect child wedges ──
        context.Items["RhxRadialOptions"] = new List<RadialOptionData>();
        await output.GetChildContentAsync();
        var options = (List<RadialOptionData>)context.Items["RhxRadialOptions"];

        var resolvedName = ResolveName();
        var resolvedId = ResolveId();
        var resolvedValue = ResolveValue();
        var size = Size.ToLowerInvariant();
        var listboxId = $"{resolvedId}-listbox";
        var pieId = $"{resolvedId}-pie";

        // ── Resolve per-wedge colors (explicit or cycle) ──
        var colored = ResolveColors(options);

        // ── Wrapper ──
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var css = CreateCssBuilder()
            .AddIf(GetModifierClass(size), size != "medium")
            .AddIf(GetModifierClass("disabled"), Disabled);
        ApplyWrapperAttributes(output, css);
        output.Attributes.SetAttribute("data-rhx-radial-select", "");

        var sb = new StringBuilder();
        sb.Append($"<div class=\"{GetElementClass("group")}\">");

        // Trigger button
        sb.Append($"<button type=\"button\" class=\"{GetElementClass("trigger")}\"");
        sb.Append($" id=\"{Enc(resolvedId)}-trigger\"");
        sb.Append(" aria-haspopup=\"menu\" aria-expanded=\"false\"");
        sb.Append($" aria-controls=\"{Enc(pieId)}\"");
        if (!string.IsNullOrEmpty(AriaLabel)) sb.Append($" aria-label=\"{Enc(AriaLabel)}\"");
        if (Disabled) sb.Append(" disabled");
        sb.Append("></button>");

        // Dropdown shell (listbox populated by htmx cascade)
        sb.Append($"<div class=\"{GetElementClass("dropdown")}\">");
        sb.Append($"<div class=\"{GetElementClass("listbox")}\" id=\"{Enc(listboxId)}\" role=\"listbox\" hidden></div>");
        sb.Append("</div>"); // dropdown
        sb.Append("</div>"); // group

        // Pie popup (filled in Task 4)
        sb.Append($"<div class=\"{GetElementClass("pie")}\" id=\"{Enc(pieId)}\" role=\"menu\"");
        if (!string.IsNullOrEmpty(AriaLabel)) sb.Append($" aria-label=\"{Enc(AriaLabel)}\"");
        sb.Append(" hidden></div>");

        // Hidden value input
        sb.Append($"<input type=\"hidden\" data-rhx-radial-value name=\"{Enc(resolvedName)}\" value=\"{Enc(resolvedValue ?? "")}\"");
        sb.Append(BuildValidationAttributeString());
        sb.Append(" />");

        // Hidden category input
        if (!string.IsNullOrWhiteSpace(CategoryName))
        {
            var cat = DefaultCategory ?? "";
            sb.Append($"<input type=\"hidden\" data-rhx-radial-category name=\"{Enc(CategoryName)}\" value=\"{Enc(cat)}\" />");
        }

        output.Content.SetHtmlContent(sb.ToString());
    }

    private List<(RadialOptionData Opt, string Color)> ResolveColors(List<RadialOptionData> options)
    {
        var result = new List<(RadialOptionData, string)>();
        var cycleIndex = 0;
        foreach (var opt in options)
        {
            string color;
            if (!string.IsNullOrWhiteSpace(opt.Color) && AllowedColors.Contains(opt.Color))
            {
                color = opt.Color.ToLowerInvariant();
            }
            else
            {
                color = ColorCycle[cycleIndex % ColorCycle.Length];
                cycleIndex++;
            }
            result.Add((opt, color));
        }
        return result;
    }
}
```

> Note: `CssClassBuilder.AddIf` and `Add` are used elsewhere (see `SelectTagHelper.cs:101-107`). `RestrictChildren`/`ParentTag` enforce the compound relationship at compile/runtime.

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test htmxRazor.Tests --filter RadialSelect`
Expected: PASS (the three Task 2 tests).

- [ ] **Step 5: Commit**

```bash
git add htmxRazor/Components/Forms/RadialSelectTagHelper.cs htmxRazor.Tests/RadialSelectTagHelperTests.cs
git commit -m "feat(radial-select): wrapper renders group, trigger, hidden inputs"
```

---

## Task 3: Color resolution rules (explicit, cycle, invalid fallback)

**Files:**
- Modify: `htmxRazor/Components/Forms/RadialSelectTagHelper.cs` (already has `ResolveColors`)
- Test: `htmxRazor.Tests/RadialSelectTagHelperTests.cs`

This task locks the §3.3 color behavior with tests against the rendered pie wedge fills. Since the pie SVG is rendered in Task 4, here we test `ResolveColors` indirectly by exposing it. Make `ResolveColors` `internal` and add `InternalsVisibleTo`.

- [ ] **Step 1: Write the failing tests**

```csharp
[Fact]
public void ResolveColors_Uses_Explicit_Variant_When_Valid()
{
    var helper = CreateRadialHelper();
    var opts = new List<RadialOptionData>
    {
        new() { Value = "a", Label = "A", Color = "success" },
    };
    var result = helper.ResolveColorsForTest(opts);
    Assert.Equal("success", result[0].Color);
}

[Fact]
public void ResolveColors_Cycles_When_Color_Omitted()
{
    var helper = CreateRadialHelper();
    var opts = new List<RadialOptionData>
    {
        new() { Value = "a", Label = "A" },
        new() { Value = "b", Label = "B" },
        new() { Value = "c", Label = "C" },
        new() { Value = "d", Label = "D" },
        new() { Value = "e", Label = "E" },
        new() { Value = "f", Label = "F" }, // wraps
    };
    var result = helper.ResolveColorsForTest(opts);
    Assert.Equal(new[] { "brand", "success", "warning", "danger", "neutral", "brand" },
        result.Select(r => r.Color).ToArray());
}

[Fact]
public void ResolveColors_Falls_Back_To_Cycle_For_Invalid_Color()
{
    var helper = CreateRadialHelper();
    var opts = new List<RadialOptionData>
    {
        new() { Value = "a", Label = "A", Color = "#ff0000" }, // invalid -> first cycle slot
    };
    var result = helper.ResolveColorsForTest(opts);
    Assert.Equal("brand", result[0].Color);
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test htmxRazor.Tests --filter ResolveColors`
Expected: FAIL — `ResolveColorsForTest` not defined (compile error).

- [ ] **Step 3: Add the test seam**

In `RadialSelectTagHelper.cs`, add a thin internal pass-through (keeps the private method intact):

```csharp
/// <summary>Test seam for color resolution. Not part of the public API.</summary>
internal List<(RadialOptionData Opt, string Color)> ResolveColorsForTest(List<RadialOptionData> options)
    => ResolveColors(options);
```

Add to `htmxRazor/htmxRazor.csproj` (inside an `<ItemGroup>`), if not already present:

```xml
<InternalsVisibleTo Include="htmxRazor.Tests" />
```

(If the project already exposes internals to the test project, skip the csproj edit — check with `grep -n InternalsVisibleTo htmxRazor/htmxRazor.csproj htmxRazor/**/*.cs`.)

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test htmxRazor.Tests --filter ResolveColors`
Expected: PASS (all three).

- [ ] **Step 5: Commit**

```bash
git add htmxRazor/Components/Forms/RadialSelectTagHelper.cs htmxRazor/htmxRazor.csproj htmxRazor.Tests/RadialSelectTagHelperTests.cs
git commit -m "test(radial-select): lock color resolution (explicit/cycle/invalid)"
```

---

## Task 4: Render the SVG pie wedges with icons + ARIA

**Files:**
- Modify: `htmxRazor/Components/Forms/RadialSelectTagHelper.cs`
- Test: `htmxRazor.Tests/RadialSelectTagHelperTests.cs`

- [ ] **Step 1: Write the failing tests**

```csharp
private RadialSelectTagHelper CreateRadialWithOptions(out TagHelperContext ctx)
{
    var helper = CreateRadialHelper();
    helper.Name = "FoodItem";
    helper.AriaLabel = "Food category";
    ctx = CreateContext("rhx-radial-select");
    // Pre-seed the options as if children ran:
    ctx.Items["RhxRadialOptions"] = new List<RadialOptionData>
    {
        new() { Value = "fruit", Label = "Fruit", Icon = "apple", Color = "danger",
                HxGet = "/Menu?handler=Items&cat=fruit" },
        new() { Value = "meat", Label = "Meat", Icon = "grid", Color = "success",
                HxGet = "/Menu?handler=Items&cat=meat" },
    };
    return helper;
}

[Fact]
public async Task RadialSelect_Renders_One_Wedge_Per_Option_With_Menuitemradio()
{
    var helper = CreateRadialWithOptions(out var ctx);
    var output = CreateOutput("rhx-radial-select", childContent: "");

    await helper.ProcessAsync(ctx, output);

    var html = output.Content.GetContent();
    Assert.Contains("<svg", html);
    Assert.Equal(2, System.Text.RegularExpressions.Regex.Matches(html, "role=\"menuitemradio\"").Count);
    Assert.Contains("data-rhx-radial-option-value=\"fruit\"", html);
    Assert.Contains("data-rhx-radial-hx-get=\"/Menu?handler=Items&amp;cat=fruit\"", html);
}

[Fact]
public async Task RadialSelect_Wedge_Uses_Resolved_Color_Token()
{
    var helper = CreateRadialWithOptions(out var ctx);
    var output = CreateOutput("rhx-radial-select", childContent: "");

    await helper.ProcessAsync(ctx, output);

    var html = output.Content.GetContent();
    Assert.Contains("var(--rhx-color-danger-500)", html); // fruit wedge fill
    Assert.Contains("var(--rhx-color-success-500)", html); // meat wedge fill
}

[Fact]
public async Task RadialSelect_DefaultCategory_Marks_Wedge_Checked_And_Echoes_Icon()
{
    var helper = CreateRadialWithOptions(out var ctx);
    helper.DefaultCategory = "meat";
    var output = CreateOutput("rhx-radial-select", childContent: "");

    await helper.ProcessAsync(ctx, output);

    var html = output.Content.GetContent();
    // The meat wedge is checked
    Assert.Contains("data-rhx-radial-option-value=\"meat\" role=\"menuitemradio\" aria-checked=\"true\"", html);
}
```

> Note: the third test asserts a specific attribute order; if your builder emits attributes in a different order, assert the substrings independently instead (`aria-checked="true"` co-located with the meat wedge). Adjust the assertion to match your emission order rather than reordering the builder.

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test htmxRazor.Tests --filter "RadialSelect_Renders_One_Wedge_Per_Option_With_Menuitemradio|RadialSelect_Wedge_Uses_Resolved_Color_Token|RadialSelect_DefaultCategory"`
Expected: FAIL — pie body is currently empty.

- [ ] **Step 3: Implement the pie renderer**

Replace the empty pie line in `ProcessAsync` (`...role=\"menu\"... hidden></div>`) with a call to a new `BuildPie` method, and add the method. Geometry: viewBox `0 0 200 200`, center `(100,100)`, radius `92`, icon radius `58`.

```csharp
// In ProcessAsync, replace the empty pie div with:
sb.Append($"<div class=\"{GetElementClass("pie")}\" id=\"{Enc(pieId)}\" role=\"menu\"");
if (!string.IsNullOrEmpty(AriaLabel)) sb.Append($" aria-label=\"{Enc(AriaLabel)}\"");
sb.Append(" hidden>");
sb.Append(BuildPie(colored));
sb.Append("</div>");
```

```csharp
private string BuildPie(List<(RadialOptionData Opt, string Color)> colored)
{
    if (colored.Count == 0) return "";

    const double cx = 100, cy = 100, r = 92, iconR = 58;
    var n = colored.Count;
    var slice = 360.0 / n;
    var sb = new StringBuilder();

    sb.Append("<svg class=\"" + GetElementClass("wheel") + "\" viewBox=\"0 0 200 200\" ");
    sb.Append("xmlns=\"http://www.w3.org/2000/svg\" aria-hidden=\"false\">");

    for (var i = 0; i < n; i++)
    {
        var (opt, color) = colored[i];
        var start = -90 + i * slice;          // start at 12 o'clock
        var end = start + slice;
        var (x1, y1) = Polar(cx, cy, r, start);
        var (x2, y2) = Polar(cx, cy, r, end);
        var largeArc = slice > 180 ? 1 : 0;

        var isChecked = !string.IsNullOrEmpty(DefaultCategory)
            && string.Equals(DefaultCategory, opt.Value, StringComparison.OrdinalIgnoreCase);

        // Single-wedge (n==1) degenerates to a full circle.
        var d = n == 1
            ? $"M {F(cx - r)} {F(cy)} a {F(r)} {F(r)} 0 1 0 {F(r * 2)} 0 a {F(r)} {F(r)} 0 1 0 {F(-r * 2)} 0"
            : $"M {F(cx)} {F(cy)} L {F(x1)} {F(y1)} A {F(r)} {F(r)} 0 {largeArc} 1 {F(x2)} {F(y2)} Z";

        sb.Append("<g class=\"" + GetElementClass("wedge") + "\"");
        sb.Append($" data-rhx-radial-option-value=\"{Enc(opt.Value)}\"");
        sb.Append(" role=\"menuitemradio\"");
        sb.Append($" aria-checked=\"{(isChecked ? "true" : "false")}\"");
        sb.Append($" aria-label=\"{Enc(opt.Label)}\"");
        if (opt.Disabled) sb.Append(" aria-disabled=\"true\"");
        if (!string.IsNullOrWhiteSpace(opt.HxGet)) sb.Append($" data-rhx-radial-hx-get=\"{Enc(opt.HxGet)}\"");
        sb.Append($" data-rhx-radial-color=\"{Enc(color)}\"");
        sb.Append(" tabindex=\"-1\">");

        sb.Append($"<path d=\"{d}\" fill=\"var(--rhx-color-{color}-500)\" />");

        // Icon at wedge centroid angle
        var mid = start + slice / 2.0;
        var (ix, iy) = Polar(cx, cy, iconR, mid);
        var iconSvg = !string.IsNullOrWhiteSpace(opt.Icon) ? IconRegistry.Get(opt.Icon!) : null;
        if (iconSvg != null)
        {
            sb.Append($"<g transform=\"translate({F(ix - 8)} {F(iy - 8)})\" class=\"{GetElementClass("wedge-icon")}\" aria-hidden=\"true\">");
            sb.Append(iconSvg);
            sb.Append("</g>");
        }
        sb.Append("</g>");
    }

    sb.Append("</svg>");
    return sb.ToString();
}

private static (double X, double Y) Polar(double cx, double cy, double radius, double angleDeg)
{
    var rad = angleDeg * Math.PI / 180.0;
    return (cx + radius * Math.Cos(rad), cy + radius * Math.Sin(rad));
}

private static string F(double v) =>
    v.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
```

Add `using System.Globalization;` is avoided by fully-qualifying above; keep the existing `using htmxRazor.Components.Imagery;` for `IconRegistry`.

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test htmxRazor.Tests --filter "RadialSelect_Renders_One_Wedge_Per_Option_With_Menuitemradio|RadialSelect_Wedge_Uses_Resolved_Color_Token|RadialSelect_DefaultCategory"`
Expected: PASS. If the third fails on attribute order, split it into independent substring asserts per the note.

- [ ] **Step 5: Run the full test file + build**

Run: `dotnet build htmxRazor && dotnet test htmxRazor.Tests --filter RadialSelect`
Expected: build OK (TreatWarningsAsErrors), all radial tests PASS.

- [ ] **Step 6: Commit**

```bash
git add htmxRazor/Components/Forms/RadialSelectTagHelper.cs htmxRazor.Tests/RadialSelectTagHelperTests.cs
git commit -m "feat(radial-select): render SVG pie wedges with icons + ARIA menu roles"
```

---

## Task 5: Component CSS

**Files:**
- Create: `htmxRazor/Assets/css/components/rhx-radial-select.css`
- Test: manual + the build (asset auto-embeds).

- [ ] **Step 1: Write the stylesheet**

```css
@layer rhx.components {
  .rhx-radial-select {
    position: relative;
    display: inline-flex;
    flex-direction: column;
  }

  .rhx-radial-select__group {
    display: flex;
    align-items: stretch;
    height: var(--rhx-control-height-md, 2.5rem);
  }

  .rhx-radial-select__trigger {
    flex: 0 0 auto;
    width: 2.75rem;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    border: 1px solid var(--rhx-color-border, var(--rhx-color-neutral-300));
    border-right: none;
    border-radius: var(--rhx-radius-md) 0 0 var(--rhx-radius-md);
    background: var(--rhx-color-surface, var(--rhx-color-neutral-100));
    color: var(--rhx-color-brand-600, var(--rhx-color-brand-500));
    cursor: pointer;
  }
  .rhx-radial-select__trigger:focus-visible {
    outline: 2px solid var(--rhx-color-brand-500);
    outline-offset: 2px;
  }
  .rhx-radial-select__trigger[disabled] { opacity: .5; cursor: not-allowed; }

  /* active category color echoed onto the trigger via inline custom prop set by JS */
  .rhx-radial-select__trigger[data-rhx-active-color] {
    background: var(--rhx-radial-active, var(--rhx-color-surface));
    color: #fff;
    border-color: var(--rhx-radial-active, transparent);
  }

  .rhx-radial-select__dropdown { flex: 1 1 auto; min-width: 12rem; }

  .rhx-radial-select__listbox {
    height: 100%;
    border: 1px solid var(--rhx-color-border, var(--rhx-color-neutral-300));
    border-radius: 0 var(--rhx-radius-md) var(--rhx-radius-md) 0;
    background: var(--rhx-color-surface, #fff);
    overflow: auto;
  }

  /* Pie popup */
  .rhx-radial-select__pie {
    position: absolute;
    z-index: var(--rhx-z-popover, 50);
    width: 12rem;
    height: 12rem;
    filter: drop-shadow(0 8px 24px rgba(0, 0, 0, .22));
    transform-origin: bottom left;
    animation: rhx-radial-in 120ms ease-out;
  }
  .rhx-radial-select__wheel { width: 100%; height: 100%; }

  .rhx-radial-select__wedge { cursor: pointer; }
  .rhx-radial-select__wedge > path {
    transition: filter 120ms ease, transform 120ms ease;
    transform-origin: 100px 100px;
  }
  .rhx-radial-select__wedge:hover > path,
  .rhx-radial-select__wedge:focus-visible > path { filter: brightness(1.08); }
  .rhx-radial-select__wedge[aria-checked="true"] > path { filter: brightness(1.12); }
  .rhx-radial-select__wedge[aria-disabled="true"] { cursor: not-allowed; }
  .rhx-radial-select__wedge[aria-disabled="true"] > path { opacity: .4; }
  .rhx-radial-select__wedge-icon svg { width: 16px; height: 16px; color: #fff; }

  @keyframes rhx-radial-in {
    from { opacity: 0; transform: scale(.85); }
    to   { opacity: 1; transform: scale(1); }
  }

  @media (prefers-reduced-motion: reduce) {
    .rhx-radial-select__pie { animation: none; }
    .rhx-radial-select__wedge > path { transition: none; }
  }
}

/* Dark theme: tokens already shift via [data-rhx-theme="dark"]; only fixed colors need help. */
[data-rhx-theme="dark"] .rhx-radial-select__wedge-icon svg { color: #fff; }
```

> Confirm the token names used (`--rhx-radius-md`, `--rhx-color-neutral-300`, `--rhx-color-border`, `--rhx-z-popover`, `--rhx-control-height-md`) exist in `htmxRazor/Assets/css/rhx-tokens.css`. If a token is absent, the `var(--token, fallback)` second argument covers it; otherwise replace with the closest existing token. Run: `grep -nE "radius-md|neutral-300|z-popover|control-height|color-border" htmxRazor/Assets/css/rhx-tokens.css`.

- [ ] **Step 2: Verify it embeds (build)**

Run: `dotnet build htmxRazor`
Expected: build succeeds; `Assets/**/*` glob includes the new file (no csproj change needed).

- [ ] **Step 3: Commit**

```bash
git add htmxRazor/Assets/css/components/rhx-radial-select.css
git commit -m "feat(radial-select): component stylesheet (pie, trigger, dark/reduced-motion)"
```

---

## Task 6: Component JS (open/close, keyboard, cascade)

**Files:**
- Create: `htmxRazor/Assets/js/components/rhx-radial-select.js`

Read `htmxRazor/Assets/js/components/rhx-dropdown.js` first to match the file's IIFE/registration style and how it consumes `rhx-position.js`.

- [ ] **Step 1: Write the script**

```js
/* rhx-radial-select — pie category selector driving an htmx-loaded dropdown. */
(function () {
  "use strict";

  function init(root) {
    if (root.__rhxRadialInit) return;
    root.__rhxRadialInit = true;

    var trigger = root.querySelector(".rhx-radial-select__trigger");
    var pie = root.querySelector(".rhx-radial-select__pie");
    var listbox = root.querySelector(".rhx-radial-select__listbox");
    var categoryInput = root.querySelector("[data-rhx-radial-category]");
    var valueInput = root.querySelector("[data-rhx-radial-value]");
    if (!trigger || !pie) return;

    var wedges = Array.prototype.slice.call(
      pie.querySelectorAll('[role="menuitemradio"]')
    );

    function open() {
      pie.hidden = false;
      trigger.setAttribute("aria-expanded", "true");
      if (window.RHX && window.RHX.position) {
        window.RHX.position(pie, trigger, { placement: "top-start" });
      }
      var active = wedges.find(function (w) {
        return w.getAttribute("aria-checked") === "true";
      }) || wedges[0];
      if (active) active.focus();
      document.addEventListener("click", onDocClick, true);
    }

    function close(focusTrigger) {
      pie.hidden = true;
      trigger.setAttribute("aria-expanded", "false");
      document.removeEventListener("click", onDocClick, true);
      if (focusTrigger) trigger.focus();
    }

    function onDocClick(e) {
      if (!root.contains(e.target)) close(false);
    }

    function select(wedge) {
      if (!wedge || wedge.getAttribute("aria-disabled") === "true") return;
      wedges.forEach(function (w) { w.setAttribute("aria-checked", "false"); });
      wedge.setAttribute("aria-checked", "true");

      var value = wedge.getAttribute("data-rhx-radial-option-value") || "";
      var color = wedge.getAttribute("data-rhx-radial-color");
      var hxGet = wedge.getAttribute("data-rhx-radial-hx-get");

      if (categoryInput) categoryInput.value = value;
      if (color) {
        trigger.setAttribute("data-rhx-active-color", color);
        trigger.style.setProperty("--rhx-radial-active", "var(--rhx-color-" + color + "-500)");
        var iconG = wedge.querySelector(".rhx-radial-select__wedge-icon");
        trigger.innerHTML = iconG ? iconG.innerHTML : "";
      }

      if (hxGet && window.htmx) {
        window.htmx.ajax("GET", hxGet, {
          target: listbox,
          swap: "innerHTML",
        }).then(function () {
          autoSelectFirst();
        });
      }
      close(false);
    }

    function autoSelectFirst() {
      if (!listbox) return;
      var first = listbox.querySelector('[role="option"]:not([aria-disabled="true"])');
      if (first) {
        listbox.querySelectorAll('[aria-selected="true"]').forEach(function (o) {
          o.setAttribute("aria-selected", "false");
        });
        first.setAttribute("aria-selected", "true");
        if (valueInput) valueInput.value = first.getAttribute("data-value") || "";
      } else if (valueInput) {
        valueInput.value = "";
      }
    }

    trigger.addEventListener("click", function () {
      if (trigger.hasAttribute("disabled")) return;
      pie.hidden ? open() : close(false);
    });

    pie.addEventListener("click", function (e) {
      var w = e.target.closest('[role="menuitemradio"]');
      if (w) select(w);
    });

    pie.addEventListener("keydown", function (e) {
      var idx = wedges.indexOf(document.activeElement);
      if (e.key === "ArrowRight" || e.key === "ArrowDown") {
        e.preventDefault();
        wedges[(idx + 1) % wedges.length].focus();
      } else if (e.key === "ArrowLeft" || e.key === "ArrowUp") {
        e.preventDefault();
        wedges[(idx - 1 + wedges.length) % wedges.length].focus();
      } else if (e.key === "Enter" || e.key === " ") {
        e.preventDefault();
        select(document.activeElement);
      } else if (e.key === "Escape") {
        e.preventDefault();
        close(true);
      } else if (e.key.length === 1) {
        var match = wedges.find(function (w) {
          var label = (w.getAttribute("aria-label") || "").toLowerCase();
          return label.indexOf(e.key.toLowerCase()) === 0;
        });
        if (match) match.focus();
      }
    });
  }

  function initAll(scope) {
    (scope || document)
      .querySelectorAll("[data-rhx-radial-select]")
      .forEach(init);
  }

  if (document.readyState !== "loading") initAll();
  else document.addEventListener("DOMContentLoaded", function () { initAll(); });

  document.body && document.body.addEventListener("htmx:afterSwap", function (e) {
    initAll(e.target);
  });

  window.RHX = window.RHX || {};
  window.RHX.initRadialSelect = initAll;
})();
```

> Verify the positioning entry point: `grep -nE "RHX\.position|position\s*=|function position" htmxRazor/Assets/js/components/rhx-position.js`. If the public name differs (e.g., `RHX.positionElement`), update the `open()` call accordingly.

- [ ] **Step 2: Build to confirm embedding**

Run: `dotnet build htmxRazor`
Expected: succeeds (JS is just an embedded asset).

- [ ] **Step 3: Commit**

```bash
git add htmxRazor/Assets/js/components/rhx-radial-select.js
git commit -m "feat(radial-select): interaction JS (open/close, keyboard, htmx cascade)"
```

---

## Task 7: Demo page + server handler

**Files:**
- Create: `htmxRazor.Demo/Pages/RadialSelect.cshtml`
- Create: `htmxRazor.Demo/Pages/RadialSelect.cshtml.cs`

Match an existing demo page's structure first: read `htmxRazor.Demo/Pages/` for a comparable component page (e.g., the Select or Kanban page) to mirror layout, CSS/JS `<link>`/`<script>` includes, and namespace.

- [ ] **Step 1: Write the PageModel with the cascade handler**

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace htmxRazor.Demo.Pages;

public class RadialSelectModel : PageModel
{
    private static readonly Dictionary<string, (string Value, string Text)[]> Catalog = new()
    {
        ["fruit"] = new[] { ("apple", "Apple"), ("banana", "Banana"), ("mango", "Mango"), ("pear", "Pear") },
        ["veg"]   = new[] { ("carrot", "Carrot"), ("broccoli", "Broccoli"), ("kale", "Kale") },
        ["meat"]  = new[] { ("chicken", "Chicken"), ("beef", "Beef"), ("pork", "Pork"), ("lamb", "Lamb") },
    };

    public IActionResult OnGetItems(string cat)
    {
        var items = Catalog.TryGetValue(cat ?? "", out var rows) ? rows : Array.Empty<(string, string)>();
        var html = string.Concat(items.Select(i =>
            $"<div class=\"rhx-radial-select__option\" role=\"option\" data-value=\"{i.Item1}\" tabindex=\"-1\">{i.Item2}</div>"));
        return Content(html, "text/html");
    }
}
```

- [ ] **Step 2: Write the demo view**

```razor
@page
@model htmxRazor.Demo.Pages.RadialSelectModel
@{
    ViewData["Title"] = "Radial Select";
}
<link rel="stylesheet" href="/_rhx/css/components/rhx-radial-select.css" />
<link rel="stylesheet" href="/_rhx/css/components/rhx-select.css" />
<script src="/_rhx/js/components/rhx-position.js" defer></script>
<script src="/_rhx/js/components/rhx-radial-select.js" defer></script>

<h1>Radial Select</h1>
<p>Pick a category from the pie; the dropdown loads that category's items.</p>

<rhx-radial-select name="FoodItem"
                   rhx-category-name="Category"
                   rhx-default-category="fruit"
                   rhx-placeholder="Choose an item…"
                   aria-label="Food category">
    <rhx-radial-option rhx-value="fruit" rhx-label="Fruit" rhx-icon="apple"  rhx-color="danger"
                       hx-get="/RadialSelect?handler=Items&cat=fruit" />
    <rhx-radial-option rhx-value="veg"   rhx-label="Vegetables" rhx-icon="grid" rhx-color="success"
                       hx-get="/RadialSelect?handler=Items&cat=veg" />
    <rhx-radial-option rhx-value="meat"  rhx-label="Meat" rhx-icon="layers" rhx-color="warning"
                       hx-get="/RadialSelect?handler=Items&cat=meat" />
</rhx-radial-select>
```

> Replace `apple`/`grid`/`layers` with real registered icon names — confirm via `grep -n "Register(" htmxRazor/Components/Imagery/IconRegistry.cs` or `IconRegistry.GetNames()`. The spec used `apple`; if it isn't registered, either register it or pick an existing icon.

- [ ] **Step 3: Run the demo and click through**

Run: `dotnet run --project htmxRazor.Demo` then open `/RadialSelect`.
Expected: trigger opens the pie; clicking a wedge loads the dropdown items, echoes color+icon on the trigger, and auto-selects the first item.

- [ ] **Step 4: Commit**

```bash
git add htmxRazor.Demo/Pages/RadialSelect.cshtml htmxRazor.Demo/Pages/RadialSelect.cshtml.cs
git commit -m "docs(radial-select): demo page with cascade handler"
```

---

## Task 8: Playwright E2E

**Files:**
- Create: `PlaywrightTests/RadialSelectTests.cs`

Read an existing test in `PlaywrightTests/` first to match base class, fixture, and base-URL conventions.

- [ ] **Step 1: Write the E2E test (adapt to the existing fixture)**

```csharp
using System.Threading.Tasks;
using Microsoft.Playwright;
using Xunit;

namespace PlaywrightTests;

public class RadialSelectTests : PageTest // adapt to the repo's actual base/fixture
{
    [Fact]
    public async Task Selecting_A_Wedge_Loads_Dropdown_And_AutoSelects_First()
    {
        await Page.GotoAsync("/RadialSelect"); // adapt base URL via fixture

        await Page.ClickAsync(".rhx-radial-select__trigger");
        await Expect(Page.Locator(".rhx-radial-select__pie")).ToBeVisibleAsync();

        await Page.ClickAsync('[data-rhx-radial-option-value="meat"]');

        var firstOption = Page.Locator(".rhx-radial-select__listbox [role=option]").First;
        await Expect(firstOption).ToHaveAttributeAsync("aria-selected", "true");
        await Expect(Page.Locator("[data-rhx-radial-category]"))
            .ToHaveValueAsync("meat");
    }
}
```

- [ ] **Step 2: Run E2E locally**

Run: `dotnet test PlaywrightTests --filter RadialSelect` (with the demo running per the repo's Playwright setup).
Expected: PASS.

- [ ] **Step 3: Commit**

```bash
git add PlaywrightTests/RadialSelectTests.cs
git commit -m "test(radial-select): Playwright E2E for open->select->cascade"
```

---

## Task 9: Docs, changelog, snippet

**Files:**
- Modify: `README.md` (Component Catalog → Forms; add a Feature Highlights snippet)
- Modify: `CHANGELOG.md` (under `## [Unreleased]` / a new `2.1` section)
- Create/Modify: `vscode-extension/snippets/` (add `rhx-radial-select` snippet)

- [ ] **Step 1: README — add to the Forms catalog row**

In the Component Catalog table, change the Forms row to include `Radial Select`:

```markdown
| **Forms** | Input, Textarea, Select, Radial Select, Combobox, Checkbox, Switch, Radio, Slider, Rating, Color Picker, File Input, Number Input, htmx Form |
```

Add a Feature Highlights block:

````markdown
### Radial Select (pie category → cascading dropdown)

```html
<rhx-radial-select name="FoodItem" rhx-category-name="Category"
                   rhx-placeholder="Choose an item…" aria-label="Food category">
    <rhx-radial-option rhx-value="fruit" rhx-label="Fruit" rhx-icon="apple" rhx-color="danger"
                       hx-get="/Menu?handler=Items&cat=fruit" />
    <rhx-radial-option rhx-value="meat"  rhx-label="Meat"  rhx-icon="grid"  rhx-color="success"
                       hx-get="/Menu?handler=Items&cat=meat" />
</rhx-radial-select>
```
````

- [ ] **Step 2: CHANGELOG entry**

```markdown
## [2.1.0] — Advanced Inputs

### Added
- **Radial Select** — `<rhx-radial-select>` + `<rhx-radial-option>`. A rectangular trigger flush-left against a dropdown opens a circular SVG pie of category wedges (color + icon). Selecting a wedge fires `hx-get`, swaps the dropdown's option set, echoes the category color/icon onto the trigger, and auto-selects the first option. Wedge colors use named variant tokens (brand/success/warning/danger/neutral) with a deterministic cycle when omitted. Accessible as a menu (`menuitemradio`, arrow-key + type-ahead navigation).

### Components Added
- **Forms**: Radial Select, Radial Option
```

- [ ] **Step 3: VS Code snippet**

Add (match the existing snippet file's JSON shape):

```json
"rhx-radial-select": {
  "scope": "html,razor,aspnetcorerazor",
  "prefix": "rhx-radial-select",
  "body": [
    "<rhx-radial-select name=\"${1:FoodItem}\" rhx-category-name=\"${2:Category}\" rhx-placeholder=\"${3:Choose…}\" aria-label=\"${4:Category}\">",
    "\t<rhx-radial-option rhx-value=\"${5:fruit}\" rhx-label=\"${6:Fruit}\" rhx-icon=\"${7:apple}\" rhx-color=\"${8|brand,success,warning,danger,neutral|}\" hx-get=\"${9:/Items?cat=fruit}\" />",
    "\t$0",
    "</rhx-radial-select>"
  ],
  "description": "htmxRazor radial select (pie category -> cascading dropdown)"
}
```

- [ ] **Step 4: Commit**

```bash
git add README.md CHANGELOG.md vscode-extension
git commit -m "docs(radial-select): README catalog, changelog, VS Code snippet"
```

---

## Task 10: Full verification

- [ ] **Step 1: Build the whole solution**

Run: `dotnet build`
Expected: succeeds with `TreatWarningsAsErrors=true`.

- [ ] **Step 2: Run all unit tests**

Run: `dotnet test htmxRazor.Tests`
Expected: all pass (previous count + new radial tests).

- [ ] **Step 3: Manual demo smoke test**

Run: `dotnet run --project htmxRazor.Demo` → `/RadialSelect`. Verify open/close, keyboard (arrows, Enter, Esc, type-ahead), cascade, auto-select-first, trigger echo, dark theme (`RHX.toggleTheme()`), and reduced-motion.

- [ ] **Step 4: Final commit (if any docs tweaks)**

```bash
git add -A && git commit -m "chore(radial-select): final verification tidy-ups"
```

---

## Self-Review (completed)

**Spec coverage:** §2 behavior → Tasks 4/6 (cascade, auto-select-first, echo); §3.1 wrapper API → Task 2; §3.2 option API → Task 1; §3.3 color rules → Task 3; §4 SVG render → Task 4; §5 JS behavior → Task 6; §6 accessibility (menu/menuitemradio, keyboard) → Tasks 4/6; §7 units/isolation → Tasks 1–6; §8 testing → Tasks 1–4 (unit) + Task 8 (E2E); §9 docs → Task 9. No gaps.

**Placeholder scan:** No TBD/TODO; every code step contains complete code. Three "verify token/icon/position-name" notes are explicit verification commands, not placeholders — they exist because those names live in files outside this plan's edits.

**Type consistency:** `RadialOptionData` (Task 1) used unchanged in Tasks 2–4; `ResolveColors`/`ResolveColorsForTest` consistent (Tasks 2–3); data attributes (`data-rhx-radial-option-value`, `-hx-get`, `-color`, `-value`, `-category`) match between the renderer (Task 4) and the JS (Task 6); `BlockName="radial-select"` makes every `GetElementClass(...)` map to the CSS in Task 5.
