using htmxRazor.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Organization;

/// <summary>
/// Renders a scroll container with native CSS overflow scrolling, gradient fade
/// edges, and prev/next scroll buttons. Scrolling, swiping, and keyboard
/// navigation are handled entirely by the browser; only the prev/next buttons
/// rely on the shared <c>rhx-scroll-buttons</c> shim.
/// </summary>
/// <remarks>
/// <para>
/// The component wraps child content in a scrollable viewport
/// (<c>overflow-x</c> or <c>overflow-y: auto</c> depending on orientation) and
/// adds start/end gradient fades plus prev/next buttons. The viewport carries
/// the <c>data-rhx-scroll-viewport</c> hook and the root carries
/// <c>data-rhx-scrollable</c> so the shared scroll-button shim can drive the
/// buttons. No per-component JavaScript is used.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;rhx-scroller rhx-orientation="horizontal"&gt;
///     &lt;div style="display: flex; gap: 1rem;"&gt;
///         &lt;div&gt;Item 1&lt;/div&gt;
///         &lt;div&gt;Item 2&lt;/div&gt;
///         ...
///     &lt;/div&gt;
/// &lt;/rhx-scroller&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-scroller")]
public class ScrollerTagHelper : htmxRazorTagHelperBase
{
    /// <inheritdoc/>
    protected override string BlockName => "scroller";

    /// <summary>
    /// The scroll direction: horizontal, vertical, or both. Default: horizontal.
    /// </summary>
    [HtmlAttributeName("rhx-orientation")]
    public string Orientation { get; set; } = "horizontal";

    /// <summary>
    /// Creates a new ScrollerTagHelper with URL generation support.
    /// </summary>
    public ScrollerTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    /// <inheritdoc/>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var childContent = await output.GetChildContentAsync();

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var orientation = Orientation.ToLowerInvariant();
        var css = CreateCssBuilder()
            .Add(GetModifierClass(orientation));
        ApplyBaseAttributes(output, css);

        output.Attributes.SetAttribute("data-rhx-orientation", orientation);
        // Scope hook for the shared scroll-button shim.
        output.Attributes.SetAttribute("data-rhx-scrollable", "");

        RenderHtmxAttributes(output);

        // Orientation drives the button axis labels and arrow glyphs.
        var vertical = orientation == "vertical";
        var prevLabel = vertical ? "Scroll up" : "Scroll back";
        var nextLabel = vertical ? "Scroll down" : "Scroll forward";
        var prevGlyph = vertical ? "↑" : "←"; // ↑ / ←
        var nextGlyph = vertical ? "↓" : "→"; // ↓ / →

        var btnClass = GetElementClass("btn");
        var viewportClass = GetElementClass("viewport");

        // Assemble inner HTML
        output.Content.Clear();

        // Prev button
        output.Content.AppendHtml(
            $"<button type=\"button\" class=\"{btnClass} {btnClass}--prev\" data-rhx-scroll-prev aria-label=\"{prevLabel}\">" +
            $"<span aria-hidden=\"true\">{prevGlyph}</span></button>");

        // Scrollable viewport — vertical/both need an explicit height to enable scrolling.
        var viewportStyle = (orientation == "vertical" || orientation == "both")
            ? " style=\"height:100%\""
            : "";
        output.Content.AppendHtml(
            $"<div class=\"{viewportClass}\" data-rhx-scroll-viewport{viewportStyle}>");
        output.Content.AppendHtml(childContent);
        output.Content.AppendHtml("</div>");

        // Next button
        output.Content.AppendHtml(
            $"<button type=\"button\" class=\"{btnClass} {btnClass}--next\" data-rhx-scroll-next aria-label=\"{nextLabel}\">" +
            $"<span aria-hidden=\"true\">{nextGlyph}</span></button>");
    }
}
