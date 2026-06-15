using htmxRazor.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Organization;

/// <summary>
/// Container for a Kanban board layout. Wraps <c>&lt;rhx-kanban-column&gt;</c> children
/// in a horizontally scrollable flex container.
/// </summary>
/// <example>
/// <code>
/// &lt;rhx-kanban&gt;
///     &lt;rhx-kanban-column rhx-column-id="todo" rhx-title="To Do"&gt;
///         &lt;rhx-kanban-card rhx-card-id="1"&gt;Task 1&lt;/rhx-kanban-card&gt;
///     &lt;/rhx-kanban-column&gt;
///     &lt;rhx-kanban-column rhx-column-id="done" rhx-title="Done" /&gt;
/// &lt;/rhx-kanban&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-kanban")]
public class KanbanTagHelper : htmxRazorTagHelperBase
{
    /// <inheritdoc/>
    protected override string BlockName => "kanban";

    /// <summary>
    /// Creates a new <see cref="KanbanTagHelper"/> instance.
    /// </summary>
    public KanbanTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    /// <inheritdoc/>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var css = CreateCssBuilder();
        ApplyBaseAttributes(output, css);

        AriaAttributeHelper.RoleGroup(output);

        RenderHtmxAttributes(output);

        var childContent = await output.GetChildContentAsync();
        output.Content.SetHtmlContent(childContent);
    }
}
