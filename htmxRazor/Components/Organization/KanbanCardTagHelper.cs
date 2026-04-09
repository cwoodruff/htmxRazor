using htmxRazor.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Organization;

/// <summary>
/// A draggable card within a <c>&lt;rhx-kanban-column&gt;</c>. When dropped into a
/// different column, fires an <c>hx-post</c> request with the card ID, source column,
/// target column, and position.
/// </summary>
/// <example>
/// <code>
/// &lt;rhx-kanban-card rhx-card-id="task-1" rhx-variant="brand"
///                  hx-post="/Board?handler=MoveCard"
///                  hx-target="#kanban-board" hx-swap="outerHTML"&gt;
///     Design the homepage
/// &lt;/rhx-kanban-card&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-kanban-card")]
public class KanbanCardTagHelper : htmxRazorTagHelperBase
{
    /// <inheritdoc/>
    protected override string BlockName => "kanban-card";

    /// <summary>
    /// Unique identifier for this card. Sent with the drop POST request.
    /// </summary>
    [HtmlAttributeName("rhx-card-id")]
    public string CardId { get; set; } = "";

    /// <summary>
    /// Whether this card can be dragged. Default: true.
    /// </summary>
    [HtmlAttributeName("rhx-draggable")]
    public bool Draggable { get; set; } = true;

    /// <summary>
    /// Optional color variant for visual categorization: brand, success, warning, danger.
    /// </summary>
    [HtmlAttributeName("rhx-variant")]
    public string? Variant { get; set; }

    /// <summary>
    /// Creates a new <see cref="KanbanCardTagHelper"/> instance.
    /// </summary>
    public KanbanCardTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    /// <inheritdoc/>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var css = CreateCssBuilder()
            .AddIf(GetModifierClass(Variant!), !string.IsNullOrWhiteSpace(Variant));
        ApplyBaseAttributes(output, css);

        output.Attributes.SetAttribute("data-rhx-kanban-card", "");
        output.Attributes.SetAttribute("data-rhx-card-id", CardId);

        if (Draggable)
        {
            output.Attributes.SetAttribute("draggable", "true");
            output.Attributes.SetAttribute("tabindex", "0");
        }

        RenderHtmxAttributes(output);

        var childContent = await output.GetChildContentAsync();
        output.Content.SetHtmlContent(childContent);
    }
}
