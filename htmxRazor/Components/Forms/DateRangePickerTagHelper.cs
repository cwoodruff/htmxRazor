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

        sb.Append($"<input type=\"hidden\" data-rhx-range-start name=\"{Enc(StartName ?? "")}\" value=\"{Enc(start is { } s1 ? s1.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "")}\" />");
        sb.Append($"<input type=\"hidden\" data-rhx-range-end name=\"{Enc(EndName ?? "")}\" value=\"{Enc(end is { } e1 ? e1.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "")}\" />");

        sb.Append($"<div class=\"{GetElementClass("popup")}\" id=\"{Enc(popupId)}\" role=\"dialog\" aria-modal=\"false\"");
        if (!string.IsNullOrEmpty(labelText)) sb.Append($" aria-labelledby=\"{Enc(labelId)}\"");
        else if (!string.IsNullOrEmpty(AriaLabel)) sb.Append($" aria-label=\"{Enc(AriaLabel)}\"");
        sb.Append(" hidden>");
        sb.Append($"<div class=\"{GetElementClass("body")}\">");

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
