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
        var iso = ResolveIsoValue();
        var selected = ParseDate(iso);
        var calId = $"{id}-cal";
        var size = Size.ToLowerInvariant();
        var hasError = HasError();
        var resolvedRequired = ResolveRequired();

        var css = CreateCssBuilder()
            .AddIf(GetModifierClass(size), size != "medium")
            .AddIf(GetModifierClass("disabled"), Disabled)
            .AddIf(GetModifierClass("readonly"), Readonly)
            .AddIf(GetModifierClass("error"), hasError);
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
            Format = Format,
        };

        var labelId = $"{id}-label";
        var hintId = $"{id}-hint";
        var errorId = $"{id}-error";
        var inputId = $"{id}-input";
        var sb = new StringBuilder();

        var labelText = ResolveLabelText();
        if (!string.IsNullOrEmpty(labelText))
            sb.Append($"<label class=\"{GetElementClass("label")}\" id=\"{Enc(labelId)}\" for=\"{Enc(inputId)}\">{Enc(labelText)}</label>");

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
        if (hasError) sb.Append(" aria-invalid=\"true\"");
        if (resolvedRequired) sb.Append(" aria-required=\"true\"");
        sb.Append(" />");

        sb.Append($"<button type=\"button\" class=\"{GetElementClass("trigger")}\" aria-haspopup=\"dialog\" aria-expanded=\"false\" aria-controls=\"{Enc(calId)}\" aria-label=\"Open calendar\"");
        if (Disabled) sb.Append(" disabled");
        sb.Append('>');
        sb.Append($"<svg viewBox=\"0 0 24 24\" width=\"18\" height=\"18\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\">{IconRegistry.Get("calendar") ?? ""}</svg>");
        sb.Append("</button>");
        sb.Append("</div>");

        sb.Append($"<input type=\"hidden\" data-rhx-date-value name=\"{Enc(name)}\" value=\"{Enc(iso ?? "")}\"");
        sb.Append(BuildValidationAttributeString());
        sb.Append(" />");

        sb.Append($"<div class=\"{GetElementClass("popup")}\" role=\"dialog\" aria-modal=\"false\"");
        if (!string.IsNullOrEmpty(labelText)) sb.Append($" aria-labelledby=\"{Enc(labelId)}\"");
        else if (!string.IsNullOrEmpty(AriaLabel)) sb.Append($" aria-label=\"{Enc(AriaLabel)}\"");
        sb.Append(" hidden>");
        sb.Append(CalendarRenderer.Render(opts));
        sb.Append("</div>");

        sb.Append(BuildHintHtml(hintId));
        sb.Append(BuildErrorHtml(errorId));

        output.Content.SetHtmlContent(sb.ToString());
    }

    /// <summary>Resolves the bound value to an ISO yyyy-MM-dd string, type-aware for date models.</summary>
    private string? ResolveIsoValue()
    {
        if (!string.IsNullOrEmpty(Value)) return NormalizeIso(Value);
        return For?.Model switch
        {
            DateOnly d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            DateTime dt => DateOnly.FromDateTime(dt).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            DateTimeOffset dto => DateOnly.FromDateTime(dto.Date).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            null => null,
            var m => NormalizeIso(m.ToString()),
        };
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
