using htmxRazor.Infrastructure;
using htmxRazor.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Organization;

/// <summary>
/// Renders a resizable two-panel layout. The start panel is made user-resizable with the CSS
/// <c>resize</c> property (no JavaScript) — drag its edge grip to change the split; the end
/// panel fills the rest. Child content is split between <c>&lt;rhx-split-start&gt;</c> and
/// <c>&lt;rhx-split-end&gt;</c> slot helpers.
/// </summary>
/// <example>
/// <code>
/// &lt;rhx-split-panel rhx-position="30"&gt;
///     &lt;rhx-split-start&gt;Sidebar&lt;/rhx-split-start&gt;
///     &lt;rhx-split-end&gt;Main content&lt;/rhx-split-end&gt;
/// &lt;/rhx-split-panel&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-split-panel")]
public class SplitPanelTagHelper : htmxRazorTagHelperBase
{
    /// <inheritdoc/>
    protected override string BlockName => "split-panel";

    /// <summary>
    /// The initial size of the start panel as a percentage (0–100). Default: 50.
    /// </summary>
    [HtmlAttributeName("rhx-position")]
    public int Position { get; set; } = 50;

    /// <summary>
    /// When true, the split is vertical (top/bottom) instead of horizontal (left/right).
    /// </summary>
    [HtmlAttributeName("rhx-vertical")]
    public bool Vertical { get; set; }

    /// <summary>
    /// When true, the panels cannot be resized.
    /// </summary>
    [HtmlAttributeName("rhx-disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// Creates a new SplitPanelTagHelper with URL generation support.
    /// </summary>
    public SplitPanelTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    /// <inheritdoc/>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // Set up slots
        var slots = SlotRenderer.CreateForContext(context);

        // Process children
        await output.GetChildContentAsync();

        // Outer container
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var css = CreateCssBuilder()
            .AddIf(GetModifierClass("vertical"), Vertical)
            .AddIf(GetModifierClass("disabled"), Disabled);
        ApplyBaseAttributes(output, css);

        RenderHtmxAttributes(output);

        // Clamp position
        var pos = Math.Max(0, Math.Min(100, Position));

        // Assemble inner HTML
        output.Content.Clear();

        // Start panel — user-resizable via CSS `resize` (wired up by the component CSS).
        var startSizeProp = Vertical ? "height" : "width";
        output.Content.AppendHtml(
            $"<div class=\"{GetElementClass("start")}\" style=\"flex-basis: {pos}%; {startSizeProp}: {pos}%\">");
        if (slots.Has("start"))
            output.Content.AppendHtml(slots.Get("start")!);
        output.Content.AppendHtml("</div>");

        // Divider — a non-interactive visual separator (the resize grip lives on the start panel).
        var orientation = Vertical ? "horizontal" : "vertical";
        output.Content.AppendHtml(
            $"<div class=\"{GetElementClass("divider")}\" role=\"separator\" aria-orientation=\"{orientation}\">");
        output.Content.AppendHtml($"<div class=\"{GetElementClass("divider-handle")}\"></div>");
        output.Content.AppendHtml("</div>");

        // End panel
        output.Content.AppendHtml($"<div class=\"{GetElementClass("end")}\">");
        if (slots.Has("end"))
            output.Content.AppendHtml(slots.Get("end")!);
        output.Content.AppendHtml("</div>");
    }
}
