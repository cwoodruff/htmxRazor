using Microsoft.AspNetCore.Mvc.RazorPages;
using htmxRazor.Components.Navigation;
using htmxRazor.Demo.Models;

namespace htmxRazor.Demo.Pages.Docs.Components;

public class ComparisonModel : PageModel
{
    public List<ComponentProperty> Properties { get; } = new()
    {
        new("rhx-before", "string", "-", "URL of the \"before\" image"),
        new("rhx-before-alt", "string", "-", "Alt text for the before image"),
        new("rhx-after", "string", "-", "URL of the \"after\" image"),
        new("rhx-after-alt", "string", "-", "Alt text for the after image"),
        new("rhx-position", "int", "50", "Initial handle position as percentage"),
    };

    public string BeforeAfterCode => @"<rhx-comparison
    rhx-before=""/img/comparison-before.svg""
    rhx-before-alt=""Original photo""
    rhx-after=""/img/comparison-after.svg""
    rhx-after-alt=""Grayscale version"" />";

    public string CustomPositionCode => @"<rhx-comparison
    rhx-before=""/img/comparison-before.svg""
    rhx-before-alt=""Original photo""
    rhx-after=""/img/comparison-after.svg""
    rhx-after-alt=""Grayscale version""
    rhx-position=""25"" />";

    public void OnGet()
    {
        ViewData["Breadcrumbs"] = new List<BreadcrumbItem>
        {
            new("Home", "/"),
            new("Components", "/Docs/Components/Comparison"),
            new("Comparison")
        };
    }
}
