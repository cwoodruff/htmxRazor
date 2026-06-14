using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Forms;

/// <summary>
/// A color input backed by the native <c>&lt;input type="color"&gt;</c> — no JavaScript. The
/// browser supplies the color-selection UI and accessibility. Binds a hex <c>#rrggbb</c> value.
/// </summary>
/// <remarks>
/// Native color inputs are hex-only and have no opacity channel or preset swatches, so
/// <c>rhx-format</c>, <c>rhx-opacity</c>, <c>rhx-swatches</c>, and <c>rhx-inline</c> are retained
/// for source compatibility but no longer change behavior (the rich custom picker is dropped).
/// </remarks>
/// <example>
/// <code>
/// &lt;rhx-color-picker name="color" rhx-label="Brand Color" value="#3b82f6" /&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-color-picker")]
public class ColorPickerTagHelper : FormControlTagHelperBase
{
    /// <inheritdoc/>
    protected override string BlockName => "color-picker";

    /// <summary>Retained for source compatibility; the native input is hex-only.</summary>
    [HtmlAttributeName("rhx-format")] public string Format { get; set; } = "hex";
    /// <summary>Retained for source compatibility; no-op for the native input.</summary>
    [HtmlAttributeName("rhx-inline")] public bool Inline { get; set; }
    /// <summary>Retained for source compatibility; the native input has no preset swatches.</summary>
    [HtmlAttributeName("rhx-swatches")] public string? Swatches { get; set; }
    /// <summary>Retained for source compatibility; the native input has no opacity channel.</summary>
    [HtmlAttributeName("rhx-opacity")] public bool Opacity { get; set; }

    public ColorPickerTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var resolvedName = ResolveName();
        var resolvedId = ResolveId();
        var resolvedValue = NormalizeHex(ResolveValue());
        var resolvedRequired = ResolveRequired();
        var hasError = HasError();
        var size = Size.ToLowerInvariant();

        var hintId = $"{resolvedId}-hint";
        var errorId = $"{resolvedId}-error";

        var css = CreateCssBuilder()
            .AddIf(GetModifierClass(size), size != "medium")
            .AddIf(GetModifierClass("disabled"), Disabled)
            .AddIf(GetModifierClass("error"), hasError);
        ApplyWrapperAttributes(output, css);

        var sb = new StringBuilder();

        sb.Append(BuildLabelHtml(resolvedId));

        sb.Append($"<input type=\"color\" class=\"{GetElementClass("control")}\"");
        sb.Append($" id=\"{Enc(resolvedId)}\"");
        if (!string.IsNullOrEmpty(resolvedName))
            sb.Append($" name=\"{Enc(resolvedName)}\"");
        sb.Append($" value=\"{Enc(resolvedValue)}\"");
        if (!string.IsNullOrEmpty(AriaLabel))
            sb.Append($" aria-label=\"{Enc(AriaLabel)}\"");
        var describedBy = BuildAriaDescribedBy(hintId, errorId);
        if (describedBy != null)
            sb.Append($" aria-describedby=\"{Enc(describedBy)}\"");
        if (hasError) sb.Append(" aria-invalid=\"true\"");
        if (resolvedRequired) sb.Append(" required");
        if (Disabled) sb.Append(" disabled");
        sb.Append(BuildHtmxAttributeString());
        sb.Append(BuildValidationAttributeString());
        sb.Append(" />");

        sb.Append(BuildHintHtml(hintId));
        sb.Append(BuildErrorHtml(errorId));

        output.Content.SetHtmlContent(sb.ToString());
    }

    /// <summary>The native color input requires a 7-char <c>#rrggbb</c> value; fall back to black.</summary>
    private static string NormalizeHex(string? value) =>
        value != null && Regex.IsMatch(value, "^#[0-9a-fA-F]{6}$") ? value.ToLowerInvariant() : "#000000";
}
