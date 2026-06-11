using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using htmxRazor.Components.Navigation;
using htmxRazor.Demo.Models;

namespace htmxRazor.Demo.Pages.Docs.Components;

public class RadialSelectModel : PageModel
{
    public List<ComponentProperty> Properties { get; } = new()
    {
        new("rhx-for", "ModelExpression", "-", "Model binding for the dropdown value (mutually exclusive with name)"),
        new("name", "string", "-", "Form field name for the dropdown value when not using rhx-for"),
        new("rhx-category-name", "string", "-", "Optional. Form field name submitting the active category value"),
        new("rhx-default-category", "string", "-", "Optional. rhx-value of the wedge to activate on initial render"),
        new("rhx-placeholder", "string", "-", "Dropdown placeholder shown before any option is selected"),
        new("rhx-size", "string", "medium", "Control size: small, medium, large"),
        new("rhx-disabled", "bool", "false", "Disables the whole control"),
        new("aria-label", "string", "-", "Accessible name for the category picker"),
    };

    public List<ComponentProperty> OptionProperties { get; } = new()
    {
        new("rhx-value", "string", "required", "Category identifier submitted via rhx-category-name"),
        new("rhx-label", "string", "required", "Accessible name + visible label for the wedge"),
        new("rhx-icon", "string", "-", "Icon name resolved through IconRegistry"),
        new("rhx-color", "variant", "cycle", "brand, success, warning, danger, neutral. Omitted = deterministic cycle"),
        new("hx-get", "string", "-", "Endpoint returning this category's option fragment"),
        new("rhx-disabled", "bool", "false", "Renders the wedge dimmed and non-selectable"),
    };

    public string BasicCode => @"<rhx-radial-select name=""FoodItem"" rhx-category-name=""Category""
                   rhx-default-category=""fruit""
                   rhx-placeholder=""Choose an item…"" aria-label=""Food category"">
    <rhx-radial-option rhx-value=""fruit"" rhx-label=""Fruit"" rhx-icon=""heart"" rhx-color=""danger""
                       hx-get=""/Docs/Components/RadialSelect?handler=Items&cat=fruit"" />
    <rhx-radial-option rhx-value=""veg"" rhx-label=""Veggies"" rhx-icon=""star"" rhx-color=""success""
                       hx-get=""/Docs/Components/RadialSelect?handler=Items&cat=veg"" />
    <rhx-radial-option rhx-value=""meat"" rhx-label=""Meat"" rhx-icon=""grid"" rhx-color=""warning""
                       hx-get=""/Docs/Components/RadialSelect?handler=Items&cat=meat"" />
    <rhx-radial-option rhx-value=""drink"" rhx-label=""Drinks"" rhx-icon=""globe"" rhx-color=""brand""
                       hx-get=""/Docs/Components/RadialSelect?handler=Items&cat=drink"" />
</rhx-radial-select>";

    public string ColorCycleCode => @"<!-- Omit rhx-color and wedges auto-cycle: brand → success → warning → danger → neutral -->
<rhx-radial-select name=""Pick"" aria-label=""Auto-colored"">
    <rhx-radial-option rhx-value=""a"" rhx-label=""One""   rhx-icon=""star""  hx-get=""...?cat=a"" />
    <rhx-radial-option rhx-value=""b"" rhx-label=""Two""   rhx-icon=""heart"" hx-get=""...?cat=b"" />
    <rhx-radial-option rhx-value=""c"" rhx-label=""Three"" rhx-icon=""grid""  hx-get=""...?cat=c"" />
</rhx-radial-select>";

    public void OnGet()
    {
        ViewData["Breadcrumbs"] = new List<BreadcrumbItem>
        {
            new("Home", "/"),
            new("Components", "/Docs/Components/RadialSelect"),
            new("Radial Select")
        };
    }

    private static readonly Dictionary<string, (string Value, string Text)[]> Catalog = new()
    {
        ["fruit"] = new[] { ("apple", "Apple"), ("banana", "Banana"), ("cherry", "Cherry"), ("mango", "Mango") },
        ["veg"] = new[] { ("carrot", "Carrot"), ("broccoli", "Broccoli"), ("spinach", "Spinach") },
        ["meat"] = new[] { ("chicken", "Chicken"), ("beef", "Beef"), ("pork", "Pork") },
        ["drink"] = new[] { ("water", "Water"), ("coffee", "Coffee"), ("juice", "Juice"), ("tea", "Tea") },
    };

    /// <summary>Cascade endpoint: returns the listbox option fragment for a category.</summary>
    public IActionResult OnGetItems(string? cat)
    {
        if (cat == null || !Catalog.TryGetValue(cat, out var items))
            return Content("<div class=\"rhx-radial-select__placeholder\">No items</div>", "text/html");

        var html = string.Concat(items.Select(i =>
            $"<div class=\"rhx-radial-select__option\" role=\"option\" data-value=\"{WebUtility.HtmlEncode(i.Value)}\" aria-selected=\"false\" tabindex=\"-1\">{WebUtility.HtmlEncode(i.Text)}</div>"));
        return Content(html, "text/html");
    }
}
