using System.Net;
using htmxRazor.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Imagery;

/// <summary>
/// Renders a before/after image comparison slider with a draggable handle.
/// Supports mouse, touch, and keyboard interaction.
/// </summary>
/// <example>
/// <code>
/// &lt;rhx-comparison rhx-before="before.jpg" rhx-before-alt="Before"
///                  rhx-after="after.jpg" rhx-after-alt="After"
///                  rhx-position="50" /&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-comparison")]
public class ComparisonTagHelper : htmxRazorTagHelperBase
{
    /// <inheritdoc/>
    protected override string BlockName => "comparison";

    /// <summary>
    /// Source URL of the "before" image.
    /// </summary>
    [HtmlAttributeName("rhx-before")]
    public string Before { get; set; } = "";

    /// <summary>
    /// Alt text for the "before" image.
    /// </summary>
    [HtmlAttributeName("rhx-before-alt")]
    public string BeforeAlt { get; set; } = "Before";

    /// <summary>
    /// Source URL of the "after" image.
    /// </summary>
    [HtmlAttributeName("rhx-after")]
    public string After { get; set; } = "";

    /// <summary>
    /// Alt text for the "after" image.
    /// </summary>
    [HtmlAttributeName("rhx-after-alt")]
    public string AfterAlt { get; set; } = "After";

    /// <summary>
    /// Initial slider position as a percentage (0–100). Default: 50.
    /// </summary>
    [HtmlAttributeName("rhx-position")]
    public int Position { get; set; } = 50;

    /// <summary>
    /// Creates a new ComparisonTagHelper with URL generation support.
    /// </summary>
    public ComparisonTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var pos = Math.Clamp(Position, 0, 100);

        var css = CreateCssBuilder();
        ApplyBaseAttributes(output, css);

        RenderHtmxAttributes(output);

        output.Content.Clear();

        // Before (full image, bottom layer — also establishes the intrinsic size)
        output.Content.AppendHtml(
            $"<div class=\"{GetElementClass("before")}\">" +
            $"<img src=\"{Enc(Before)}\" alt=\"{Enc(BeforeAlt)}\" />" +
            "</div>");

        // After (top layer) — user-resizable via CSS `resize` (drag the edge grip, no JS).
        // The inner image is sized to the whole comparison (100cqw) so resizing this box
        // clips rather than scales it, revealing the "before" image underneath.
        output.Content.AppendHtml(
            $"<div class=\"{GetElementClass("after")}\" style=\"width: {pos}%\">" +
            $"<img src=\"{Enc(After)}\" alt=\"{Enc(AfterAlt)}\" />" +
            // Visual handle on the resized edge (decorative; the native resize grip is the control).
            $"<div class=\"{GetElementClass("handle")}\" aria-hidden=\"true\">" +
            $"<div class=\"{GetElementClass("handle-line")}\"></div>" +
            $"<div class=\"{GetElementClass("handle-grip")}\">" +
            "<svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" aria-hidden=\"true\">" +
            "<path d=\"M8 18l-6-6 6-6\" /><path d=\"M16 6l6 6-6 6\" />" +
            "</svg></div>" +
            $"<div class=\"{GetElementClass("handle-line")}\"></div>" +
            "</div>" + // close handle
            "</div>"); // close after
    }

    private static string Enc(string? value) => WebUtility.HtmlEncode(value ?? "") ?? "";
}
