using htmxRazor.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Navigation;

/// <summary>
/// Renders a tree view container. Contains <c>&lt;rhx-tree-item&gt;</c> children
/// that can be nested for hierarchical display. Supports single, multiple, and
/// leaf-only selection modes.
/// </summary>
/// <remarks>
/// <para>
/// JavaScript (<c>rhx-tree.js</c>) handles expand/collapse, keyboard navigation
/// (arrow keys, Home/End, Enter/Space), selection management, and lazy-loading
/// via htmx integration.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;rhx-tree rhx-selection="single" aria-label="File explorer"&gt;
///     &lt;rhx-tree-item rhx-label="Documents" rhx-expanded&gt;
///         &lt;rhx-tree-item rhx-label="report.pdf" /&gt;
///         &lt;rhx-tree-item rhx-label="readme.txt" /&gt;
///     &lt;/rhx-tree-item&gt;
/// &lt;/rhx-tree&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-tree")]
public class TreeTagHelper : htmxRazorTagHelperBase
{
    /// <inheritdoc/>
    protected override string BlockName => "tree";

    /// <summary>
    /// The selection mode. Options:
    /// "single" (default): one item selected at a time.
    /// "multiple": toggle selection on multiple items.
    /// "leaf": only leaf items (no children) can be selected.
    /// </summary>
    [HtmlAttributeName("rhx-selection")]
    public string Selection { get; set; } = "single";

    /// <summary>
    /// Accessible label for the tree navigation region.
    /// </summary>
    [HtmlAttributeName("aria-label")]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Creates a new TreeTagHelper with URL generation support.
    /// </summary>
    public TreeTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var css = CreateCssBuilder();
        ApplyBaseAttributes(output, css);

        output.Attributes.SetAttribute("role", "tree");

        if (!string.IsNullOrWhiteSpace(AriaLabel))
        {
            output.Attributes.SetAttribute("aria-label", AriaLabel);
        }

        RenderHtmxAttributes(output);
    }
}
