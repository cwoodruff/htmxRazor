using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using htmxRazor.Components.Navigation;
using htmxRazor.Demo.Models;

namespace htmxRazor.Demo.Pages.Docs.Components;

public class PopoverModel : PageModel
{
    public List<ComponentProperty> Properties { get; } = new()
    {
        new("id", "string", "auto", "Popover id; the trigger points at it via popovertarget"),
        new("rhx-placement", "string", "bottom", "Position: top, bottom, left, right (+ -start/-end)"),
        new("rhx-arrow", "bool", "true", "Show a directional arrow"),
    };

    public string ClickTriggerCode => @"<button class=""rhx-button rhx-button--brand""
        popovertarget=""pop-click"" style=""anchor-name: --pop-click"">
    Show Popover
</button>
<rhx-popover id=""pop-click"" rhx-placement=""bottom"">
    <h4>Popover Title</h4>
    <p>This is a rich content popover. It supports
       <strong>HTML</strong>, images, and interactive elements.</p>
</rhx-popover>";

    public string HoverTriggerCode => @"<button class=""rhx-button rhx-button--neutral""
        popovertarget=""pop-up"" style=""anchor-name: --pop-up"">
    Open above
</button>
<rhx-popover id=""pop-up"" rhx-placement=""top"">
    <p>This popover opens above its trigger.</p>
</rhx-popover>";

    public string PlacementsCode => @"<button popovertarget=""pop-top"" style=""anchor-name: --pop-top"">Top</button>
<rhx-popover id=""pop-top"" rhx-placement=""top""><p>Top placement</p></rhx-popover>

<button popovertarget=""pop-bottom"" style=""anchor-name: --pop-bottom"">Bottom</button>
<rhx-popover id=""pop-bottom"" rhx-placement=""bottom""><p>Bottom placement</p></rhx-popover>

<button popovertarget=""pop-left"" style=""anchor-name: --pop-left"">Left</button>
<rhx-popover id=""pop-left"" rhx-placement=""left""><p>Left placement</p></rhx-popover>

<button popovertarget=""pop-right"" style=""anchor-name: --pop-right"">Right</button>
<rhx-popover id=""pop-right"" rhx-placement=""right""><p>Right placement</p></rhx-popover>";

    public string NoArrowCode => @"<button class=""rhx-button rhx-button--brand""
        popovertarget=""pop-noarrow"" style=""anchor-name: --pop-noarrow"">
    No Arrow
</button>
<rhx-popover id=""pop-noarrow"" rhx-placement=""bottom"" rhx-arrow=""false"">
    <p>This popover has no directional arrow.</p>
</rhx-popover>";

    public string HtmxCode => @"<button class=""rhx-button rhx-button--brand""
        popovertarget=""user-popover"" style=""anchor-name: --user-popover""
        hx-get=""/Docs/Components/Popover?handler=UserCard""
        hx-target=""#user-popover .rhx-popover__content""
        hx-trigger=""click once"">
    Load User Card
</button>
<rhx-popover id=""user-popover"" rhx-placement=""bottom"">
    <p>Loading...</p>
</rhx-popover>";

    public void OnGet()
    {
        ViewData["Breadcrumbs"] = new List<BreadcrumbItem>
        {
            new("Home", "/"),
            new("Components", "/Docs/Components/Popover"),
            new("Popover")
        };
    }

    public IActionResult OnGetUserCard()
    {
        return Content("""
            <div style="display: flex; align-items: center; gap: var(--rhx-space-md); min-width: 200px;">
                <div style="width: 48px; height: 48px; border-radius: 50%; background: var(--rhx-color-brand-100); display: flex; align-items: center; justify-content: center; font-weight: var(--rhx-font-weight-bold); color: var(--rhx-color-brand-700);">JS</div>
                <div>
                    <div style="font-weight: var(--rhx-font-weight-bold);">Jane Smith</div>
                    <div style="color: var(--rhx-color-text-muted); font-size: var(--rhx-font-size-sm);">Software Engineer</div>
                </div>
            </div>
            """, "text/html");
    }
}
