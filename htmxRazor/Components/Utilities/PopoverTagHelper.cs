using htmxRazor.Infrastructure;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Utilities;

/// <summary>
/// A content-rich popover rendered as a native <c>popover="auto"</c> element — the browser
/// handles open/close, light-dismiss, and Escape with no JavaScript. The trigger is any button
/// that points at it via the native <c>popovertarget</c> attribute; for placement next to the
/// trigger, give the trigger an <c>anchor-name</c> matching the popover's id (see example).
/// </summary>
/// <example>
/// <code>
/// &lt;button popovertarget="info" style="anchor-name: --info"&gt;Info&lt;/button&gt;
/// &lt;rhx-popover rhx-id="info" rhx-placement="bottom"&gt;
///     &lt;h3&gt;Details&lt;/h3&gt;
///     &lt;p&gt;Rich content here.&lt;/p&gt;
/// &lt;/rhx-popover&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-popover")]
public class PopoverTagHelper : htmxRazorTagHelperBase
{
    protected override string BlockName => "popover";

    /// <summary>Whether to show a directional arrow. Default: true.</summary>
    [HtmlAttributeName("rhx-arrow")]
    public bool Arrow { get; set; } = true;

    /// <summary>
    /// Placement relative to the trigger: top, top-start, top-end, bottom, bottom-start,
    /// bottom-end, left, right. Default: bottom. (Requires the trigger to declare a matching
    /// <c>anchor-name</c>; otherwise the popover uses the UA default position.)
    /// </summary>
    [HtmlAttributeName("rhx-placement")]
    public string Placement { get; set; } = "bottom";

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var id = string.IsNullOrWhiteSpace(Id) ? $"rhx-popover-{context.UniqueId}" : Id!;
        var anchorName = $"--{id}";
        var positionArea = PlacementToPositionArea(Placement.ToLowerInvariant());

        var css = CreateCssBuilder();
        ApplyBaseAttributes(output, css);

        // Native popover: trigger via [popovertarget="{id}"]; browser handles show/hide/dismiss.
        output.Attributes.SetAttribute("id", id);
        output.Attributes.SetAttribute("popover", "auto");

        var existingStyle = output.Attributes["style"]?.Value?.ToString();
        var posStyle = $"position-anchor:{anchorName};position-area:{positionArea};position-try-fallbacks:flip-block,flip-inline";
        output.Attributes.SetAttribute("style",
            string.IsNullOrWhiteSpace(existingStyle) ? posStyle : existingStyle!.TrimEnd(';', ' ') + ";" + posStyle);

        var childContent = await output.GetChildContentAsync();

        output.Content.AppendHtml("<div class=\"rhx-popover__content\">");
        output.Content.AppendHtml(childContent);
        output.Content.AppendHtml("</div>");

        if (Arrow)
            output.Content.AppendHtml("<div class=\"rhx-popover__arrow\"></div>");

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
        _ => "bottom"
    };
}
