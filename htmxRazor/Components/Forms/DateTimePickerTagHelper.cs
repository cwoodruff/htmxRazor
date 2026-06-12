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
    /// <summary>Earliest selectable date (ISO yyyy-MM-dd). Bounds the calendar only — the time list always spans the full day at <c>rhx-step</c>.</summary>
    [HtmlAttributeName("rhx-min")] public string? Min { get; set; }
    /// <summary>Latest selectable date (ISO yyyy-MM-dd). Bounds the calendar only — the time list always spans the full day at <c>rhx-step</c>.</summary>
    [HtmlAttributeName("rhx-max")] public string? Max { get; set; }
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
        var timePart = dt is { } d1 ? new TimeOnly(d1.Hour, d1.Minute) : (TimeOnly?)null;
        // Minute precision; seconds are not supported by the picker.
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
            ShowToday = false,
            ShowClear = false,
            Format = DateFormat,
        };

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
        sb.Append("</div>");

        sb.Append($"<div class=\"{GetElementClass("footer")}\">");
        sb.Append($"<button type=\"button\" class=\"{GetElementClass("action")}\" data-rhx-dt-clear>Clear</button>");
        sb.Append($"<button type=\"button\" class=\"{GetElementClass("action")}\" data-rhx-dt-done>Done</button>");
        sb.Append("</div>");

        sb.Append("</div>");

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
