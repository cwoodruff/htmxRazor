using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using htmxRazor.Components.Navigation;
using htmxRazor.Demo.Models;
using htmxRazor.Infrastructure;

namespace htmxRazor.Demo.Pages.Docs.Components;

public class ToastModel : PageModel
{
    public List<ComponentProperty> ContainerProperties { get; } = new()
    {
        new("rhx-position", "string", "top-end", "Position on screen: top-start, top-center, top-end, bottom-start, bottom-center, bottom-end"),
        new("rhx-max", "int", "5", "Maximum number of toasts shown simultaneously"),
        new("rhx-duration", "int", "5000", "Default auto-dismiss duration in milliseconds (0 = no auto-dismiss)"),
    };

    public List<ComponentProperty> ToastProperties { get; } = new()
    {
        new("rhx-variant", "string", "neutral", "Color variant: neutral, brand, success, warning, danger"),
        new("rhx-closable", "bool", "true", "Show a close/dismiss button"),
        new("rhx-duration", "int?", "-", "Auto-dismiss duration in ms (overrides container default, 0 = no auto-dismiss)"),
        new("rhx-icon", "string", "-", "Override the default icon for the variant"),
    };

    public string ContainerCode => @"<!-- Place once in your _Layout.cshtml -->
<rhx-toast-container rhx-position=""top-end""
                     rhx-max=""5""
                     rhx-duration=""5000"" />";

    public string VariantsCode => @"<!-- Toasts are typically created via server-side helpers.
     These static examples show the rendered HTML structure. -->
<rhx-toast rhx-variant=""neutral"">This is a neutral notification.</rhx-toast>
<rhx-toast rhx-variant=""brand"">New feature available!</rhx-toast>
<rhx-toast rhx-variant=""success"">Item saved successfully!</rhx-toast>
<rhx-toast rhx-variant=""warning"">Your session expires in 5 minutes.</rhx-toast>
<rhx-toast rhx-variant=""danger"">Failed to delete the item.</rhx-toast>";

    public string ServerTriggerCode => @"// In your Razor Page handler:
public IActionResult OnPostSave()
{
    // ... save logic ...
    Response.HxToast(""Changes saved!"", ""success"");
    return Partial(""_Form"", Model);
}

// Or with a custom duration:
Response.HxToast(""Processing..."", ""brand"", duration: 8000);";

    public string OobSwapCode => @"// Return a toast via out-of-band swap:
public IActionResult OnPostDelete()
{
    // ... delete logic ...
    return this.HxToastOob(""Item deleted"", ""danger"", duration: 3000);
}";

    public string ButtonTriggerCode => @"<rhx-button rhx-variant=""success""
            hx-post=""/Docs/Components/Toast?handler=ShowSuccess""
            hx-target=""#toast-demo-target""
            hx-swap=""innerHTML"">
    Show Success Toast
</rhx-button>
<rhx-button rhx-variant=""danger""
            hx-post=""/Docs/Components/Toast?handler=ShowError""
            hx-target=""#toast-demo-target""
            hx-swap=""innerHTML"">
    Show Error Toast
</rhx-button>";

    public void OnGet()
    {
        ViewData["Breadcrumbs"] = new List<BreadcrumbItem>
        {
            new("Home", "/"),
            new("Components", "/Docs/Components/Toast"),
            new("Toast")
        };
    }

    // Each handler returns a fully-rendered toast as an htmx out-of-band swap (zero JS):
    // htmx places it into #rhx-toasts; CSS handles auto-dismiss and the close control.
    public IActionResult OnPostShowSuccess() =>
        this.HxToastOob("Item saved successfully!", "success");

    public IActionResult OnPostShowError() =>
        this.HxToastOob("An error occurred while processing your request.", "danger");

    public IActionResult OnPostShowWarning() =>
        this.HxToastOob("Your session expires in 5 minutes.", "warning", duration: 8000);

    public IActionResult OnPostShowBrand() =>
        this.HxToastOob("New feature available! Check it out.", "brand");
}
