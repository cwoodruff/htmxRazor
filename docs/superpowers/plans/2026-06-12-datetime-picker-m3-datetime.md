# Date/Time Picker — Milestone 3: DateTime Picker

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

> **⚠️ PREREQUISITE:** This milestone REUSES Milestone 1 (`CalendarRenderer`/`CalendarOptions`/`CalendarEndpoint`, PR #36) and Milestone 2 (`TimeListRenderer`, PR #37). Both must be merged to `main` (or present on the working branch) before executing — this plan makes NO changes to those files, only calls them. Branch off an up-to-date `main` that contains both.

**Goal:** Ship `<rhx-datetime-picker>` — one control binding a single `DateTime`, whose popup shows the M1 calendar (left) and the M2 time list (right) side by side; selecting a day sets the date part, selecting a time sets the time part, and the control commits a hidden ISO `yyyy-MM-ddTHH:mm` value once both are set.

**Architecture:** A `DateTimePickerTagHelper` (extends `FormControlTagHelperBase`) composes `CalendarRenderer.Render(...)` and `TimeListRenderer.RenderOptions(...)` verbatim into a two-pane popup. The calendar's month navigation is htmx-driven against the built-in `/_rhx/calendar` endpoint (M1), targeting only the calendar pane. JS (`rhx-datetimepicker.js`) tracks the date and time parts (read from the server-rendered selected day cell + time option), updates the visible display from server-formatted parts (the calendar's `data-display` + the time option's text — culture-consistent, no client re-formatting), commits the hidden ISO value when both parts are set, and re-applies the date highlight after each htmx calendar swap. Reuses the M1/M2 CSS (calendar + time-picker option styles are globally loaded); only a thin two-pane layout stylesheet is new.

**Tech Stack:** ASP.NET Core Tag Helpers (.NET 10, C#, `DateTime`/`DateOnly`/`TimeOnly`), htmx (calendar nav, via M1 endpoint), vanilla JS, BEM CSS with `rhx-` tokens, xUnit, Playwright.

**Spec:** `docs/superpowers/specs/2026-06-11-datetime-picker-family-design.md` (§6, plus §3.3/§3.5/§8/§10/§11).

**Reusable APIs (verify present before starting):**
- `htmxRazor.Components.Forms.Calendar.CalendarRenderer.Render(CalendarOptions)` → full calendar widget HTML. `CalendarOptions` is a `record` with init props: `Year, Month, View, Selected (DateOnly?), Min (DateOnly?), Max (DateOnly?), WeekStart (DayOfWeek), Today (DateOnly), HxGetUrl ("/_rhx/calendar"), TargetId, ShowToday (bool), ShowClear (bool), Format (string?)`. Day cells emit `<button class="rhx-calendar__day[ --selected/--today/--muted]" role="gridcell" data-date="yyyy-MM-dd" data-display="{date display}" ...>`; nav buttons carry `hx-get` to `/_rhx/calendar` with `hx-target="#{TargetId}"` `hx-swap="outerHTML"`.
- `htmxRazor.Components.Forms.Time.TimeListRenderer.RenderOptions(int stepMinutes, TimeOnly? min, TimeOnly? max, bool twelveHour, string? format, TimeOnly? selected)` → `<button class="rhx-time-picker__option[ --selected]" role="option" data-time="HH:mm">{display}</button>` list. Also `TimeListRenderer.FormatDisplay(TimeOnly, bool, string?)`.

**Conventions (read before starting):**
- Form base + the two sibling pickers: `htmxRazor/Components/Forms/FormControlTagHelperBase.cs`, `DatePickerTagHelper.cs`, `TimePickerTagHelper.cs` (match their idioms exactly: `Enc`, `CreateCssBuilder`, `GetElementClass/GetModifierClass`, `ApplyWrapperAttributes`, `ResolveName/ResolveId`, label/hint/error builders, `HasError`/`ResolveRequired`, aria-invalid/required, `[HtmlAttributeNotBound] Today` injection for tests).
- JS: `htmxRazor/Assets/js/components/rhx-datepicker.js` (htmx grid swap + day select) and `rhx-timepicker.js` (option select). `RHX.register(name, fn)`, `_rhx*Init` guard, `RHX.positionElement`.
- Icons: `IconRegistry.Get("calendar")`.
- Assets auto-embed; link CSS/JS in `_DocsLayout.cshtml` + `_Layout.cshtml`. The M1/M2 CSS (`rhx-calendar.css`, `rhx-date-picker.css`, `rhx-time-picker.css`) are already linked there — the time-picker option styles the datetime time pane reuses are therefore available globally.
- Test base: `htmxRazor.Tests/TagHelperTestBase.cs`. Playwright base: `PlaywrightTests/Infrastructure/ComponentTestBase.cs` (`#panel-<rhx-name>-preview`). Chromium installed locally.
- **Lesson from M1/M2:** display formatting stays server-side / culture-invariant; the JS NEVER re-formats a date or time — it only echoes server-rendered `data-display` / option text. The combined display is `"{date display} {time display}"`.

---

## File Structure

| File | Responsibility |
|---|---|
| `htmxRazor/Components/Forms/DateTimePickerTagHelper.cs` | `<rhx-datetime-picker>` — input + trigger + hidden ISO `yyyy-MM-ddTHH:mm` + two-pane popup (calendar via `CalendarRenderer`, time list via `TimeListRenderer`) + footer (Clear/Done). |
| `htmxRazor/Assets/css/components/rhx-datetime-picker.css` | Input/trigger + two-pane popup layout + footer. Reuses `rhx-calendar` + `rhx-time-picker__option` styles. |
| `htmxRazor/Assets/js/components/rhx-datetimepicker.js` | Open/close, date-part + time-part tracking (read from server-rendered selection), commit on both-set, Clear/Done, re-highlight date after htmx calendar swap. |
| `htmxRazor.Tests/DateTimePickerTagHelperTests.cs` | Unit tests. |
| `htmxRazor.Demo/Pages/Docs/Components/DateTimePicker.cshtml(.cs)` | Demo page. |
| `PlaywrightTests/ComponentTests/DateTimePickerTests.cs` | E2E. |

---

## Task 1: `DateTimePickerTagHelper`

**Files:**
- Create: `htmxRazor/Components/Forms/DateTimePickerTagHelper.cs`
- Test: `htmxRazor.Tests/DateTimePickerTagHelperTests.cs`

- [ ] **Step 1: Write the failing tests**

```csharp
using System;
using htmxRazor.Components.Forms;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Xunit;

namespace htmxRazor.Tests;

public class DateTimePickerTagHelperTests : TagHelperTestBase
{
    private DateTimePickerTagHelper CreateHelper() => new(CreateUrlHelperFactory())
    {
        ViewContext = CreateViewContext(),
        Today = new DateOnly(2026, 10, 9),
    };

    private class TestModel { public DateTime StartsAt { get; set; } }

    private static ModelExpression Expr(string prop, object? value)
    {
        var provider = new EmptyModelMetadataProvider();
        var metadata = provider.GetMetadataForProperty(typeof(TestModel), prop);
        return new ModelExpression(prop, new ModelExplorer(provider, metadata, value));
    }

    [Fact]
    public async Task Renders_Wrapper_Input_Trigger_And_Hidden_Iso_DateTime()
    {
        var helper = CreateHelper();
        helper.Name = "StartsAt";
        helper.Value = "2026-10-15T09:30";
        var ctx = CreateContext("rhx-datetime-picker");
        var output = CreateOutput("rhx-datetime-picker");

        await helper.ProcessAsync(ctx, output);

        Assert.Equal("div", output.TagName);
        Assert.True(HasClass(output, "rhx-datetime-picker"));
        Assert.True(output.Attributes.TryGetAttribute("data-rhx-datetime-picker", out _));
        var html = output.Content.GetContent();
        Assert.Contains("rhx-datetime-picker__input", html);
        Assert.Contains("rhx-datetime-picker__trigger", html);
        Assert.Contains("aria-haspopup=\"dialog\"", html);
        Assert.Contains("type=\"hidden\"", html);
        Assert.Contains("name=\"StartsAt\"", html);
        Assert.Contains("value=\"2026-10-15T09:30\"", html); // hidden ISO datetime
        Assert.Contains("data-rhx-dt-value", html);
    }

    [Fact]
    public async Task Popup_Has_Calendar_For_Date_And_TimeList_With_Selected_Time()
    {
        var helper = CreateHelper();
        helper.Name = "d";
        helper.Value = "2026-10-15T09:30";
        var ctx = CreateContext("rhx-datetime-picker");
        var output = CreateOutput("rhx-datetime-picker");

        await helper.ProcessAsync(ctx, output);
        var html = output.Content.GetContent();

        // calendar pane shows October 2026 with the 15th selected
        Assert.Contains("rhx-calendar", html);
        Assert.Contains("October 2026", html);
        Assert.Contains("data-date=\"2026-10-15\"", html);
        // time pane shows the time options with 09:30 selected
        Assert.Contains("rhx-datetime-picker__times", html);
        Assert.Contains("data-time=\"09:30\"", html);
        Assert.Contains("rhx-time-picker__option--selected", html);
        // footer
        Assert.Contains("data-rhx-dt-clear", html);
        Assert.Contains("data-rhx-dt-done", html);
    }

    [Fact]
    public async Task Empty_Value_Shows_Today_Month_No_Selection_Empty_Hidden()
    {
        var helper = CreateHelper();
        helper.Name = "d";
        var ctx = CreateContext("rhx-datetime-picker");
        var output = CreateOutput("rhx-datetime-picker");

        await helper.ProcessAsync(ctx, output);
        var html = output.Content.GetContent();

        Assert.Contains("October 2026", html); // today = 2026-10-09
        Assert.DoesNotContain("aria-selected=\"true\"", html);
        Assert.Contains("value=\"\"", html); // empty hidden value
    }

    [Fact]
    public async Task DateTime_Model_Binding_Produces_Iso_And_Selection()
    {
        var helper = CreateHelper();
        helper.For = Expr("StartsAt", new DateTime(2026, 10, 15, 14, 0, 0));
        var ctx = CreateContext("rhx-datetime-picker");
        var output = CreateOutput("rhx-datetime-picker");

        await helper.ProcessAsync(ctx, output);
        var html = output.Content.GetContent();

        Assert.Contains("value=\"2026-10-15T14:00\"", html);
        Assert.Contains("data-time=\"14:00\"", html);
    }

    [Fact]
    public async Task Calendar_Pane_Hides_Calendar_Own_Footer()
    {
        // The datetime picker has its own footer; the embedded calendar's Today/Clear are suppressed.
        var helper = CreateHelper();
        helper.Name = "d";
        var ctx = CreateContext("rhx-datetime-picker");
        var output = CreateOutput("rhx-datetime-picker");

        await helper.ProcessAsync(ctx, output);
        var html = output.Content.GetContent();

        Assert.DoesNotContain("data-rhx-cal-today", html);
        Assert.DoesNotContain("data-rhx-cal-clear", html);
    }
}
```

- [ ] **Step 2: Run to verify they fail**

Run: `dotnet test htmxRazor.Tests --filter DateTimePickerTagHelperTests`
Expected: FAIL — `DateTimePickerTagHelper` does not exist.

- [ ] **Step 3: Write `DateTimePickerTagHelper`**

```csharp
using System;
using System.Globalization;
using System.Text;
using htmxRazor.Components.Forms.Calendar;
using htmxRazor.Components.Forms.Time;
using htmxRazor.Components.Imagery;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Forms;

/// <summary>
/// A single-value <see cref="DateTime"/> picker: one input opens a popup with a calendar
/// (left) and a time list (right). Selecting a day sets the date part; selecting a time sets
/// the time part; the control commits a hidden ISO <c>yyyy-MM-ddTHH:mm</c> value once both are
/// set. Calendar month navigation is htmx-driven against <c>/_rhx/calendar</c>.
/// </summary>
/// <example>
/// <code>
/// &lt;rhx-datetime-picker rhx-for="StartsAt" rhx-step="30" rhx-week-start="mon" /&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-datetime-picker")]
public class DateTimePickerTagHelper : FormControlTagHelperBase
{
    protected override string BlockName => "datetime-picker";

    [HtmlAttributeName("rhx-placeholder")] public string? Placeholder { get; set; }
    [HtmlAttributeName("rhx-min")] public string? Min { get; set; }       // ISO date yyyy-MM-dd (calendar bound)
    [HtmlAttributeName("rhx-max")] public string? Max { get; set; }       // ISO date yyyy-MM-dd (calendar bound)
    [HtmlAttributeName("rhx-week-start")] public string WeekStartName { get; set; } = "mon";
    [HtmlAttributeName("rhx-step")] public int Step { get; set; } = 30;
    [HtmlAttributeName("rhx-12hour")] public bool TwelveHour { get; set; } = true;
    [HtmlAttributeName("rhx-date-format")] public string? DateFormat { get; set; }
    [HtmlAttributeName("rhx-time-format")] public string? TimeFormat { get; set; }

    /// <summary>"Today" — injectable for deterministic tests; defaults to the system date.</summary>
    [HtmlAttributeNotBound]
    public DateOnly Today { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public DateTimePickerTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        await Task.CompletedTask;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var name = ResolveName();
        var id = ResolveId();
        if (string.IsNullOrEmpty(id)) id = "rhx-dtp-" + context.UniqueId;
        var dt = ResolveDateTime();
        var datePart = dt is { } d0 ? DateOnly.FromDateTime(d0) : (DateOnly?)null;
        var timePart = dt is { } d1 ? TimeOnly.FromDateTime(new DateTime(d1.Year, d1.Month, d1.Day, d1.Hour, d1.Minute, 0)) : (TimeOnly?)null;
        var iso = dt is { } d2 ? d2.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture) : "";

        var calId = $"{id}-cal";
        var popupId = $"{id}-popup";
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
        output.Attributes.SetAttribute("data-rhx-datetime-picker", "");

        var weekStart = Enum.TryParse<DayOfWeek>(ExpandWeekStart(WeekStartName), true, out var ws) ? ws : DayOfWeek.Monday;
        var view = datePart ?? Today;

        var calOpts = new CalendarOptions
        {
            Year = view.Year,
            Month = view.Month,
            Selected = datePart,
            Min = ParseDate(Min),
            Max = ParseDate(Max),
            WeekStart = weekStart,
            Today = Today,
            HxGetUrl = "/_rhx/calendar",
            TargetId = calId,
            ShowToday = false,   // datetime picker provides its own footer
            ShowClear = false,
            Format = DateFormat,
        };

        // Combined display = "{date display} {time display}".
        var dateDisp = datePart is { } dp ? dp.ToString(string.IsNullOrEmpty(DateFormat) ? "d" : DateFormat, CultureInfo.CurrentCulture) : "";
        var timeDisp = timePart is { } tp ? TimeListRenderer.FormatDisplay(tp, TwelveHour, TimeFormat) : "";
        var display = (datePart != null && timePart != null) ? $"{dateDisp} {timeDisp}" : "";

        var sb = new StringBuilder();

        var labelText = ResolveLabelText();
        if (!string.IsNullOrEmpty(labelText))
            sb.Append($"<label class=\"{GetElementClass("label")}\" id=\"{Enc(labelId)}\" for=\"{Enc(inputId)}\">{Enc(labelText)}</label>");

        sb.Append($"<div class=\"{GetElementClass("control")}\">");
        sb.Append($"<input class=\"{GetElementClass("input")}\" id=\"{Enc(inputId)}\" type=\"text\" autocomplete=\"off\" data-rhx-dt-display");
        sb.Append($" aria-haspopup=\"dialog\" aria-expanded=\"false\" aria-controls=\"{Enc(popupId)}\"");
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

        sb.Append($"<button type=\"button\" class=\"{GetElementClass("trigger")}\" tabindex=\"-1\" aria-haspopup=\"dialog\" aria-expanded=\"false\" aria-controls=\"{Enc(popupId)}\" aria-label=\"Open date and time picker\"");
        if (Disabled) sb.Append(" disabled");
        sb.Append('>');
        sb.Append($"<svg viewBox=\"0 0 24 24\" width=\"18\" height=\"18\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\">{IconRegistry.Get("calendar") ?? ""}</svg>");
        sb.Append("</button>");
        sb.Append("</div>");

        sb.Append($"<input type=\"hidden\" data-rhx-dt-value name=\"{Enc(name)}\" value=\"{Enc(iso)}\"");
        sb.Append(BuildValidationAttributeString());
        sb.Append(" />");

        sb.Append($"<div class=\"{GetElementClass("popup")}\" id=\"{Enc(popupId)}\" role=\"dialog\" aria-modal=\"false\"");
        if (!string.IsNullOrEmpty(labelText)) sb.Append($" aria-labelledby=\"{Enc(labelId)}\"");
        else if (!string.IsNullOrEmpty(AriaLabel)) sb.Append($" aria-label=\"{Enc(AriaLabel)}\"");
        sb.Append(" hidden>");

        sb.Append($"<div class=\"{GetElementClass("panes")}\">");
        sb.Append($"<div class=\"{GetElementClass("calendar")}\">");
        sb.Append(CalendarRenderer.Render(calOpts));
        sb.Append("</div>");
        sb.Append($"<div class=\"{GetElementClass("times")}\" role=\"listbox\" aria-label=\"Time\">");
        sb.Append(TimeListRenderer.RenderOptions(Step, null, null, TwelveHour, TimeFormat, timePart));
        sb.Append("</div>");
        sb.Append("</div>"); // panes

        sb.Append($"<div class=\"{GetElementClass("footer")}\">");
        sb.Append($"<button type=\"button\" class=\"{GetElementClass("action")}\" data-rhx-dt-clear>Clear</button>");
        sb.Append($"<button type=\"button\" class=\"{GetElementClass("action")}\" data-rhx-dt-done>Done</button>");
        sb.Append("</div>");

        sb.Append("</div>"); // popup

        sb.Append(BuildHintHtml(hintId));
        sb.Append(BuildErrorHtml(errorId));

        output.Content.SetHtmlContent(sb.ToString());
    }

    private DateTime? ResolveDateTime()
    {
        if (!string.IsNullOrEmpty(Value)) return ParseDateTime(Value);
        return For?.Model switch
        {
            DateTime dt => dt,
            DateTimeOffset dto => dto.DateTime,
            _ => null,
        };
    }

    private static DateTime? ParseDateTime(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (DateTime.TryParseExact(s, "yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)) return dt;
        if (DateTime.TryParse(s, CultureInfo.CurrentCulture, DateTimeStyles.None, out dt)) return dt;
        if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)) return dt;
        return null;
    }

    private static DateOnly? ParseDate(string? s) =>
        DateOnly.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;

    private static string ExpandWeekStart(string s) => s.ToLowerInvariant() switch
    {
        "mon" or "monday" => "Monday",
        "sun" or "sunday" => "Sunday",
        "tue" or "tuesday" => "Tuesday",
        "wed" or "wednesday" => "Wednesday",
        "thu" or "thursday" => "Thursday",
        "fri" or "friday" => "Friday",
        "sat" or "saturday" => "Saturday",
        _ => "Monday",
    };
}
```

> Note: the embedded calendar is rendered with `ShowToday=false`/`ShowClear=false` so it has no footer of its own (the datetime picker owns the footer). The time pane reuses `TimeListRenderer.RenderOptions`, so its options carry `rhx-time-picker__option` classes — styled by the globally-loaded `rhx-time-picker.css`. The `Calendar_Pane_Hides_Calendar_Own_Footer` test pins the no-double-footer behavior.

- [ ] **Step 4: Run to verify they pass**

Run: `dotnet test htmxRazor.Tests --filter DateTimePickerTagHelperTests`
Expected: PASS (5 tests). Then `dotnet test htmxRazor.Tests` (whole suite) and `dotnet build htmxRazor` (0 warnings).

- [ ] **Step 5: Commit**

```bash
git add htmxRazor/Components/Forms/DateTimePickerTagHelper.cs htmxRazor.Tests/DateTimePickerTagHelperTests.cs
git commit -m "feat(datetime-picker): DateTimePickerTagHelper (calendar + time list popup)"
```

---

## Task 2: CSS — two-pane popup + footer

**Files:**
- Create: `htmxRazor/Assets/css/components/rhx-datetime-picker.css`

- [ ] **Step 1: Verify token names, then write the stylesheet**

First: `grep -nE "rhx-z-dropdown|rhx-shadow-lg|rhx-color-surface-raised|rhx-color-border|rhx-color-brand-100|rhx-color-brand-600|rhx-radius-md|rhx-radius-sm|rhx-space-2xs|rhx-font-weight-medium|rhx-color-danger-500" htmxRazor/Assets/css/rhx-tokens.css`. Drop the `var(--rhx-x, fallback)` fallback where the token exists; keep it where absent. Don't invent tokens.

```css
@layer rhx.components {
  .rhx-datetime-picker { position: relative; display: inline-flex; flex-direction: column; gap: var(--rhx-space-xs); font-family: var(--rhx-font-family); max-width: 20rem; }
  .rhx-datetime-picker__label { font-size: var(--rhx-font-size-sm); font-weight: var(--rhx-font-weight-medium); color: var(--rhx-color-text); }

  .rhx-datetime-picker__control { display: flex; align-items: center; border: var(--rhx-border-width) solid var(--rhx-color-border); border-radius: var(--rhx-radius-md); background: var(--rhx-color-surface); }
  .rhx-datetime-picker__control:focus-within { border-color: var(--rhx-color-brand-500); box-shadow: 0 0 0 1px var(--rhx-color-brand-500); }
  .rhx-datetime-picker__input { flex: 1; min-width: 10rem; min-height: 2.5rem; padding: var(--rhx-space-sm) var(--rhx-space-md); border: none; background: transparent; color: var(--rhx-color-text); font: inherit; outline: none; }
  .rhx-datetime-picker__input::placeholder { color: var(--rhx-color-text-muted); }
  .rhx-datetime-picker__trigger { display: inline-flex; align-items: center; justify-content: center; width: 2.5rem; align-self: stretch; border: none; background: none; color: var(--rhx-color-text-muted); cursor: pointer; }
  .rhx-datetime-picker__trigger:hover:not([disabled]) { color: var(--rhx-color-brand-600); }

  .rhx-datetime-picker__popup { position: absolute; top: calc(100% + var(--rhx-space-2xs, 0.25rem)); left: 0; z-index: var(--rhx-z-dropdown); border: var(--rhx-border-width) solid var(--rhx-color-border); border-radius: var(--rhx-radius-md); background: var(--rhx-color-surface-raised); box-shadow: var(--rhx-shadow-lg); }
  .rhx-datetime-picker__popup[hidden] { display: none; }

  .rhx-datetime-picker__panes { display: flex; align-items: stretch; }
  .rhx-datetime-picker__calendar { border-right: var(--rhx-border-width) solid var(--rhx-color-border); }
  .rhx-datetime-picker__times { width: 7rem; max-height: 17.5rem; overflow-y: auto; padding: var(--rhx-space-2xs, 0.25rem); }

  .rhx-datetime-picker__footer { display: flex; justify-content: flex-end; gap: var(--rhx-space-sm); padding: var(--rhx-space-xs) var(--rhx-space-sm); border-top: var(--rhx-border-width) solid var(--rhx-color-border); }
  .rhx-datetime-picker__action { border: none; background: none; color: var(--rhx-color-brand-600); font: inherit; font-weight: var(--rhx-font-weight-medium); cursor: pointer; padding: var(--rhx-space-2xs, 0.25rem) var(--rhx-space-sm); border-radius: var(--rhx-radius-sm); }
  .rhx-datetime-picker__action:hover { background: var(--rhx-color-brand-100); }

  .rhx-datetime-picker--small .rhx-datetime-picker__input { min-height: 2rem; font-size: var(--rhx-font-size-sm); }
  .rhx-datetime-picker--large .rhx-datetime-picker__input { min-height: 3rem; font-size: var(--rhx-font-size-md); }
  .rhx-datetime-picker--disabled { opacity: 0.6; pointer-events: none; }
  .rhx-datetime-picker--readonly .rhx-datetime-picker__trigger,
  .rhx-datetime-picker--readonly .rhx-datetime-picker__input { cursor: default; }
  .rhx-datetime-picker--error .rhx-datetime-picker__control { border-color: var(--rhx-color-danger-500); }

  @media (max-width: 30rem) {
    .rhx-datetime-picker__panes { flex-direction: column; }
    .rhx-datetime-picker__calendar { border-right: none; border-bottom: var(--rhx-border-width) solid var(--rhx-color-border); }
    .rhx-datetime-picker__times { width: auto; max-height: 10rem; }
  }
}
```

- [ ] **Step 2: Build to confirm embedding**

Run: `dotnet build htmxRazor`
Expected: 0 warnings.

- [ ] **Step 3: Commit**

```bash
git add htmxRazor/Assets/css/components/rhx-datetime-picker.css
git commit -m "feat(datetime-picker): two-pane popup stylesheet"
```

---

## Task 3: JS — date+time tracking, commit, footer, htmx re-highlight

**Files:**
- Create: `htmxRazor/Assets/js/components/rhx-datetimepicker.js`

Read `htmxRazor/Assets/js/components/rhx-datepicker.js` (htmx grid swap + day select) and `rhx-timepicker.js` (option select) first.

- [ ] **Step 1: Write the script**

```js
/**
 * htmxRazor DateTime Picker
 * Popup with a calendar (left) and a time list (right). Selecting a day sets the date part;
 * selecting a time sets the time part; the control commits a hidden ISO yyyy-MM-ddTHH:mm value
 * once both are set, then closes. Calendar month nav is htmx-driven (the calendar pane swaps);
 * the JS only echoes server-rendered display text (no client re-formatting) and re-applies the
 * date highlight after a swap. Positioning reuses rhx-position.js.
 */
(function () {
  "use strict";

  var DAY = ".rhx-calendar__day:not([disabled])";
  var TIME = ".rhx-time-picker__option:not([disabled])";

  function initDateTimePickers(root) {
    root.querySelectorAll("[data-rhx-datetime-picker]").forEach(function (dtp) {
      if (dtp._rhxDtInit) return;
      dtp._rhxDtInit = true;

      var input = dtp.querySelector("[data-rhx-dt-display]");
      var trigger = dtp.querySelector(".rhx-datetime-picker__trigger");
      var popup = dtp.querySelector(".rhx-datetime-picker__popup");
      var hidden = dtp.querySelector("[data-rhx-dt-value]");
      var times = dtp.querySelector(".rhx-datetime-picker__times");
      if (!input || !popup || !hidden) return;

      // Parts (ISO) + display strings, seeded from the server-rendered selection.
      var dateIso = "", dateDisp = "", timeIso = "", timeDisp = "";
      (function seed() {
        var selDay = popup.querySelector(".rhx-calendar__day--selected");
        if (selDay) { dateIso = selDay.getAttribute("data-date") || ""; dateDisp = selDay.getAttribute("data-display") || selDay.textContent.trim(); }
        var selTime = popup.querySelector(".rhx-time-picker__option--selected");
        if (selTime) { timeIso = selTime.getAttribute("data-time") || ""; timeDisp = selTime.textContent.trim(); }
      })();

      function isOpen() { return !popup.hidden; }

      function open() {
        if (input.hasAttribute("disabled") || input.hasAttribute("readonly")) return;
        popup.hidden = false;
        input.setAttribute("aria-expanded", "true");
        if (trigger) trigger.setAttribute("aria-expanded", "true");
        if (window.RHX && typeof window.RHX.positionElement === "function") {
          window.RHX.positionElement(input.parentNode, popup, { placement: "bottom-start", distance: 4, flip: true, shift: true });
        }
      }

      function close(focusInput) {
        popup.hidden = true;
        input.setAttribute("aria-expanded", "false");
        if (trigger) trigger.setAttribute("aria-expanded", "false");
        if (focusInput) input.focus();
      }

      function refresh() {
        if (dateIso && timeIso) {
          hidden.value = dateIso + "T" + timeIso;
          input.value = dateDisp + " " + timeDisp;
        } else {
          hidden.value = "";
          input.value = [dateDisp, timeDisp].filter(Boolean).join(" ");
        }
        hidden.dispatchEvent(new Event("input", { bubbles: true }));
        hidden.dispatchEvent(new Event("change", { bubbles: true }));
        dtp.dispatchEvent(new CustomEvent("rhx:datetime-picker:change", { bubbles: true, detail: { value: hidden.value } }));
      }

      function selectDay(cell) {
        var prev = popup.querySelector(".rhx-calendar__day--selected");
        if (prev) { prev.classList.remove("rhx-calendar__day--selected"); prev.removeAttribute("aria-selected"); }
        cell.classList.add("rhx-calendar__day--selected");
        cell.setAttribute("aria-selected", "true");
        dateIso = cell.getAttribute("data-date") || "";
        dateDisp = cell.getAttribute("data-display") || cell.textContent.trim();
        refresh();
        if (timeIso) close(true); // both set -> done
      }

      function selectTime(opt) {
        if (times) {
          var prev = times.querySelector(".rhx-time-picker__option--selected");
          if (prev) { prev.classList.remove("rhx-time-picker__option--selected"); prev.removeAttribute("aria-selected"); }
        }
        opt.classList.add("rhx-time-picker__option--selected");
        opt.setAttribute("aria-selected", "true");
        timeIso = opt.getAttribute("data-time") || "";
        timeDisp = opt.textContent.trim();
        refresh();
        if (dateIso) close(true); // both set -> done
      }

      // Open interactions
      if (trigger) trigger.addEventListener("click", function () { isOpen() ? close(false) : open(); });
      input.addEventListener("focus", function () { if (!isOpen()) open(); });

      // Delegated selection + footer (survives htmx calendar swaps)
      popup.addEventListener("click", function (e) {
        var day = e.target.closest(DAY);
        if (day && popup.contains(day)) { selectDay(day); return; }
        var t = e.target.closest(TIME);
        if (t && times && times.contains(t)) { selectTime(t); return; }
        if (e.target.closest("[data-rhx-dt-clear]")) {
          dateIso = dateDisp = timeIso = timeDisp = "";
          var sd = popup.querySelector(".rhx-calendar__day--selected"); if (sd) { sd.classList.remove("rhx-calendar__day--selected"); sd.removeAttribute("aria-selected"); }
          var st = times && times.querySelector(".rhx-time-picker__option--selected"); if (st) { st.classList.remove("rhx-time-picker__option--selected"); st.removeAttribute("aria-selected"); }
          refresh();
          return;
        }
        if (e.target.closest("[data-rhx-dt-done]")) { close(true); }
      });

      // After an htmx calendar-pane swap, re-apply the client's date highlight if visible.
      popup.addEventListener("htmx:afterSwap", function () {
        if (!dateIso) return;
        var cell = popup.querySelector('.rhx-calendar__day[data-date="' + dateIso + '"]');
        if (cell) {
          var prev = popup.querySelector(".rhx-calendar__day--selected");
          if (prev && prev !== cell) { prev.classList.remove("rhx-calendar__day--selected"); prev.removeAttribute("aria-selected"); }
          cell.classList.add("rhx-calendar__day--selected");
          cell.setAttribute("aria-selected", "true");
        }
      });

      document.addEventListener("click", function (e) { if (isOpen() && !dtp.contains(e.target)) close(false); });
    });
  }

  if (window.RHX) window.RHX.register("datetime-picker", initDateTimePickers);
})();
```

- [ ] **Step 2: Build to confirm embedding**

Run: `dotnet build htmxRazor`
Expected: 0 warnings.

- [ ] **Step 3: Commit**

```bash
git add htmxRazor/Assets/js/components/rhx-datetimepicker.js
git commit -m "feat(datetime-picker): interaction JS (date+time parts, commit, htmx re-highlight)"
```

---

## Task 4: Demo page + sidebar + asset registration

**Files:**
- Create: `htmxRazor.Demo/Pages/Docs/Components/DateTimePicker.cshtml` + `.cshtml.cs`
- Modify: `_DocsLayout.cshtml`, `_Layout.cshtml` (CSS + JS), `_SidebarNav.cshtml` (Forms entry)

- [ ] **Step 1: Register assets in BOTH `_DocsLayout.cshtml` and `_Layout.cshtml`**

After the `rhx-time-picker.css` link, add:
```html
    <link rel="stylesheet" href="/_rhx/css/components/rhx-datetime-picker.css" />
```
After the `rhx-timepicker.js` script, add:
```html
    <script src="/_rhx/js/components/rhx-datetimepicker.js" defer></script>
```
(Verify both layouts get the css + js. The datetime time pane relies on `rhx-time-picker.css` + `rhx-calendar.css` already being linked — confirm they are.)

- [ ] **Step 2: Sidebar** — in `_SidebarNav.cshtml`, after the Time Picker link:
```html
            <a href="/Docs/Components/DateTimePicker" class="docs-nav__sublink @(currentPath == "/Docs/Components/DateTimePicker" ? "docs-nav__sublink--active" : "")">Date &amp; Time Picker</a>
```

- [ ] **Step 3: PageModel `DateTimePicker.cshtml.cs`** (confirm `ComponentProperty`/`BreadcrumbItem` ctor against `DatePicker.cshtml.cs`):
```csharp
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using htmxRazor.Components.Navigation;
using htmxRazor.Demo.Models;

namespace htmxRazor.Demo.Pages.Docs.Components;

public class DateTimePickerModel : PageModel
{
    public List<ComponentProperty> Properties { get; } = new()
    {
        new("rhx-for", "ModelExpression", "-", "Binds DateTime for two-way model binding"),
        new("name", "string", "-", "Form field name for the hidden ISO (yyyy-MM-ddTHH:mm) value"),
        new("rhx-placeholder", "string", "-", "Placeholder for the text input"),
        new("rhx-min", "string", "-", "Earliest selectable date (ISO yyyy-MM-dd)"),
        new("rhx-max", "string", "-", "Latest selectable date (ISO yyyy-MM-dd)"),
        new("rhx-week-start", "string", "mon", "First day of the week: mon or sun"),
        new("rhx-step", "int", "30", "Minutes between time options"),
        new("rhx-12hour", "bool", "true", "12-hour vs 24-hour time display"),
        new("rhx-date-format", "string", "-", ".NET date format for display (default: culture short date)"),
        new("rhx-time-format", "string", "-", ".NET time format for display (overrides 12/24h)"),
        new("rhx-size", "string", "medium", "small, medium, large"),
        new("rhx-disabled", "bool", "false", "Disable the control"),
    };

    public string BasicCode => "<rhx-datetime-picker name=\"StartsAt\" rhx-placeholder=\"Pick date & time…\" rhx-step=\"30\" rhx-week-start=\"mon\" />";

    public void OnGet()
    {
        ViewData["Breadcrumbs"] = new List<BreadcrumbItem>
        {
            new("Home", "/"),
            new("Components", "/Docs/Components/DateTimePicker"),
            new("Date & Time Picker"),
        };
    }
}
```

- [ ] **Step 4: View `DateTimePicker.cshtml`** (mirror `DatePicker.cshtml` tab structure; confirm tag names against it):
```razor
@page
@model DateTimePickerModel
@{
    Layout = "_ComponentPage";
    ViewData["Title"] = "Date & Time Picker";
    ViewData["ComponentName"] = "Date & Time Picker";
    ViewData["ComponentTag"] = "rhx-datetime-picker";
    ViewData["ComponentDescription"] = "One control binding a single DateTime: a popup with a calendar and a time list. Commits a hidden ISO yyyy-MM-ddTHH:mm value once both date and time are set.";
}

<section class="docs-component-section">
    <h2>Examples</h2>

    <div class="docs-example">
        <h3>Basic</h3>
        <rhx-tab-group>
            <rhx-tab rhx-panel="basic-preview" rhx-active="true">Preview</rhx-tab>
            <rhx-tab rhx-panel="basic-code">Code</rhx-tab>
            <rhx-tab-panel rhx-name="basic-preview" rhx-active="true">
                <div class="docs-preview docs-preview--vertical" style="min-height: 26rem; align-items: flex-start;">
                    <rhx-datetime-picker name="StartsAt" rhx-placeholder="Pick date &amp; time…" rhx-step="30" rhx-week-start="mon" />
                </div>
            </rhx-tab-panel>
            <rhx-tab-panel rhx-name="basic-code">
                <div class="docs-code"><pre><code>@Model.BasicCode</code></pre></div>
            </rhx-tab-panel>
        </rhx-tab-group>
    </div>
</section>

@await Html.PartialAsync("_PropertyTable", Model.Properties)

<section class="docs-component-section">
    <h2>Accessibility</h2>
    <ul class="docs-a11y-list">
        <li>Trigger uses <code>aria-haspopup="dialog"</code>/<code>aria-expanded</code>; the popup is <code>role="dialog"</code> containing the calendar <code>grid</code> and a time <code>listbox</code>.</li>
        <li>Calendar keyboard (arrows, PageUp/Down, Enter, Escape) and time-list selection work as in the standalone Date and Time pickers.</li>
        <li>The hidden value is ISO <code>yyyy-MM-ddTHH:mm</code> and is committed only once both a date and a time are chosen.</li>
    </ul>
</section>
```

- [ ] **Step 5: Build + smoke**

Run: `dotnet build htmxRazor.Demo`, then run the demo and:
```
curl -s http://localhost:5215/Docs/Components/DateTimePicker | grep -o 'rhx-datetime-picker__times\|rhx-calendar__day\|data-rhx-dt-value\|data-rhx-dt-done' | sort | uniq -c
```
Expect: the times pane, calendar days, hidden value, and Done button all present. Use a fresh port; `pkill -f htmxRazor.Demo` after.

- [ ] **Step 6: Commit**

```bash
git add htmxRazor.Demo/Pages/Docs/Components/DateTimePicker.cshtml htmxRazor.Demo/Pages/Docs/Components/DateTimePicker.cshtml.cs htmxRazor.Demo/Pages/Shared/_DocsLayout.cshtml htmxRazor.Demo/Pages/Shared/_Layout.cshtml htmxRazor.Demo/Pages/Shared/_SidebarNav.cshtml
git commit -m "docs(datetime-picker): demo page, sidebar entry, asset registration"
```

---

## Task 5: Playwright E2E

**Files:**
- Create: `PlaywrightTests/ComponentTests/DateTimePickerTests.cs`

- [ ] **Step 1: Write the E2E tests** (read `PlaywrightTests/ComponentTests/DatePickerTests.cs` for conventions):

```csharp
using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class DateTimePickerTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/DateTimePicker";
    private const string Scope = "#panel-basic-preview ";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Picking_a_day_then_a_time_commits_iso_and_closes(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var input = page.Locator(Scope + ".rhx-datetime-picker__input");
        var popup = page.Locator(Scope + ".rhx-datetime-picker__popup");
        await Assertions.Expect(popup).Not.ToBeVisibleAsync();

        await input.ClickAsync();
        await Assertions.Expect(popup).ToBeVisibleAsync();

        // pick a day (a non-muted, enabled day in the current month)
        await popup.Locator(".rhx-calendar__day:not(.rhx-calendar__day--muted):not([disabled])").First.ClickAsync();
        // popup stays open until a time is also chosen
        await Assertions.Expect(popup).ToBeVisibleAsync();
        // hidden value not yet committed (only date set)
        await Assertions.Expect(page.Locator(Scope + "[data-rhx-dt-value]")).ToHaveValueAsync("");

        // pick a time -> both set -> commits + closes
        await popup.Locator(".rhx-datetime-picker__times [role='option'][data-time='09:30']").ClickAsync();
        await Assertions.Expect(popup).Not.ToBeVisibleAsync();
        await Assertions.Expect(input).Not.ToHaveValueAsync("");
        // hidden value is a full ISO datetime
        await Assertions.Expect(page.Locator(Scope + "[data-rhx-dt-value]")).ToHaveValueAsync(new Regex(@"^\d{4}-\d{2}-\d{2}T09:30$"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Month_navigation_swaps_calendar_and_keeps_time_pane(string browserName)
    {
        var page = await OpenAsync(browserName, Path);
        await page.Locator(Scope + ".rhx-datetime-picker__input").ClickAsync();

        var grid = page.Locator(Scope + ".rhx-calendar__grid");
        var before = await grid.GetAttributeAsync("aria-label");
        await page.Locator(Scope + ".rhx-calendar__nav[aria-label='Next month']").ClickAsync();
        await Assertions.Expect(grid).Not.ToHaveAttributeAsync("aria-label", before!);
        // time pane is unaffected by the calendar swap
        await Assertions.Expect(page.Locator(Scope + ".rhx-datetime-picker__times [role='option']").First).ToBeVisibleAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clear_resets_value(string browserName)
    {
        var page = await OpenAsync(browserName, Path);
        await page.Locator(Scope + ".rhx-datetime-picker__input").ClickAsync();
        var popup = page.Locator(Scope + ".rhx-datetime-picker__popup");

        await popup.Locator(".rhx-calendar__day:not(.rhx-calendar__day--muted):not([disabled])").First.ClickAsync();
        await popup.Locator("[data-rhx-dt-clear]").ClickAsync();
        await Assertions.Expect(page.Locator(Scope + ".rhx-datetime-picker__input")).ToHaveValueAsync("");
        await Assertions.Expect(page.Locator(Scope + "[data-rhx-dt-value]")).ToHaveValueAsync("");
    }
}
```

Add `using System.Text.RegularExpressions;` at the top.

- [ ] **Step 2: Run on Chromium**

Run: `dotnet test PlaywrightTests --filter "FullyQualifiedName~DateTimePicker&DisplayName~chromium"`
Expected: 3 PASS. (If browsers missing: `npx -y playwright@1.60.0 install chromium`.) If a test fails, investigate the component/JS before weakening assertions; report any real bug.

- [ ] **Step 3: Commit**

```bash
git add PlaywrightTests/ComponentTests/DateTimePickerTests.cs
git commit -m "test(datetime-picker): Playwright E2E (day+time commit, month nav, clear)"
```

---

## Task 6: Docs + final verification

**Files:**
- Modify: `README.md`, `CHANGELOG.md`, `vscode-extension/snippets/htmxrazor.code-snippets`

- [ ] **Step 1: README Forms row** — add `Date & Time Picker` after `Time Picker` in the Forms catalog row (edit only that row, match format).

- [ ] **Step 2: CHANGELOG** — append under `## [2.1.0]` → `### Added`:
```markdown
- **Date & Time Picker** — `<rhx-datetime-picker>`: a single-`DateTime` control whose popup pairs the calendar (htmx month navigation, via `/_rhx/calendar`) with a time list. Selecting a day sets the date part and a time sets the time part; the control commits a hidden ISO `yyyy-MM-ddTHH:mm` value once both are set, then closes. Footer Clear/Done. Configurable `rhx-min`/`rhx-max` (date bounds), `rhx-week-start`, `rhx-step`, `rhx-12hour`, `rhx-date-format`/`rhx-time-format`. Reuses the Date Picker calendar core and the Time Picker list; `role="dialog"` containing a `grid` + `listbox`.
```
If a `### Components Added` Forms list exists under 2.1.0, append "Date & Time Picker" there too.

- [ ] **Step 3: VS Code snippet** — after the `"rhx-time-picker"` snippet, add (mind JSON commas; verify with the node parse command used in M2):
```json
  "rhx-datetime-picker": {
    "prefix": "rhx-datetime-picker",
    "body": [
      "<rhx-datetime-picker name=\"${1:StartsAt}\" rhx-placeholder=\"${2:Pick date & time…}\" rhx-step=\"${3:30}\" rhx-week-start=\"${4|mon,sun|}\" />"
    ],
    "description": "htmxRazor date & time picker (calendar + time list)"
  },
```

- [ ] **Step 4: Final verification (milestone gate)**
1. `dotnet build htmxRazor.sln` → 0 warnings.
2. `dotnet test htmxRazor.Tests` → all pass; report total.
3. `dotnet test PlaywrightTests --filter "FullyQualifiedName~DateTimePicker&DisplayName~chromium"` → 3 pass.

- [ ] **Step 5: Commit**

```bash
git add README.md CHANGELOG.md vscode-extension/snippets/htmxrazor.code-snippets
git commit -m "docs(datetime-picker): README catalog, changelog, VS Code snippet"
```

---

## Self-Review (completed)

**Spec coverage (§6):** one `DateTime` control, calendar + time list popup side by side → Task 1; hidden ISO `yyyy-MM-ddTHH:mm` → Task 1; "picking a day keeps the popup open; value commits when both set (or explicit Done)" → Tasks 1 (footer) + 3 (commit-on-both + Done); reuses §4 calendar (CalendarRenderer) + §5 time list (TimeListRenderer) verbatim → Task 1; htmx month nav via `/_rhx/calendar` → Task 1 (calendar TargetId) + Task 3 (re-highlight after swap); §3.5 popup positioning → Task 3; §8 a11y (dialog + grid + listbox) → Tasks 1, 4; §10 testing → Tasks 1, 5; §11 docs → Tasks 4, 6. Min/max are date bounds for the calendar (the spec's `rhx-min`/`max` for datetime are date-level); time bounds are full-day — documented. The "Now" button was intentionally omitted (spec mentions only "explicit Done"); avoids the client-side "today" display-divergence trap from M1.

**Placeholder scan:** Every code step is complete. The token-existence + browser-install lines are verification commands, not placeholders. The PREREQUISITE banner (M1/M2 merged) is a real execution gate, not a TODO in the code.

**Type consistency:** Reused APIs match the verified signatures — `CalendarRenderer.Render(CalendarOptions{...,Format,ShowToday=false,ShowClear=false,TargetId})` and `TimeListRenderer.RenderOptions(Step, null, null, TwelveHour, TimeFormat, timePart)`. Data attributes (`data-rhx-datetime-picker`, `data-rhx-dt-display`, `data-rhx-dt-value`, `data-rhx-dt-clear`, `data-rhx-dt-done`, and the reused `data-date`/`data-display`/`data-time`/`rhx-calendar__day--selected`/`rhx-time-picker__option--selected`) match between the Tag Helper (Task 1), JS (Task 3), and CSS (Task 2). `BlockName="datetime-picker"` maps `GetElementClass(...)` to the Task 2 stylesheet. Hidden value is ISO `yyyy-MM-ddTHH:mm` in both the Tag Helper and the JS commit. The E2E regex `^\d{4}-\d{2}-\d{2}T09:30$` matches that format.
