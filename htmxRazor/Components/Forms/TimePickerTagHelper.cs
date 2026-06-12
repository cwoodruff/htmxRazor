using System;
using System.Globalization;
using System.Text;
using htmxRazor.Components.Forms.Time;
using htmxRazor.Components.Imagery;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Forms;

/// <summary>
/// A time input with a popup list of selectable times. The list is static (no server round-trip):
/// times run from <c>rhx-min</c> (default 00:00) to <c>rhx-max</c> (default 23:59) stepping by
/// <c>rhx-step</c> minutes. Selection commits a hidden ISO <c>HH:mm</c> value for form binding;
/// the visible input shows 12-hour (default) or 24-hour display.
/// </summary>
/// <example>
/// <code>
/// &lt;rhx-time-picker rhx-for="StartTime" rhx-step="30" rhx-12hour="true" /&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-time-picker")]
public class TimePickerTagHelper : FormControlTagHelperBase
{
    protected override string BlockName => "time-picker";

    [HtmlAttributeName("rhx-placeholder")] public string? Placeholder { get; set; }
    [HtmlAttributeName("rhx-min")] public string? Min { get; set; }
    [HtmlAttributeName("rhx-max")] public string? Max { get; set; }
    [HtmlAttributeName("rhx-step")] public int Step { get; set; } = 30;
    [HtmlAttributeName("rhx-12hour")] public bool TwelveHour { get; set; } = true;
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
        var listboxId = $"{id}-listbox";
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
        output.Attributes.SetAttribute("data-rhx-time-picker", "");

        var min = ParseTime(Min);
        var max = ParseTime(Max);
        var display = selected is { } s1 ? TimeListRenderer.FormatDisplay(s1, TwelveHour, Format) : "";

        var sb = new StringBuilder();

        var labelText = ResolveLabelText();
        if (!string.IsNullOrEmpty(labelText))
            sb.Append($"<label class=\"{GetElementClass("label")}\" id=\"{Enc(labelId)}\" for=\"{Enc(inputId)}\">{Enc(labelText)}</label>");

        sb.Append($"<div class=\"{GetElementClass("control")}\">");
        sb.Append($"<input class=\"{GetElementClass("input")}\" id=\"{Enc(inputId)}\" type=\"text\" autocomplete=\"off\" data-rhx-time-display");
        sb.Append(" role=\"combobox\" aria-haspopup=\"listbox\" aria-expanded=\"false\"");
        sb.Append($" aria-controls=\"{Enc(listboxId)}\"");
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

        sb.Append($"<button type=\"button\" class=\"{GetElementClass("trigger")}\" tabindex=\"-1\" aria-label=\"Open time list\"");
        if (Disabled) sb.Append(" disabled");
        sb.Append('>');
        sb.Append($"<svg viewBox=\"0 0 24 24\" width=\"18\" height=\"18\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\">{IconRegistry.Get("clock") ?? ""}</svg>");
        sb.Append("</button>");
        sb.Append("</div>");

        sb.Append($"<input type=\"hidden\" data-rhx-time-value name=\"{Enc(name)}\" value=\"{Enc(iso)}\"");
        sb.Append(BuildValidationAttributeString());
        sb.Append(" />");

        sb.Append($"<div class=\"{GetElementClass("listbox")}\" id=\"{Enc(listboxId)}\" role=\"listbox\"");
        if (!string.IsNullOrEmpty(labelText)) sb.Append($" aria-labelledby=\"{Enc(labelId)}\"");
        else if (!string.IsNullOrEmpty(AriaLabel)) sb.Append($" aria-label=\"{Enc(AriaLabel)}\"");
        sb.Append(" hidden>");
        sb.Append(TimeListRenderer.RenderOptions(Step, min, max, TwelveHour, Format, selected));
        sb.Append("</div>");

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
            // dto.DateTime uses the stored offset's local time (wall-clock), consistent with DatePickerTagHelper.
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
}
