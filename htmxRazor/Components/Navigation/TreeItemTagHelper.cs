using System.Net;
using htmxRazor.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Navigation;

/// <summary>
/// Renders a tree item inside an <c>&lt;rhx-tree&gt;</c> or another
/// <c>&lt;rhx-tree-item&gt;</c>. Supports expand/collapse, selection,
/// lazy-loading, and htmx integration.
/// </summary>
/// <remarks>
/// <para>
/// When <c>rhx-label</c> is set, the label comes from the property and child content
/// populates the children group. When <c>rhx-label</c> is not set, the child content
/// becomes the label text (leaf item). For branch items with nested children,
/// always use the <c>rhx-label</c> property.
/// </para>
/// <para>
/// Lazy-loaded items use <c>rhx-lazy</c> with htmx attributes. JavaScript dispatches
/// a <c>toggle</c> event on first expand, which htmx can listen for via
/// <c>hx-trigger="toggle once"</c>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;!-- Branch item with nested children --&gt;
/// &lt;rhx-tree-item rhx-label="Documents" rhx-expanded&gt;
///     &lt;rhx-tree-item&gt;report.pdf&lt;/rhx-tree-item&gt;
///     &lt;rhx-tree-item&gt;readme.txt&lt;/rhx-tree-item&gt;
/// &lt;/rhx-tree-item&gt;
///
/// &lt;!-- Lazy-loaded item --&gt;
/// &lt;rhx-tree-item rhx-lazy hx-get="/api/children/1"
///                hx-trigger="toggle once"
///                hx-target="find .rhx-tree__children"
///                hx-swap="innerHTML"&gt;
///     Projects
/// &lt;/rhx-tree-item&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-tree-item", ParentTag = "rhx-tree")]
[HtmlTargetElement("rhx-tree-item", ParentTag = "rhx-tree-item")]
public class TreeItemTagHelper : htmxRazorTagHelperBase
{
    /// <inheritdoc/>
    protected override string BlockName => "tree";

    /// <summary>
    /// The display label for this item. When set, child content populates the
    /// children group. When not set, child content becomes the label (leaf item).
    /// </summary>
    [HtmlAttributeName("rhx-label")]
    public string? Label { get; set; }

    /// <summary>
    /// Whether this item's children group is expanded.
    /// </summary>
    [HtmlAttributeName("rhx-expanded")]
    public bool Expanded { get; set; }

    /// <summary>
    /// Whether this item is selected.
    /// </summary>
    [HtmlAttributeName("rhx-selected")]
    public bool Selected { get; set; }

    /// <summary>
    /// Whether this item is disabled. Disabled items cannot be expanded or selected.
    /// </summary>
    [HtmlAttributeName("rhx-disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// Whether this item lazily loads its children on first expand.
    /// JavaScript dispatches a <c>toggle</c> event that htmx can intercept.
    /// </summary>
    [HtmlAttributeName("rhx-lazy")]
    public bool Lazy { get; set; }

    /// <summary>
    /// Creates a new TreeItemTagHelper with URL generation support.
    /// </summary>
    public TreeItemTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    /// <inheritdoc/>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var childContent = await output.GetChildContentAsync();
        var childHtml = childContent.GetContent();

        // Determine label and children
        string labelHtml;
        string childrenHtml;
        bool hasChildren;

        if (!string.IsNullOrWhiteSpace(Label))
        {
            // Label from property (needs encoding); child content is children
            labelHtml = WebUtility.HtmlEncode(Label);
            childrenHtml = childHtml;
            hasChildren = !string.IsNullOrWhiteSpace(childHtml);
        }
        else
        {
            // Child content is the label (already HTML-safe from Razor)
            labelHtml = childHtml.Trim();
            childrenHtml = "";
            hasChildren = false;
        }

        // Branch items (with children, or lazy-loaded) use a native <details>; the browser
        // handles expand/collapse and keyboard with no JS. Lazy children load via htmx on the
        // native `toggle` event (hx-trigger="toggle once"). Leaf items are a plain element.
        var isBranch = hasChildren || Lazy;

        output.TagName = isBranch ? "details" : "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var css = new CssClassBuilder("rhx-tree__item")
            .AddIf("rhx-tree__item--selected", Selected)
            .AddIf("rhx-tree__item--disabled", Disabled)
            .AddIf("rhx-tree__item--lazy", Lazy)
            .AddIf("rhx-tree__item--leaf", !isBranch);

        if (!string.IsNullOrWhiteSpace(CssClass))
            css.Add(CssClass);

        output.Attributes.SetAttribute("class", css.Build());

        if (isBranch && Expanded)
            output.Attributes.SetAttribute("open", "open");

        if (Selected)
            output.Attributes.SetAttribute("aria-selected", "true");

        if (Disabled)
            output.Attributes.SetAttribute("aria-disabled", "true");

        if (!string.IsNullOrWhiteSpace(Id))
            output.Attributes.SetAttribute("id", Id);

        RenderHtmxAttributes(output);

        // Assemble inner content
        output.Content.Clear();

        if (isBranch)
        {
            // <summary> is the focusable, clickable disclosure label.
            output.Content.AppendHtml("<summary class=\"rhx-tree__item-content\">");
            output.Content.AppendHtml("<span class=\"rhx-tree__expand-icon\" aria-hidden=\"true\"></span>");
            output.Content.AppendHtml($"<span class=\"rhx-tree__item-label\">{labelHtml}</span>");
            output.Content.AppendHtml("</summary>");

            // Native <details> shows/hides this group; no `hidden` attribute needed.
            output.Content.AppendHtml("<div class=\"rhx-tree__children\" role=\"group\">");
            output.Content.AppendHtml(childrenHtml);
            output.Content.AppendHtml("</div>");
        }
        else
        {
            output.Content.AppendHtml("<div class=\"rhx-tree__item-content\">");
            output.Content.AppendHtml($"<span class=\"rhx-tree__item-label\">{labelHtml}</span>");
            output.Content.AppendHtml("</div>");
        }
    }
}
