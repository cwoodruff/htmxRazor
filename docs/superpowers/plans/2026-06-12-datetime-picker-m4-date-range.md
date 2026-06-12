# Date/Time Picker — Milestone 4: Date Range Picker

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

> **PREREQUISITE:** Reuses Milestone 1's calendar core (`CalendarRenderer`/`CalendarOptions`/`CalendarEndpoint`, merged via PR #36, on `main`). Branch off an up-to-date `main`. This milestone makes ONE behavior-preserving change to M1 (`RenderDaysBody` → `internal`) plus all-new range files; it does NOT depend on M2/M3.

**Goal:** Ship `<rhx-date-range-picker>` — a two-date range control with **two side-by-side months** (synced prev/next), **live in-range hover preview**, and quick **presets** (Today / Last 7 days / This month / Last 30 days), committing two hidden ISO `yyyy-MM-dd` values (start + end).

**Architecture:** The range highlighting is **client-side**: the server renders plain dual-month grids; the JS paints `--range-start`/`--in-range`/`--range-end` (and a hover preview while picking) from its own start/end state on init, selection, hover, and after each htmx month swap. A new `CalendarRangeRenderer` renders a shared header (one prev/next pair + two month captions) over two day-grids, reusing M1's day-grid via a now-`internal` `CalendarRenderer.RenderDaysBody`. A `/_rhx/calendar-range` endpoint serves the dual widget for month navigation. `DateRangePickerTagHelper` (extends `FormControlTagHelperBase`) renders the input, two hidden inputs, the popup (range widget + presets), and seeds the JS via `data-range-start`/`data-range-end`. JS owns the two-click selection state machine, hover preview, presets, and committing both hidden inputs.

**Tech Stack:** ASP.NET Core Tag Helpers (.NET 10, C#, `DateOnly`), htmx (month nav), vanilla JS, BEM CSS with `rhx-` tokens, xUnit, Playwright.

**Spec:** `docs/superpowers/specs/2026-06-11-datetime-picker-family-design.md` (§7, plus §3.4/§3.5/§8/§10/§11).

**Reusable APIs (verify present before starting):**
- `htmxRazor.Components.Forms.Calendar.CalendarRenderer` — `Render(CalendarOptions)`. After Task 1, `RenderDaysBody(CalendarOptions)` is `internal static` and returns the `<div class="rhx-calendar__grid" role="grid" ...>` body (weekday header row + 6 week rows of `<button class="rhx-calendar__day[ --muted/--today/--selected]" role="gridcell" data-date="yyyy-MM-dd" data-display="{display}" [disabled]>`).
- `CalendarOptions` record: `Year, Month, View, Selected (DateOnly?), Min, Max (DateOnly?), WeekStart (DayOfWeek), Today (DateOnly), HxGetUrl, TargetId, ShowToday, ShowClear, Format (string?)`.
- `htmxRazor/Infrastructure/ApplicationBuilderExtensions.cs` mounts `/_rhx/calendar` via an `app.Use(...)` branch before `UseStaticFiles`; add `/_rhx/calendar-range` the same way.

**Conventions (read before starting):**
- `htmxRazor/Components/Forms/FormControlTagHelperBase.cs`; sibling pickers `DatePickerTagHelper.cs` for idioms (`Enc`, `CreateCssBuilder`, `GetElementClass/GetModifierClass`, `ApplyWrapperAttributes`, label/hint/error, `HasError/ResolveRequired`, `[HtmlAttributeNotBound] Today`).
- JS: `rhx-datepicker.js` (htmx grid swap + day delegation), `RHX.register`, `_rhx*Init` guard, `RHX.positionElement`. `IconRegistry.Get("calendar")`.
- Assets auto-embed; link CSS/JS in `_DocsLayout.cshtml` + `_Layout.cshtml`. `rhx-calendar.css` is already linked (styles the base day cells).
- Test base `htmxRazor.Tests/TagHelperTestBase.cs`; Playwright base `PlaywrightTests/Infrastructure/ComponentTestBase.cs` (`#panel-<rhx-name>-preview`). Chromium installed.
- **Lesson from M1–M3:** keep display server-formatted / culture-consistent. Calendar picks read each cell's `data-display`; presets (dates possibly off-screen) format client-side via `toLocaleDateString` — a documented minor inconsistency limited to coarse presets.

---

## File Structure

| File | Responsibility |
|---|---|
| `htmxRazor/Components/Forms/Calendar/CalendarRenderer.cs` | **Modify:** `RenderDaysBody` `private`→`internal`. No behavior change. |
| `htmxRazor/Components/Forms/Calendar/CalendarRangeRenderer.cs` | New. `CalendarRangeOptions` + `Render(...)` → dual-month widget (shared header + two day-grids), nav hx-get to `/_rhx/calendar-range`. |
| `htmxRazor/Components/Forms/Calendar/CalendarRangeEndpoint.cs` | New. `Render(IQueryCollection, today)` → parse → `CalendarRangeRenderer`. |
| `htmxRazor/Infrastructure/ApplicationBuilderExtensions.cs` | **Modify:** mount `/_rhx/calendar-range`. |
| `htmxRazor/Components/Forms/DateRangePickerTagHelper.cs` | New. `<rhx-date-range-picker>` — input + two hidden inputs + popup (range widget + presets) + JS seed. |
| `htmxRazor/Assets/css/components/rhx-date-range-picker.css` | New. Dual-month layout, range highlight classes, presets. |
| `htmxRazor/Assets/js/components/rhx-daterangepicker.js` | New. Two-click selection, hover preview, presets, range highlight, afterSwap repaint, commit two hidden inputs. |
| `htmxRazor.Tests/CalendarRangeRendererTests.cs`, `DateRangePickerTagHelperTests.cs` | Unit tests. |
| `htmxRazor.Demo/Pages/Docs/Components/DateRangePicker.cshtml(.cs)` | Demo. |
| `PlaywrightTests/ComponentTests/DateRangePickerTests.cs` | E2E. |

---

## Task 1: Expose the grid + `CalendarRangeRenderer` (dual-month widget)

**Files:**
- Modify: `htmxRazor/Components/Forms/Calendar/CalendarRenderer.cs` (one keyword)
- Create: `htmxRazor/Components/Forms/Calendar/CalendarRangeRenderer.cs`
- Test: `htmxRazor.Tests/CalendarRangeRendererTests.cs`

- [ ] **Step 1: Write the failing tests**

```csharp
using System;
using htmxRazor.Components.Forms.Calendar;
using Xunit;

namespace htmxRazor.Tests;

public class CalendarRangeRendererTests
{
    private static CalendarRangeOptions Opts() => new()
    {
        Year = 2026, Month = 10,
        Today = new DateOnly(2026, 10, 9),
        WeekStart = DayOfWeek.Monday,
        HxGetUrl = "/_rhx/calendar-range", TargetId = "rp1-cal",
    };

    [Fact]
    public void Renders_Two_Month_Grids_With_Captions()
    {
        var html = CalendarRangeRenderer.Render(Opts());
        Assert.Contains("id=\"rp1-cal\"", html);
        Assert.Contains("data-rhx-range-cal", html);
        // two month captions: October 2026 and November 2026
        Assert.Contains("October 2026", html);
        Assert.Contains("November 2026", html);
        // two grids => 84 day cells (42 each)
        Assert.Equal(84, System.Text.RegularExpressions.Regex.Matches(html, "role=\"gridcell\"").Count);
    }

    [Fact]
    public void Nav_Moves_Both_Months_By_One()
    {
        var html = CalendarRangeRenderer.Render(Opts());
        // prev -> September base; next -> November base
        Assert.Contains("hx-get=\"/_rhx/calendar-range?year=2026&amp;month=9", html);
        Assert.Contains("hx-get=\"/_rhx/calendar-range?year=2026&amp;month=11", html);
        Assert.Contains("hx-target=\"#rp1-cal\"", html);
        Assert.Contains("hx-swap=\"outerHTML\"", html);
    }

    [Fact]
    public void Grids_Are_Plain_No_Range_Classes_Server_Side()
    {
        // The server renders plain grids; the JS paints range classes at runtime.
        var html = CalendarRangeRenderer.Render(Opts());
        Assert.DoesNotContain("rhx-calendar__day--range", html);
        Assert.DoesNotContain("rhx-calendar__day--selected", html);
    }

    [Fact]
    public void Year_Boundary_December_Pairs_With_Next_January()
    {
        var html = CalendarRangeRenderer.Render(Opts() with { Year = 2026, Month = 12 });
        Assert.Contains("December 2026", html);
        Assert.Contains("January 2027", html);
        Assert.Contains("hx-get=\"/_rhx/calendar-range?year=2027&amp;month=1", html);  // next
        Assert.Contains("hx-get=\"/_rhx/calendar-range?year=2026&amp;month=11", html); // prev
    }
}
```

- [ ] **Step 2: Run to verify they fail**

Run: `dotnet test htmxRazor.Tests --filter CalendarRangeRendererTests`
Expected: FAIL — types don't exist.

- [ ] **Step 3: Make `RenderDaysBody` internal**

In `htmxRazor/Components/Forms/Calendar/CalendarRenderer.cs`, change the `RenderDaysBody` signature from:
```csharp
    private static string RenderDaysBody(CalendarOptions o)
```
to:
```csharp
    internal static string RenderDaysBody(CalendarOptions o)
```
(Only the access modifier changes. Run `dotnet test htmxRazor.Tests --filter CalendarRendererTests` afterward to confirm M1's tests still pass — they must.)

- [ ] **Step 4: Create `CalendarRangeRenderer`**

```csharp
using System;
using System.Globalization;
using System.Net;
using System.Text;

namespace htmxRazor.Components.Forms.Calendar;

/// <summary>Immutable inputs for <see cref="CalendarRangeRenderer"/>. Pure data.</summary>
public sealed record CalendarRangeOptions
{
    /// <summary>Year of the LEFT month.</summary>
    public int Year { get; init; }
    /// <summary>Month (1-12) of the LEFT month; the right month is this + 1.</summary>
    public int Month { get; init; }
    public DateOnly? Min { get; init; }
    public DateOnly? Max { get; init; }
    public DayOfWeek WeekStart { get; init; } = DayOfWeek.Monday;
    public DateOnly Today { get; init; }
    public string HxGetUrl { get; init; } = "/_rhx/calendar-range";
    public string TargetId { get; init; } = "rhx-range-cal";
    /// <summary>Optional .NET date format for day-cell data-display.</summary>
    public string? Format { get; init; }
}

/// <summary>
/// Renders a two-month range calendar: a single shared header (one prev/next pair + the two
/// month captions) over two plain day-grids. Range highlighting is applied client-side by the JS;
/// the grids carry no range/selected classes. Nav re-requests <see cref="CalendarRangeOptions.HxGetUrl"/>.
/// </summary>
public static class CalendarRangeRenderer
{
    private static string Enc(string s) => WebUtility.HtmlEncode(s);

    private static string NavUrl(CalendarRangeOptions o, int year, int month)
    {
        var sb = new StringBuilder();
        var sep = o.HxGetUrl.Contains('?') ? '&' : '?';
        sb.Append(o.HxGetUrl).Append(sep).Append("year=").Append(year).Append("&month=").Append(month);
        if (o.Min is { } mn) sb.Append("&min=").Append(mn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        if (o.Max is { } mx) sb.Append("&max=").Append(mx.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        sb.Append("&week-start=").Append(o.WeekStart.ToString().ToLowerInvariant());
        if (!string.IsNullOrEmpty(o.Format)) sb.Append("&format=").Append(Uri.EscapeDataString(o.Format));
        sb.Append("&id=").Append(Uri.EscapeDataString(o.TargetId));
        return Enc(sb.ToString());
    }

    /// <summary>Builds a single-month <see cref="CalendarOptions"/> for the grid body (no selection — JS paints range).</summary>
    private static CalendarOptions MonthOpts(CalendarRangeOptions o, int year, int month) => new()
    {
        Year = year, Month = month, View = CalendarView.Days,
        Selected = null, Min = o.Min, Max = o.Max, WeekStart = o.WeekStart, Today = o.Today, Format = o.Format,
        TargetId = o.TargetId, HxGetUrl = o.HxGetUrl,
    };

    public static string Render(CalendarRangeOptions o)
    {
        var left = new DateOnly(o.Year, o.Month, 1);
        var right = left.AddMonths(1);
        var prev = left.AddMonths(-1);
        var next = left.AddMonths(1); // base advances by one month
        var leftLabel = left.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
        var rightLabel = right.ToString("MMMM yyyy", CultureInfo.InvariantCulture);

        var sb = new StringBuilder();
        sb.Append($"<div class=\"rhx-date-range-picker__cal\" id=\"{Enc(o.TargetId)}\" data-rhx-range-cal>");

        // Shared header
        sb.Append("<div class=\"rhx-date-range-picker__cal-header\">");
        sb.Append($"<button type=\"button\" class=\"rhx-calendar__nav\" aria-label=\"Previous month\" hx-get=\"{NavUrl(o, prev.Year, prev.Month)}\" hx-target=\"#{Enc(o.TargetId)}\" hx-swap=\"outerHTML\">&#8249;</button>");
        sb.Append($"<span class=\"rhx-date-range-picker__cal-caption\">{Enc(leftLabel)}</span>");
        sb.Append($"<span class=\"rhx-date-range-picker__cal-caption\">{Enc(rightLabel)}</span>");
        sb.Append($"<button type=\"button\" class=\"rhx-calendar__nav\" aria-label=\"Next month\" hx-get=\"{NavUrl(o, next.Year, next.Month)}\" hx-target=\"#{Enc(o.TargetId)}\" hx-swap=\"outerHTML\">&#8250;</button>");
        sb.Append("</div>");

        // Two month grids
        sb.Append("<div class=\"rhx-date-range-picker__months\">");
        sb.Append("<div class=\"rhx-date-range-picker__month\">").Append(CalendarRenderer.RenderDaysBody(MonthOpts(o, left.Year, left.Month))).Append("</div>");
        sb.Append("<div class=\"rhx-date-range-picker__month\">").Append(CalendarRenderer.RenderDaysBody(MonthOpts(o, right.Year, right.Month))).Append("</div>");
        sb.Append("</div>");

        sb.Append("</div>");
        return sb.ToString();
    }
}
```

> Note: `MonthOpts` sets `Selected = null` so `RenderDaysBody` emits no `--selected`. The JS paints all range state. The two `RenderDaysBody` calls reuse M1's day-cell logic verbatim (weekday header, 42 cells, muted/today/disabled, `data-date`, `data-display`).

- [ ] **Step 5: Run to verify they pass**

Run: `dotnet test htmxRazor.Tests --filter "CalendarRangeRendererTests|CalendarRendererTests"`
Expected: PASS (new range tests + all M1 calendar tests still green). Then `dotnet build htmxRazor` (0 warnings).

- [ ] **Step 6: Commit**

```bash
git add htmxRazor/Components/Forms/Calendar/CalendarRenderer.cs htmxRazor/Components/Forms/Calendar/CalendarRangeRenderer.cs htmxRazor.Tests/CalendarRangeRendererTests.cs
git commit -m "feat(date-range): CalendarRangeRenderer dual-month widget (RenderDaysBody internal)"
```

---

## Task 2: `/_rhx/calendar-range` endpoint

**Files:**
- Create: `htmxRazor/Components/Forms/Calendar/CalendarRangeEndpoint.cs`
- Modify: `htmxRazor/Infrastructure/ApplicationBuilderExtensions.cs`
- Test: `htmxRazor.Tests/CalendarRangeRendererTests.cs`

- [ ] **Step 1: Write the failing tests**

```csharp
[Fact]
public void Endpoint_Parses_Query_And_Renders_Two_Months()
{
    var q = new Microsoft.AspNetCore.Http.QueryCollection(
        new System.Collections.Generic.Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            ["year"] = "2026", ["month"] = "10", ["min"] = "2026-01-01", ["max"] = "2026-12-31",
            ["week-start"] = "monday", ["id"] = "rp1-cal",
        });
    var html = CalendarRangeEndpoint.Render(q, today: new DateOnly(2026, 10, 9));
    Assert.Contains("id=\"rp1-cal\"", html);
    Assert.Contains("October 2026", html);
    Assert.Contains("November 2026", html);
}

[Fact]
public void Endpoint_Clamps_Out_Of_Range_Year_And_Defaults_To_Today()
{
    var q = new Microsoft.AspNetCore.Http.QueryCollection(
        new System.Collections.Generic.Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            ["year"] = "0", ["month"] = "abc",
        });
    var html = CalendarRangeEndpoint.Render(q, today: new DateOnly(2026, 3, 4));
    Assert.Contains("March 2026", html); // graceful fallback, no throw
}
```

- [ ] **Step 2: Run to verify they fail**

Run: `dotnet test htmxRazor.Tests --filter "Endpoint_Parses_Query_And_Renders_Two_Months"`
Expected: FAIL — `CalendarRangeEndpoint` missing.

- [ ] **Step 3: Create `CalendarRangeEndpoint`**

```csharp
using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace htmxRazor.Components.Forms.Calendar;

/// <summary>Translates a query string into <see cref="CalendarRangeOptions"/> and renders the dual-month widget.</summary>
public static class CalendarRangeEndpoint
{
    public static string Render(IQueryCollection q, DateOnly today)
    {
        var year = ParseInt(q["year"].ToString(), today.Year);
        if (year < 1 || year > 9999) year = today.Year;
        var month = ParseInt(q["month"].ToString(), today.Month);
        if (month < 1 || month > 12) month = today.Month;

        var weekStart = Enum.TryParse<DayOfWeek>(q["week-start"], ignoreCase: true, out var ws) ? ws : DayOfWeek.Monday;

        var opts = new CalendarRangeOptions
        {
            Year = year,
            Month = month,
            Min = ParseDate(q["min"]),
            Max = ParseDate(q["max"]),
            WeekStart = weekStart,
            Today = today,
            Format = q["format"].Count == 0 ? null : q["format"].ToString(),
            TargetId = string.IsNullOrWhiteSpace(q["id"]) ? "rhx-range-cal" : q["id"].ToString(),
        };
        return CalendarRangeRenderer.Render(opts);
    }

    private static int ParseInt(string? s, int fallback) =>
        int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : fallback;

    private static DateOnly? ParseDate(string? s) =>
        DateOnly.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;
}
```

- [ ] **Step 4: Mount in `UsehtmxRazor`**

In `htmxRazor/Infrastructure/ApplicationBuilderExtensions.cs`, immediately AFTER the existing `/_rhx/calendar` `app.Use(...)` block (and still before `UseStaticFiles`), add:

```csharp
app.Use(async (context, next) =>
{
    if (context.Request.Path.Equals("/_rhx/calendar-range", StringComparison.OrdinalIgnoreCase)
        && HttpMethods.IsGet(context.Request.Method))
    {
        var html = CalendarRangeEndpoint.Render(context.Request.Query, DateOnly.FromDateTime(DateTime.Today));
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.WriteAsync(html);
        return;
    }
    await next();
});
```

- [ ] **Step 5: Run + build + commit**

Run: `dotnet test htmxRazor.Tests --filter CalendarRangeRendererTests` (all pass), `dotnet build htmxRazor` (0 warnings).
```bash
git add htmxRazor/Components/Forms/Calendar/CalendarRangeEndpoint.cs htmxRazor/Infrastructure/ApplicationBuilderExtensions.cs htmxRazor.Tests/CalendarRangeRendererTests.cs
git commit -m "feat(date-range): /_rhx/calendar-range endpoint"
```

---

## Task 3: `DateRangePickerTagHelper`

**Files:**
- Create: `htmxRazor/Components/Forms/DateRangePickerTagHelper.cs`
- Test: `htmxRazor.Tests/DateRangePickerTagHelperTests.cs`

- [ ] **Step 1: Write the failing tests**

```csharp
using System;
using htmxRazor.Components.Forms;
using Xunit;

namespace htmxRazor.Tests;

public class DateRangePickerTagHelperTests : TagHelperTestBase
{
    private DateRangePickerTagHelper CreateHelper() => new(CreateUrlHelperFactory())
    {
        ViewContext = CreateViewContext(),
        Today = new DateOnly(2026, 10, 9),
    };

    [Fact]
    public async Task Renders_Wrapper_Input_Two_Hidden_Inputs_And_Range_Calendar()
    {
        var helper = CreateHelper();
        helper.StartName = "From";
        helper.EndName = "To";
        helper.StartValue = "2026-10-13";
        helper.EndValue = "2026-10-17";
        var ctx = CreateContext("rhx-date-range-picker");
        var output = CreateOutput("rhx-date-range-picker");

        await helper.ProcessAsync(ctx, output);

        Assert.Equal("div", output.TagName);
        Assert.True(HasClass(output, "rhx-date-range-picker"));
        Assert.True(output.Attributes.TryGetAttribute("data-rhx-date-range-picker", out _));
        var html = output.Content.GetContent();
        Assert.Contains("rhx-date-range-picker__input", html);
        Assert.Contains("name=\"From\"", html);
        Assert.Contains("name=\"To\"", html);
        Assert.Contains("data-rhx-range-start", html);
        Assert.Contains("data-rhx-range-end", html);
        Assert.Contains("value=\"2026-10-13\"", html);
        Assert.Contains("value=\"2026-10-17\"", html);
        // dual-month calendar present
        Assert.Contains("data-rhx-range-cal", html);
        Assert.Contains("October 2026", html);
        Assert.Contains("November 2026", html);
    }

    [Fact]
    public async Task Seeds_Js_With_Range_Data_Attributes()
    {
        var helper = CreateHelper();
        helper.StartName = "From"; helper.EndName = "To";
        helper.StartValue = "2026-10-13"; helper.EndValue = "2026-10-17";
        var ctx = CreateContext("rhx-date-range-picker");
        var output = CreateOutput("rhx-date-range-picker");

        await helper.ProcessAsync(ctx, output);

        Assert.Equal("2026-10-13", GetAttribute(output, "data-range-start"));
        Assert.Equal("2026-10-17", GetAttribute(output, "data-range-end"));
    }

    [Fact]
    public async Task Renders_Presets_When_Requested()
    {
        var helper = CreateHelper();
        helper.StartName = "From"; helper.EndName = "To";
        helper.Presets = "today,last7,thismonth,last30";
        var ctx = CreateContext("rhx-date-range-picker");
        var output = CreateOutput("rhx-date-range-picker");

        await helper.ProcessAsync(ctx, output);
        var html = output.Content.GetContent();

        Assert.Contains("rhx-date-range-picker__presets", html);
        Assert.Contains("data-range-preset=\"last7\"", html);
        Assert.Contains("data-range-preset=\"thismonth\"", html);
        Assert.Contains(">Last 7 days<", html);
    }

    [Fact]
    public async Task Empty_Values_Render_Today_Months_And_Empty_Hidden()
    {
        var helper = CreateHelper();
        helper.StartName = "From"; helper.EndName = "To";
        var ctx = CreateContext("rhx-date-range-picker");
        var output = CreateOutput("rhx-date-range-picker");

        await helper.ProcessAsync(ctx, output);
        var html = output.Content.GetContent();

        Assert.Contains("October 2026", html);   // today = 2026-10-09 left month
        Assert.Contains("November 2026", html);
        Assert.Equal("", GetAttribute(output, "data-range-start"));
    }
}
```

- [ ] **Step 2: Run to verify they fail**

Run: `dotnet test htmxRazor.Tests --filter DateRangePickerTagHelperTests`
Expected: FAIL — type missing.

- [ ] **Step 3: Create `DateRangePickerTagHelper`**

```csharp
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using htmxRazor.Components.Forms.Calendar;
using htmxRazor.Components.Imagery;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Forms;

/// <summary>
/// A two-date range picker: two side-by-side months with synced navigation, live in-range hover
/// preview, and optional presets. Commits two hidden ISO yyyy-MM-dd values (start + end).
/// </summary>
/// <example>
/// <code>
/// &lt;rhx-date-range-picker rhx-start-name="From" rhx-end-name="To"
///                        rhx-presets="today,last7,thismonth,last30" /&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-date-range-picker")]
public class DateRangePickerTagHelper : FormControlTagHelperBase
{
    protected override string BlockName => "date-range-picker";

    [HtmlAttributeName("rhx-start-name")] public string? StartName { get; set; }
    [HtmlAttributeName("rhx-end-name")] public string? EndName { get; set; }
    [HtmlAttributeName("rhx-start-value")] public string? StartValue { get; set; }
    [HtmlAttributeName("rhx-end-value")] public string? EndValue { get; set; }
    [HtmlAttributeName("rhx-placeholder")] public string? Placeholder { get; set; }
    [HtmlAttributeName("rhx-min")] public string? Min { get; set; }
    [HtmlAttributeName("rhx-max")] public string? Max { get; set; }
    [HtmlAttributeName("rhx-week-start")] public string WeekStartName { get; set; } = "mon";
    [HtmlAttributeName("rhx-format")] public string? Format { get; set; }
    [HtmlAttributeName("rhx-presets")] public string? Presets { get; set; }

    [HtmlAttributeNotBound] public DateOnly Today { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    private static readonly Dictionary<string, string> PresetLabels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["today"] = "Today", ["yesterday"] = "Yesterday", ["last7"] = "Last 7 days",
        ["last30"] = "Last 30 days", ["thismonth"] = "This month", ["lastmonth"] = "Last month",
    };

    public DateRangePickerTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        await Task.CompletedTask;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var id = ResolveId();
        if (string.IsNullOrEmpty(id)) id = "rhx-rp-" + context.UniqueId;
        var start = ParseDate(StartValue);
        var end = ParseDate(EndValue);
        var calId = $"{id}-cal";
        var popupId = $"{id}-popup";
        var inputId = $"{id}-input";
        var labelId = $"{id}-label";
        var hintId = $"{id}-hint";
        var errorId = $"{id}-error";
        var size = Size.ToLowerInvariant();
        var hasError = HasError();

        var css = CreateCssBuilder()
            .AddIf(GetModifierClass(size), size != "medium")
            .AddIf(GetModifierClass("disabled"), Disabled)
            .AddIf(GetModifierClass("readonly"), Readonly)
            .AddIf(GetModifierClass("error"), hasError);
        ApplyWrapperAttributes(output, css);
        output.Attributes.SetAttribute("data-rhx-date-range-picker", "");
        output.Attributes.SetAttribute("data-range-start", start is { } s0 ? s0.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "");
        output.Attributes.SetAttribute("data-range-end", end is { } e0 ? e0.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "");

        var weekStart = Enum.TryParse<DayOfWeek>(ExpandWeekStart(WeekStartName), true, out var ws) ? ws : DayOfWeek.Monday;
        var view = start ?? Today;
        var rangeOpts = new CalendarRangeOptions
        {
            Year = view.Year, Month = view.Month, Min = ParseDate(Min), Max = ParseDate(Max),
            WeekStart = weekStart, Today = Today, HxGetUrl = "/_rhx/calendar-range", TargetId = calId, Format = Format,
        };

        var startDisp = start is { } sd ? sd.ToString(string.IsNullOrEmpty(Format) ? "d" : Format, CultureInfo.CurrentCulture) : "";
        var endDisp = end is { } ed ? ed.ToString(string.IsNullOrEmpty(Format) ? "d" : Format, CultureInfo.CurrentCulture) : "";
        var display = (start != null && end != null) ? $"{startDisp} – {endDisp}" : "";

        var sb = new StringBuilder();

        var labelText = ResolveLabelText();
        if (!string.IsNullOrEmpty(labelText))
            sb.Append($"<label class=\"{GetElementClass("label")}\" id=\"{Enc(labelId)}\" for=\"{Enc(inputId)}\">{Enc(labelText)}</label>");

        sb.Append($"<div class=\"{GetElementClass("control")}\">");
        sb.Append($"<input class=\"{GetElementClass("input")}\" id=\"{Enc(inputId)}\" type=\"text\" autocomplete=\"off\" data-rhx-range-display");
        sb.Append($" aria-haspopup=\"dialog\" aria-expanded=\"false\" aria-controls=\"{Enc(popupId)}\"");
        if (!string.IsNullOrEmpty(Placeholder)) sb.Append($" placeholder=\"{Enc(Placeholder)}\"");
        if (!string.IsNullOrEmpty(display)) sb.Append($" value=\"{Enc(display)}\"");
        if (!string.IsNullOrEmpty(labelText)) sb.Append($" aria-labelledby=\"{Enc(labelId)}\"");
        if (!string.IsNullOrEmpty(AriaLabel)) sb.Append($" aria-label=\"{Enc(AriaLabel)}\"");
        if (Disabled) sb.Append(" disabled");
        if (Readonly) sb.Append(" readonly");
        sb.Append(" />");
        sb.Append($"<button type=\"button\" class=\"{GetElementClass("trigger")}\" tabindex=\"-1\" aria-haspopup=\"dialog\" aria-expanded=\"false\" aria-controls=\"{Enc(popupId)}\" aria-label=\"Open date range picker\"");
        if (Disabled) sb.Append(" disabled");
        sb.Append('>');
        sb.Append($"<svg viewBox=\"0 0 24 24\" width=\"18\" height=\"18\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\">{IconRegistry.Get("calendar") ?? ""}</svg>");
        sb.Append("</button>");
        sb.Append("</div>");

        // Two hidden inputs
        sb.Append($"<input type=\"hidden\" data-rhx-range-start name=\"{Enc(StartName ?? "")}\" value=\"{Enc(start is { } s1 ? s1.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "")}\" />");
        sb.Append($"<input type=\"hidden\" data-rhx-range-end name=\"{Enc(EndName ?? "")}\" value=\"{Enc(end is { } e1 ? e1.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "")}\" />");

        // Popup
        sb.Append($"<div class=\"{GetElementClass("popup")}\" id=\"{Enc(popupId)}\" role=\"dialog\" aria-modal=\"false\"");
        if (!string.IsNullOrEmpty(labelText)) sb.Append($" aria-labelledby=\"{Enc(labelId)}\"");
        else if (!string.IsNullOrEmpty(AriaLabel)) sb.Append($" aria-label=\"{Enc(AriaLabel)}\"");
        sb.Append(" hidden>");
        sb.Append($"<div class=\"{GetElementClass("body")}\">");

        // Presets column
        if (!string.IsNullOrWhiteSpace(Presets))
        {
            sb.Append($"<div class=\"{GetElementClass("presets")}\">");
            foreach (var raw in Presets.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var key = raw.ToLowerInvariant();
                var label = PresetLabels.TryGetValue(key, out var l) ? l : raw;
                sb.Append($"<button type=\"button\" class=\"{GetElementClass("preset")}\" data-range-preset=\"{Enc(key)}\">{Enc(label)}</button>");
            }
            sb.Append("</div>");
        }

        sb.Append(CalendarRangeRenderer.Render(rangeOpts));
        sb.Append("</div>"); // body
        sb.Append("</div>"); // popup

        sb.Append(BuildHintHtml(hintId));
        sb.Append(BuildErrorHtml(errorId));

        output.Content.SetHtmlContent(sb.ToString());
    }

    private static DateOnly? ParseDate(string? s) =>
        DateOnly.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;

    private static string ExpandWeekStart(string s) => s.ToLowerInvariant() switch
    {
        "mon" or "monday" => "Monday", "sun" or "sunday" => "Sunday", "tue" or "tuesday" => "Tuesday",
        "wed" or "wednesday" => "Wednesday", "thu" or "thursday" => "Thursday", "fri" or "friday" => "Friday",
        "sat" or "saturday" => "Saturday", _ => "Monday",
    };
}
```

- [ ] **Step 4: Run to verify they pass**

Run: `dotnet test htmxRazor.Tests --filter DateRangePickerTagHelperTests` (4 pass), then `dotnet test htmxRazor.Tests` (whole suite) and `dotnet build htmxRazor` (0 warnings).

- [ ] **Step 5: Commit**

```bash
git add htmxRazor/Components/Forms/DateRangePickerTagHelper.cs htmxRazor.Tests/DateRangePickerTagHelperTests.cs
git commit -m "feat(date-range): DateRangePickerTagHelper (input + two hidden + range popup + presets)"
```

---

## Task 4: CSS

**Files:**
- Create: `htmxRazor/Assets/css/components/rhx-date-range-picker.css`

- [ ] **Step 1: Verify tokens, then write the stylesheet**

`grep -nE "rhx-z-dropdown|rhx-shadow-lg|rhx-color-surface-raised|rhx-color-border|rhx-color-brand-100|rhx-color-brand-500|rhx-color-brand-600|rhx-color-text-inverse|rhx-radius-md|rhx-radius-sm|rhx-space-2xs|rhx-font-weight-medium|rhx-color-danger-500" htmxRazor/Assets/css/rhx-tokens.css`. Drop fallbacks where the token exists; keep where absent.

```css
@layer rhx.components {
  .rhx-date-range-picker { position: relative; display: inline-flex; flex-direction: column; gap: var(--rhx-space-xs); font-family: var(--rhx-font-family); max-width: 22rem; }
  .rhx-date-range-picker__label { font-size: var(--rhx-font-size-sm); font-weight: var(--rhx-font-weight-medium); color: var(--rhx-color-text); }

  .rhx-date-range-picker__control { display: flex; align-items: center; border: var(--rhx-border-width) solid var(--rhx-color-border); border-radius: var(--rhx-radius-md); background: var(--rhx-color-surface); }
  .rhx-date-range-picker__control:focus-within { border-color: var(--rhx-color-brand-500); box-shadow: 0 0 0 1px var(--rhx-color-brand-500); }
  .rhx-date-range-picker__input { flex: 1; min-width: 12rem; min-height: 2.5rem; padding: var(--rhx-space-sm) var(--rhx-space-md); border: none; background: transparent; color: var(--rhx-color-text); font: inherit; outline: none; }
  .rhx-date-range-picker__input::placeholder { color: var(--rhx-color-text-muted); }
  .rhx-date-range-picker__trigger { display: inline-flex; align-items: center; justify-content: center; width: 2.5rem; align-self: stretch; border: none; background: none; color: var(--rhx-color-text-muted); cursor: pointer; }
  .rhx-date-range-picker__trigger:hover:not([disabled]) { color: var(--rhx-color-brand-600); }

  .rhx-date-range-picker__popup { position: absolute; top: calc(100% + var(--rhx-space-2xs, 0.25rem)); left: 0; z-index: var(--rhx-z-dropdown); border: var(--rhx-border-width) solid var(--rhx-color-border); border-radius: var(--rhx-radius-md); background: var(--rhx-color-surface-raised); box-shadow: var(--rhx-shadow-lg); }
  .rhx-date-range-picker__popup[hidden] { display: none; }
  .rhx-date-range-picker__body { display: flex; align-items: stretch; }

  .rhx-date-range-picker__presets { display: flex; flex-direction: column; gap: var(--rhx-space-2xs, 0.25rem); padding: var(--rhx-space-sm); border-right: var(--rhx-border-width) solid var(--rhx-color-border); min-width: 8rem; }
  .rhx-date-range-picker__preset { text-align: left; border: none; background: none; border-radius: var(--rhx-radius-sm); padding: var(--rhx-space-xs) var(--rhx-space-sm); color: var(--rhx-color-text); font: inherit; cursor: pointer; white-space: nowrap; }
  .rhx-date-range-picker__preset:hover { background: var(--rhx-color-brand-100); }

  .rhx-date-range-picker__cal-header { display: flex; align-items: center; justify-content: space-between; padding: var(--rhx-space-xs) var(--rhx-space-sm); border-bottom: var(--rhx-border-width) solid var(--rhx-color-border); }
  .rhx-date-range-picker__cal-caption { font-weight: var(--rhx-font-weight-semibold); color: var(--rhx-color-text); flex: 1; text-align: center; }
  .rhx-date-range-picker__months { display: flex; }
  .rhx-date-range-picker__month { padding: var(--rhx-space-sm); }
  .rhx-date-range-picker__month + .rhx-date-range-picker__month { border-left: var(--rhx-border-width) solid var(--rhx-color-border); }

  /* Range highlight — applied client-side to .rhx-calendar__day cells inside this picker */
  .rhx-date-range-picker .rhx-calendar__day--in-range { background: var(--rhx-color-brand-100); border-radius: 0; }
  .rhx-date-range-picker .rhx-calendar__day--range-start,
  .rhx-date-range-picker .rhx-calendar__day--range-end { background: var(--rhx-color-brand-500); color: var(--rhx-color-text-inverse); font-weight: var(--rhx-font-weight-semibold); }
  .rhx-date-range-picker .rhx-calendar__day--range-start { border-radius: var(--rhx-radius-sm) 0 0 var(--rhx-radius-sm); }
  .rhx-date-range-picker .rhx-calendar__day--range-end { border-radius: 0 var(--rhx-radius-sm) var(--rhx-radius-sm) 0; }

  .rhx-date-range-picker--small .rhx-date-range-picker__input { min-height: 2rem; font-size: var(--rhx-font-size-sm); }
  .rhx-date-range-picker--large .rhx-date-range-picker__input { min-height: 3rem; font-size: var(--rhx-font-size-md); }
  .rhx-date-range-picker--disabled { opacity: 0.6; pointer-events: none; }
  .rhx-date-range-picker--readonly .rhx-date-range-picker__trigger,
  .rhx-date-range-picker--readonly .rhx-date-range-picker__input { cursor: default; }
  .rhx-date-range-picker--error .rhx-date-range-picker__control { border-color: var(--rhx-color-danger-500); }

  @media (max-width: 36rem) {
    .rhx-date-range-picker__months { flex-direction: column; }
    .rhx-date-range-picker__month + .rhx-date-range-picker__month { border-left: none; border-top: var(--rhx-border-width) solid var(--rhx-color-border); }
  }
}
```

- [ ] **Step 2: Build + commit**

Run: `dotnet build htmxRazor` (0 warnings).
```bash
git add htmxRazor/Assets/css/components/rhx-date-range-picker.css
git commit -m "feat(date-range): stylesheet (dual-month layout, range highlight, presets)"
```

---

## Task 5: JS — selection state machine, hover preview, presets, highlight

**Files:**
- Create: `htmxRazor/Assets/js/components/rhx-daterangepicker.js`

Read `htmxRazor/Assets/js/components/rhx-datepicker.js` first.

- [ ] **Step 1: Write the script**

```js
/**
 * htmxRazor Date Range Picker
 * Two side-by-side months. First day click sets the range start; the second sets the end (swapped
 * if earlier). While picking, hovering a day shows a live in-range preview. Presets set both dates.
 * Range highlighting is painted entirely client-side onto the .rhx-calendar__day cells (on init,
 * select, hover, and after each htmx month swap). Commits two hidden ISO yyyy-MM-dd inputs.
 */
(function () {
  "use strict";

  var DAY = ".rhx-calendar__day:not([disabled])";

  function iso(d) {
    return d.getFullYear() + "-" + String(d.getMonth() + 1).padStart(2, "0") + "-" + String(d.getDate()).padStart(2, "0");
  }
  function parse(s) { var p = (s || "").split("-"); return p.length === 3 ? new Date(Number(p[0]), Number(p[1]) - 1, Number(p[2])) : null; }

  function initRangePickers(root) {
    root.querySelectorAll("[data-rhx-date-range-picker]").forEach(function (rp) {
      if (rp._rhxRpInit) return;
      rp._rhxRpInit = true;

      var input = rp.querySelector("[data-rhx-range-display]");
      var trigger = rp.querySelector(".rhx-date-range-picker__trigger");
      var popup = rp.querySelector(".rhx-date-range-picker__popup");
      var hiddenStart = rp.querySelector("[data-rhx-range-start]");
      var hiddenEnd = rp.querySelector("[data-rhx-range-end]");
      if (!input || !popup || !hiddenStart || !hiddenEnd) return;

      var startIso = rp.getAttribute("data-range-start") || "";
      var endIso = rp.getAttribute("data-range-end") || "";
      var selecting = false; // start picked, awaiting end

      function isOpen() { return !popup.hidden; }
      function open() {
        if (input.hasAttribute("disabled") || input.hasAttribute("readonly")) return;
        popup.hidden = false;
        input.setAttribute("aria-expanded", "true");
        if (trigger) trigger.setAttribute("aria-expanded", "true");
        if (window.RHX && typeof window.RHX.positionElement === "function") {
          window.RHX.positionElement(input.parentNode, popup, { placement: "bottom-start", distance: 4, flip: true, shift: true });
        }
        paint();
      }
      function close() {
        popup.hidden = true;
        input.setAttribute("aria-expanded", "false");
        if (trigger) trigger.setAttribute("aria-expanded", "false");
      }

      // Paint range classes onto every visible day cell from startIso/endIso (+ optional hover end).
      function paint(hoverIso) {
        var lo = startIso, hi = endIso;
        if (selecting && startIso && hoverIso) {
          lo = startIso < hoverIso ? startIso : hoverIso;
          hi = startIso < hoverIso ? hoverIso : startIso;
        } else if (startIso && endIso) {
          lo = startIso < endIso ? startIso : endIso;
          hi = startIso < endIso ? endIso : startIso;
        } else { lo = startIso; hi = startIso; }
        popup.querySelectorAll(".rhx-calendar__day").forEach(function (c) {
          c.classList.remove("rhx-calendar__day--in-range", "rhx-calendar__day--range-start", "rhx-calendar__day--range-end");
          var d = c.getAttribute("data-date");
          if (!d || !lo) return;
          if (d === lo) c.classList.add("rhx-calendar__day--range-start");
          if (hi && d === hi) c.classList.add("rhx-calendar__day--range-end");
          if (hi && d > lo && d < hi) c.classList.add("rhx-calendar__day--in-range");
        });
      }

      function commit() {
        hiddenStart.value = startIso;
        hiddenEnd.value = endIso;
        hiddenStart.dispatchEvent(new Event("change", { bubbles: true }));
        hiddenEnd.dispatchEvent(new Event("change", { bubbles: true }));
        rp.dispatchEvent(new CustomEvent("rhx:date-range-picker:change", { bubbles: true, detail: { start: startIso, end: endIso } }));
      }

      function display() {
        if (startIso && endIso) {
          input.value = dispOf(startIso) + " – " + dispOf(endIso);
        } else if (startIso) {
          input.value = dispOf(startIso) + " – …";
        } else { input.value = ""; }
      }
      // Prefer the visible cell's server-formatted data-display; fall back to a locale string.
      function dispOf(isoStr) {
        var cell = popup.querySelector('.rhx-calendar__day[data-date="' + isoStr + '"]');
        if (cell && cell.getAttribute("data-display")) return cell.getAttribute("data-display");
        var d = parse(isoStr); return d ? d.toLocaleDateString() : isoStr;
      }

      function pickDay(cell) {
        var d = cell.getAttribute("data-date");
        if (!startIso || (startIso && endIso)) {        // begin a new range
          startIso = d; endIso = ""; selecting = true;
        } else {                                         // complete the range
          if (d < startIso) { endIso = startIso; startIso = d; } else { endIso = d; }
          selecting = false;
        }
        paint(); display();
        if (!selecting) { commit(); close(); }           // range complete
      }

      function applyPreset(key) {
        var t = new Date(); t.setHours(0, 0, 0, 0);
        var s = new Date(t), e = new Date(t);
        switch (key) {
          case "today": break;
          case "yesterday": s.setDate(s.getDate() - 1); e.setDate(e.getDate() - 1); break;
          case "last7": s.setDate(s.getDate() - 6); break;
          case "last30": s.setDate(s.getDate() - 29); break;
          case "thismonth": s = new Date(t.getFullYear(), t.getMonth(), 1); e = new Date(t.getFullYear(), t.getMonth() + 1, 0); break;
          case "lastmonth": s = new Date(t.getFullYear(), t.getMonth() - 1, 1); e = new Date(t.getFullYear(), t.getMonth(), 0); break;
          default: return;
        }
        startIso = iso(s); endIso = iso(e); selecting = false;
        paint(); display(); commit(); close();
      }

      if (trigger) trigger.addEventListener("click", function () { isOpen() ? close() : open(); });
      input.addEventListener("focus", function () { if (!isOpen()) open(); });

      popup.addEventListener("click", function (e) {
        var day = e.target.closest(DAY);
        if (day && popup.contains(day)) { pickDay(day); return; }
        var preset = e.target.closest("[data-range-preset]");
        if (preset) applyPreset(preset.getAttribute("data-range-preset"));
      });

      // Live hover preview while selecting.
      popup.addEventListener("mouseover", function (e) {
        if (!selecting) return;
        var day = e.target.closest(DAY);
        if (day && popup.contains(day)) paint(day.getAttribute("data-date"));
      });

      // Re-paint after an htmx month swap (the calendar widget is replaced).
      popup.addEventListener("htmx:afterSwap", function () { paint(); });

      document.addEventListener("click", function (e) { if (isOpen() && !rp.contains(e.target)) close(); });
    });
  }

  if (window.RHX) window.RHX.register("date-range-picker", initRangePickers);
})();
```

- [ ] **Step 2: Build + commit**

Run: `dotnet build htmxRazor` (0 warnings).
```bash
git add htmxRazor/Assets/js/components/rhx-daterangepicker.js
git commit -m "feat(date-range): interaction JS (two-click select, hover preview, presets, highlight)"
```

---

## Task 6: Demo page + sidebar + asset registration

**Files:**
- Create: `htmxRazor.Demo/Pages/Docs/Components/DateRangePicker.cshtml` + `.cshtml.cs`
- Modify: `_DocsLayout.cshtml`, `_Layout.cshtml` (CSS + JS), `_SidebarNav.cshtml`

- [ ] **Step 1: Register assets in BOTH layouts** — after `rhx-date-picker.css` add `<link rel="stylesheet" href="/_rhx/css/components/rhx-date-range-picker.css" />`; after `rhx-datepicker.js` add `<script src="/_rhx/js/components/rhx-daterangepicker.js" defer></script>`. Verify both layouts.

- [ ] **Step 2: Sidebar** — in `_SidebarNav.cshtml`, after the Date Picker link:
```html
            <a href="/Docs/Components/DateRangePicker" class="docs-nav__sublink @(currentPath == "/Docs/Components/DateRangePicker" ? "docs-nav__sublink--active" : "")">Date Range Picker</a>
```

- [ ] **Step 3: PageModel `DateRangePicker.cshtml.cs`** (confirm ctor shapes against `DatePicker.cshtml.cs`):
```csharp
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using htmxRazor.Components.Navigation;
using htmxRazor.Demo.Models;

namespace htmxRazor.Demo.Pages.Docs.Components;

public class DateRangePickerModel : PageModel
{
    public List<ComponentProperty> Properties { get; } = new()
    {
        new("rhx-start-name", "string", "-", "Form field name for the hidden start date (ISO yyyy-MM-dd)"),
        new("rhx-end-name", "string", "-", "Form field name for the hidden end date (ISO yyyy-MM-dd)"),
        new("rhx-start-value", "string", "-", "Initial start date (ISO yyyy-MM-dd)"),
        new("rhx-end-value", "string", "-", "Initial end date (ISO yyyy-MM-dd)"),
        new("rhx-presets", "string", "-", "Comma list: today, yesterday, last7, last30, thismonth, lastmonth"),
        new("rhx-min", "string", "-", "Earliest selectable date (ISO yyyy-MM-dd)"),
        new("rhx-max", "string", "-", "Latest selectable date (ISO yyyy-MM-dd)"),
        new("rhx-week-start", "string", "mon", "First day of the week: mon or sun"),
        new("rhx-format", "string", "-", ".NET date format for display (default: culture short date)"),
        new("rhx-size", "string", "medium", "small, medium, large"),
        new("rhx-disabled", "bool", "false", "Disable the control"),
    };

    public string BasicCode => "<rhx-date-range-picker rhx-start-name=\"From\" rhx-end-name=\"To\"\n                       rhx-presets=\"today,last7,thismonth,last30\" rhx-placeholder=\"Pick a range…\" />";

    public void OnGet()
    {
        ViewData["Breadcrumbs"] = new List<BreadcrumbItem>
        {
            new("Home", "/"),
            new("Components", "/Docs/Components/DateRangePicker"),
            new("Date Range Picker"),
        };
    }
}
```

- [ ] **Step 4: View `DateRangePicker.cshtml`** (mirror DatePicker.cshtml; confirm tag names):
```razor
@page
@model DateRangePickerModel
@{
    Layout = "_ComponentPage";
    ViewData["Title"] = "Date Range Picker";
    ViewData["ComponentName"] = "Date Range Picker";
    ViewData["ComponentTag"] = "rhx-date-range-picker";
    ViewData["ComponentDescription"] = "A two-date range picker: two side-by-side months with synced navigation, live in-range hover preview, and quick presets. Commits two hidden ISO yyyy-MM-dd values.";
}

<section class="docs-component-section">
    <h2>Examples</h2>

    <div class="docs-example">
        <h3>Basic with presets</h3>
        <rhx-tab-group>
            <rhx-tab rhx-panel="basic-preview" rhx-active="true">Preview</rhx-tab>
            <rhx-tab rhx-panel="basic-code">Code</rhx-tab>
            <rhx-tab-panel rhx-name="basic-preview" rhx-active="true">
                <div class="docs-preview docs-preview--vertical" style="min-height: 26rem; align-items: flex-start;">
                    <rhx-date-range-picker rhx-start-name="From" rhx-end-name="To"
                                           rhx-presets="today,last7,thismonth,last30" rhx-placeholder="Pick a range…" />
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
        <li>Trigger uses <code>aria-haspopup="dialog"</code>/<code>aria-expanded</code>; the popup is <code>role="dialog"</code> with two calendar grids.</li>
        <li>Two hidden ISO <code>yyyy-MM-dd</code> inputs (start + end) for reliable model binding.</li>
        <li>First day click sets the start; the second sets the end (auto-swapped if earlier); hovering previews the range; presets set both at once.</li>
    </ul>
</section>
```

- [ ] **Step 5: Build + smoke**

`dotnet build htmxRazor.Demo`, then run and:
```
curl -s http://localhost:5217/Docs/Components/DateRangePicker | grep -o 'data-rhx-range-cal\|data-rhx-range-start\|data-rhx-range-end\|data-range-preset\|rhx-calendar__day\|November 2026' | sort | uniq -c
```
Expect: the range calendar, both hidden inputs, preset buttons, day cells, and "November 2026" (the right month) present. Use a fresh port; `pkill -f htmxRazor.Demo` after.

- [ ] **Step 6: Commit**

```bash
git add htmxRazor.Demo/Pages/Docs/Components/DateRangePicker.cshtml htmxRazor.Demo/Pages/Docs/Components/DateRangePicker.cshtml.cs htmxRazor.Demo/Pages/Shared/_DocsLayout.cshtml htmxRazor.Demo/Pages/Shared/_Layout.cshtml htmxRazor.Demo/Pages/Shared/_SidebarNav.cshtml
git commit -m "docs(date-range): demo page, sidebar entry, asset registration"
```

---

## Task 7: Playwright E2E

**Files:**
- Create: `PlaywrightTests/ComponentTests/DateRangePickerTests.cs`

- [ ] **Step 1: Write the E2E tests** (read `DatePickerTests.cs` for conventions):

```csharp
using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class DateRangePickerTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/DateRangePicker";
    private const string Scope = "#panel-basic-preview ";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Picking_two_days_commits_both_hidden_inputs_and_closes(string browserName)
    {
        var page = await OpenAsync(browserName, Path);
        var popup = page.Locator(Scope + ".rhx-date-range-picker__popup");
        await Assertions.Expect(popup).Not.ToBeVisibleAsync();

        await page.Locator(Scope + ".rhx-date-range-picker__input").ClickAsync();
        await Assertions.Expect(popup).ToBeVisibleAsync();

        var enabled = popup.Locator(".rhx-calendar__day:not(.rhx-calendar__day--muted):not([disabled])");
        await enabled.Nth(5).ClickAsync();   // start
        // still open after first pick
        await Assertions.Expect(popup).ToBeVisibleAsync();
        await enabled.Nth(9).ClickAsync();   // end -> commits + closes
        await Assertions.Expect(popup).Not.ToBeVisibleAsync();

        await Assertions.Expect(page.Locator(Scope + "[data-rhx-range-start]")).Not.ToHaveValueAsync("");
        await Assertions.Expect(page.Locator(Scope + "[data-rhx-range-end]")).Not.ToHaveValueAsync("");
        await Assertions.Expect(page.Locator(Scope + ".rhx-date-range-picker__input")).Not.ToHaveValueAsync("");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Preset_last7_sets_both_dates(string browserName)
    {
        var page = await OpenAsync(browserName, Path);
        await page.Locator(Scope + ".rhx-date-range-picker__input").ClickAsync();
        await page.Locator(Scope + "[data-range-preset='last7']").ClickAsync();
        await Assertions.Expect(page.Locator(Scope + "[data-rhx-range-start]")).Not.ToHaveValueAsync("");
        await Assertions.Expect(page.Locator(Scope + "[data-rhx-range-end]")).Not.ToHaveValueAsync("");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Two_months_render_and_nav_moves_both(string browserName)
    {
        var page = await OpenAsync(browserName, Path);
        await page.Locator(Scope + ".rhx-date-range-picker__input").ClickAsync();
        await Assertions.Expect(page.Locator(Scope + ".rhx-date-range-picker__month")).ToHaveCountAsync(2);

        var caption = page.Locator(Scope + ".rhx-date-range-picker__cal-caption").First;
        var before = await caption.TextContentAsync();
        await page.Locator(Scope + ".rhx-calendar__nav[aria-label='Next month']").ClickAsync();
        await Assertions.Expect(caption).Not.ToHaveTextAsync(before!);
    }
}
```

- [ ] **Step 2: Run on Chromium**

Run: `dotnet test PlaywrightTests --filter "FullyQualifiedName~DateRangePicker&DisplayName~chromium"`
Expected: 3 PASS. (If browsers missing: `npx -y playwright@1.60.0 install chromium`.) Investigate real failures before weakening assertions; report any bug (e.g., if the first pick closes the popup, the commit logic is wrong).

- [ ] **Step 3: Commit**

```bash
git add PlaywrightTests/ComponentTests/DateRangePickerTests.cs
git commit -m "test(date-range): Playwright E2E (two-day range, preset, dual-month nav)"
```

---

## Task 8: Docs + final verification

**Files:**
- Modify: `README.md`, `CHANGELOG.md`, `vscode-extension/snippets/htmxrazor.code-snippets`

- [ ] **Step 1: README Forms row** — add `Date Range Picker` after `Date Picker` (or after `Date & Time Picker` if present) in the Forms catalog row; match format.

- [ ] **Step 2: CHANGELOG** — append under `## [2.1.0]` → `### Added`:
```markdown
- **Date Range Picker** — `<rhx-date-range-picker>`: a two-date range control with two side-by-side months (synced prev/next via `/_rhx/calendar-range`), live in-range hover preview, and quick presets (today, last7, last30, thismonth, lastmonth, yesterday). Commits two hidden ISO `yyyy-MM-dd` inputs (`rhx-start-name`/`rhx-end-name`); first click sets the start, second sets the end (auto-swapped if earlier). Range highlighting is painted client-side over the reused calendar grids. Configurable `rhx-min`/`rhx-max`, `rhx-week-start`, `rhx-format`.
```
If a `### Components Added` Forms list exists under 2.1.0, append "Date Range Picker".

- [ ] **Step 3: VS Code snippet** — after the `"rhx-date-picker"` snippet, add:
```json
  "rhx-date-range-picker": {
    "prefix": "rhx-date-range-picker",
    "body": [
      "<rhx-date-range-picker rhx-start-name=\"${1:From}\" rhx-end-name=\"${2:To}\" rhx-presets=\"${3:today,last7,thismonth,last30}\" rhx-placeholder=\"${4:Pick a range…}\" />"
    ],
    "description": "htmxRazor date range picker (two months + presets)"
  },
```
Verify parse with the node command used in earlier milestones.

- [ ] **Step 4: Final verification (milestone gate)**
1. `dotnet build htmxRazor.sln` → 0 warnings.
2. `dotnet test htmxRazor.Tests` → all pass; report total.
3. `dotnet test PlaywrightTests --filter "FullyQualifiedName~DateRangePicker&DisplayName~chromium"` → 3 pass.

- [ ] **Step 5: Commit**

```bash
git add README.md CHANGELOG.md vscode-extension/snippets/htmxrazor.code-snippets
git commit -m "docs(date-range): README catalog, changelog, VS Code snippet"
```

---

## Self-Review (completed)

**Spec coverage (§7):** input shows `From – To` + two hidden inputs → Task 3; two side-by-side months (Layout B) + synced nav → Tasks 1–2; first click start / second end / auto-swap → Task 5; live in-range hover preview → Task 5 (`paint(hoverIso)` on `mouseover` while `selecting`); presets set both dates → Tasks 3 (render) + 5 (`applyPreset`); in-range/endpoints highlight → Tasks 4 (CSS) + 5 (paint); reuses the M1 calendar grid → Task 1 (`RenderDaysBody` internal); htmx month nav → Tasks 1–2 (`/_rhx/calendar-range`) + 5 (afterSwap repaint); §3.5 positioning → Task 5; §8 a11y (dialog + grids) → Tasks 3, 6; §10 testing → Tasks 1–3, 7; §11 docs → Tasks 6, 8. Range highlighting is client-side (documented design): server renders plain grids, JS paints — so no range-state change to M1's `CalendarOptions`/renderer. Preset display uses client `toLocaleDateString` for off-screen dates (documented minor inconsistency); calendar picks use server `data-display`.

**Placeholder scan:** Every code step is complete. Token/browser-install lines are verification commands. The single M1 change (`RenderDaysBody` → `internal`) is behavior-preserving and guarded by M1's `CalendarRendererTests` (Task 1 Step 5 re-runs them).

**Type consistency:** `CalendarRangeOptions` (Year, Month, Min, Max, WeekStart, Today, HxGetUrl, TargetId, Format) consistent across Tasks 1–3; `CalendarRangeRenderer.Render` / `CalendarRangeEndpoint.Render` signatures consistent; data attributes (`data-rhx-date-range-picker`, `data-range-start`/`-end` on the wrapper for JS seed, `data-rhx-range-start`/`-end` on the hidden inputs, `data-rhx-range-display`, `data-range-preset`, `data-rhx-range-cal`, reused `data-date`/`data-display`) match between Tag Helper (Task 3), JS (Task 5), CSS (Task 4). Range CSS classes (`rhx-calendar__day--in-range/--range-start/--range-end`) match between the JS `paint()` and the Task 4 stylesheet. Hidden values are ISO `yyyy-MM-dd`. The E2E `__cal-caption`/`__month`/preset selectors match the rendered markup.
