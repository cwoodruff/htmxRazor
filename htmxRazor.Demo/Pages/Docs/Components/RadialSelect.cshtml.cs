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
        new("rhx-category-name", "string", "-", "Form field name for the category <select>"),
        new("rhx-default-category", "string", "-", "rhx-value of the category selected on initial render"),
        new("hx-get", "string", "-", "Cascade endpoint returning the item <option> set for the chosen category"),
        new("rhx-placeholder", "string", "-", "Item-select placeholder shown before options load"),
        new("rhx-size", "string", "medium", "Control size: small, medium, large"),
        new("rhx-disabled", "bool", "false", "Disables the whole control"),
        new("aria-label", "string", "-", "Accessible name for the control"),
    };

    public List<ComponentProperty> OptionProperties { get; } = new()
    {
        new("rhx-value", "string", "required", "Category value (an <option> in the category <select>)"),
        new("rhx-label", "string", "required", "Category option's visible label"),
        new("rhx-disabled", "bool", "false", "Renders the category option disabled"),
    };

    public string BasicCode => @"<rhx-radial-select name=""FoodItem"" rhx-category-name=""cat""
                   rhx-default-category=""fruit""
                   hx-get=""/Docs/Components/RadialSelect?handler=Items""
                   rhx-placeholder=""Choose an item…"" aria-label=""Food"">
    <rhx-radial-option rhx-value=""fruit"" rhx-label=""Fruit"" />
    <rhx-radial-option rhx-value=""veg"" rhx-label=""Veggies"" />
    <rhx-radial-option rhx-value=""meat"" rhx-label=""Meat"" />
    <rhx-radial-option rhx-value=""drink"" rhx-label=""Drinks"" />
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

    /// <summary>Cascade endpoint: returns the item &lt;option&gt; set for a category, pre-selecting
    /// <paramref name="selected"/> when it belongs to that category.</summary>
    public IActionResult OnGetItems(string? cat, string? selected)
    {
        if (cat == null || !Catalog.TryGetValue(cat, out var items))
            return Content("<option value=\"\">No items</option>", "text/html");

        var html = string.Concat(items.Select(i =>
        {
            var sel = string.Equals(i.Value, selected, StringComparison.Ordinal) ? " selected" : "";
            return $"<option value=\"{WebUtility.HtmlEncode(i.Value)}\"{sel}>{WebUtility.HtmlEncode(i.Text)}</option>";
        }));
        return Content(html, "text/html");
    }
}
