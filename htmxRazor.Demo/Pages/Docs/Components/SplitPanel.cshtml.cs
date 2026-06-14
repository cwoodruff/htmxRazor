using Microsoft.AspNetCore.Mvc.RazorPages;
using htmxRazor.Components.Navigation;
using htmxRazor.Demo.Models;

namespace htmxRazor.Demo.Pages.Docs.Components;

public class SplitPanelModel : PageModel
{
    public List<ComponentProperty> Properties { get; } = new()
    {
        new("rhx-position", "int", "50", "Initial start-panel size as a percentage (0-100)"),
        new("rhx-vertical", "bool", "false", "Use vertical (top/bottom) layout instead of horizontal"),
        new("rhx-disabled", "bool", "false", "Turns off resizing (fixed panels)"),
    };

    public string HorizontalCode => @"<rhx-split-panel rhx-position=""30"">
    <rhx-split-start>
        <div style=""padding: var(--rhx-space-lg);"">
            <h4 style=""margin:0 0 var(--rhx-space-sm) 0;"">Sidebar</h4>
            <p>Navigation or filter panel. Drag the panel's edge grip to resize.</p>
        </div>
    </rhx-split-start>
    <rhx-split-end>
        <div style=""padding: var(--rhx-space-lg);"">
            <h4 style=""margin:0 0 var(--rhx-space-sm) 0;"">Main Content</h4>
            <p>Primary content area. The end panel takes the remaining space.</p>
        </div>
    </rhx-split-end>
</rhx-split-panel>";

    public string VerticalCode => @"<rhx-split-panel rhx-position=""40"" rhx-vertical>
    <rhx-split-start>
        <div style=""padding: var(--rhx-space-lg);"">
            <h4 style=""margin:0 0 var(--rhx-space-sm) 0;"">Editor</h4>
            <p>Code or text editing area.</p>
        </div>
    </rhx-split-start>
    <rhx-split-end>
        <div style=""padding: var(--rhx-space-lg);"">
            <h4 style=""margin:0 0 var(--rhx-space-sm) 0;"">Output</h4>
            <p>Console or preview area below the editor.</p>
        </div>
    </rhx-split-end>
</rhx-split-panel>";

    public string DisabledCode => @"<rhx-split-panel rhx-position=""60"" rhx-disabled>
    <rhx-split-start>
        <div style=""padding: var(--rhx-space-lg);"">
            <p>Fixed start panel (60%).</p>
        </div>
    </rhx-split-start>
    <rhx-split-end>
        <div style=""padding: var(--rhx-space-lg);"">
            <p>Fixed end panel (40%).</p>
        </div>
    </rhx-split-end>
</rhx-split-panel>";

    public void OnGet()
    {
        ViewData["Breadcrumbs"] = new List<BreadcrumbItem>
        {
            new("Home", "/"),
            new("Components", "/Docs/Components/SplitPanel"),
            new("Split Panel")
        };
    }
}
