using System;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Forms;

/// <summary>
/// A single-value <see cref="DateTime"/> picker backed by the native
/// <c>&lt;input type="datetime-local"&gt;</c> — no JavaScript. The browser supplies the
/// combined date/time entry UI and accessibility. Binds an ISO <c>yyyy-MM-ddTHH:mm</c> value.
/// </summary>
/// <example>
/// <code>
/// &lt;rhx-datetime-picker rhx-for="StartsAt" rhx-step="30" /&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-datetime-picker")]
public class DateTimePickerTagHelper : FormControlTagHelperBase
{
    protected override string BlockName => "datetime-picker";

    [HtmlAttributeName("rhx-placeholder")] public string? Placeholder { get; set; }
    /// <summary>Earliest selectable date (ISO yyyy-MM-dd); applied to the native input as the start of that day.</summary>
    [HtmlAttributeName("rhx-min")] public string? Min { get; set; }
    /// <summary>Latest selectable date (ISO yyyy-MM-dd); applied to the native input as the end of that day.</summary>
    [HtmlAttributeName("rhx-max")] public string? Max { get; set; }
    /// <summary>Retained for source compatibility; the native calendar's week start follows the user's locale.</summary>
    [HtmlAttributeName("rhx-week-start")] public string WeekStartName { get; set; } = "mon";
    /// <summary>Selection granularity in minutes (mapped to the native <c>step</c> in seconds). Default: 30.</summary>
    [HtmlAttributeName("rhx-step")] public int Step { get; set; } = 30;
    /// <summary>Retained for source compatibility; the native input formats per the user's locale.</summary>
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
        // Minute precision; seconds are not supported by the picker.
        var iso = dt is { } d2 ? d2.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture) : "";

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

        var sb = new StringBuilder();

        var labelText = ResolveLabelText();
        if (!string.IsNullOrEmpty(labelText))
            sb.Append($"<label class=\"{GetElementClass("label")}\" id=\"{Enc(labelId)}\" for=\"{Enc(id)}\">{Enc(labelText)}</label>");

        sb.Append($"<input class=\"{GetElementClass("control")}\" id=\"{Enc(id)}\" type=\"datetime-local\" name=\"{Enc(name)}\"");
        if (!string.IsNullOrEmpty(iso)) sb.Append($" value=\"{Enc(iso)}\"");
        if (ParseDate(Min) is { } mn) sb.Append($" min=\"{Enc(mn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))}T00:00\"");
        if (ParseDate(Max) is { } mx) sb.Append($" max=\"{Enc(mx.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))}T23:59\"");
        if (Step > 0) sb.Append($" step=\"{Step * 60}\"");
        if (!string.IsNullOrEmpty(labelText)) sb.Append($" aria-labelledby=\"{Enc(labelId)}\"");
        if (!string.IsNullOrEmpty(AriaLabel)) sb.Append($" aria-label=\"{Enc(AriaLabel)}\"");
        var describedBy = BuildAriaDescribedBy(hintId, errorId);
        if (describedBy != null) sb.Append($" aria-describedby=\"{Enc(describedBy)}\"");
        if (hasError) sb.Append(" aria-invalid=\"true\"");
        if (resolvedRequired) sb.Append(" required");
        if (Disabled) sb.Append(" disabled");
        if (Readonly) sb.Append(" readonly");
        sb.Append(BuildHtmxAttributeString());
        sb.Append(BuildValidationAttributeString());
        sb.Append(" />");

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
}
