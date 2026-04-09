using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using htmxRazor.Components.Navigation;
using htmxRazor.Demo.Models;

namespace htmxRazor.Demo.Pages.Docs.Components;

public class ButtonModel : PageModel
{
    public List<PlaygroundProperty> PlaygroundProperties { get; } = new()
    {
        new("rhx-variant", "select", "neutral", "Color variant", new[] { "neutral", "brand", "success", "warning", "danger" }),
        new("rhx-appearance", "select", "filled", "Visual style", new[] { "filled", "outlined", "plain" }),
        new("rhx-size", "select", "medium", "Button size", new[] { "small", "medium", "large" }),
        new("rhx-disabled", "bool", "false", "Disabled state"),
        new("rhx-loading", "bool", "false", "Loading spinner"),
        new("rhx-pill", "bool", "false", "Pill shape"),
        new("rhx-circle", "bool", "false", "Circle shape"),
        new("content", "string", "Button", "Inner text content"),
    };

    public List<ComponentProperty> Properties { get; } = new()
    {
        new("rhx-variant", "string", "neutral", "Color variant: neutral, brand, success, warning, danger"),
        new("rhx-appearance", "string", "filled", "Visual style: filled, outlined, plain"),
        new("rhx-size", "string", "medium", "Button size: small, medium, large"),
        new("rhx-disabled", "bool", "false", "Whether the button is disabled"),
        new("rhx-loading", "bool", "false", "Shows a spinner and disables interaction"),
        new("rhx-pill", "bool", "false", "Renders with fully rounded corners"),
        new("rhx-circle", "bool", "false", "Renders as a circle for icon-only buttons"),
        new("rhx-href", "string", "-", "When set, renders as an <a> tag with this URL"),
        new("rhx-target", "string", "-", "Link target (e.g. _blank). Only with rhx-href"),
        new("rhx-download", "string", "-", "Download filename. Only with rhx-href"),
        new("type", "string", "button", "HTML button type: button, submit, reset"),
        new("name", "string", "-", "Form submission name"),
        new("value", "string", "-", "Form submission value"),
        new("aria-label", "string", "-", "Accessible label for icon-only buttons"),
    };

    public string VariantsCode => @"<rhx-button rhx-variant=""neutral"">Neutral</rhx-button>
<rhx-button rhx-variant=""brand"">Brand</rhx-button>
<rhx-button rhx-variant=""success"">Success</rhx-button>
<rhx-button rhx-variant=""warning"">Warning</rhx-button>
<rhx-button rhx-variant=""danger"">Danger</rhx-button>";

    public string AppearancesCode => @"<rhx-button rhx-variant=""brand"" rhx-appearance=""filled"">Filled</rhx-button>
<rhx-button rhx-variant=""brand"" rhx-appearance=""outlined"">Outlined</rhx-button>
<rhx-button rhx-variant=""brand"" rhx-appearance=""plain"">Plain</rhx-button>";

    public string SizesCode => @"<rhx-button rhx-variant=""brand"" rhx-size=""small"">Small</rhx-button>
<rhx-button rhx-variant=""brand"">Medium</rhx-button>
<rhx-button rhx-variant=""brand"" rhx-size=""large"">Large</rhx-button>";

    public string StatesCode => @"<rhx-button rhx-variant=""brand"" rhx-disabled=""true"">Disabled</rhx-button>
<rhx-button rhx-variant=""brand"" rhx-loading=""true"">Loading</rhx-button>";

    public string ShapesCode => @"<rhx-button rhx-variant=""brand"">Default</rhx-button>
<rhx-button rhx-variant=""brand"" rhx-pill=""true"">Pill</rhx-button>
<rhx-button rhx-variant=""brand"" rhx-circle=""true"" aria-label=""Add"">+</rhx-button>";

    public string LinksCode => @"<rhx-button rhx-variant=""brand"" rhx-href=""/about"">Navigate</rhx-button>
<rhx-button rhx-variant=""neutral"" rhx-appearance=""outlined""
            rhx-href=""https://github.com"" rhx-target=""_blank"">
    External Link
</rhx-button>";

    public string HtmxCode => @"<rhx-button rhx-variant=""brand""
            hx-get=""/Docs/Components/Button?handler=Greeting""
            hx-target=""#result"" hx-swap=""innerHTML"">
    Load Greeting
</rhx-button>

<rhx-button rhx-variant=""danger"" rhx-appearance=""outlined""
            hx-delete=""/Docs/Components/Button?handler=DeleteItem&amp;id=42""
            hx-confirm=""Are you sure?""
            hx-target=""#result"" hx-swap=""innerHTML"">
    Delete Item
</rhx-button>";

    public void OnGet()
    {
        ViewData["Breadcrumbs"] = new List<BreadcrumbItem>
        {
            new("Home", "/"),
            new("Components", "/Docs/Components/Button"),
            new("Button")
        };
    }

    public IActionResult OnGetPlayground()
    {
        return Partial("_ButtonPlayground");
    }

    public IActionResult OnGetGreeting()
    {
        return Content("<strong>Hello from the server!</strong> This was loaded via htmx GET.", "text/html");
    }

    public IActionResult OnDeleteDeleteItem(int id)
    {
        return Content($"<em>Item #{id} deleted successfully.</em>", "text/html");
    }
}
