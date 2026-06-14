using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using htmxRazor.Components.Navigation;
using htmxRazor.Demo.Models;

namespace htmxRazor.Demo.Pages.Docs.Components;

public class ComboboxModel : PageModel
{
    public List<SelectListItem> Countries { get; } = new()
    {
        new("United States", "US"),
        new("Canada", "CA"),
        new("United Kingdom", "UK"),
        new("Germany", "DE"),
        new("France", "FR"),
        new("Japan", "JP"),
        new("Australia", "AU"),
        new SelectListItem { Text = "Antarctica", Value = "AQ", Disabled = true }
    };

    public List<SelectListItem> Cities { get; } = new()
    {
        new("New York", "NYC"),
        new("Los Angeles", "LA"),
        new("Chicago", "CHI"),
        new("Houston", "HOU"),
        new("Phoenix", "PHX")
    };

    public enum Priority
    {
        [Display(Name = "Low Priority")]
        Low,
        [Display(Name = "Medium Priority")]
        Medium,
        [Display(Name = "High Priority")]
        High,
        [Display(Name = "Critical")]
        Critical
    }

    public Priority SelectedPriority { get; set; } = Priority.Medium;

    public List<ComponentProperty> Properties { get; } = new()
    {
        new("rhx-for", "ModelExpression", "-", "ASP.NET Core model expression for two-way binding"),
        new("name", "string", "-", "The form field name"),
        new("value", "string", "-", "The current value"),
        new("rhx-label", "string", "-", "Label text displayed above the combobox"),
        new("rhx-hint", "string", "-", "Hint text displayed below the combobox"),
        new("rhx-size", "string", "medium", "Combobox size: small, medium, large"),
        new("rhx-disabled", "bool", "false", "Whether the combobox is disabled"),
        new("rhx-readonly", "bool", "false", "Whether the combobox is read-only"),
        new("rhx-placeholder", "string", "-", "Placeholder text"),
        new("rhx-items", "List<SelectListItem>", "-", "Server-side items to render as options"),
        new("rhx-filled", "bool", "false", "Use filled appearance"),
        new("rhx-server-filter", "bool", "false", "Filter options server-side via htmx instead of client-side JS"),
        new("rhx-search-param", "string", "q", "Query parameter name for the search text when server filtering is enabled"),
    };

    public string BasicCode => @"<rhx-combobox rhx-label=""City"" name=""city""
           rhx-placeholder=""Search cities...""
           rhx-items=""Model.Cities"" />";

    public string EnumBindingCode => @"<rhx-combobox rhx-label=""Priority"" name=""cbo-priority""
           rhx-placeholder=""Search priorities...""
           rhx-for=""SelectedPriority"" />";

    public string FilledCode => @"<rhx-combobox rhx-label=""Country"" name=""cbo-filled""
           rhx-filled=""true""
           rhx-placeholder=""Search countries...""
           rhx-items=""Model.Countries"" />";

    public string ChildOptionsCode => @"<rhx-combobox rhx-label=""Fruit"" name=""fruit""
           rhx-placeholder=""Search fruits..."">
    <rhx-option value=""apple"">Apple</rhx-option>
    <rhx-option value=""banana"">Banana</rhx-option>
    <rhx-option value=""cherry"">Cherry</rhx-option>
    <rhx-option value=""grape"">Grape</rhx-option>
    <rhx-option value=""mango"">Mango</rhx-option>
</rhx-combobox>";

    public string SizesCode => @"<rhx-combobox rhx-label=""Small"" rhx-size=""small""
           rhx-placeholder=""Small"" rhx-items=""Model.Cities"" name=""cbo-sm"" />
<rhx-combobox rhx-label=""Medium (default)""
           rhx-placeholder=""Medium"" rhx-items=""Model.Cities"" name=""cbo-md"" />
<rhx-combobox rhx-label=""Large"" rhx-size=""large""
           rhx-placeholder=""Large"" rhx-items=""Model.Cities"" name=""cbo-lg"" />";

    public string StatesCode => @"<rhx-combobox rhx-label=""Disabled"" name=""cbo-dis""
           value=""NYC"" rhx-disabled=""true""
           rhx-items=""Model.Cities"" />
<rhx-combobox rhx-label=""Readonly"" name=""cbo-ro""
           value=""NYC"" rhx-readonly=""true""
           rhx-items=""Model.Cities"" />";

    public string HtmxCode => @"<rhx-combobox rhx-label=""Search Users"" name=""userId""
           rhx-placeholder=""Type a name...""
           rhx-server-filter=""true""
           hx-get=""/Docs/Components/Combobox?handler=SearchUsers""
           hx-trigger=""input changed delay:300ms""
           hx-target=""next .rhx-combobox__listbox"" />";

    public void OnGet()
    {
        ViewData["Breadcrumbs"] = new List<BreadcrumbItem>
        {
            new("Home", "/"),
            new("Components", "/Docs/Components/Combobox"),
            new("Combobox")
        };
    }

    // The native datalist's <input> submits its typed text under the field name ("userId").
    // htmx sends that value; the handler returns <option> elements that replace the datalist.
    public IActionResult OnGetSearchUsers(string? userId)
    {
        var allUsers = new[]
        {
            "Alice Johnson", "Bob Smith", "Carol Williams", "David Brown",
            "Eve Davis", "Frank Miller", "Grace Wilson", "Henry Moore"
        };

        var matches = string.IsNullOrWhiteSpace(userId)
            ? allUsers
            : allUsers.Where(u => u.Contains(userId, StringComparison.OrdinalIgnoreCase)).ToArray();

        var options = string.Join("", matches.Select(m =>
            $"<option value=\"{System.Net.WebUtility.HtmlEncode(m)}\"></option>"));
        return Content(options, "text/html");
    }
}
