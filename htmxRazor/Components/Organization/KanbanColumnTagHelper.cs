using htmxRazor.Infrastructure;
using System.Net;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Organization;

/// <summary>
/// A column within a <c>&lt;rhx-kanban&gt;</c> board. Acts as a drop zone for
/// <c>&lt;rhx-kanban-card&gt;</c> elements.
/// </summary>
/// <example>
/// <code>
/// &lt;rhx-kanban-column rhx-column-id="in-progress" rhx-title="In Progress" rhx-max-cards="5"&gt;
///     &lt;rhx-kanban-card rhx-card-id="1"&gt;Task 1&lt;/rhx-kanban-card&gt;
/// &lt;/rhx-kanban-column&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-kanban-column")]
public class KanbanColumnTagHelper : htmxRazorTagHelperBase
{
    /// <inheritdoc/>
    protected override string BlockName => "kanban-column";

    /// <summary>
    /// Unique identifier for this column. Sent with card drop POST requests.
    /// </summary>
    [HtmlAttributeName("rhx-column-id")]
    public string ColumnId { get; set; } = "";

    /// <summary>
    /// Display title for the column header.
    /// </summary>
    [HtmlAttributeName("rhx-title")]
    public string? Title { get; set; }

    /// <summary>
    /// Optional work-in-progress limit. When the number of cards reaches this limit,
    /// a visual indicator is shown.
    /// </summary>
    [HtmlAttributeName("rhx-max-cards")]
    public int? MaxCards { get; set; }

    /// <summary>
    /// Whether cards can be dropped into this column. Default: true.
    /// </summary>
    [HtmlAttributeName("rhx-droppable")]
    public bool Droppable { get; set; } = true;

    /// <summary>
    /// Creates a new <see cref="KanbanColumnTagHelper"/> instance.
    /// </summary>
    public KanbanColumnTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    /// <inheritdoc/>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var css = CreateCssBuilder();
        ApplyBaseAttributes(output, css);

        output.Attributes.SetAttribute("data-rhx-kanban-column", "");
        output.Attributes.SetAttribute("data-rhx-column-id", ColumnId);

        if (MaxCards.HasValue)
            output.Attributes.SetAttribute("data-rhx-max-cards", MaxCards.Value.ToString());

        if (!Droppable)
            output.Attributes.SetAttribute("data-rhx-no-drop", "");

        var childContent = await output.GetChildContentAsync();

        // Render header
        if (!string.IsNullOrWhiteSpace(Title))
        {
            output.Content.AppendHtml("<div class=\"rhx-kanban-column__header\">");
            output.Content.AppendHtml("<span class=\"rhx-kanban-column__title\">");
            output.Content.Append(Title);
            output.Content.AppendHtml("</span>");
            output.Content.AppendHtml("</div>");
        }

        // Render body (drop zone)
        output.Content.AppendHtml("<div class=\"rhx-kanban-column__body\" data-rhx-drop-zone>");
        output.Content.AppendHtml(childContent);
        output.Content.AppendHtml("</div>");

        RenderHtmxAttributes(output);
    }
}
