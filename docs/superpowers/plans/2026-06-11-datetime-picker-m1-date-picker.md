# Date/Time Picker — Milestone 1: Calendar core + endpoint + Date Picker

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ship a working `<rhx-date-picker>` — a text input + popup calendar whose month/year grid is server-rendered, with prev/next and clickable-label navigation driven by htmx against a built-in `/_rhx/calendar` endpoint (overridable per app).

**Architecture:** A pure `CalendarRenderer` produces the full calendar widget HTML (header nav + day/month/year grid + footer) from a `CalendarOptions` value; nav controls carry `hx-get` URLs (with all state as query params) that swap the whole widget (`hx-swap="outerHTML"`). A query-driven `CalendarEndpoint` wraps the renderer and is mounted at `/_rhx/calendar` by `UsehtmxRazor()`. `RhxCalendar.RenderMonth(...)` exposes the renderer for app overrides. `DatePickerTagHelper` (extends `FormControlTagHelperBase`) renders the input + trigger + hidden ISO input + initial popup; JS commits the value on day click (hidden input + display + close) and handles keyboard/Today/Clear, while htmx owns grid navigation.

**Tech Stack:** ASP.NET Core Tag Helpers (.NET 10, C#), htmx, vanilla JS, BEM CSS with `rhx-` tokens, xUnit, Playwright.

**Spec:** `docs/superpowers/specs/2026-06-11-datetime-picker-family-design.md` (§§3–4, 8–11).

**Conventions verified in the codebase (read before starting):**
- Form base: `htmxRazor/Components/Forms/FormControlTagHelperBase.cs` — `ResolveName/ResolveId/ResolveValue`, `Enc`, `ApplyWrapperAttributes`, `BuildHtmxAttributeString`, `BuildValidationAttributeString`, `ResolveLabelText`, `BuildHintHtml`, `BuildErrorHtml`, `BuildAriaDescribedBy`.
- Base helpers: `htmxRazor/Infrastructure/htmxRazorTagHelperBase.cs` — `GetElementClass`, `GetModifierClass`, `CreateCssBuilder`.
- Reference component (input + trigger + popup): `htmxRazor/Components/Forms/ComboboxTagHelper.cs`.
- Icons present: `calendar`, `clock`, `chevron-left`, `chevron-right`, `x` (`htmxRazor/Components/Imagery/IconRegistry.cs`); access via `IconRegistry.Get(name)` (returns inner SVG or null).
- Middleware: `htmxRazor/Infrastructure/ApplicationBuilderExtensions.cs` — `UsehtmxRazor` adds a `/_rhx` static-file provider; insert the dynamic `/_rhx/calendar` branch BEFORE `UseStaticFiles`.
- Assets auto-embed via `htmxRazor/htmxRazor.csproj` (`<EmbeddedResource Include="Assets/**/*" />`) — new CSS/JS need no csproj edit; demo pages link them in `_DocsLayout.cshtml` + `_Layout.cshtml`.
- Test base: `htmxRazor.Tests/TagHelperTestBase.cs` — `CreateContext`, `CreateOutput`, `CreateViewContext`, `CreateUrlHelperFactory`, `HasClass`, `GetAttribute`.
- Playwright base: `PlaywrightTests/Infrastructure/ComponentTestBase.cs` — `OpenAsync(browser, path)`, `Browsers()` member data; panels are `#panel-<name>-preview`.

---

## File Structure

| File | Responsibility |
|---|---|
| `htmxRazor/Components/Forms/Calendar/CalendarOptions.cs` | Value object: year, month, view, selected, min, max, week-start, today, hx-get url, target id, show-today/clear. Plus `CalendarView` enum. |
| `htmxRazor/Components/Forms/Calendar/CalendarRenderer.cs` | Pure: `CalendarOptions` → full calendar widget HTML (header nav + day/month/year grid + footer). No HTTP/DI. |
| `htmxRazor/Components/Forms/Calendar/CalendarEndpoint.cs` | `Render(IQueryCollection, DateOnly today)` → HTML string (parse query → `CalendarOptions` → `CalendarRenderer`). |
| `htmxRazor/Components/Forms/Calendar/RhxCalendar.cs` | Public static facade `RenderMonth(CalendarOptions)` for app overrides. |
| `htmxRazor/Infrastructure/ApplicationBuilderExtensions.cs` | Add the `/_rhx/calendar` middleware branch. |
| `htmxRazor/Components/Forms/DatePickerTagHelper.cs` | `<rhx-date-picker>` — input + trigger + hidden ISO input + initial popup. |
| `htmxRazor/Assets/css/components/rhx-calendar.css` | Shared calendar/popup styling. |
| `htmxRazor/Assets/css/components/rhx-date-picker.css` | Date-picker input + trigger styling. |
| `htmxRazor/Assets/js/components/rhx-datepicker.js` | Open/close, day select, Today/Clear, keyboard, re-init after htmx grid swaps. |
| `htmxRazor.Tests/CalendarRendererTests.cs` | Unit tests for renderer + endpoint. |
| `htmxRazor.Tests/DatePickerTagHelperTests.cs` | Unit tests for the Tag Helper. |
| `htmxRazor.Demo/Pages/Docs/Components/DatePicker.cshtml(.cs)` | Demo page. |
| `PlaywrightTests/ComponentTests/DatePickerTests.cs` | E2E. |

---

## Task 1: `CalendarOptions` + `CalendarRenderer` days view (full widget)

**Files:**
- Create: `htmxRazor/Components/Forms/Calendar/CalendarOptions.cs`
- Create: `htmxRazor/Components/Forms/Calendar/CalendarRenderer.cs`
- Test: `htmxRazor.Tests/CalendarRendererTests.cs`

- [ ] **Step 1: Write the failing test**

```csharp
using System;
using htmxRazor.Components.Forms.Calendar;
using Xunit;

namespace htmxRazor.Tests;

public class CalendarRendererTests
{
    private static CalendarOptions DaysOpts() => new()
    {
        Year = 2026, Month = 10, View = CalendarView.Days,
        Selected = new DateOnly(2026, 10, 15),
        Today = new DateOnly(2026, 10, 9),
        WeekStart = DayOfWeek.Monday,
        HxGetUrl = "/_rhx/calendar", TargetId = "dp1-cal",
    };

    [Fact]
    public void Days_Renders_Grid_With_Header_And_42_Day_Cells()
    {
        var html = CalendarRenderer.Render(DaysOpts());

        Assert.Contains("data-rhx-calendar", html);
        Assert.Contains("id=\"dp1-cal\"", html);
        Assert.Contains("role=\"grid\"", html);
        Assert.Contains("October 2026", html);
        // 6 weeks x 7 days = 42 day cells
        Assert.Equal(42, System.Text.RegularExpressions.Regex.Matches(html, "role=\"gridcell\"").Count);
    }

    [Fact]
    public void Days_Marks_Today_Selected_And_Muted()
    {
        var html = CalendarRenderer.Render(DaysOpts());

        Assert.Contains("data-date=\"2026-10-09\"", html);
        Assert.Contains("rhx-calendar__day--today", html);
        Assert.Contains("data-date=\"2026-10-15\" ", html);
        Assert.Contains("rhx-calendar__day--selected", html);
        Assert.Contains("aria-selected=\"true\"", html);
        // Sep 28 (Monday) leads the Monday-start October grid and is muted.
        Assert.Contains("data-date=\"2026-09-28\"", html);
        Assert.Contains("rhx-calendar__day--muted", html);
    }

    [Fact]
    public void Days_Weekday_Header_Starts_Monday()
    {
        var html = CalendarRenderer.Render(DaysOpts());
        var moIdx = html.IndexOf(">Mo<", StringComparison.Ordinal);
        var suIdx = html.IndexOf(">Su<", StringComparison.Ordinal);
        Assert.True(moIdx >= 0 && suIdx > moIdx, "Mo should appear before Su for Monday-start.");
    }

    [Fact]
    public void Days_Disables_Out_Of_Range_Days()
    {
        var opts = DaysOpts() with { Min = new DateOnly(2026, 10, 10), Max = new DateOnly(2026, 10, 20) };
        var html = CalendarRenderer.Render(opts);
        // Oct 5 is before min -> disabled
        var idx = html.IndexOf("data-date=\"2026-10-05\"", StringComparison.Ordinal);
        Assert.True(idx >= 0);
        var cell = html.Substring(idx, 120);
        Assert.Contains("disabled", cell);
        Assert.Contains("aria-disabled=\"true\"", cell);
    }

    [Fact]
    public void Days_Nav_Arrows_Carry_HxGet_With_PrevNext_Month()
    {
        var html = CalendarRenderer.Render(DaysOpts());
        Assert.Contains("hx-get=\"/_rhx/calendar?view=days&amp;year=2026&amp;month=9", html);  // prev
        Assert.Contains("hx-get=\"/_rhx/calendar?view=days&amp;year=2026&amp;month=11", html); // next
        Assert.Contains("hx-target=\"#dp1-cal\"", html);
        Assert.Contains("hx-swap=\"outerHTML\"", html);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test htmxRazor.Tests --filter CalendarRendererTests`
Expected: FAIL — `CalendarOptions`/`CalendarRenderer` do not exist (compile error).

- [ ] **Step 3: Write `CalendarOptions`**

```csharp
using System;

namespace htmxRazor.Components.Forms.Calendar;

/// <summary>Which calendar view to render.</summary>
public enum CalendarView { Days, Months, Years }

/// <summary>Immutable inputs for <see cref="CalendarRenderer"/>. Pure data; no HTTP/DI.</summary>
public sealed record CalendarOptions
{
    /// <summary>Displayed year.</summary>
    public int Year { get; init; }
    /// <summary>Displayed month, 1-12.</summary>
    public int Month { get; init; }
    /// <summary>Which view to render.</summary>
    public CalendarView View { get; init; } = CalendarView.Days;
    /// <summary>The committed selection (highlighted when visible).</summary>
    public DateOnly? Selected { get; init; }
    /// <summary>Earliest selectable date (inclusive).</summary>
    public DateOnly? Min { get; init; }
    /// <summary>Latest selectable date (inclusive).</summary>
    public DateOnly? Max { get; init; }
    /// <summary>First day of the week. Default Monday.</summary>
    public DayOfWeek WeekStart { get; init; } = DayOfWeek.Monday;
    /// <summary>"Today" for highlighting — injectable for deterministic tests.</summary>
    public DateOnly Today { get; init; }
    /// <summary>Base URL the nav controls call (built-in endpoint or app override).</summary>
    public string HxGetUrl { get; init; } = "/_rhx/calendar";
    /// <summary>Id of the calendar container; nav swaps it via hx-target/outerHTML.</summary>
    public string TargetId { get; init; } = "rhx-cal";
    /// <summary>Show the footer "Today" button.</summary>
    public bool ShowToday { get; init; } = true;
    /// <summary>Show the footer "Clear" button.</summary>
    public bool ShowClear { get; init; } = true;
}
```

- [ ] **Step 4: Write `CalendarRenderer` (days view + header + footer)**

```csharp
using System;
using System.Globalization;
using System.Net;
using System.Text;

namespace htmxRazor.Components.Forms.Calendar;

/// <summary>
/// Pure renderer: turns <see cref="CalendarOptions"/> into the full calendar widget HTML
/// (header navigation + grid body + footer). Navigation controls carry hx-get URLs that
/// re-request the endpoint and swap the whole widget (outerHTML).
/// </summary>
public static class CalendarRenderer
{
    public static string Render(CalendarOptions o) => o.View switch
    {
        CalendarView.Months => RenderShell(o, RenderMonthsBody(o)),
        CalendarView.Years => RenderShell(o, RenderYearsBody(o)),
        _ => RenderShell(o, RenderDaysBody(o)),
    };

    private static string Enc(string s) => WebUtility.HtmlEncode(s);

    private static string NavUrl(CalendarOptions o, CalendarView view, int year, int month)
    {
        var sb = new StringBuilder();
        sb.Append(o.HxGetUrl).Append("?view=").Append(view.ToString().ToLowerInvariant());
        sb.Append("&year=").Append(year).Append("&month=").Append(month);
        if (o.Selected is { } s) sb.Append("&selected=").Append(s.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        if (o.Min is { } mn) sb.Append("&min=").Append(mn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        if (o.Max is { } mx) sb.Append("&max=").Append(mx.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        sb.Append("&week-start=").Append(o.WeekStart.ToString().ToLowerInvariant());
        sb.Append("&id=").Append(o.TargetId);
        return Enc(sb.ToString());
    }

    private static string RenderShell(CalendarOptions o, string body)
    {
        var label = new DateOnly(o.Year, o.Month, 1).ToString("MMMM yyyy", CultureInfo.InvariantCulture);
        var cur = new DateOnly(o.Year, o.Month, 1);
        var prev = cur.AddMonths(-1);
        var next = cur.AddMonths(1);

        var sb = new StringBuilder();
        sb.Append($"<div class=\"rhx-calendar\" id=\"{Enc(o.TargetId)}\" data-rhx-calendar>");

        // Header
        sb.Append("<div class=\"rhx-calendar__header\">");
        sb.Append($"<button type=\"button\" class=\"rhx-calendar__nav\" aria-label=\"Previous month\" hx-get=\"{NavUrl(o, CalendarView.Days, prev.Year, prev.Month)}\" hx-target=\"#{Enc(o.TargetId)}\" hx-swap=\"outerHTML\">&#8249;</button>");
        sb.Append($"<button type=\"button\" class=\"rhx-calendar__label\" hx-get=\"{NavUrl(o, CalendarView.Months, o.Year, o.Month)}\" hx-target=\"#{Enc(o.TargetId)}\" hx-swap=\"outerHTML\">{Enc(label)}</button>");
        sb.Append($"<button type=\"button\" class=\"rhx-calendar__nav\" aria-label=\"Next month\" hx-get=\"{NavUrl(o, CalendarView.Days, next.Year, next.Month)}\" hx-target=\"#{Enc(o.TargetId)}\" hx-swap=\"outerHTML\">&#8250;</button>");
        sb.Append("</div>");

        // Body (grid)
        sb.Append("<div class=\"rhx-calendar__body\">").Append(body).Append("</div>");

        // Footer
        if (o.ShowToday || o.ShowClear)
        {
            sb.Append("<div class=\"rhx-calendar__footer\">");
            if (o.ShowToday) sb.Append("<button type=\"button\" class=\"rhx-calendar__action\" data-rhx-cal-today>Today</button>");
            if (o.ShowClear) sb.Append("<button type=\"button\" class=\"rhx-calendar__action\" data-rhx-cal-clear>Clear</button>");
            sb.Append("</div>");
        }

        sb.Append("</div>");
        return sb.ToString();
    }

    private static readonly string[] DayAbbrev = { "Su", "Mo", "Tu", "We", "Th", "Fr", "Sa" }; // index by (int)DayOfWeek

    private static string RenderDaysBody(CalendarOptions o)
    {
        var first = new DateOnly(o.Year, o.Month, 1);
        var label = first.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
        var offset = ((int)first.DayOfWeek - (int)o.WeekStart + 7) % 7;
        var gridStart = first.AddDays(-offset);

        // Roving-tabindex focus target: selected (if this month) else today (if this month) else the 1st.
        DateOnly focus = o.Selected is { } s && s.Year == o.Year && s.Month == o.Month ? s
            : (o.Today.Year == o.Year && o.Today.Month == o.Month ? o.Today : first);

        var sb = new StringBuilder();
        sb.Append($"<div class=\"rhx-calendar__grid\" role=\"grid\" aria-label=\"{Enc(label)}\" data-rhx-calendar-grid data-year=\"{o.Year}\" data-month=\"{o.Month}\">");

        // Weekday header row
        sb.Append("<div class=\"rhx-calendar__weekdays\" role=\"row\">");
        for (var i = 0; i < 7; i++)
        {
            var dow = (DayOfWeek)(((int)o.WeekStart + i) % 7);
            sb.Append($"<span class=\"rhx-calendar__weekday\" role=\"columnheader\" aria-label=\"{dow}\">{DayAbbrev[(int)dow]}</span>");
        }
        sb.Append("</div>");

        // 6 weeks
        for (var w = 0; w < 6; w++)
        {
            sb.Append("<div class=\"rhx-calendar__week\" role=\"row\">");
            for (var d = 0; d < 7; d++)
            {
                var date = gridStart.AddDays(w * 7 + d);
                var iso = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                var muted = date.Month != o.Month;
                var isToday = date == o.Today;
                var isSelected = o.Selected is { } sel && date == sel;
                var disabled = (o.Min is { } mn && date < mn) || (o.Max is { } mx && date > mx);

                var cls = "rhx-calendar__day";
                if (muted) cls += " rhx-calendar__day--muted";
                if (isToday) cls += " rhx-calendar__day--today";
                if (isSelected) cls += " rhx-calendar__day--selected";

                sb.Append($"<button type=\"button\" class=\"{cls}\" role=\"gridcell\" data-date=\"{iso}\" ");
                sb.Append(isSelected ? "aria-selected=\"true\" " : "");
                sb.Append(date == focus ? "tabindex=\"0\"" : "tabindex=\"-1\"");
                if (disabled) sb.Append(" disabled aria-disabled=\"true\"");
                sb.Append('>').Append(date.Day).Append("</button>");
            }
            sb.Append("</div>");
        }

        sb.Append("</div>");
        return sb.ToString();
    }

    // Implemented in Task 2.
    private static string RenderMonthsBody(CalendarOptions o) => "";
    private static string RenderYearsBody(CalendarOptions o) => "";
}
```

- [ ] **Step 5: Run tests to verify they pass**

Run: `dotnet test htmxRazor.Tests --filter CalendarRendererTests`
Expected: PASS (5 tests). Note `DaysOpts() with { ... }` requires `CalendarOptions` to be a `record` (it is).

- [ ] **Step 6: Commit**

```bash
git add htmxRazor/Components/Forms/Calendar/CalendarOptions.cs htmxRazor/Components/Forms/Calendar/CalendarRenderer.cs htmxRazor.Tests/CalendarRendererTests.cs
git commit -m "feat(date-picker): CalendarRenderer days view + nav (CalendarOptions)"
```

---

## Task 2: Months & years views (clickable label)

**Files:**
- Modify: `htmxRazor/Components/Forms/Calendar/CalendarRenderer.cs` (replace the two stub methods)
- Test: `htmxRazor.Tests/CalendarRendererTests.cs`

- [ ] **Step 1: Write the failing tests**

```csharp
[Fact]
public void Months_View_Renders_12_Month_Buttons_With_HxGet_To_Days()
{
    var o = DaysOpts() with { View = CalendarView.Months };
    var html = CalendarRenderer.Render(o);
    Assert.Equal(12, System.Text.RegularExpressions.Regex.Matches(html, "rhx-calendar__month-cell").Count);
    Assert.Contains(">Jan<", html);
    Assert.Contains(">Dec<", html);
    // Clicking a month navigates to that month's days view.
    Assert.Contains("hx-get=\"/_rhx/calendar?view=days&amp;year=2026&amp;month=10", html);
    Assert.Contains("rhx-calendar__month-cell--selected", html); // current month (October)
}

[Fact]
public void Years_View_Renders_Decade_Of_Year_Buttons()
{
    var o = DaysOpts() with { View = CalendarView.Years };
    var html = CalendarRenderer.Render(o);
    // 2020..2031 decade window contains the current year, marked selected.
    Assert.Contains(">2026<", html);
    Assert.Contains("rhx-calendar__year-cell--selected", html);
    Assert.Contains("hx-get=\"/_rhx/calendar?view=months&amp;year=2026", html);
}
```

- [ ] **Step 2: Run to verify they fail**

Run: `dotnet test htmxRazor.Tests --filter CalendarRendererTests`
Expected: FAIL — months/years bodies are empty stubs.

- [ ] **Step 3: Replace the two stub methods**

```csharp
private static readonly string[] MonthAbbrev =
    { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

private static string RenderMonthsBody(CalendarOptions o)
{
    var sb = new StringBuilder();
    sb.Append("<div class=\"rhx-calendar__months\" role=\"grid\" aria-label=\"Select month\">");
    for (var m = 1; m <= 12; m++)
    {
        var cls = "rhx-calendar__month-cell";
        if (m == o.Month) cls += " rhx-calendar__month-cell--selected";
        sb.Append($"<button type=\"button\" class=\"{cls}\" role=\"gridcell\" hx-get=\"{NavUrl(o, CalendarView.Days, o.Year, m)}\" hx-target=\"#{Enc(o.TargetId)}\" hx-swap=\"outerHTML\">{MonthAbbrev[m - 1]}</button>");
    }
    sb.Append("</div>");
    return sb.ToString();
}

private static string RenderYearsBody(CalendarOptions o)
{
    var start = o.Year - (o.Year % 10) - 1; // decade window with one leading/trailing year
    var sb = new StringBuilder();
    sb.Append("<div class=\"rhx-calendar__years\" role=\"grid\" aria-label=\"Select year\">");
    for (var i = 0; i < 12; i++)
    {
        var y = start + i;
        var cls = "rhx-calendar__year-cell";
        if (y == o.Year) cls += " rhx-calendar__year-cell--selected";
        sb.Append($"<button type=\"button\" class=\"{cls}\" role=\"gridcell\" hx-get=\"{NavUrl(o, CalendarView.Months, y, o.Month)}\" hx-target=\"#{Enc(o.TargetId)}\" hx-swap=\"outerHTML\">{y}</button>");
    }
    sb.Append("</div>");
    return sb.ToString();
}
```

- [ ] **Step 4: Run to verify they pass**

Run: `dotnet test htmxRazor.Tests --filter CalendarRendererTests`
Expected: PASS (7 tests).

- [ ] **Step 5: Commit**

```bash
git add htmxRazor/Components/Forms/Calendar/CalendarRenderer.cs htmxRazor.Tests/CalendarRendererTests.cs
git commit -m "feat(date-picker): month + year calendar views"
```

---

## Task 3: `CalendarEndpoint` + mount at `/_rhx/calendar`

**Files:**
- Create: `htmxRazor/Components/Forms/Calendar/CalendarEndpoint.cs`
- Modify: `htmxRazor/Infrastructure/ApplicationBuilderExtensions.cs`
- Test: `htmxRazor.Tests/CalendarRendererTests.cs`

- [ ] **Step 1: Write the failing test**

```csharp
[Fact]
public void Endpoint_Parses_Query_And_Renders_Days_By_Default()
{
    var q = new Microsoft.AspNetCore.Http.QueryCollection(
        new System.Collections.Generic.Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            ["year"] = "2026", ["month"] = "10", ["selected"] = "2026-10-15",
            ["min"] = "2026-10-01", ["max"] = "2026-10-31",
            ["week-start"] = "monday", ["id"] = "dp1-cal",
        });

    var html = CalendarEndpoint.Render(q, today: new DateOnly(2026, 10, 9));

    Assert.Contains("id=\"dp1-cal\"", html);
    Assert.Contains("October 2026", html);
    Assert.Contains("data-date=\"2026-10-15\" aria-selected=\"true\"", html);
}

[Fact]
public void Endpoint_Defaults_To_Today_Month_When_Year_Month_Missing()
{
    var q = new Microsoft.AspNetCore.Http.QueryCollection(
        new System.Collections.Generic.Dictionary<string, Microsoft.Extensions.Primitives.StringValues>());
    var html = CalendarEndpoint.Render(q, today: new DateOnly(2026, 3, 4));
    Assert.Contains("March 2026", html);
}
```

- [ ] **Step 2: Run to verify it fails**

Run: `dotnet test htmxRazor.Tests --filter "Endpoint_Parses_Query_And_Renders_Days_By_Default"`
Expected: FAIL — `CalendarEndpoint` does not exist.

- [ ] **Step 3: Write `CalendarEndpoint`**

```csharp
using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace htmxRazor.Components.Forms.Calendar;

/// <summary>Translates a query string into a <see cref="CalendarOptions"/> and renders it.</summary>
public static class CalendarEndpoint
{
    public static string Render(IQueryCollection q, DateOnly today)
    {
        var year = ParseInt(q["year"], today.Year);
        var month = ParseInt(q["month"], today.Month);
        if (month < 1 || month > 12) month = today.Month;

        var view = q["view"].ToString().ToLowerInvariant() switch
        {
            "months" => CalendarView.Months,
            "years" => CalendarView.Years,
            _ => CalendarView.Days,
        };

        var weekStart = Enum.TryParse<DayOfWeek>(q["week-start"], ignoreCase: true, out var ws)
            ? ws : DayOfWeek.Monday;

        var opts = new CalendarOptions
        {
            Year = year,
            Month = month,
            View = view,
            Selected = ParseDate(q["selected"]),
            Min = ParseDate(q["min"]),
            Max = ParseDate(q["max"]),
            WeekStart = weekStart,
            Today = today,
            TargetId = string.IsNullOrWhiteSpace(q["id"]) ? "rhx-cal" : q["id"].ToString(),
        };
        return CalendarRenderer.Render(opts);
    }

    private static int ParseInt(string? s, int fallback) =>
        int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : fallback;

    private static DateOnly? ParseDate(string? s) =>
        DateOnly.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
            ? d : null;
}
```

- [ ] **Step 4: Run to verify it passes**

Run: `dotnet test htmxRazor.Tests --filter "Endpoint_"`
Expected: PASS.

- [ ] **Step 5: Mount the endpoint in `UsehtmxRazor`**

In `htmxRazor/Infrastructure/ApplicationBuilderExtensions.cs`, add `using htmxRazor.Components.Forms.Calendar;` at the top, and insert this BEFORE the `app.UseStaticFiles(...)` call:

```csharp
// Dynamic calendar grid endpoint (must precede the static-file provider for /_rhx).
app.Use(async (context, next) =>
{
    if (context.Request.Path.Equals("/_rhx/calendar", StringComparison.OrdinalIgnoreCase)
        && HttpMethods.IsGet(context.Request.Method))
    {
        var html = CalendarEndpoint.Render(context.Request.Query, DateOnly.FromDateTime(DateTime.Today));
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.WriteAsync(html);
        return;
    }
    await next();
});
```

Add `using System;` if not already present.

- [ ] **Step 6: Build to confirm wiring**

Run: `dotnet build htmxRazor`
Expected: build succeeds (TreatWarningsAsErrors).

- [ ] **Step 7: Commit**

```bash
git add htmxRazor/Components/Forms/Calendar/CalendarEndpoint.cs htmxRazor/Infrastructure/ApplicationBuilderExtensions.cs htmxRazor.Tests/CalendarRendererTests.cs
git commit -m "feat(date-picker): /_rhx/calendar endpoint + query parsing"
```

---

## Task 4: `RhxCalendar.RenderMonth` public helper (app overrides)

**Files:**
- Create: `htmxRazor/Components/Forms/Calendar/RhxCalendar.cs`
- Test: `htmxRazor.Tests/CalendarRendererTests.cs`

- [ ] **Step 1: Write the failing test**

```csharp
[Fact]
public void RhxCalendar_RenderMonth_Produces_Same_Grid_As_Renderer()
{
    var opts = DaysOpts();
    Assert.Equal(CalendarRenderer.Render(opts), htmxRazor.Components.Forms.Calendar.RhxCalendar.RenderMonth(opts));
}
```

- [ ] **Step 2: Run to verify it fails**

Run: `dotnet test htmxRazor.Tests --filter RhxCalendar_RenderMonth`
Expected: FAIL — `RhxCalendar` does not exist.

- [ ] **Step 3: Write `RhxCalendar`**

```csharp
namespace htmxRazor.Components.Forms.Calendar;

/// <summary>
/// Public facade for rendering a calendar grid from app code. Use this in a custom
/// page handler when overriding <c>hx-get</c> on a date picker to inject app-specific
/// data (availability, disabled dates) while returning the standard grid markup.
/// </summary>
public static class RhxCalendar
{
    /// <summary>Renders the full calendar widget HTML for the given options.</summary>
    public static string RenderMonth(CalendarOptions options) => CalendarRenderer.Render(options);
}
```

- [ ] **Step 4: Run to verify it passes**

Run: `dotnet test htmxRazor.Tests --filter RhxCalendar_RenderMonth`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add htmxRazor/Components/Forms/Calendar/RhxCalendar.cs htmxRazor.Tests/CalendarRendererTests.cs
git commit -m "feat(date-picker): public RhxCalendar.RenderMonth helper for overrides"
```

---

## Task 5: `DatePickerTagHelper`

**Files:**
- Create: `htmxRazor/Components/Forms/DatePickerTagHelper.cs`
- Test: `htmxRazor.Tests/DatePickerTagHelperTests.cs`

- [ ] **Step 1: Write the failing tests**

```csharp
using System;
using System.Globalization;
using htmxRazor.Components.Forms;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Xunit;

namespace htmxRazor.Tests;

public class DatePickerTagHelperTests : TagHelperTestBase
{
    private DatePickerTagHelper CreateHelper() => new(CreateUrlHelperFactory())
    {
        ViewContext = CreateViewContext(),
        Today = new DateOnly(2026, 10, 9),
    };

    [Fact]
    public async Task Renders_Wrapper_Input_And_Hidden_Iso_Value()
    {
        var helper = CreateHelper();
        helper.Name = "DueDate";
        helper.Value = "2026-10-15";
        var ctx = CreateContext("rhx-date-picker");
        var output = CreateOutput("rhx-date-picker");

        await helper.ProcessAsync(ctx, output);

        Assert.Equal("div", output.TagName);
        Assert.True(HasClass(output, "rhx-date-picker"));
        var html = output.Content.GetContent();
        Assert.Contains("data-rhx-date-picker", GetAttribute(output, "class") is null ? "" : "");
        Assert.Contains("rhx-date-picker__input", html);
        Assert.Contains("type=\"hidden\"", html);
        Assert.Contains("name=\"DueDate\"", html);
        Assert.Contains("value=\"2026-10-15\"", html);   // hidden ISO value
        Assert.Contains("data-rhx-date-value", html);
    }

    [Fact]
    public async Task Renders_Trigger_And_Initial_Calendar_For_Value_Month()
    {
        var helper = CreateHelper();
        helper.Name = "DueDate";
        helper.Value = "2026-10-15";
        var ctx = CreateContext("rhx-date-picker");
        var output = CreateOutput("rhx-date-picker");

        await helper.ProcessAsync(ctx, output);
        var html = output.Content.GetContent();

        Assert.Contains("rhx-date-picker__trigger", html);     // calendar icon button
        Assert.Contains("aria-haspopup=\"dialog\"", html);
        Assert.Contains("rhx-calendar", html);                 // initial grid embedded
        Assert.Contains("October 2026", html);
        Assert.Contains("data-date=\"2026-10-15\" aria-selected=\"true\"", html);
    }

    [Fact]
    public async Task Empty_Value_Shows_Today_Month_And_No_Selection()
    {
        var helper = CreateHelper();
        helper.Name = "d";
        var ctx = CreateContext("rhx-date-picker");
        var output = CreateOutput("rhx-date-picker");

        await helper.ProcessAsync(ctx, output);
        var html = output.Content.GetContent();

        Assert.Contains("October 2026", html);                 // today is 2026-10-09
        Assert.DoesNotContain("aria-selected=\"true\"", html);
        Assert.Contains("value=\"\"", html);                   // empty hidden value
    }

    [Fact]
    public async Task WeekStart_And_MinMax_Flow_Into_Calendar()
    {
        var helper = CreateHelper();
        helper.Name = "d";
        helper.Value = "2026-10-15";
        helper.Min = "2026-10-10";
        var ctx = CreateContext("rhx-date-picker");
        var output = CreateOutput("rhx-date-picker");

        await helper.ProcessAsync(ctx, output);
        var html = output.Content.GetContent();

        var idx = html.IndexOf("data-date=\"2026-10-05\"", StringComparison.Ordinal);
        Assert.True(idx >= 0);
        Assert.Contains("disabled", html.Substring(idx, 120)); // before min
    }
}
```

- [ ] **Step 2: Run to verify they fail**

Run: `dotnet test htmxRazor.Tests --filter DatePickerTagHelperTests`
Expected: FAIL — `DatePickerTagHelper` does not exist.

- [ ] **Step 3: Write `DatePickerTagHelper`**

```csharp
using System;
using System.Globalization;
using System.Text;
using htmxRazor.Components.Forms.Calendar;
using htmxRazor.Components.Imagery;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Forms;

/// <summary>
/// A date input with a popup calendar. The month grid is server-rendered; prev/next and the
/// clickable month/year label navigate via htmx against <c>/_rhx/calendar</c> (overridable).
/// Day selection is committed client-side to a hidden ISO (yyyy-MM-dd) input for form binding.
/// </summary>
/// <example>
/// <code>
/// &lt;rhx-date-picker rhx-for="DueDate" rhx-placeholder="Pick a date…"
///                  rhx-min="2026-01-01" rhx-max="2026-12-31" rhx-week-start="mon" /&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-date-picker")]
public class DatePickerTagHelper : FormControlTagHelperBase
{
    protected override string BlockName => "date-picker";

    [HtmlAttributeName("rhx-placeholder")] public string? Placeholder { get; set; }
    [HtmlAttributeName("rhx-min")] public string? Min { get; set; }
    [HtmlAttributeName("rhx-max")] public string? Max { get; set; }
    [HtmlAttributeName("rhx-week-start")] public string WeekStartName { get; set; } = "mon";
    [HtmlAttributeName("rhx-format")] public string? Format { get; set; }
    [HtmlAttributeName("rhx-show-today")] public bool ShowToday { get; set; } = true;
    [HtmlAttributeName("rhx-show-clear")] public bool ShowClear { get; set; } = true;

    /// <summary>"Today" — injectable for deterministic tests; defaults to the system date.</summary>
    [HtmlAttributeNotBound]
    public DateOnly Today { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public DatePickerTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        await Task.CompletedTask;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var name = ResolveName();
        var id = ResolveId();
        if (string.IsNullOrEmpty(id)) id = "rhx-dp-" + context.UniqueId;
        var iso = NormalizeIso(ResolveValue());
        var selected = ParseDate(iso);
        var calId = $"{id}-cal";
        var size = Size.ToLowerInvariant();

        var css = CreateCssBuilder()
            .AddIf(GetModifierClass(size), size != "medium")
            .AddIf(GetModifierClass("disabled"), Disabled);
        ApplyWrapperAttributes(output, css);
        output.Attributes.SetAttribute("data-rhx-date-picker", "");

        var weekStart = Enum.TryParse<DayOfWeek>(ExpandWeekStart(WeekStartName), true, out var ws) ? ws : DayOfWeek.Monday;
        var view = selected ?? Today;

        var opts = new CalendarOptions
        {
            Year = view.Year,
            Month = view.Month,
            Selected = selected,
            Min = ParseDate(Min),
            Max = ParseDate(Max),
            WeekStart = weekStart,
            Today = Today,
            HxGetUrl = "/_rhx/calendar",
            TargetId = calId,
            ShowToday = ShowToday,
            ShowClear = ShowClear,
        };

        var labelId = $"{id}-label";
        var hintId = $"{id}-hint";
        var errorId = $"{id}-error";
        var inputId = $"{id}-input";
        var sb = new StringBuilder();

        // Label
        var labelText = ResolveLabelText();
        if (!string.IsNullOrEmpty(labelText))
            sb.Append($"<label class=\"{GetElementClass("label")}\" id=\"{Enc(labelId)}\" for=\"{Enc(inputId)}\">{Enc(labelText)}</label>");

        // Control: text input + trigger
        sb.Append($"<div class=\"{GetElementClass("control")}\">");
        sb.Append($"<input class=\"{GetElementClass("input")}\" id=\"{Enc(inputId)}\" type=\"text\" autocomplete=\"off\" data-rhx-date-display");
        if (!string.IsNullOrEmpty(Placeholder)) sb.Append($" placeholder=\"{Enc(Placeholder)}\"");
        if (selected is { } s) sb.Append($" value=\"{Enc(DisplayText(s))}\"");
        if (!string.IsNullOrEmpty(labelText)) sb.Append($" aria-labelledby=\"{Enc(labelId)}\"");
        if (!string.IsNullOrEmpty(AriaLabel)) sb.Append($" aria-label=\"{Enc(AriaLabel)}\"");
        var describedBy = BuildAriaDescribedBy(hintId, errorId);
        if (describedBy != null) sb.Append($" aria-describedby=\"{Enc(describedBy)}\"");
        if (Disabled) sb.Append(" disabled");
        if (Readonly) sb.Append(" readonly");
        sb.Append(" />");

        sb.Append($"<button type=\"button\" class=\"{GetElementClass("trigger")}\" aria-haspopup=\"dialog\" aria-expanded=\"false\" aria-controls=\"{Enc(calId)}\" aria-label=\"Open calendar\"");
        if (Disabled) sb.Append(" disabled");
        sb.Append('>');
        var icon = IconRegistry.Get("calendar");
        sb.Append($"<svg viewBox=\"0 0 24 24\" width=\"18\" height=\"18\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\">{icon}</svg>");
        sb.Append("</button>");
        sb.Append("</div>");

        // Hidden ISO value (form/htmx)
        sb.Append($"<input type=\"hidden\" data-rhx-date-value name=\"{Enc(name)}\" value=\"{Enc(iso ?? "")}\"");
        sb.Append(BuildValidationAttributeString());
        sb.Append(" />");

        // Popup (initial calendar)
        sb.Append($"<div class=\"{GetElementClass("popup")}\" role=\"dialog\" aria-modal=\"false\"");
        if (!string.IsNullOrEmpty(labelText)) sb.Append($" aria-labelledby=\"{Enc(labelId)}\"");
        else if (!string.IsNullOrEmpty(AriaLabel)) sb.Append($" aria-label=\"{Enc(AriaLabel)}\"");
        sb.Append(" hidden>");
        sb.Append(CalendarRenderer.Render(opts));
        sb.Append("</div>");

        // Hint + error
        sb.Append(BuildHintHtml(hintId));
        sb.Append(BuildErrorHtml(errorId));

        output.Content.SetHtmlContent(sb.ToString());
    }

    private static DateOnly? ParseDate(string? s) =>
        DateOnly.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;

    private static string? NormalizeIso(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        if (DateOnly.TryParse(raw, CultureInfo.CurrentCulture, DateTimeStyles.None, out var d)
            || DateOnly.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
            return d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return null;
    }

    private string DisplayText(DateOnly d) =>
        string.IsNullOrEmpty(Format)
            ? d.ToString("d", CultureInfo.CurrentCulture)
            : d.ToString(Format, CultureInfo.CurrentCulture);

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

> Note: the test asserts `value="2026-10-15"` for both the hidden input and (indirectly) the calendar. `NormalizeIso` keeps an already-ISO value as-is. `DisplayText` uses culture short-date for the visible input; tests assert the hidden ISO + the embedded calendar, not the localized display, to stay culture-independent.

- [ ] **Step 4: Run to verify they pass**

Run: `dotnet test htmxRazor.Tests --filter DatePickerTagHelperTests`
Expected: PASS (4 tests).

- [ ] **Step 5: Run the whole unit suite + build**

Run: `dotnet build htmxRazor && dotnet test htmxRazor.Tests`
Expected: build OK, all tests pass.

- [ ] **Step 6: Commit**

```bash
git add htmxRazor/Components/Forms/DatePickerTagHelper.cs htmxRazor.Tests/DatePickerTagHelperTests.cs
git commit -m "feat(date-picker): DatePickerTagHelper (input + trigger + popup calendar)"
```

---

## Task 6: CSS — calendar + date-picker

**Files:**
- Create: `htmxRazor/Assets/css/components/rhx-calendar.css`
- Create: `htmxRazor/Assets/css/components/rhx-date-picker.css`

- [ ] **Step 1: Write `rhx-calendar.css`**

```css
@layer rhx.components {
  .rhx-calendar { width: 17.5rem; font-family: var(--rhx-font-family); color: var(--rhx-color-text); }

  .rhx-calendar__header { display: flex; align-items: center; justify-content: space-between; padding: var(--rhx-space-xs) var(--rhx-space-sm); border-bottom: var(--rhx-border-width) solid var(--rhx-color-border); }
  .rhx-calendar__nav { width: 1.75rem; height: 1.75rem; border: none; border-radius: var(--rhx-radius-sm); background: var(--rhx-color-surface-raised); color: var(--rhx-color-text); cursor: pointer; font-size: var(--rhx-font-size-md); line-height: 1; }
  .rhx-calendar__nav:hover { background: var(--rhx-color-brand-100); }
  .rhx-calendar__label { border: none; background: none; font: inherit; font-weight: var(--rhx-font-weight-semibold); color: var(--rhx-color-text); cursor: pointer; padding: var(--rhx-space-2xs, 0.25rem) var(--rhx-space-sm); border-radius: var(--rhx-radius-sm); }
  .rhx-calendar__label:hover { background: var(--rhx-color-surface-raised); }
  .rhx-calendar__nav:focus-visible, .rhx-calendar__label:focus-visible { outline: 2px solid var(--rhx-color-focus-ring); outline-offset: 1px; }

  .rhx-calendar__body { padding: var(--rhx-space-sm); }
  .rhx-calendar__weekdays, .rhx-calendar__week { display: grid; grid-template-columns: repeat(7, 1fr); }
  .rhx-calendar__weekday { text-align: center; font-size: var(--rhx-font-size-xs, 0.75rem); font-weight: var(--rhx-font-weight-semibold); color: var(--rhx-color-text-muted); padding: var(--rhx-space-2xs, 0.25rem) 0; }

  .rhx-calendar__day { aspect-ratio: 1; border: none; background: none; border-radius: var(--rhx-radius-sm); color: var(--rhx-color-text); font: inherit; cursor: pointer; }
  .rhx-calendar__day:hover:not([disabled]) { background: var(--rhx-color-brand-100); }
  .rhx-calendar__day:focus-visible { outline: 2px solid var(--rhx-color-focus-ring); outline-offset: -2px; }
  .rhx-calendar__day--muted { color: var(--rhx-color-text-muted); }
  .rhx-calendar__day--today { box-shadow: inset 0 0 0 1.5px var(--rhx-color-brand-500); font-weight: var(--rhx-font-weight-semibold); }
  .rhx-calendar__day--selected { background: var(--rhx-color-brand-500); color: var(--rhx-color-text-inverse); font-weight: var(--rhx-font-weight-semibold); }
  .rhx-calendar__day--selected:hover { background: var(--rhx-color-brand-600); }
  .rhx-calendar__day[disabled] { color: var(--rhx-color-text-muted); opacity: 0.4; cursor: not-allowed; }

  .rhx-calendar__months, .rhx-calendar__years { display: grid; grid-template-columns: repeat(3, 1fr); gap: var(--rhx-space-2xs, 0.25rem); }
  .rhx-calendar__month-cell, .rhx-calendar__year-cell { border: none; background: none; border-radius: var(--rhx-radius-sm); padding: var(--rhx-space-sm) 0; color: var(--rhx-color-text); font: inherit; cursor: pointer; }
  .rhx-calendar__month-cell:hover, .rhx-calendar__year-cell:hover { background: var(--rhx-color-brand-100); }
  .rhx-calendar__month-cell--selected, .rhx-calendar__year-cell--selected { background: var(--rhx-color-brand-500); color: var(--rhx-color-text-inverse); }

  .rhx-calendar__footer { display: flex; justify-content: space-between; padding: var(--rhx-space-xs) var(--rhx-space-sm); border-top: var(--rhx-border-width) solid var(--rhx-color-border); }
  .rhx-calendar__action { border: none; background: none; color: var(--rhx-color-brand-600); font: inherit; font-weight: var(--rhx-font-weight-medium); cursor: pointer; padding: var(--rhx-space-2xs, 0.25rem) var(--rhx-space-xs); border-radius: var(--rhx-radius-sm); }
  .rhx-calendar__action:hover { background: var(--rhx-color-brand-100); }
}
```

- [ ] **Step 2: Write `rhx-date-picker.css`**

```css
@layer rhx.components {
  .rhx-date-picker { position: relative; display: inline-flex; flex-direction: column; gap: var(--rhx-space-xs); font-family: var(--rhx-font-family); max-width: 18rem; }
  .rhx-date-picker__label { font-size: var(--rhx-font-size-sm); font-weight: var(--rhx-font-weight-medium); color: var(--rhx-color-text); }

  .rhx-date-picker__control { display: flex; align-items: center; border: var(--rhx-border-width) solid var(--rhx-color-border); border-radius: var(--rhx-radius-md); background: var(--rhx-color-surface); }
  .rhx-date-picker__control:focus-within { border-color: var(--rhx-color-brand-500); box-shadow: 0 0 0 1px var(--rhx-color-brand-500); }
  .rhx-date-picker__input { flex: 1; min-height: 2.5rem; padding: var(--rhx-space-sm) var(--rhx-space-md); border: none; background: transparent; color: var(--rhx-color-text); font: inherit; outline: none; }
  .rhx-date-picker__input::placeholder { color: var(--rhx-color-text-muted); }
  .rhx-date-picker__trigger { display: inline-flex; align-items: center; justify-content: center; width: 2.5rem; align-self: stretch; border: none; background: none; color: var(--rhx-color-text-muted); cursor: pointer; }
  .rhx-date-picker__trigger:hover:not([disabled]) { color: var(--rhx-color-brand-600); }
  .rhx-date-picker__trigger:focus-visible { outline: 2px solid var(--rhx-color-focus-ring); outline-offset: -2px; }

  .rhx-date-picker__popup { position: absolute; top: calc(100% + var(--rhx-space-2xs, 0.25rem)); left: 0; z-index: var(--rhx-z-dropdown); border: var(--rhx-border-width) solid var(--rhx-color-border); border-radius: var(--rhx-radius-md); background: var(--rhx-color-surface-raised); box-shadow: var(--rhx-shadow-lg); }
  .rhx-date-picker__popup[hidden] { display: none; }

  .rhx-date-picker--small .rhx-date-picker__input { min-height: 2rem; font-size: var(--rhx-font-size-sm); }
  .rhx-date-picker--large .rhx-date-picker__input { min-height: 3rem; font-size: var(--rhx-font-size-md); }
  .rhx-date-picker--disabled { opacity: 0.6; pointer-events: none; }
}
```

> Token check: confirm `--rhx-font-size-xs` and `--rhx-space-2xs` exist; both are used with literal fallbacks above, so absence is safe. Run `grep -nE "font-size-xs|space-2xs" htmxRazor/Assets/css/rhx-tokens.css` and drop the fallback if present.

- [ ] **Step 3: Build to confirm embedding**

Run: `dotnet build htmxRazor`
Expected: succeeds (CSS auto-embeds via `Assets/**/*`).

- [ ] **Step 4: Commit**

```bash
git add htmxRazor/Assets/css/components/rhx-calendar.css htmxRazor/Assets/css/components/rhx-date-picker.css
git commit -m "feat(date-picker): calendar + date-picker stylesheets"
```

---

## Task 7: JS — open/close, select, keyboard, htmx re-init

**Files:**
- Create: `htmxRazor/Assets/js/components/rhx-datepicker.js`

Read `htmxRazor/Assets/js/components/rhx-combobox.js` first to match the IIFE + `RHX.register` style and `htmx:afterSwap` handling.

- [ ] **Step 1: Write the script**

```js
/**
 * htmxRazor Date Picker
 * Opens a popup calendar; month/year navigation is htmx-driven (the grid swaps in).
 * Day selection is committed client-side: hidden ISO input + visible display + close.
 * Keyboard follows the APG grid pattern; Today/Clear footer actions are JS.
 */
(function () {
  "use strict";

  var DAY = ".rhx-calendar__day:not([disabled])";

  function fmtDisplay(iso) {
    // Locale-aware short date from an ISO yyyy-MM-dd.
    var p = iso.split("-");
    var d = new Date(Number(p[0]), Number(p[1]) - 1, Number(p[2]));
    return isNaN(d) ? iso : d.toLocaleDateString();
  }

  function initDatePickers(root) {
    root.querySelectorAll("[data-rhx-date-picker]").forEach(function (dp) {
      if (dp._rhxDpInit) return;
      dp._rhxDpInit = true;

      var input = dp.querySelector("[data-rhx-date-display]");
      var trigger = dp.querySelector(".rhx-date-picker__trigger");
      var popup = dp.querySelector(".rhx-date-picker__popup");
      var hidden = dp.querySelector("[data-rhx-date-value]");
      if (!popup || !hidden) return;

      function isOpen() { return !popup.hidden; }

      function open() {
        if (trigger && trigger.hasAttribute("disabled")) return;
        popup.hidden = false;
        if (trigger) trigger.setAttribute("aria-expanded", "true");
        var focusDay = popup.querySelector(".rhx-calendar__day[tabindex='0']") || popup.querySelector(DAY);
        if (focusDay) focusDay.focus();
      }

      function close(focusTrigger) {
        popup.hidden = true;
        if (trigger) trigger.setAttribute("aria-expanded", "false");
        if (focusTrigger && trigger) trigger.focus();
      }

      function commit(iso) {
        hidden.value = iso;
        if (input) input.value = iso ? fmtDisplay(iso) : "";
        hidden.dispatchEvent(new Event("input", { bubbles: true }));
        hidden.dispatchEvent(new Event("change", { bubbles: true }));
        dp.dispatchEvent(new CustomEvent("rhx:date-picker:change", { bubbles: true, detail: { value: iso } }));
      }

      // Open interactions
      if (trigger) trigger.addEventListener("click", function () { isOpen() ? close(false) : open(); });
      if (input) input.addEventListener("focus", function () { if (!isOpen()) open(); });

      // Day select / footer actions (delegated; survives htmx grid swaps)
      popup.addEventListener("click", function (e) {
        var day = e.target.closest(DAY);
        if (day && popup.contains(day)) { commit(day.getAttribute("data-date")); close(true); return; }
        if (e.target.closest("[data-rhx-cal-today]")) {
          var t = new Date();
          var iso = t.getFullYear() + "-" + String(t.getMonth() + 1).padStart(2, "0") + "-" + String(t.getDate()).padStart(2, "0");
          commit(iso); close(true); return;
        }
        if (e.target.closest("[data-rhx-cal-clear]")) { commit(""); close(true); }
      });

      // Keyboard (APG grid). Arrow-across-month-edge: let htmx handle via the nav buttons.
      popup.addEventListener("keydown", function (e) {
        var cur = popup.querySelector(".rhx-calendar__day[tabindex='0']") || document.activeElement;
        if (!cur || !cur.classList || !cur.classList.contains("rhx-calendar__day")) {
          if (e.key === "Escape") { e.preventDefault(); close(true); }
          return;
        }
        var days = Array.prototype.slice.call(popup.querySelectorAll(".rhx-calendar__day"));
        var i = days.indexOf(cur);
        var to = null;
        switch (e.key) {
          case "ArrowRight": to = i + 1; break;
          case "ArrowLeft": to = i - 1; break;
          case "ArrowDown": to = i + 7; break;
          case "ArrowUp": to = i - 7; break;
          case "Home": to = i - (i % 7); break;
          case "End": to = i - (i % 7) + 6; break;
          case "PageUp": e.preventDefault(); clickNav(".rhx-calendar__nav[aria-label='Previous month']"); return;
          case "PageDown": e.preventDefault(); clickNav(".rhx-calendar__nav[aria-label='Next month']"); return;
          case "Enter": case " ":
            e.preventDefault();
            if (!cur.hasAttribute("disabled")) { commit(cur.getAttribute("data-date")); close(true); }
            return;
          case "Escape": e.preventDefault(); close(true); return;
          default: return;
        }
        if (to != null) {
          e.preventDefault();
          if (to < 0 || to >= days.length) { clickNav(to < 0 ? ".rhx-calendar__nav[aria-label='Previous month']" : ".rhx-calendar__nav[aria-label='Next month']"); return; }
          days.forEach(function (d) { d.setAttribute("tabindex", "-1"); });
          days[to].setAttribute("tabindex", "0");
          days[to].focus();
        }
      });

      function clickNav(sel) { var b = popup.querySelector(sel); if (b) b.click(); }

      // After an htmx grid swap, move focus into the new grid for keyboard continuity.
      popup.addEventListener("htmx:afterSwap", function () {
        if (!isOpen()) return;
        var f = popup.querySelector(".rhx-calendar__day[tabindex='0']") || popup.querySelector(DAY);
        if (f) f.focus();
      });

      // Click outside
      document.addEventListener("click", function (e) { if (isOpen() && !dp.contains(e.target)) close(false); });
    });
  }

  if (window.RHX) window.RHX.register("date-picker", initDatePickers);
})();
```

- [ ] **Step 2: Build to confirm embedding**

Run: `dotnet build htmxRazor`
Expected: succeeds (JS auto-embeds).

- [ ] **Step 3: Commit**

```bash
git add htmxRazor/Assets/js/components/rhx-datepicker.js
git commit -m "feat(date-picker): interaction JS (open/close, select, keyboard, htmx re-init)"
```

---

## Task 8: Demo page + sidebar + asset registration

**Files:**
- Create: `htmxRazor.Demo/Pages/Docs/Components/DatePicker.cshtml`
- Create: `htmxRazor.Demo/Pages/Docs/Components/DatePicker.cshtml.cs`
- Modify: `htmxRazor.Demo/Pages/Shared/_DocsLayout.cshtml` (CSS + JS links)
- Modify: `htmxRazor.Demo/Pages/Shared/_Layout.cshtml` (CSS + JS links)
- Modify: `htmxRazor.Demo/Pages/Shared/_SidebarNav.cshtml` (Forms nav entry)

- [ ] **Step 1: Register the new assets**

In BOTH `_DocsLayout.cshtml` and `_Layout.cshtml`, add after the `rhx-combobox.css` link:

```html
    <link rel="stylesheet" href="/_rhx/css/components/rhx-calendar.css" />
    <link rel="stylesheet" href="/_rhx/css/components/rhx-date-picker.css" />
```

and after the `rhx-combobox.js` script:

```html
    <script src="/_rhx/js/components/rhx-datepicker.js" defer></script>
```

(`_DocsLayout.cshtml` already loads `rhx-position.js`; no change needed there.)

- [ ] **Step 2: Add the sidebar entry**

In `_SidebarNav.cshtml`, after the Combobox link in the Forms section, add:

```html
            <a href="/Docs/Components/DatePicker" class="docs-nav__sublink @(currentPath == "/Docs/Components/DatePicker" ? "docs-nav__sublink--active" : "")">Date Picker</a>
```

- [ ] **Step 3: Write the PageModel**

```csharp
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using htmxRazor.Components.Navigation;
using htmxRazor.Demo.Models;

namespace htmxRazor.Demo.Pages.Docs.Components;

public class DatePickerModel : PageModel
{
    public List<ComponentProperty> Properties { get; } = new()
    {
        new("rhx-for", "ModelExpression", "-", "Binds DateOnly/DateTime for two-way model binding"),
        new("name", "string", "-", "Form field name for the hidden ISO (yyyy-MM-dd) value"),
        new("rhx-placeholder", "string", "-", "Placeholder for the text input"),
        new("rhx-min", "string", "-", "Earliest selectable date (ISO yyyy-MM-dd)"),
        new("rhx-max", "string", "-", "Latest selectable date (ISO yyyy-MM-dd)"),
        new("rhx-week-start", "string", "mon", "First day of the week: mon or sun"),
        new("rhx-format", "string", "-", ".NET format string for the visible display (default: culture short date)"),
        new("rhx-size", "string", "medium", "small, medium, large"),
        new("rhx-disabled", "bool", "false", "Disable the control"),
    };

    public string BasicCode => "<rhx-date-picker name=\"DueDate\" rhx-placeholder=\"Pick a date…\" rhx-week-start=\"mon\" />";
    public string MinMaxCode => "<rhx-date-picker name=\"d\" rhx-min=\"2026-01-01\" rhx-max=\"2026-12-31\" />";

    public void OnGet()
    {
        ViewData["Breadcrumbs"] = new List<BreadcrumbItem>
        {
            new("Home", "/"),
            new("Components", "/Docs/Components/DatePicker"),
            new("Date Picker"),
        };
    }
}
```

- [ ] **Step 4: Write the demo view**

```razor
@page
@model DatePickerModel
@{
    Layout = "_ComponentPage";
    ViewData["Title"] = "Date Picker";
    ViewData["ComponentName"] = "Date Picker";
    ViewData["ComponentTag"] = "rhx-date-picker";
    ViewData["ComponentDescription"] = "A text input with a popup calendar. The month grid is server-rendered; prev/next and the month/year label navigate via htmx. Day selection commits a hidden ISO value.";
}

<section class="docs-component-section">
    <h2>Examples</h2>

    <div class="docs-example">
        <h3>Basic</h3>
        <rhx-tab-group>
            <rhx-tab rhx-panel="basic-preview" rhx-active="true">Preview</rhx-tab>
            <rhx-tab rhx-panel="basic-code">Code</rhx-tab>
            <rhx-tab-panel rhx-name="basic-preview" rhx-active="true">
                <div class="docs-preview docs-preview--vertical" style="min-height: 24rem; align-items: flex-start;">
                    <rhx-date-picker name="DueDate" rhx-placeholder="Pick a date…" rhx-week-start="mon" />
                </div>
            </rhx-tab-panel>
            <rhx-tab-panel rhx-name="basic-code">
                <div class="docs-code"><pre><code>@Model.BasicCode</code></pre></div>
            </rhx-tab-panel>
        </rhx-tab-group>
    </div>

    <div class="docs-example">
        <h3>Min / Max range</h3>
        <rhx-tab-group>
            <rhx-tab rhx-panel="mm-preview" rhx-active="true">Preview</rhx-tab>
            <rhx-tab rhx-panel="mm-code">Code</rhx-tab>
            <rhx-tab-panel rhx-name="mm-preview" rhx-active="true">
                <div class="docs-preview docs-preview--vertical" style="min-height: 24rem; align-items: flex-start;">
                    <rhx-date-picker name="d" rhx-min="2026-01-01" rhx-max="2026-12-31" rhx-placeholder="2026 only" />
                </div>
            </rhx-tab-panel>
            <rhx-tab-panel rhx-name="mm-code">
                <div class="docs-code"><pre><code>@Model.MinMaxCode</code></pre></div>
            </rhx-tab-panel>
        </rhx-tab-group>
    </div>
</section>

@await Html.PartialAsync("_PropertyTable", Model.Properties)

<section class="docs-component-section">
    <h2>Accessibility</h2>
    <ul class="docs-a11y-list">
        <li>Trigger uses <code>aria-haspopup="dialog"</code>/<code>aria-expanded</code>; the popup is <code>role="dialog"</code>.</li>
        <li>The grid uses <code>role="grid"</code> with <code>gridcell</code> days and roving <code>tabindex</code>; arrows move by day/week, PageUp/Down by month, Enter selects, Escape closes.</li>
        <li>The hidden value is ISO <code>yyyy-MM-dd</code> for reliable model binding; the visible input shows the culture short date.</li>
    </ul>
</section>
```

- [ ] **Step 5: Build the demo + manual smoke**

Run: `dotnet build htmxRazor.Demo` then `dotnet run --project htmxRazor.Demo` and open `/Docs/Components/DatePicker`.
Expected: clicking the field opens the calendar; prev/next swaps the month (htmx); clicking the month/year label opens month then year views; picking a day fills the input and closes; Today/Clear work.

- [ ] **Step 6: Commit**

```bash
git add htmxRazor.Demo/Pages/Docs/Components/DatePicker.cshtml htmxRazor.Demo/Pages/Docs/Components/DatePicker.cshtml.cs htmxRazor.Demo/Pages/Shared/_DocsLayout.cshtml htmxRazor.Demo/Pages/Shared/_Layout.cshtml htmxRazor.Demo/Pages/Shared/_SidebarNav.cshtml
git commit -m "docs(date-picker): demo page, sidebar entry, asset registration"
```

---

## Task 9: Playwright E2E

**Files:**
- Create: `PlaywrightTests/ComponentTests/DatePickerTests.cs`

Read an existing test (e.g. `PlaywrightTests/ComponentTests/SwitchTests.cs`) for fixture conventions.

- [ ] **Step 1: Write the E2E tests**

```csharp
using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class DatePickerTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/DatePicker";
    private const string Scope = "#panel-basic-preview ";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Opening_shows_calendar_and_picking_a_day_fills_input(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var input = page.Locator(Scope + ".rhx-date-picker__input");
        var popup = page.Locator(Scope + ".rhx-date-picker__popup");
        await Assertions.Expect(popup).Not.ToBeVisibleAsync();

        await page.Locator(Scope + ".rhx-date-picker__trigger").ClickAsync();
        await Assertions.Expect(popup).ToBeVisibleAsync();

        await popup.Locator(".rhx-calendar__day:not(.rhx-calendar__day--muted):not([disabled])").First.ClickAsync();
        await Assertions.Expect(popup).Not.ToBeVisibleAsync();
        await Assertions.Expect(input).Not.ToHaveValueAsync("");
        await Assertions.Expect(page.Locator(Scope + "[data-rhx-date-value]")).Not.ToHaveValueAsync("");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Next_month_navigation_swaps_grid_via_htmx(string browserName)
    {
        var page = await OpenAsync(browserName, Path);
        await page.Locator(Scope + ".rhx-date-picker__trigger").ClickAsync();

        var grid = page.Locator(Scope + ".rhx-calendar__grid");
        var before = await grid.GetAttributeAsync("aria-label");
        await page.Locator(Scope + ".rhx-calendar__nav[aria-label='Next month']").ClickAsync();
        await Assertions.Expect(grid).Not.ToHaveAttributeAsync("aria-label", before!);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Label_opens_month_then_year_views(string browserName)
    {
        var page = await OpenAsync(browserName, Path);
        await page.Locator(Scope + ".rhx-date-picker__trigger").ClickAsync();

        await page.Locator(Scope + ".rhx-calendar__label").ClickAsync();
        await Assertions.Expect(page.Locator(Scope + ".rhx-calendar__months")).ToBeVisibleAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_days_outside_min_max_are_not_selectable(string browserName)
    {
        var page = await OpenAsync(browserName, "/Docs/Components/DatePicker");
        var scope = "#panel-mm-preview ";
        await page.Locator(scope + ".rhx-date-picker__trigger").ClickAsync();
        var disabled = page.Locator(scope + ".rhx-calendar__day[disabled]").First;
        await Assertions.Expect(disabled).ToBeDisabledAsync();
    }
}
```

- [ ] **Step 2: Run E2E on Chromium**

Run: `dotnet test PlaywrightTests --filter "FullyQualifiedName~DatePicker&DisplayName~chromium"`
Expected: PASS. (If browsers aren't installed: `npx -y playwright@1.60.0 install chromium`.)

- [ ] **Step 3: Commit**

```bash
git add PlaywrightTests/ComponentTests/DatePickerTests.cs
git commit -m "test(date-picker): Playwright E2E (open, select, htmx nav, min/max)"
```

---

## Task 10: Docs, snippet, final verification

**Files:**
- Modify: `README.md` (Forms catalog row)
- Modify: `CHANGELOG.md` (2.1.0 section)
- Modify: `vscode-extension/snippets/htmxrazor.code-snippets`

- [ ] **Step 1: README — add to the Forms row**

Add `Date Picker` to the Forms catalog row (after `Radial Select`):

```markdown
| **Forms** | Input, Textarea, Select, Combobox, Radial Select, Date Picker, Checkbox, Switch, Radio, Slider, Rating, Color Picker, File Input, Number Input, htmx Form |
```

- [ ] **Step 2: CHANGELOG — add under the 2.1.0 "Added" list**

```markdown
- **Date Picker** — `<rhx-date-picker>`: a text input + popup calendar. The month grid is server-rendered; prev/next and the clickable month/year label navigate via htmx against a built-in `/_rhx/calendar` endpoint (registered by `UsehtmxRazor()`), overridable per app via `hx-get` + `RhxCalendar.RenderMonth(...)`. Day selection commits a hidden ISO `yyyy-MM-dd` value for model binding; the visible input shows the culture short date. Configurable `rhx-min`/`rhx-max`, `rhx-week-start` (default Monday), `rhx-format`, Today/Clear. APG grid accessibility (roving tabindex, arrow/PageUp-Down nav, Esc).
```

- [ ] **Step 3: VS Code snippet**

In `vscode-extension/snippets/htmxrazor.code-snippets`, add after the `rhx-radial-select` block:

```json
  "rhx-date-picker": {
    "prefix": "rhx-date-picker",
    "body": [
      "<rhx-date-picker name=\"${1:DueDate}\" rhx-placeholder=\"${2:Pick a date…}\" rhx-week-start=\"${3|mon,sun|}\" />"
    ],
    "description": "htmxRazor date picker (input + popup calendar, htmx month nav)"
  },
```

- [ ] **Step 4: Full verification**

Run: `dotnet build htmxRazor.sln` (expect 0 warnings — `TreatWarningsAsErrors`).
Run: `dotnet test htmxRazor.Tests` (expect all pass, incl. new Calendar + DatePicker tests).
Run: `dotnet test PlaywrightTests --filter "FullyQualifiedName~DatePicker&DisplayName~chromium"` (expect pass).

- [ ] **Step 5: Commit**

```bash
git add README.md CHANGELOG.md vscode-extension/snippets/htmxrazor.code-snippets
git commit -m "docs(date-picker): README catalog, changelog, VS Code snippet"
```

---

## Self-Review (completed)

**Spec coverage (§ → task):** §3.1 server/htmx render → Tasks 1–3, 5; §3.2 endpoint + override → Tasks 3, 4; §3.3 value/binding/ISO → Task 5; §3.4 grid contract → Tasks 1–2; §3.5 popup/positioning → Tasks 5–7; §4 date-picker API + Layout A nav + Today/Clear → Tasks 1, 5, 7; §8 a11y (grid roles, roving tabindex, keyboard) → Tasks 1, 7; §10 testing (renderer/endpoint/helper unit + E2E) → Tasks 1–5, 9; §11 docs (demo, sidebar, assets, README, changelog, snippet) → Tasks 8, 10. Time/DateTime/Range pickers (§§5–7) are out of scope for this milestone (separate plans).

**Placeholder scan:** Every code step contains complete code. The two `RenderMonthsBody`/`RenderYearsBody` stubs in Task 1 are intentional, filled in Task 2 (called out explicitly). The token-existence and browser-install notes are verification commands, not placeholders.

**Type consistency:** `CalendarOptions` (record, `with`-able) used unchanged across Tasks 1–5; `CalendarView` enum values (`Days`/`Months`/`Years`) consistent in renderer + endpoint; data attributes (`data-rhx-date-picker`, `data-rhx-date-value`, `data-rhx-date-display`, `data-rhx-calendar`, `data-rhx-cal-today`, `data-rhx-cal-clear`, `data-date`) match between the renderer (Tasks 1–2), Tag Helper (Task 5), and JS (Task 7); CSS classes (`rhx-calendar__day--selected/--muted/--today`, `__nav`, `__label`, `__months`, `__years`) match between renderer and CSS (Task 6); `BlockName="date-picker"` makes `GetElementClass(...)` map to the Task 6 stylesheet. `/_rhx/calendar` URL identical in renderer NavUrl, endpoint mount, and Tag Helper `HxGetUrl`.
