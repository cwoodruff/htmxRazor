# Date/Time Picker — Milestone 2: Time Picker

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ship `<rhx-time-picker>` — a text input + popup list of selectable times (e.g. every 30 min), committing a hidden ISO `HH:mm` value for form binding, with 12-hour display by default.

**Architecture:** A pure `TimeListRenderer` generates the time set (from `rhx-step`, bounded by `rhx-min`/`rhx-max`) and renders the listbox option HTML; display formatting is culture-invariant and deterministic (12-hour `9:30 AM` or 24-hour `09:30`). `TimePickerTagHelper` (extends `FormControlTagHelperBase`) renders the input + clock trigger + hidden ISO value + the static popup listbox (no server endpoint — the list is fixed). `rhx-timepicker.js` handles open/close, option selection (commit hidden + visible display + close), scroll-to-selected, and listbox keyboard (arrows, Enter, Escape, type-ahead). This is independent of the calendar core (Milestone 1); Milestone 3 (datetime) will reuse `TimeListRenderer`.

**Tech Stack:** ASP.NET Core Tag Helpers (.NET 10, C#, `TimeOnly`), htmx (asset injection only — no nav), vanilla JS, BEM CSS with `rhx-` tokens, xUnit, Playwright.

**Spec:** `docs/superpowers/specs/2026-06-11-datetime-picker-family-design.md` (§5, plus §3.3/§3.5/§8/§10/§11).

**Conventions verified in the codebase (read before starting):**
- Form base: `htmxRazor/Components/Forms/FormControlTagHelperBase.cs` — `ResolveName/ResolveId/ResolveValue`, `Enc`, `CreateCssBuilder`, `GetElementClass`, `GetModifierClass`, `ApplyWrapperAttributes`, `ResolveLabelText`, `BuildAriaDescribedBy`, `BuildValidationAttributeString`, `BuildHintHtml`, `BuildErrorHtml`, `HasError`, `ResolveRequired`.
- Reference component (input + trigger + popup listbox + hidden value + keyboard + type-ahead): `htmxRazor/Components/Forms/ComboboxTagHelper.cs` and `htmxRazor/Assets/js/components/rhx-combobox.js`. Match their idioms.
- A just-built sibling using the same overall shape: `htmxRazor/Components/Forms/DatePickerTagHelper.cs` + `htmxRazor/Assets/js/components/rhx-datepicker.js` (popup, `data-rhx-*-display`/`-value`, `RHX.register`).
- Icons: `IconRegistry.Get("clock")` returns the inner SVG (no `<svg>` wrapper).
- Assets auto-embed via `htmxRazor/htmxRazor.csproj` (`<EmbeddedResource Include="Assets/**/*" />`); demo pages link CSS/JS in `_DocsLayout.cshtml` + `_Layout.cshtml`.
- `rhx-position.js` exposes `RHX.positionElement(anchor, floating, {placement, distance, flip, shift})`; `_DocsLayout.cshtml` already loads it.
- Test base: `htmxRazor.Tests/TagHelperTestBase.cs` — `CreateContext`, `CreateOutput`, `CreateViewContext`, `CreateUrlHelperFactory`, `HasClass`, `GetAttribute`.
- Playwright base: `PlaywrightTests/Infrastructure/ComponentTestBase.cs` — `OpenAsync(browser, path)`, `Browsers()`; demo tab panels are `#panel-<rhx-name>-preview`. Chromium already installed locally.
- **Lesson from Milestone 1:** keep display formatting culture-INVARIANT so tests aren't fragile. Use `CultureInfo.InvariantCulture` for all time display formats here.

---

## File Structure

| File | Responsibility |
|---|---|
| `htmxRazor/Components/Forms/Time/TimeListRenderer.cs` | Pure: generate the `TimeOnly` set (step + min/max), format a time for display (12/24h, optional `rhx-format`), and render the `<button role="option">` list HTML. No HTTP/DI. Reused by Milestone 3. |
| `htmxRazor/Components/Forms/TimePickerTagHelper.cs` | `<rhx-time-picker>` — input + clock trigger + hidden ISO value + popup listbox. |
| `htmxRazor/Assets/css/components/rhx-time-picker.css` | Input/trigger/popup-listbox/option styling. |
| `htmxRazor/Assets/js/components/rhx-timepicker.js` | Open/close, select, scroll-to-selected, listbox keyboard (arrows/Enter/Escape/type-ahead), click-outside. |
| `htmxRazor.Tests/TimeListRendererTests.cs` | Unit tests for the renderer. |
| `htmxRazor.Tests/TimePickerTagHelperTests.cs` | Unit tests for the Tag Helper. |
| `htmxRazor.Demo/Pages/Docs/Components/TimePicker.cshtml(.cs)` | Demo page. |
| `PlaywrightTests/ComponentTests/TimePickerTests.cs` | E2E. |

---

## Task 1: `TimeListRenderer` — time set + display formatting

**Files:**
- Create: `htmxRazor/Components/Forms/Time/TimeListRenderer.cs`
- Test: `htmxRazor.Tests/TimeListRendererTests.cs`

- [ ] **Step 1: Write the failing tests**

```csharp
using System;
using htmxRazor.Components.Forms.Time;
using Xunit;

namespace htmxRazor.Tests;

public class TimeListRendererTests
{
    [Fact]
    public void Times_Default_Step_30_Gives_48_Entries_From_Midnight()
    {
        var times = TimeListRenderer.Times(30, null, null);
        Assert.Equal(48, times.Count);
        Assert.Equal(new TimeOnly(0, 0), times[0]);
        Assert.Equal(new TimeOnly(0, 30), times[1]);
        Assert.Equal(new TimeOnly(23, 30), times[^1]);
    }

    [Fact]
    public void Times_Respects_Min_Max_And_Step()
    {
        var times = TimeListRenderer.Times(15, new TimeOnly(9, 0), new TimeOnly(10, 0));
        Assert.Equal(new[] { new TimeOnly(9, 0), new TimeOnly(9, 15), new TimeOnly(9, 30), new TimeOnly(9, 45), new TimeOnly(10, 0) }, times);
    }

    [Fact]
    public void Times_Invalid_Step_Falls_Back_To_30()
    {
        Assert.Equal(48, TimeListRenderer.Times(0, null, null).Count);
        Assert.Equal(48, TimeListRenderer.Times(-5, null, null).Count);
    }

    [Fact]
    public void FormatDisplay_TwelveHour_And_TwentyFourHour()
    {
        var t = new TimeOnly(9, 30);
        Assert.Equal("9:30 AM", TimeListRenderer.FormatDisplay(t, twelveHour: true, format: null));
        Assert.Equal("09:30", TimeListRenderer.FormatDisplay(t, twelveHour: false, format: null));
        Assert.Equal("9:30 AM", TimeListRenderer.FormatDisplay(new TimeOnly(9, 30), true, null));
        Assert.Equal("1:05 PM", TimeListRenderer.FormatDisplay(new TimeOnly(13, 5), true, null));
        Assert.Equal("21:00", TimeListRenderer.FormatDisplay(new TimeOnly(21, 0), false, null));
    }

    [Fact]
    public void FormatDisplay_Honors_Custom_Format()
    {
        Assert.Equal("09.30", TimeListRenderer.FormatDisplay(new TimeOnly(9, 30), true, "HH.mm"));
    }
}
```

- [ ] **Step 2: Run to verify they fail**

Run: `dotnet test htmxRazor.Tests --filter TimeListRendererTests`
Expected: FAIL — `TimeListRenderer` does not exist.

- [ ] **Step 3: Write `TimeListRenderer` (times + formatting; render added in Task 2)**

```csharp
using System;
using System.Collections.Generic;
using System.Globalization;

namespace htmxRazor.Components.Forms.Time;

/// <summary>
/// Pure helper for the time picker: generates the selectable <see cref="TimeOnly"/> set from a
/// step (minutes) bounded by optional min/max, and formats a time for display. Culture-invariant
/// so output is deterministic. No HTTP/DI. Reused by the date+time picker (Milestone 3).
/// </summary>
public static class TimeListRenderer
{
    /// <summary>
    /// The selectable times from <paramref name="min"/> (default 00:00) to <paramref name="max"/>
    /// (default 23:59) inclusive, stepping by <paramref name="stepMinutes"/> (default 30 when &lt; 1).
    /// </summary>
    public static IReadOnlyList<TimeOnly> Times(int stepMinutes, TimeOnly? min, TimeOnly? max)
    {
        if (stepMinutes < 1) stepMinutes = 30;
        var startMin = min is { } mn ? (int)mn.ToTimeSpan().TotalMinutes : 0;
        var endMin = max is { } mx ? (int)mx.ToTimeSpan().TotalMinutes : 23 * 60 + 59;
        var list = new List<TimeOnly>();
        for (var m = startMin; m <= endMin; m += stepMinutes)
            list.Add(TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(m)));
        return list;
    }

    /// <summary>
    /// Formats a time for the visible display. <paramref name="format"/> (a .NET time format string)
    /// wins; otherwise 12-hour (<c>9:30 AM</c>) when <paramref name="twelveHour"/>, else 24-hour (<c>09:30</c>).
    /// Always uses <see cref="CultureInfo.InvariantCulture"/> for deterministic output.
    /// </summary>
    public static string FormatDisplay(TimeOnly t, bool twelveHour, string? format)
    {
        if (!string.IsNullOrEmpty(format)) return t.ToString(format, CultureInfo.InvariantCulture);
        return twelveHour
            ? t.ToString("h:mm tt", CultureInfo.InvariantCulture)
            : t.ToString("HH:mm", CultureInfo.InvariantCulture);
    }
}
```

- [ ] **Step 4: Run to verify they pass**

Run: `dotnet test htmxRazor.Tests --filter TimeListRendererTests`
Expected: PASS (5 tests).

- [ ] **Step 5: Commit**

```bash
git add htmxRazor/Components/Forms/Time/TimeListRenderer.cs htmxRazor.Tests/TimeListRendererTests.cs
git commit -m "feat(time-picker): TimeListRenderer time set + display formatting"
```

---

## Task 2: `TimeListRenderer.RenderOptions` — listbox option HTML

**Files:**
- Modify: `htmxRazor/Components/Forms/Time/TimeListRenderer.cs`
- Test: `htmxRazor.Tests/TimeListRendererTests.cs`

- [ ] **Step 1: Write the failing tests**

```csharp
[Fact]
public void RenderOptions_Emits_One_Option_Per_Time_With_Iso_And_Display()
{
    var html = TimeListRenderer.RenderOptions(30, null, null, twelveHour: true, format: null, selected: new TimeOnly(9, 30));
    Assert.Equal(48, System.Text.RegularExpressions.Regex.Matches(html, "role=\"option\"").Count);
    // ISO 24h in data-time, 12h in the visible label
    Assert.Contains("data-time=\"09:30\"", html);
    Assert.Contains(">9:30 AM</button>", html);
    Assert.Contains("data-time=\"00:00\"", html);
}

[Fact]
public void RenderOptions_Marks_Selected_Option()
{
    var html = TimeListRenderer.RenderOptions(30, null, null, true, null, new TimeOnly(9, 30));
    var idx = html.IndexOf("data-time=\"09:30\"", StringComparison.Ordinal);
    Assert.True(idx >= 0);
    var seg = html.Substring(idx, 60);
    Assert.Contains("aria-selected=\"true\"", seg);
    Assert.Contains("rhx-time-picker__option--selected", html);
}

[Fact]
public void RenderOptions_No_Selection_Has_No_AriaSelected()
{
    var html = TimeListRenderer.RenderOptions(30, null, null, true, null, selected: null);
    Assert.DoesNotContain("aria-selected=\"true\"", html);
}
```

- [ ] **Step 2: Run to verify they fail**

Run: `dotnet test htmxRazor.Tests --filter RenderOptions`
Expected: FAIL — `RenderOptions` not defined.

- [ ] **Step 3: Add `RenderOptions` (and a private `Enc`) to `TimeListRenderer`**

Add `using System.Net;` and `using System.Text;` to the file, then add:

```csharp
/// <summary>Renders the listbox option buttons for the given times. data-time is ISO HH:mm; the label is the display form.</summary>
public static string RenderOptions(int stepMinutes, TimeOnly? min, TimeOnly? max, bool twelveHour, string? format, TimeOnly? selected)
{
    var sb = new StringBuilder();
    foreach (var t in Times(stepMinutes, min, max))
    {
        var iso = t.ToString("HH:mm", CultureInfo.InvariantCulture);
        var disp = FormatDisplay(t, twelveHour, format);
        var isSelected = selected is { } s && s.Hour == t.Hour && s.Minute == t.Minute;

        var cls = "rhx-time-picker__option";
        if (isSelected) cls += " rhx-time-picker__option--selected";
        sb.Append($"<button type=\"button\" class=\"{cls}\" role=\"option\" data-time=\"{iso}\"");
        if (isSelected) sb.Append(" aria-selected=\"true\"");
        sb.Append('>').Append(Enc(disp)).Append("</button>");
    }
    return sb.ToString();
}

private static string Enc(string s) => WebUtility.HtmlEncode(s);
```

- [ ] **Step 4: Run to verify they pass**

Run: `dotnet test htmxRazor.Tests --filter TimeListRendererTests`
Expected: PASS (8 total).

- [ ] **Step 5: Commit**

```bash
git add htmxRazor/Components/Forms/Time/TimeListRenderer.cs htmxRazor.Tests/TimeListRendererTests.cs
git commit -m "feat(time-picker): render listbox option HTML"
```

---

## Task 3: `TimePickerTagHelper`

**Files:**
- Create: `htmxRazor/Components/Forms/TimePickerTagHelper.cs`
- Test: `htmxRazor.Tests/TimePickerTagHelperTests.cs`

- [ ] **Step 1: Write the failing tests**

```csharp
using System;
using htmxRazor.Components.Forms;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Xunit;

namespace htmxRazor.Tests;

public class TimePickerTagHelperTests : TagHelperTestBase
{
    private TimePickerTagHelper CreateHelper() => new(CreateUrlHelperFactory()) { ViewContext = CreateViewContext() };

    private class TestModel { public TimeOnly StartTime { get; set; } public DateTime Event { get; set; } }

    private static ModelExpression Expr(string prop, object? value)
    {
        var provider = new EmptyModelMetadataProvider();
        var metadata = provider.GetMetadataForProperty(typeof(TestModel), prop);
        return new ModelExpression(prop, new ModelExplorer(provider, metadata, value));
    }

    [Fact]
    public async Task Renders_Wrapper_Input_Trigger_And_Hidden_Iso_Value()
    {
        var helper = CreateHelper();
        helper.Name = "StartTime";
        helper.Value = "09:30";
        var ctx = CreateContext("rhx-time-picker");
        var output = CreateOutput("rhx-time-picker");

        await helper.ProcessAsync(ctx, output);

        Assert.Equal("div", output.TagName);
        Assert.True(HasClass(output, "rhx-time-picker"));
        var html = output.Content.GetContent();
        Assert.Contains("data-rhx-time-picker", GetAttribute(output, "class") is null ? "" : ""); // marker checked below
        Assert.True(output.Attributes.TryGetAttribute("data-rhx-time-picker", out _));
        Assert.Contains("rhx-time-picker__input", html);
        Assert.Contains("rhx-time-picker__trigger", html);
        Assert.Contains("aria-haspopup=\"listbox\"", html);
        Assert.Contains("type=\"hidden\"", html);
        Assert.Contains("name=\"StartTime\"", html);
        Assert.Contains("value=\"09:30\"", html);
        Assert.Contains("data-rhx-time-value", html);
    }

    [Fact]
    public async Task Renders_Listbox_With_Options_And_Selected()
    {
        var helper = CreateHelper();
        helper.Name = "t";
        helper.Value = "09:30";
        var ctx = CreateContext("rhx-time-picker");
        var output = CreateOutput("rhx-time-picker");

        await helper.ProcessAsync(ctx, output);
        var html = output.Content.GetContent();

        Assert.Contains("rhx-time-picker__listbox", html);
        Assert.Contains("role=\"listbox\"", html);
        Assert.Contains("data-time=\"09:30\"", html);
        Assert.Contains("aria-selected=\"true\"", html);
        // 12-hour display by default, in both the input value and the selected option
        Assert.Contains(">9:30 AM</button>", html);
        Assert.Contains("value=\"9:30 AM\"", html); // visible input display
    }

    [Fact]
    public async Task TwentyFourHour_Mode_Uses_24h_Display()
    {
        var helper = CreateHelper();
        helper.Name = "t";
        helper.Value = "21:00";
        helper.TwelveHour = false;
        var ctx = CreateContext("rhx-time-picker");
        var output = CreateOutput("rhx-time-picker");

        await helper.ProcessAsync(ctx, output);
        var html = output.Content.GetContent();
        Assert.Contains(">21:00</button>", html);
        Assert.Contains("value=\"21:00\"", html);
    }

    [Fact]
    public async Task Step_Controls_Option_Count()
    {
        var helper = CreateHelper();
        helper.Name = "t";
        helper.Step = 60;
        var ctx = CreateContext("rhx-time-picker");
        var output = CreateOutput("rhx-time-picker");

        await helper.ProcessAsync(ctx, output);
        var html = output.Content.GetContent();
        Assert.Equal(24, System.Text.RegularExpressions.Regex.Matches(html, "role=\"option\"").Count);
    }

    [Fact]
    public async Task TimeOnly_Model_Binding_Produces_Iso_Hidden_Value()
    {
        var helper = CreateHelper();
        helper.For = Expr("StartTime", new TimeOnly(14, 15));
        var ctx = CreateContext("rhx-time-picker");
        var output = CreateOutput("rhx-time-picker");

        await helper.ProcessAsync(ctx, output);
        var html = output.Content.GetContent();
        Assert.Contains("value=\"14:15\"", html); // hidden ISO
    }

    [Fact]
    public async Task DateTime_Model_Binding_Uses_Time_Component()
    {
        var helper = CreateHelper();
        helper.For = Expr("Event", new DateTime(2026, 10, 15, 8, 5, 0));
        var ctx = CreateContext("rhx-time-picker");
        var output = CreateOutput("rhx-time-picker");

        await helper.ProcessAsync(ctx, output);
        var html = output.Content.GetContent();
        Assert.Contains("value=\"08:05\"", html);
    }
}
```

- [ ] **Step 2: Run to verify they fail**

Run: `dotnet test htmxRazor.Tests --filter TimePickerTagHelperTests`
Expected: FAIL — `TimePickerTagHelper` does not exist.

- [ ] **Step 3: Write `TimePickerTagHelper`**

```csharp
using System;
using System.Globalization;
using System.Text;
using htmxRazor.Components.Forms.Time;
using htmxRazor.Components.Imagery;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Forms;

/// <summary>
/// A time input with a popup list of selectable times. The list is static (no server round-trip):
/// times run from <c>rhx-min</c> (default 00:00) to <c>rhx-max</c> (default 23:59) stepping by
/// <c>rhx-step</c> minutes. Selection commits a hidden ISO <c>HH:mm</c> value for form binding;
/// the visible input shows 12-hour (default) or 24-hour display.
/// </summary>
/// <example>
/// <code>
/// &lt;rhx-time-picker rhx-for="StartTime" rhx-step="30" rhx-12hour="true" /&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-time-picker")]
public class TimePickerTagHelper : FormControlTagHelperBase
{
    protected override string BlockName => "time-picker";

    [HtmlAttributeName("rhx-placeholder")] public string? Placeholder { get; set; }
    [HtmlAttributeName("rhx-min")] public string? Min { get; set; }
    [HtmlAttributeName("rhx-max")] public string? Max { get; set; }
    [HtmlAttributeName("rhx-step")] public int Step { get; set; } = 30;
    [HtmlAttributeName("rhx-12hour")] public bool TwelveHour { get; set; } = true;
    [HtmlAttributeName("rhx-format")] public string? Format { get; set; }

    public TimePickerTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        await Task.CompletedTask;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var name = ResolveName();
        var id = ResolveId();
        if (string.IsNullOrEmpty(id)) id = "rhx-tp-" + context.UniqueId;
        var selected = ResolveTime();
        var iso = selected is { } s0 ? s0.ToString("HH:mm", CultureInfo.InvariantCulture) : "";
        var listboxId = $"{id}-listbox";
        var inputId = $"{id}-input";
        var labelId = $"{id}-label";
        var hintId = $"{id}-hint";
        var errorId = $"{id}-error";
        var size = Size.ToLowerInvariant();
        var hasError = HasError();
        var resolvedRequired = ResolveRequired();

        var css = CreateCssBuilder()
            .AddIf(GetModifierClass(size), size != "medium")
            .AddIf(GetModifierClass("disabled"), Disabled)
            .AddIf(GetModifierClass("readonly"), Readonly)
            .AddIf(GetModifierClass("error"), hasError);
        ApplyWrapperAttributes(output, css);
        output.Attributes.SetAttribute("data-rhx-time-picker", "");

        var min = ParseTime(Min);
        var max = ParseTime(Max);
        var display = selected is { } s1 ? TimeListRenderer.FormatDisplay(s1, TwelveHour, Format) : "";

        var sb = new StringBuilder();

        var labelText = ResolveLabelText();
        if (!string.IsNullOrEmpty(labelText))
            sb.Append($"<label class=\"{GetElementClass("label")}\" id=\"{Enc(labelId)}\" for=\"{Enc(inputId)}\">{Enc(labelText)}</label>");

        sb.Append($"<div class=\"{GetElementClass("control")}\">");
        sb.Append($"<input class=\"{GetElementClass("input")}\" id=\"{Enc(inputId)}\" type=\"text\" autocomplete=\"off\" data-rhx-time-display");
        sb.Append(" role=\"combobox\" aria-haspopup=\"listbox\" aria-expanded=\"false\"");
        sb.Append($" aria-controls=\"{Enc(listboxId)}\"");
        if (!string.IsNullOrEmpty(Placeholder)) sb.Append($" placeholder=\"{Enc(Placeholder)}\"");
        if (!string.IsNullOrEmpty(display)) sb.Append($" value=\"{Enc(display)}\"");
        if (!string.IsNullOrEmpty(labelText)) sb.Append($" aria-labelledby=\"{Enc(labelId)}\"");
        if (!string.IsNullOrEmpty(AriaLabel)) sb.Append($" aria-label=\"{Enc(AriaLabel)}\"");
        var describedBy = BuildAriaDescribedBy(hintId, errorId);
        if (describedBy != null) sb.Append($" aria-describedby=\"{Enc(describedBy)}\"");
        if (hasError) sb.Append(" aria-invalid=\"true\"");
        if (resolvedRequired) sb.Append(" aria-required=\"true\"");
        if (Disabled) sb.Append(" disabled");
        if (Readonly) sb.Append(" readonly");
        sb.Append(" />");

        sb.Append($"<button type=\"button\" class=\"{GetElementClass("trigger")}\" tabindex=\"-1\" aria-label=\"Open time list\"");
        if (Disabled) sb.Append(" disabled");
        sb.Append('>');
        sb.Append($"<svg viewBox=\"0 0 24 24\" width=\"18\" height=\"18\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\">{IconRegistry.Get("clock") ?? ""}</svg>");
        sb.Append("</button>");
        sb.Append("</div>");

        sb.Append($"<input type=\"hidden\" data-rhx-time-value name=\"{Enc(name)}\" value=\"{Enc(iso)}\"");
        sb.Append(BuildValidationAttributeString());
        sb.Append(" />");

        sb.Append($"<div class=\"{GetElementClass("listbox")}\" id=\"{Enc(listboxId)}\" role=\"listbox\"");
        if (!string.IsNullOrEmpty(labelText)) sb.Append($" aria-labelledby=\"{Enc(labelId)}\"");
        else if (!string.IsNullOrEmpty(AriaLabel)) sb.Append($" aria-label=\"{Enc(AriaLabel)}\"");
        sb.Append(" hidden>");
        sb.Append(TimeListRenderer.RenderOptions(Step, min, max, TwelveHour, Format, selected));
        sb.Append("</div>");

        sb.Append(BuildHintHtml(hintId));
        sb.Append(BuildErrorHtml(errorId));

        output.Content.SetHtmlContent(sb.ToString());
    }

    private TimeOnly? ResolveTime()
    {
        if (!string.IsNullOrEmpty(Value)) return ParseTime(Value);
        return For?.Model switch
        {
            TimeOnly t => t,
            DateTime dt => TimeOnly.FromDateTime(dt),
            DateTimeOffset dto => TimeOnly.FromDateTime(dto.DateTime),
            _ => null,
        };
    }

    private static TimeOnly? ParseTime(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (TimeOnly.TryParseExact(s, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var t)) return t;
        if (TimeOnly.TryParse(s, CultureInfo.CurrentCulture, DateTimeStyles.None, out t)) return t;
        if (TimeOnly.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out t)) return t;
        return null;
    }
}
```

- [ ] **Step 4: Run to verify they pass**

Run: `dotnet test htmxRazor.Tests --filter TimePickerTagHelperTests`
Expected: PASS (6 tests). Then `dotnet test htmxRazor.Tests` (whole suite green) and `dotnet build htmxRazor` (0 warnings).

- [ ] **Step 5: Commit**

```bash
git add htmxRazor/Components/Forms/TimePickerTagHelper.cs htmxRazor.Tests/TimePickerTagHelperTests.cs
git commit -m "feat(time-picker): TimePickerTagHelper (input + trigger + popup listbox)"
```

---

## Task 4: CSS

**Files:**
- Create: `htmxRazor/Assets/css/components/rhx-time-picker.css`

- [ ] **Step 1: Verify token names, then write the stylesheet**

First: `grep -nE "rhx-z-dropdown|rhx-shadow-lg|rhx-color-surface-raised|rhx-color-focus-ring|rhx-color-brand-100|rhx-color-brand-700|rhx-radius-sm|rhx-space-2xs|rhx-font-weight-medium" htmxRazor/Assets/css/rhx-tokens.css`. For any `var(--rhx-x, fallback)` below where the token EXISTS, you may drop the fallback; where it does NOT exist, keep the fallback. Do not invent tokens.

```css
@layer rhx.components {
  .rhx-time-picker { position: relative; display: inline-flex; flex-direction: column; gap: var(--rhx-space-xs); font-family: var(--rhx-font-family); max-width: 14rem; }
  .rhx-time-picker__label { font-size: var(--rhx-font-size-sm); font-weight: var(--rhx-font-weight-medium); color: var(--rhx-color-text); }

  .rhx-time-picker__control { display: flex; align-items: center; border: var(--rhx-border-width) solid var(--rhx-color-border); border-radius: var(--rhx-radius-md); background: var(--rhx-color-surface); }
  .rhx-time-picker__control:focus-within { border-color: var(--rhx-color-brand-500); box-shadow: 0 0 0 1px var(--rhx-color-brand-500); }
  .rhx-time-picker__input { flex: 1; min-width: 6rem; min-height: 2.5rem; padding: var(--rhx-space-sm) var(--rhx-space-md); border: none; background: transparent; color: var(--rhx-color-text); font: inherit; outline: none; }
  .rhx-time-picker__input::placeholder { color: var(--rhx-color-text-muted); }
  .rhx-time-picker__trigger { display: inline-flex; align-items: center; justify-content: center; width: 2.5rem; align-self: stretch; border: none; background: none; color: var(--rhx-color-text-muted); cursor: pointer; }
  .rhx-time-picker__trigger:hover:not([disabled]) { color: var(--rhx-color-brand-600); }

  .rhx-time-picker__listbox { position: absolute; top: calc(100% + var(--rhx-space-2xs, 0.25rem)); left: 0; right: 0; z-index: var(--rhx-z-dropdown); max-height: 14rem; overflow-y: auto; padding: var(--rhx-space-2xs, 0.25rem); border: var(--rhx-border-width) solid var(--rhx-color-border); border-radius: var(--rhx-radius-md); background: var(--rhx-color-surface-raised); box-shadow: var(--rhx-shadow-lg); }
  .rhx-time-picker__listbox[hidden] { display: none; }

  .rhx-time-picker__option { display: block; width: 100%; text-align: left; padding: var(--rhx-space-sm) var(--rhx-space-md); border: none; background: none; border-radius: var(--rhx-radius-sm); color: var(--rhx-color-text); font: inherit; cursor: pointer; }
  .rhx-time-picker__option:hover, .rhx-time-picker__option[data-rhx-focused] { background: var(--rhx-color-brand-100); }
  .rhx-time-picker__option--selected { background: var(--rhx-color-brand-100); color: var(--rhx-color-brand-700); font-weight: var(--rhx-font-weight-medium); }

  .rhx-time-picker--small .rhx-time-picker__input { min-height: 2rem; font-size: var(--rhx-font-size-sm); }
  .rhx-time-picker--large .rhx-time-picker__input { min-height: 3rem; font-size: var(--rhx-font-size-md); }
  .rhx-time-picker--disabled { opacity: 0.6; pointer-events: none; }
  .rhx-time-picker--error .rhx-time-picker__control { border-color: var(--rhx-color-danger-500); }
}
```

- [ ] **Step 2: Build to confirm embedding**

Run: `dotnet build htmxRazor`
Expected: 0 warnings.

- [ ] **Step 3: Commit**

```bash
git add htmxRazor/Assets/css/components/rhx-time-picker.css
git commit -m "feat(time-picker): stylesheet"
```

---

## Task 5: JS — open/close, select, keyboard, type-ahead

**Files:**
- Create: `htmxRazor/Assets/js/components/rhx-timepicker.js`

Read `htmxRazor/Assets/js/components/rhx-combobox.js` first to match the IIFE + `RHX.register` style.

- [ ] **Step 1: Write the script**

```js
/**
 * htmxRazor Time Picker
 * A static popup list of times. The visible input opens the list; selecting an option commits a
 * hidden ISO HH:mm value + the display label and closes. Keyboard: Down/Up move, Enter selects,
 * Escape closes, printable keys type-ahead by label. Positioning reuses rhx-position.js.
 */
(function () {
  "use strict";

  var OPT = ".rhx-time-picker__option:not([disabled])";

  function initTimePickers(root) {
    root.querySelectorAll("[data-rhx-time-picker]").forEach(function (tp) {
      if (tp._rhxTpInit) return;
      tp._rhxTpInit = true;

      var input = tp.querySelector("[data-rhx-time-display]");
      var trigger = tp.querySelector(".rhx-time-picker__trigger");
      var listbox = tp.querySelector(".rhx-time-picker__listbox");
      var hidden = tp.querySelector("[data-rhx-time-value]");
      if (!input || !listbox || !hidden) return;

      function isOpen() { return !listbox.hidden; }

      function open() {
        if (input.hasAttribute("disabled")) return;
        listbox.hidden = false;
        input.setAttribute("aria-expanded", "true");
        if (window.RHX && typeof window.RHX.positionElement === "function") {
          window.RHX.positionElement(input.parentNode, listbox, { placement: "bottom-start", distance: 4, flip: true, shift: true });
        }
        var sel = listbox.querySelector(".rhx-time-picker__option--selected") || listbox.querySelector(OPT);
        focusOption(sel, true);
      }

      function close() {
        listbox.hidden = true;
        input.setAttribute("aria-expanded", "false");
        clearFocused();
      }

      function options() { return Array.prototype.slice.call(listbox.querySelectorAll(OPT)); }
      function clearFocused() {
        var f = listbox.querySelector("[data-rhx-focused]");
        if (f) f.removeAttribute("data-rhx-focused");
        input.removeAttribute("aria-activedescendant");
      }
      function focusOption(opt, scroll) {
        if (!opt) return;
        clearFocused();
        if (!opt.id) opt.id = listbox.id + "-o" + options().indexOf(opt);
        opt.setAttribute("data-rhx-focused", "");
        input.setAttribute("aria-activedescendant", opt.id);
        if (scroll) opt.scrollIntoView({ block: "nearest" });
      }
      function move(delta) {
        var opts = options();
        if (!opts.length) return;
        var cur = listbox.querySelector("[data-rhx-focused]");
        var i = cur ? opts.indexOf(cur) : -1;
        var next = Math.max(0, Math.min(opts.length - 1, i + delta));
        focusOption(opts[next], true);
      }

      function commit(opt) {
        if (!opt) return;
        var prev = listbox.querySelector(".rhx-time-picker__option--selected");
        if (prev) { prev.classList.remove("rhx-time-picker__option--selected"); prev.removeAttribute("aria-selected"); }
        opt.classList.add("rhx-time-picker__option--selected");
        opt.setAttribute("aria-selected", "true");
        input.value = opt.textContent.trim();
        hidden.value = opt.getAttribute("data-time") || "";
        hidden.dispatchEvent(new Event("input", { bubbles: true }));
        hidden.dispatchEvent(new Event("change", { bubbles: true }));
        tp.dispatchEvent(new CustomEvent("rhx:time-picker:change", { bubbles: true, detail: { value: hidden.value } }));
        close();
        input.focus();
      }

      input.addEventListener("focus", function () { if (!isOpen()) open(); });
      if (trigger) trigger.addEventListener("click", function () { isOpen() ? close() : (input.focus(), open()); });

      listbox.addEventListener("click", function (e) {
        var opt = e.target.closest(OPT);
        if (opt && listbox.contains(opt)) commit(opt);
      });

      input.addEventListener("keydown", function (e) {
        switch (e.key) {
          case "ArrowDown": e.preventDefault(); if (!isOpen()) open(); else move(1); break;
          case "ArrowUp": e.preventDefault(); if (!isOpen()) open(); else move(-1); break;
          case "Home": if (isOpen()) { e.preventDefault(); focusOption(options()[0], true); } break;
          case "End": if (isOpen()) { e.preventDefault(); var o = options(); focusOption(o[o.length - 1], true); } break;
          case "Enter":
            if (isOpen()) { e.preventDefault(); var f = listbox.querySelector("[data-rhx-focused]"); if (f) commit(f); }
            break;
          case "Escape": if (isOpen()) { e.preventDefault(); close(); } break;
          case "Tab": if (isOpen()) close(); break;
          default:
            if (e.key.length === 1 && /\S/.test(e.key)) {
              if (!isOpen()) open();
              var q = e.key.toLowerCase();
              var match = options().find(function (o) { return o.textContent.trim().toLowerCase().indexOf(q) === 0; });
              if (match) focusOption(match, true);
            }
        }
      });

      document.addEventListener("click", function (e) { if (isOpen() && !tp.contains(e.target)) close(); });
    });
  }

  if (window.RHX) window.RHX.register("time-picker", initTimePickers);
})();
```

- [ ] **Step 2: Build to confirm embedding**

Run: `dotnet build htmxRazor`
Expected: 0 warnings.

- [ ] **Step 3: Commit**

```bash
git add htmxRazor/Assets/js/components/rhx-timepicker.js
git commit -m "feat(time-picker): interaction JS (open/close, select, keyboard, type-ahead)"
```

---

## Task 6: Demo page + sidebar + asset registration

**Files:**
- Create: `htmxRazor.Demo/Pages/Docs/Components/TimePicker.cshtml` + `.cshtml.cs`
- Modify: `_DocsLayout.cshtml`, `_Layout.cshtml` (CSS + JS), `_SidebarNav.cshtml` (Forms entry)

- [ ] **Step 1: Register assets in BOTH `_DocsLayout.cshtml` and `_Layout.cshtml`**

After the `rhx-date-picker.css` link, add:
```html
    <link rel="stylesheet" href="/_rhx/css/components/rhx-time-picker.css" />
```
After the `rhx-datepicker.js` script, add:
```html
    <script src="/_rhx/js/components/rhx-timepicker.js" defer></script>
```
(If a layout lacks the date-picker lines, add after the radial-select ones; verify both layouts end up with the time-picker css + js.)

- [ ] **Step 2: Sidebar entry** — in `_SidebarNav.cshtml`, after the Date Picker link:
```html
            <a href="/Docs/Components/TimePicker" class="docs-nav__sublink @(currentPath == "/Docs/Components/TimePicker" ? "docs-nav__sublink--active" : "")">Time Picker</a>
```

- [ ] **Step 3: PageModel `TimePicker.cshtml.cs`** — confirm `ComponentProperty`/`BreadcrumbItem`/namespace against `DatePicker.cshtml.cs`, then:
```csharp
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using htmxRazor.Components.Navigation;
using htmxRazor.Demo.Models;

namespace htmxRazor.Demo.Pages.Docs.Components;

public class TimePickerModel : PageModel
{
    public List<ComponentProperty> Properties { get; } = new()
    {
        new("rhx-for", "ModelExpression", "-", "Binds TimeOnly/DateTime for two-way model binding"),
        new("name", "string", "-", "Form field name for the hidden ISO (HH:mm) value"),
        new("rhx-placeholder", "string", "-", "Placeholder for the text input"),
        new("rhx-min", "string", "-", "Earliest time, HH:mm"),
        new("rhx-max", "string", "-", "Latest time, HH:mm"),
        new("rhx-step", "int", "30", "Minutes between options"),
        new("rhx-12hour", "bool", "true", "12-hour (9:30 AM) vs 24-hour (09:30) display"),
        new("rhx-format", "string", "-", ".NET time format string for display (overrides 12/24h)"),
        new("rhx-size", "string", "medium", "small, medium, large"),
        new("rhx-disabled", "bool", "false", "Disable the control"),
    };

    public string BasicCode => "<rhx-time-picker name=\"StartTime\" rhx-placeholder=\"Pick a time…\" rhx-step=\"30\" />";
    public string RangeCode => "<rhx-time-picker name=\"slot\" rhx-min=\"09:00\" rhx-max=\"17:00\" rhx-step=\"15\" rhx-12hour=\"false\" />";

    public void OnGet()
    {
        ViewData["Breadcrumbs"] = new List<BreadcrumbItem>
        {
            new("Home", "/"),
            new("Components", "/Docs/Components/TimePicker"),
            new("Time Picker"),
        };
    }
}
```

- [ ] **Step 4: View `TimePicker.cshtml`** — mirror `DatePicker.cshtml`'s tab structure (confirm tag names against it):
```razor
@page
@model TimePickerModel
@{
    Layout = "_ComponentPage";
    ViewData["Title"] = "Time Picker";
    ViewData["ComponentName"] = "Time Picker";
    ViewData["ComponentTag"] = "rhx-time-picker";
    ViewData["ComponentDescription"] = "A text input with a popup list of selectable times. Commits a hidden ISO HH:mm value; shows 12-hour or 24-hour display.";
}

<section class="docs-component-section">
    <h2>Examples</h2>

    <div class="docs-example">
        <h3>Basic</h3>
        <rhx-tab-group>
            <rhx-tab rhx-panel="basic-preview" rhx-active="true">Preview</rhx-tab>
            <rhx-tab rhx-panel="basic-code">Code</rhx-tab>
            <rhx-tab-panel rhx-name="basic-preview" rhx-active="true">
                <div class="docs-preview docs-preview--vertical" style="min-height: 18rem; align-items: flex-start;">
                    <rhx-time-picker name="StartTime" rhx-placeholder="Pick a time…" rhx-step="30" />
                </div>
            </rhx-tab-panel>
            <rhx-tab-panel rhx-name="basic-code">
                <div class="docs-code"><pre><code>@Model.BasicCode</code></pre></div>
            </rhx-tab-panel>
        </rhx-tab-group>
    </div>

    <div class="docs-example">
        <h3>Business hours, 15-min, 24-hour</h3>
        <rhx-tab-group>
            <rhx-tab rhx-panel="rng-preview" rhx-active="true">Preview</rhx-tab>
            <rhx-tab rhx-panel="rng-code">Code</rhx-tab>
            <rhx-tab-panel rhx-name="rng-preview" rhx-active="true">
                <div class="docs-preview docs-preview--vertical" style="min-height: 18rem; align-items: flex-start;">
                    <rhx-time-picker name="slot" rhx-min="09:00" rhx-max="17:00" rhx-step="15" rhx-12hour="false" />
                </div>
            </rhx-tab-panel>
            <rhx-tab-panel rhx-name="rng-code">
                <div class="docs-code"><pre><code>@Model.RangeCode</code></pre></div>
            </rhx-tab-panel>
        </rhx-tab-group>
    </div>
</section>

@await Html.PartialAsync("_PropertyTable", Model.Properties)

<section class="docs-component-section">
    <h2>Accessibility</h2>
    <ul class="docs-a11y-list">
        <li>The input is a <code>role="combobox"</code> with <code>aria-haspopup="listbox"</code>/<code>aria-expanded</code>; the popup is <code>role="listbox"</code> with <code>role="option"</code> items.</li>
        <li>Keyboard: Down/Up move, Home/End jump, Enter selects, Escape closes, printable keys type-ahead by label; the active option is tracked via <code>aria-activedescendant</code>.</li>
        <li>The hidden value is ISO <code>HH:mm</code> (24-hour) for reliable model binding regardless of the display mode.</li>
    </ul>
</section>
```

- [ ] **Step 5: Build + smoke**

Run: `dotnet build htmxRazor.Demo`, then run the demo and `curl -s http://localhost:5213/Docs/Components/TimePicker | grep -oc 'role="option"'` (expect 48 for the basic + 33 for the 09:00–17:00@15 example = 81 total; any nonzero confirms render). Use a fresh port; `pkill -f htmxRazor.Demo` after.

- [ ] **Step 6: Commit**

```bash
git add htmxRazor.Demo/Pages/Docs/Components/TimePicker.cshtml htmxRazor.Demo/Pages/Docs/Components/TimePicker.cshtml.cs htmxRazor.Demo/Pages/Shared/_DocsLayout.cshtml htmxRazor.Demo/Pages/Shared/_Layout.cshtml htmxRazor.Demo/Pages/Shared/_SidebarNav.cshtml
git commit -m "docs(time-picker): demo page, sidebar entry, asset registration"
```

---

## Task 7: Playwright E2E

**Files:**
- Create: `PlaywrightTests/ComponentTests/TimePickerTests.cs`

- [ ] **Step 1: Write the E2E tests** (read `PlaywrightTests/ComponentTests/DatePickerTests.cs` for fixture conventions):

```csharp
using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class TimePickerTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/TimePicker";
    private const string Scope = "#panel-basic-preview ";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Opening_shows_times_and_picking_one_fills_input(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var input = page.Locator(Scope + ".rhx-time-picker__input");
        var listbox = page.Locator(Scope + ".rhx-time-picker__listbox");
        await Assertions.Expect(listbox).Not.ToBeVisibleAsync();

        await input.ClickAsync();
        await Assertions.Expect(listbox).ToBeVisibleAsync();

        await listbox.Locator("[role='option'][data-time='09:30']").ClickAsync();
        await Assertions.Expect(listbox).Not.ToBeVisibleAsync();
        await Assertions.Expect(input).ToHaveValueAsync("9:30 AM");
        await Assertions.Expect(page.Locator(Scope + "[data-rhx-time-value]")).ToHaveValueAsync("09:30");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Keyboard_down_and_enter_selects(string browserName)
    {
        var page = await OpenAsync(browserName, Path);
        var input = page.Locator(Scope + ".rhx-time-picker__input");
        await input.ClickAsync();
        await Assertions.Expect(page.Locator(Scope + ".rhx-time-picker__listbox")).ToBeVisibleAsync();

        await input.PressAsync("ArrowDown");
        await input.PressAsync("Enter");
        await Assertions.Expect(input).Not.ToHaveValueAsync("");
        await Assertions.Expect(page.Locator(Scope + "[data-rhx-time-value]")).Not.ToHaveValueAsync("");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task TwentyFourHour_range_example_uses_24h_options(string browserName)
    {
        var page = await OpenAsync(browserName, Path);
        var scope = "#panel-rng-preview ";
        await page.Locator(scope + ".rhx-time-picker__input").ClickAsync();
        var listbox = page.Locator(scope + ".rhx-time-picker__listbox");
        await Assertions.Expect(listbox).ToBeVisibleAsync();
        // 09:00..17:00 @ 15min = 33 options; first is 09:00 in 24h form
        await Assertions.Expect(listbox.Locator("[role='option']")).ToHaveCountAsync(33);
        await Assertions.Expect(listbox.Locator("[role='option']").First).ToHaveTextAsync("09:00");
    }
}
```

- [ ] **Step 2: Run on Chromium**

Run: `dotnet test PlaywrightTests --filter "FullyQualifiedName~TimePicker&DisplayName~chromium"`
Expected: 3 PASS. (If browsers missing: `npx -y playwright@1.60.0 install chromium`.) If a test fails, investigate the component/selectors before weakening assertions; report any real bug.

- [ ] **Step 3: Commit**

```bash
git add PlaywrightTests/ComponentTests/TimePickerTests.cs
git commit -m "test(time-picker): Playwright E2E (open, select, keyboard, 24h range)"
```

---

## Task 8: Docs + final verification

**Files:**
- Modify: `README.md`, `CHANGELOG.md`, `vscode-extension/snippets/htmxrazor.code-snippets`

- [ ] **Step 1: README Forms row** — add `Time Picker` after `Date Picker` in the Forms catalog row (edit only that row, match its format).

- [ ] **Step 2: CHANGELOG** — append under `## [2.1.0]` → `### Added`:
```markdown
- **Time Picker** — `<rhx-time-picker>`: a text input + popup list of selectable times (from `rhx-min` to `rhx-max`, stepping by `rhx-step` minutes, default 30). Commits a hidden ISO `HH:mm` value for model binding (`TimeOnly`/`DateTime`); the visible input shows 12-hour (`9:30 AM`, default) or 24-hour (`09:30`, `rhx-12hour="false"`) display, or a custom `rhx-format`. Listbox accessibility (`role="combobox"`/`listbox`/`option`, `aria-activedescendant`, Down/Up/Home/End/Enter/Escape + type-ahead). Static list — no server round-trip.
```
If a `### Components Added` Forms list exists under 2.1.0, add "Time Picker" there too.

- [ ] **Step 3: VS Code snippet** — after the `"rhx-date-picker"` snippet, add (mind the JSON commas; verify with `node -e "JSON.parse(require('fs').readFileSync('vscode-extension/snippets/htmxrazor.code-snippets','utf8').replace(/^\s*\/\/.*$/gm,''))" && echo JSON_OK`):
```json
  "rhx-time-picker": {
    "prefix": "rhx-time-picker",
    "body": [
      "<rhx-time-picker name=\"${1:StartTime}\" rhx-placeholder=\"${2:Pick a time…}\" rhx-step=\"${3:30}\" rhx-12hour=\"${4|true,false|}\" />"
    ],
    "description": "htmxRazor time picker (input + popup time list)"
  },
```

- [ ] **Step 4: Final verification (milestone gate)**
1. `dotnet build htmxRazor.sln` → 0 warnings.
2. `dotnet test htmxRazor.Tests` → all pass; report total.
3. `dotnet test PlaywrightTests --filter "FullyQualifiedName~TimePicker&DisplayName~chromium"` → 3 pass.

- [ ] **Step 5: Commit**

```bash
git add README.md CHANGELOG.md vscode-extension/snippets/htmxrazor.code-snippets
git commit -m "docs(time-picker): README catalog, changelog, VS Code snippet"
```

---

## Self-Review (completed)

**Spec coverage (§5):** input + clock trigger + hidden ISO `HH:mm` → Task 3; static scrollable time list from min→max at `rhx-step` (default 30) → Tasks 1–3; 12-hour default / 24-hour display, `rhx-format` → Tasks 1, 3; `role="listbox"`/`option` + select/scroll/keyboard/type-ahead → Tasks 3, 5; model binding `TimeOnly`/`DateTime` → Task 3; §3.5 popup positioning (rhx-position.js) → Task 5; §8 a11y (combobox/listbox, activedescendant, keyboard) → Tasks 3, 5; §10 testing (renderer unit + tag helper unit + E2E) → Tasks 1–3, 7; §11 docs → Tasks 6, 8. No server endpoint (correct — the spec says the time list is static). DateTime/Range pickers are out of scope (later milestones).

**Placeholder scan:** Every code step has complete code. The token-existence and browser-install lines are verification commands, not placeholders.

**Type consistency:** `TimeListRenderer.Times`/`FormatDisplay`/`RenderOptions` signatures are consistent across Tasks 1–3; data attributes (`data-rhx-time-picker`, `data-rhx-time-value`, `data-rhx-time-display`, `data-time`, `data-rhx-focused`) match between the Tag Helper (Task 3), CSS (Task 4), and JS (Task 5); CSS classes (`rhx-time-picker__option--selected`, `__listbox`, `__control`, `__input`, `__trigger`) match between renderer/Tag Helper and the stylesheet; `BlockName="time-picker"` makes `GetElementClass(...)` map to the Task 4 CSS; hidden value is ISO `HH:mm` everywhere; the 24h-range E2E count (33 = 09:00..17:00 @15) matches `Times` math.
