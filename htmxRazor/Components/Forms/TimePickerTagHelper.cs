using System;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Forms;

/// <summary>
/// A time input backed by the native <c>&lt;input type="time"&gt;</c> — no JavaScript. The
/// browser supplies the time entry UI, keyboard support, and accessibility. Binds an ISO
/// <c>HH:mm</c> value; <c>rhx-step</c> (minutes) and <c>rhx-min</c>/<c>rhx-max</c> bound it.
/// </summary>
/// <example>
/// <code>
/// &lt;rhx-time-picker rhx-for="StartTime" rhx-step="30" /&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-time-picker")]
public class TimePickerTagHelper : FormControlTagHelperBase
{
    protected override string BlockName => "time-picker";

    [HtmlAttributeName("rhx-placeholder")] public string? Placeholder { get; set; }
    [HtmlAttributeName("rhx-min")] public string? Min { get; set; }
    [HtmlAttributeName("rhx-max")] public string? Max { get; set; }
    /// <summary>Selection granularity in minutes (mapped to the native <c>step</c> in seconds). Default: 30.</summary>
    [HtmlAttributeName("rhx-step")] public int Step { get; set; } = 30;
    /// <summary>Retained for source compatibility; the native input formats time per the user's locale.</summary>
    [HtmlAttributeName("rhx-12hour")] public bool TwelveHour { get; set; } = true;
    /// <summary>Retained for source compatibility; the native input formats time per the user's locale.</summary>
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

        sb.Append($"<input class=\"{GetElementClass("control")}\" id=\"{Enc(id)}\" type=\"time\" name=\"{Enc(name)}\"");
        if (!string.IsNullOrEmpty(iso)) sb.Append($" value=\"{Enc(iso)}\"");
        if (FormatTime(Min) is { } mn) sb.Append($" min=\"{Enc(mn)}\"");
        if (FormatTime(Max) is { } mx) sb.Append($" max=\"{Enc(mx)}\"");
        // Native step is in seconds; rhx-step is in minutes. Default 30min ⇒ step="1800".
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

    private static string? FormatTime(string? s) =>
        ParseTime(s) is { } t ? t.ToString("HH:mm", CultureInfo.InvariantCulture) : null;
}
