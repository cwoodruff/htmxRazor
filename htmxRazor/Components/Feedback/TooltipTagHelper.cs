using System.Net;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Feedback;

/// <summary>
/// Wraps child content and shows a tooltip bubble on hover/focus — pure CSS, no JavaScript.
/// The bubble appears when the wrapper is hovered or contains focus (so keyboard users get it
/// too). Placement is set via a modifier class.
/// </summary>
/// <example>
/// <code>
/// &lt;rhx-tooltip rhx-content="Save your changes"&gt;
///     &lt;rhx-button rhx-variant="brand"&gt;Save&lt;/rhx-button&gt;
/// &lt;/rhx-tooltip&gt;
///
/// &lt;rhx-tooltip rhx-content="Copy" rhx-placement="bottom"&gt;
///     &lt;rhx-button&gt;Copy&lt;/rhx-button&gt;
/// &lt;/rhx-tooltip&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-tooltip")]
public class TooltipTagHelper : TagHelper
{
    /// <summary>The tooltip text content.</summary>
    [HtmlAttributeName("rhx-content")]
    public string Content { get; set; } = "";

    /// <summary>
    /// Placement of the tooltip relative to the trigger. Options: top, bottom, left, right.
    /// Default: top.
    /// </summary>
    [HtmlAttributeName("rhx-placement")]
    public string Placement { get; set; } = "top";

    /// <summary>Whether the tooltip is disabled (bubble not shown). Default: false.</summary>
    [HtmlAttributeName("rhx-disabled")]
    public bool Disabled { get; set; }

    /// <inheritdoc/>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;

        var placement = Placement.ToLowerInvariant();
        var cls = $"rhx-tooltip-wrap rhx-tooltip-wrap--{placement}";
        if (Disabled) cls += " rhx-tooltip-wrap--disabled";
        output.Attributes.SetAttribute("class", cls);

        var childContent = await output.GetChildContentAsync();
        output.Content.SetHtmlContent(childContent);

        // Tooltip bubble — shown on :hover / :focus-within via CSS only.
        if (!Disabled && !string.IsNullOrEmpty(Content))
        {
            output.Content.AppendHtml(
                $"<span class=\"rhx-tooltip\" role=\"tooltip\">{WebUtility.HtmlEncode(Content)}</span>");
        }
    }
}
