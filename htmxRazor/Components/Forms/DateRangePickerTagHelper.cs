using System;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Forms;

/// <summary>
/// A two-date range picker built from a pair of native <c>&lt;input type="date"&gt;</c> controls
/// (start + end) — no JavaScript. Each input has the browser's calendar popup; the end input's
/// <c>min</c> follows the chosen start so the range stays ordered. Submits two ISO yyyy-MM-dd
/// values under <c>rhx-start-name</c> / <c>rhx-end-name</c>.
/// </summary>
/// <remarks>
/// This control submits two separately-named fields rather than a single bound field, so it does
/// not emit single-field validation attributes; validate the two date fields on the server.
/// </remarks>
/// <example>
/// <code>
/// &lt;rhx-date-range-picker rhx-start-name="From" rhx-end-name="To" /&gt;
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
    /// <summary>Retained for source compatibility; the native calendar's week start follows the user's locale.</summary>
    [HtmlAttributeName("rhx-week-start")] public string WeekStartName { get; set; } = "mon";
    /// <summary>Retained for source compatibility; the native inputs render dates per the user's locale.</summary>
    [HtmlAttributeName("rhx-format")] public string? Format { get; set; }
    /// <summary>Retained for source compatibility; preset shortcuts are dropped with the custom popup.</summary>
    [HtmlAttributeName("rhx-presets")] public string? Presets { get; set; }

    [HtmlAttributeNotBound] public DateOnly Today { get; set; } = DateOnly.FromDateTime(DateTime.Today);

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
        var startIso = start is { } s0 ? s0.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "";
        var endIso = end is { } e0 ? e0.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "";
        var minIso = ParseDate(Min) is { } mnv ? mnv.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "";
        var maxIso = ParseDate(Max) is { } mxv ? mxv.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "";

        var startId = $"{id}-start";
        var endId = $"{id}-end";
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

        var sb = new StringBuilder();

        var labelText = ResolveLabelText();
        if (!string.IsNullOrEmpty(labelText))
            sb.Append($"<label class=\"{GetElementClass("label")}\" id=\"{Enc(labelId)}\" for=\"{Enc(startId)}\">{Enc(labelText)}</label>");

        sb.Append($"<div class=\"{GetElementClass("control")}\"");
        if (!string.IsNullOrEmpty(labelText)) sb.Append($" role=\"group\" aria-labelledby=\"{Enc(labelId)}\"");
        else if (!string.IsNullOrEmpty(AriaLabel)) sb.Append($" role=\"group\" aria-label=\"{Enc(AriaLabel)}\"");
        sb.Append(">");

        // Start date — its max is bounded by the selected end (kept in sync natively is not
        // possible without JS, so the end's `min` follows the start value at render time).
        sb.Append(DateInput(startId, StartName, startIso, GetElementClass("start"),
            min: minIso, max: !string.IsNullOrEmpty(endIso) ? endIso : maxIso, ariaLabel: "Start date"));

        sb.Append($"<span class=\"{GetElementClass("separator")}\" aria-hidden=\"true\">–</span>");

        // End date — its min follows the chosen start so the range stays ordered.
        sb.Append(DateInput(endId, EndName, endIso, GetElementClass("end"),
            min: !string.IsNullOrEmpty(startIso) ? startIso : minIso, max: maxIso, ariaLabel: "End date"));

        sb.Append("</div>");

        sb.Append(BuildHintHtml(hintId));
        sb.Append(BuildErrorHtml(errorId));

        output.Content.SetHtmlContent(sb.ToString());
    }

    private string DateInput(string inputId, string? name, string value, string cls, string min, string max, string ariaLabel)
    {
        var sb = new StringBuilder();
        sb.Append($"<input class=\"{Enc(cls)}\" id=\"{Enc(inputId)}\" type=\"date\" name=\"{Enc(name ?? "")}\"");
        if (!string.IsNullOrEmpty(value)) sb.Append($" value=\"{Enc(value)}\"");
        if (!string.IsNullOrEmpty(min)) sb.Append($" min=\"{Enc(min)}\"");
        if (!string.IsNullOrEmpty(max)) sb.Append($" max=\"{Enc(max)}\"");
        sb.Append($" aria-label=\"{Enc(ariaLabel)}\"");
        if (Disabled) sb.Append(" disabled");
        if (Readonly) sb.Append(" readonly");
        sb.Append(" />");
        return sb.ToString();
    }

    private static DateOnly? ParseDate(string? s) =>
        DateOnly.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;
}
