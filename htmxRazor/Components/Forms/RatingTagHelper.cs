using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Forms;

/// <summary>
/// Renders a JS-free star rating component.
/// </summary>
/// <remarks>
/// <para>
/// Interactive ratings (not readonly and not disabled) use the classic reversed
/// radio-button CSS pattern: a <c>role="radiogroup"</c> wrapper containing native
/// radio inputs and labels rendered in reverse order. This gives click selection,
/// keyboard navigation (arrow keys) and form submission for free — no JavaScript.
/// </para>
/// <para>
/// <b>Interactive half-star precision is not supported.</b> A pure-CSS half-radio
/// is out of scope, so interactive ratings always select whole stars regardless of
/// <c>rhx-precision</c>. <c>rhx-precision</c> still affects the READONLY/disabled
/// display, where half stars are rendered via the <c>--half</c> modifier.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;rhx-rating name="rating" rhx-label="Rate this product" rhx-max="5" /&gt;
///
/// &lt;rhx-rating rhx-for="Score" rhx-precision="0.5" rhx-readonly="true" /&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-rating")]
public class RatingTagHelper : FormControlTagHelperBase
{
    /// <inheritdoc/>
    protected override string BlockName => "rating";

    // ──────────────────────────────────────────────
    //  Rating-specific properties
    // ──────────────────────────────────────────────

    /// <summary>Maximum number of stars. Default: 5.</summary>
    [HtmlAttributeName("rhx-max")]
    public int Max { get; set; } = 5;

    /// <summary>
    /// Rating precision: 1 for whole stars, 0.5 for half stars. Default: 1.
    /// Only affects the readonly/disabled display — interactive ratings always use whole stars.
    /// </summary>
    [HtmlAttributeName("rhx-precision")]
    public double Precision { get; set; } = 1;

    /// <summary>
    /// Whether to enable optimistic UI. Emits <c>data-rhx-optimistic</c> on the
    /// wrapper, which the optimistic CSS uses to reflect the change immediately.
    /// </summary>
    [HtmlAttributeName("rhx-optimistic")]
    public bool Optimistic { get; set; }

    // ──────────────────────────────────────────────
    //  Constructor
    // ──────────────────────────────────────────────

    public RatingTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    // ──────────────────────────────────────────────
    //  Star SVG
    // ──────────────────────────────────────────────

    private static void AppendStarSvg(StringBuilder sb)
    {
        sb.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\" fill=\"currentColor\" stroke=\"currentColor\" stroke-width=\"1\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\">");
        sb.Append("<polygon points=\"12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2\"></polygon>");
        sb.Append("</svg>");
    }

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
        var resolvedValue = ResolveValue() ?? "0";
        var resolvedRequired = ResolveRequired();
        var hasError = HasError();
        var size = Size.ToLowerInvariant();
        var isReadonly = Readonly;
        var isInteractive = !isReadonly && !Disabled;

        var hintId = $"{resolvedId}-hint";
        var errorId = $"{resolvedId}-error";

        if (!double.TryParse(resolvedValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var currentValue))
            currentValue = 0;

        // Interactive mode snaps to whole stars (half precision is display-only).
        var currentInt = (int)Math.Round(currentValue, MidpointRounding.AwayFromZero);

        // ── CSS classes on wrapper ──
        var css = CreateCssBuilder()
            .AddIf(GetModifierClass(size), size != "medium")
            .AddIf(GetModifierClass("disabled"), Disabled)
            .AddIf(GetModifierClass("readonly"), isReadonly)
            .AddIf(GetModifierClass("error"), hasError);

        ApplyWrapperAttributes(output, css);

        if (Optimistic)
            output.Attributes.SetAttribute("data-rhx-optimistic", "");

        var describedBy = BuildAriaDescribedBy(hintId, errorId);

        var sb = new StringBuilder();

        // Label
        sb.Append(BuildLabelHtml(resolvedId));

        if (isInteractive)
        {
            // ── Interactive: role="radiogroup" with reversed radios ──
            var groupLabel = AriaLabel ?? ResolveLabelText();

            sb.Append($"<div class=\"{GetElementClass("stars")}\" role=\"radiogroup\"");
            if (!string.IsNullOrEmpty(groupLabel))
                sb.Append($" aria-label=\"{Enc(groupLabel)}\"");
            if (describedBy != null)
                sb.Append($" aria-describedby=\"{Enc(describedBy)}\"");
            sb.Append('>');

            // Radios in reverse order (Max down to 1). CSS uses row-reverse so the
            // visual order is 1..Max left-to-right.
            for (var i = Max; i >= 1; i--)
            {
                var inputId = $"{Enc(resolvedId)}-{i}";

                sb.Append($"<input type=\"radio\" class=\"{GetElementClass("input")} rhx-sr-only\"");
                sb.Append($" id=\"{inputId}\"");
                if (!string.IsNullOrEmpty(resolvedName))
                    sb.Append($" name=\"{Enc(resolvedName)}\"");
                sb.Append($" value=\"{i.ToString(CultureInfo.InvariantCulture)}\"");
                if (i == currentInt) sb.Append(" checked");

                if (resolvedRequired)
                {
                    sb.Append(" required");
                    sb.Append(" aria-required=\"true\"");
                }
                if (hasError) sb.Append(" aria-invalid=\"true\"");

                // htmx and validation on the radios so selection posts/triggers.
                sb.Append(BuildHtmxAttributeString());
                sb.Append(BuildValidationAttributeString());
                sb.Append(" />");

                sb.Append($"<label class=\"{GetElementClass("star")}\" for=\"{inputId}\"");
                sb.Append($" aria-label=\"{Enc($"{i} of {Max} stars")}\">");
                AppendStarSvg(sb);
                sb.Append("</label>");
            }

            sb.Append("</div>"); // close stars
        }
        else
        {
            // ── Display-only (readonly or disabled): static stars + hidden input ──
            sb.Append($"<div class=\"{GetElementClass("stars")}\"");
            sb.Append($" role=\"img\" aria-label=\"{Enc($"{currentValue.ToString("G", CultureInfo.InvariantCulture)} of {Max} stars")}\">");

            for (var i = 1; i <= Max; i++)
            {
                var starClass = GetElementClass("star");
                if (currentValue >= i)
                    starClass += $" {GetElementClass("star")}--filled";
                else if (Precision <= 0.5 && currentValue >= i - 0.5)
                    starClass += $" {GetElementClass("star")}--half";

                sb.Append($"<span class=\"{starClass}\">");
                AppendStarSvg(sb);
                sb.Append("</span>");
            }

            sb.Append("</div>"); // close stars

            // Hidden input so the value still posts.
            sb.Append($"<input type=\"hidden\" class=\"{GetElementClass("value")}\"");
            sb.Append($" id=\"{Enc(resolvedId)}\"");
            if (!string.IsNullOrEmpty(resolvedName))
                sb.Append($" name=\"{Enc(resolvedName)}\"");
            sb.Append($" value=\"{Enc(resolvedValue)}\"");

            if (resolvedRequired)
            {
                sb.Append(" required");
                sb.Append(" aria-required=\"true\"");
            }
            if (hasError) sb.Append(" aria-invalid=\"true\"");

            sb.Append(BuildHtmxAttributeString());
            sb.Append(BuildValidationAttributeString());
            sb.Append(" />");
        }

        // Hint
        sb.Append(BuildHintHtml(hintId));

        // Error
        sb.Append(BuildErrorHtml(errorId));

        output.Content.SetHtmlContent(sb.ToString());
    }
}
