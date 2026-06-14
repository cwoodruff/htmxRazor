using System;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Forms;

/// <summary>
/// A date input backed by the native <c>&lt;input type="date"&gt;</c> — no JavaScript. The
/// browser supplies the calendar popup, keyboard entry, and accessibility. Binds an ISO
/// (yyyy-MM-dd) value for the form; <c>rhx-min</c>/<c>rhx-max</c> bound the selectable range.
/// </summary>
/// <example>
/// <code>
/// &lt;rhx-date-picker rhx-for="DueDate" rhx-min="2026-01-01" rhx-max="2026-12-31" /&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-date-picker")]
public class DatePickerTagHelper : FormControlTagHelperBase
{
    protected override string BlockName => "date-picker";

    [HtmlAttributeName("rhx-placeholder")] public string? Placeholder { get; set; }
    [HtmlAttributeName("rhx-min")] public string? Min { get; set; }
    [HtmlAttributeName("rhx-max")] public string? Max { get; set; }

    /// <summary>Retained for source compatibility; the native calendar's week start follows the user's locale.</summary>
    [HtmlAttributeName("rhx-week-start")] public string WeekStartName { get; set; } = "mon";
    /// <summary>Retained for source compatibility; the native input renders dates per the user's locale.</summary>
    [HtmlAttributeName("rhx-format")] public string? Format { get; set; }
    /// <summary>Retained for source compatibility; the native calendar provides its own "today" affordance.</summary>
    [HtmlAttributeName("rhx-show-today")] public bool ShowToday { get; set; } = true;
    /// <summary>Retained for source compatibility; the native input provides its own clear affordance.</summary>
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
        var size = Size.ToLowerInvariant();
        var hasError = HasError();
        var resolvedRequired = ResolveRequired();

        var css = CreateCssBuilder()
            .AddIf(GetModifierClass(size), size != "medium")
            .AddIf(GetModifierClass("disabled"), Disabled)
            .AddIf(GetModifierClass("readonly"), Readonly)
            .AddIf(GetModifierClass("error"), hasError);
        ApplyWrapperAttributes(output, css);

        var labelId = $"{id}-label";
        var hintId = $"{id}-hint";
        var errorId = $"{id}-error";
        var sb = new StringBuilder();

        var labelText = ResolveLabelText();
        if (!string.IsNullOrEmpty(labelText))
            sb.Append($"<label class=\"{GetElementClass("label")}\" id=\"{Enc(labelId)}\" for=\"{Enc(id)}\">{Enc(labelText)}</label>");

        sb.Append($"<input class=\"{GetElementClass("control")}\" id=\"{Enc(id)}\" type=\"date\" name=\"{Enc(name)}\"");
        if (!string.IsNullOrEmpty(iso)) sb.Append($" value=\"{Enc(iso)}\"");
        if (NormalizeIso(Min) is { } mn) sb.Append($" min=\"{Enc(mn)}\"");
        if (NormalizeIso(Max) is { } mx) sb.Append($" max=\"{Enc(mx)}\"");
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

    private static string? NormalizeIso(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        if (DateOnly.TryParse(raw, CultureInfo.CurrentCulture, DateTimeStyles.None, out var d)
            || DateOnly.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
            return d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return null;
    }
}
