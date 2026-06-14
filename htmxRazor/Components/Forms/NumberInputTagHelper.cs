using System.Text;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Forms;

/// <summary>
/// Renders a native <c>&lt;input type="number"&gt;</c> with label, hint, and error display —
/// no JavaScript. The browser provides the increment/decrement spinners; <c>rhx-no-steppers</c>
/// hides them. Supports model binding via <c>rhx-for</c>.
/// </summary>
/// <example>
/// <code>
/// &lt;rhx-number-input rhx-for="Quantity" rhx-min="0" rhx-max="99" rhx-step="1" /&gt;
///
/// &lt;rhx-number-input rhx-label="Price" name="price" value="9.99"
///                   rhx-min="0" rhx-step="0.01" rhx-no-steppers="true" /&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-number-input")]
public class NumberInputTagHelper : FormControlTagHelperBase
{
    /// <inheritdoc/>
    protected override string BlockName => "number-input";

    // ──────────────────────────────────────────────
    //  Number-specific properties
    // ──────────────────────────────────────────────

    /// <summary>Minimum allowed value.</summary>
    [HtmlAttributeName("rhx-min")]
    public string? Min { get; set; }

    /// <summary>Maximum allowed value.</summary>
    [HtmlAttributeName("rhx-max")]
    public string? Max { get; set; }

    /// <summary>Step increment. Default: "1".</summary>
    [HtmlAttributeName("rhx-step")]
    public string? Step { get; set; }

    /// <summary>Hide the browser's native increment/decrement spinners (via the --no-steppers CSS modifier).</summary>
    [HtmlAttributeName("rhx-no-steppers")]
    public bool NoSteppers { get; set; }

    // ──────────────────────────────────────────────
    //  Constructor
    // ──────────────────────────────────────────────

    public NumberInputTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    // ──────────────────────────────────────────────
    //  Rendering
    // ──────────────────────────────────────────────

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var resolvedName = ResolveName();
        var resolvedId = ResolveId();
        var resolvedValue = ResolveValue();
        var resolvedRequired = ResolveRequired();
        var hasError = HasError();
        var size = Size.ToLowerInvariant();

        var hintId = $"{resolvedId}-hint";
        var errorId = $"{resolvedId}-error";

        // ── CSS classes on wrapper ──
        var css = CreateCssBuilder()
            .AddIf(GetModifierClass(size), size != "medium")
            .AddIf(GetModifierClass("disabled"), Disabled)
            .AddIf(GetModifierClass("readonly"), Readonly)
            .AddIf(GetModifierClass("error"), hasError)
            .AddIf(GetModifierClass("no-steppers"), NoSteppers);

        ApplyWrapperAttributes(output, css);

        // ── Build inner HTML ──
        var sb = new StringBuilder();

        // Label
        sb.Append(BuildLabelHtml(resolvedId));

        // Control wrapper
        sb.Append($"<div class=\"{GetElementClass("control")}\">");

        // Native input \u2014 the browser provides increment/decrement spinners (no JavaScript).
        // rhx-no-steppers hides those spinners via the --no-steppers CSS modifier.
        sb.Append($"<input class=\"{GetElementClass("native")}\" type=\"number\"");
        sb.Append($" id=\"{Enc(resolvedId)}\"");

        if (!string.IsNullOrEmpty(resolvedName))
            sb.Append($" name=\"{Enc(resolvedName)}\"");

        if (resolvedValue != null)
            sb.Append($" value=\"{Enc(resolvedValue)}\"");

        if (!string.IsNullOrEmpty(Min))
            sb.Append($" min=\"{Enc(Min)}\"");
        if (!string.IsNullOrEmpty(Max))
            sb.Append($" max=\"{Enc(Max)}\"");
        if (!string.IsNullOrEmpty(Step))
            sb.Append($" step=\"{Enc(Step)}\"");

        if (resolvedRequired)
            sb.Append(" required");
        if (Disabled)
            sb.Append(" disabled");
        if (Readonly)
            sb.Append(" readonly");

        // ARIA
        if (!string.IsNullOrEmpty(AriaLabel))
            sb.Append($" aria-label=\"{Enc(AriaLabel)}\"");

        var describedBy = BuildAriaDescribedBy(hintId, errorId);
        if (describedBy != null)
            sb.Append($" aria-describedby=\"{Enc(describedBy)}\"");

        if (hasError)
            sb.Append(" aria-invalid=\"true\"");

        if (resolvedRequired)
            sb.Append(" aria-required=\"true\"");

        // htmx attributes on native input
        sb.Append(BuildHtmxAttributeString());

        // data-val attributes
        sb.Append(BuildValidationAttributeString());

        sb.Append(" />");

        sb.Append("</div>"); // close control

        // Hint
        sb.Append(BuildHintHtml(hintId));

        // Error
        sb.Append(BuildErrorHtml(errorId));

        output.Content.SetHtmlContent(sb.ToString());
    }
}
