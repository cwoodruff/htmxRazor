using htmxRazor.Infrastructure;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Utilities;

/// <summary>
/// A lower-level positioning utility — a container that positions itself next to an anchor
/// element using CSS Anchor Positioning (no JavaScript). Because the anchor is an external
/// element referenced by selector, the consumer must declare a matching <c>anchor-name</c>
/// on that anchor: for an <c>rhx-id</c> of <c>X</c> the popup uses <c>position-anchor: --X</c>,
/// so the anchor must set <c>anchor-name: --X</c>. Toggle visibility by adding/removing the
/// <c>rhx-popup--active</c> class (or set <c>rhx-active="true"</c> to render it shown).
/// </summary>
/// <example>
/// <code>
/// &lt;button id="my-trigger" style="anchor-name: --my-popup"&gt;Open&lt;/button&gt;
/// &lt;rhx-popup rhx-id="my-popup" rhx-placement="bottom-start" rhx-active="true"&gt;
///     Positioned content
/// &lt;/rhx-popup&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-popup")]
public class PopupTagHelper : htmxRazorTagHelperBase
{
    protected override string BlockName => "popup";

    /// <summary>
    /// CSS selector for the anchor element to position relative to. The referenced element
    /// must declare an <c>anchor-name</c> matching the popup's id (<c>--{id}</c>).
    /// </summary>
    [HtmlAttributeName("rhx-anchor")]
    public string? Anchor { get; set; }

    /// <summary>
    /// Placement relative to anchor: top, top-start, top-end, bottom,
    /// bottom-start, bottom-end, left, left-start, left-end, right,
    /// right-start, right-end. Default: bottom-start.
    /// </summary>
    [HtmlAttributeName("rhx-placement")]
    public string Placement { get; set; } = "bottom-start";

    /// <summary>
    /// Whether the popup is currently shown. Default: false.
    /// </summary>
    [HtmlAttributeName("rhx-active")]
    public bool Active { get; set; }

    /// <summary>
    /// Whether to show a directional arrow. Default: false.
    /// </summary>
    [HtmlAttributeName("rhx-arrow")]
    public bool Arrow { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var id = string.IsNullOrWhiteSpace(Id) ? $"rhx-popup-{context.UniqueId}" : Id!;
        var anchorName = $"--{id}";
        var positionArea = PlacementToPositionArea(Placement.ToLowerInvariant());

        var css = CreateCssBuilder()
            .AddIf(GetModifierClass("active"), Active);
        ApplyBaseAttributes(output, css);

        output.Attributes.SetAttribute("id", id);

        // CSS Anchor Positioning: the consumer's anchor element must declare
        // `anchor-name: --{id}` for this to take effect.
        var existingStyle = output.Attributes["style"]?.Value?.ToString();
        var posStyle = $"position-anchor:{anchorName};position-area:{positionArea};position-try-fallbacks:flip-block,flip-inline";
        output.Attributes.SetAttribute("style",
            string.IsNullOrWhiteSpace(existingStyle) ? posStyle : existingStyle!.TrimEnd(';', ' ') + ";" + posStyle);

        if (Arrow)
            output.PostContent.AppendHtml("<div class=\"rhx-popup__arrow\"></div>");

        RenderHtmxAttributes(output);
    }

    private static string PlacementToPositionArea(string placement) => placement switch
    {
        "top" => "top",
        "top-start" => "top span-right",
        "top-end" => "top span-left",
        "bottom" => "bottom",
        "bottom-start" => "bottom span-right",
        "bottom-end" => "bottom span-left",
        "left" => "left",
        "left-start" => "left span-bottom",
        "left-end" => "left span-top",
        "right" => "right",
        "right-start" => "right span-bottom",
        "right-end" => "right span-top",
        _ => "bottom span-right"
    };
}
