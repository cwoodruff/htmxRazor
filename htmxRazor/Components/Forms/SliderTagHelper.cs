using System.Text;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Forms;

/// <summary>
/// Renders a native <c>&lt;input type="range"&gt;</c> slider styled with
/// <c>accent-color</c>. JS-free: the browser draws the track, fill, and thumb.
/// Supports model binding via <c>rhx-for</c>, htmx integration on the input,
/// and configurable min/max/step values.
/// </summary>
/// <example>
/// <code>
/// &lt;rhx-slider name="volume" rhx-label="Volume" rhx-min="0" rhx-max="100" rhx-step="1" /&gt;
///
/// &lt;rhx-slider rhx-for="Brightness" rhx-show-value="true"
///              hx-post="/settings" hx-trigger="change" /&gt;
/// </code>
/// </example>
/// <remarks>
/// When <see cref="ShowValue"/> is enabled, a static <c>&lt;output&gt;</c> shows the
/// INITIAL value next to the slider. Without JavaScript it does not live-update as the
/// thumb moves.
/// </remarks>
[HtmlTargetElement("rhx-slider")]
public class SliderTagHelper : FormControlTagHelperBase
{
    /// <inheritdoc/>
    protected override string BlockName => "slider";

    // ──────────────────────────────────────────────
    //  Slider-specific properties
    // ──────────────────────────────────────────────

    /// <summary>Minimum value for the slider. Default: "0".</summary>
    [HtmlAttributeName("rhx-min")]
    public string Min { get; set; } = "0";

    /// <summary>Maximum value for the slider. Default: "100".</summary>
    [HtmlAttributeName("rhx-max")]
    public string Max { get; set; } = "100";

    /// <summary>Step increment for the slider. Default: "1".</summary>
    [HtmlAttributeName("rhx-step")]
    public string Step { get; set; } = "1";

    /// <summary>
    /// Renders a static <c>&lt;output&gt;</c> showing the initial value next to the
    /// slider. Note: without JavaScript this does not live-update. Default: false.
    /// </summary>
    [HtmlAttributeName("rhx-show-value")]
    public bool ShowValue { get; set; }

    // ──────────────────────────────────────────────
    //  Constructor
    // ──────────────────────────────────────────────

    public SliderTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

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
        var resolvedValue = ResolveValue() ?? Min;
        var resolvedRequired = ResolveRequired();
        var hasError = HasError();
        var size = Size.ToLowerInvariant();

        var hintId = $"{resolvedId}-hint";
        var errorId = $"{resolvedId}-error";

        // ── CSS classes on wrapper ──
        var css = CreateCssBuilder()
            .AddIf(GetModifierClass(size), size != "medium")
            .AddIf(GetModifierClass("disabled"), Disabled)
            .AddIf(GetModifierClass("error"), hasError);

        ApplyWrapperAttributes(output, css);

        // ── Build inner HTML ──
        var sb = new StringBuilder();

        // Label
        sb.Append(BuildLabelHtml(resolvedId));

        // Control row (slider + optional static value output)
        sb.Append($"<div class=\"{GetElementClass("control")}\">");

        // Native range input
        sb.Append($"<input type=\"range\" class=\"{GetElementClass("input")}\"");
        sb.Append($" id=\"{Enc(resolvedId)}\"");
        if (!string.IsNullOrEmpty(resolvedName))
            sb.Append($" name=\"{Enc(resolvedName)}\"");
        sb.Append($" value=\"{Enc(resolvedValue)}\"");
        sb.Append($" min=\"{Enc(Min)}\"");
        sb.Append($" max=\"{Enc(Max)}\"");
        sb.Append($" step=\"{Enc(Step)}\"");

        if (Disabled) sb.Append(" disabled");
        if (resolvedRequired) sb.Append(" required");

        // ARIA
        if (!string.IsNullOrEmpty(AriaLabel))
            sb.Append($" aria-label=\"{Enc(AriaLabel)}\"");

        var describedBy = BuildAriaDescribedBy(hintId, errorId);
        if (describedBy != null)
            sb.Append($" aria-describedby=\"{Enc(describedBy)}\"");

        if (hasError) sb.Append(" aria-invalid=\"true\"");
        if (resolvedRequired) sb.Append(" aria-required=\"true\"");

        // htmx and validation on the input
        sb.Append(BuildHtmxAttributeString());
        sb.Append(BuildValidationAttributeString());
        sb.Append(" />");

        // Static value output (initial value only; does not live-update without JS)
        if (ShowValue)
        {
            sb.Append($"<output class=\"{GetElementClass("value")}\"");
            sb.Append($" for=\"{Enc(resolvedId)}\">{Enc(resolvedValue)}</output>");
        }

        sb.Append("</div>"); // close control

        // Hint
        sb.Append(BuildHintHtml(hintId));

        // Error
        sb.Append(BuildErrorHtml(errorId));

        output.Content.SetHtmlContent(sb.ToString());
    }
}
