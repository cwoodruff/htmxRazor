using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using htmxRazor.Components.Navigation;
using htmxRazor.Demo.Models;

namespace htmxRazor.Demo.Pages.Docs.Components;

public class RatingModel : PageModel
{
    public int ProductRating { get; set; } = 3;

    public List<ComponentProperty> Properties { get; } = new()
    {
        new("rhx-for", "ModelExpression", "-", "ASP.NET Core model expression for two-way binding"),
        new("name", "string", "-", "The form field name"),
        new("value", "string", "-", "The current rating value"),
        new("rhx-label", "string", "-", "Accessible label for the rating group"),
        new("rhx-max", "int", "5", "Maximum number of stars"),
        new("rhx-precision", "double", "1", "Rating precision: 1 or 0.5"),
        new("rhx-readonly", "bool", "false", "Display-only mode"),
        new("rhx-size", "string", "medium", "Rating size: small, medium, large"),
        new("rhx-disabled", "bool", "false", "Whether the rating is disabled"),
    };

    public string BasicCode => @"<rhx-rating name=""rating"" rhx-label=""Rate this product"" />";

    public string PresetValueCode => @"<rhx-rating name=""rating-pre"" rhx-label=""Rating"" value=""3"" />";

    public string HalfStarCode => @"<rhx-rating name=""rating-half"" rhx-label=""Detailed Rating""
           value=""3.5"" rhx-precision=""0.5"" rhx-readonly=""true"" />";

    public string CustomMaxCode => @"<rhx-rating name=""rating-10"" rhx-label=""Extended Rating""
           rhx-max=""10"" value=""7"" />";

    public string ReadonlyCode => @"<rhx-rating name=""rating-ro"" rhx-label=""Average Rating""
           value=""4"" rhx-readonly=""true"" />";

    public string SizesCode => @"<rhx-rating name=""rt-sm"" rhx-label=""Small"" rhx-size=""small"" value=""3"" />
<rhx-rating name=""rt-md"" rhx-label=""Medium (default)"" value=""3"" />
<rhx-rating name=""rt-lg"" rhx-label=""Large"" rhx-size=""large"" value=""3"" />";

    public string StatesCode => @"<rhx-rating name=""rt-dis"" rhx-label=""Disabled rating""
           rhx-disabled=""true"" value=""2"" />";

    public string ModelBindingCode => @"<rhx-rating rhx-for=""ProductRating"" rhx-label=""Product Rating"" />";

    public string HtmxCode => @"<rhx-rating name=""productRating""
           rhx-label=""Rate this product""
           hx-post=""/Docs/Components/Rating?handler=SubmitRating""
           hx-trigger=""change""
           hx-target=""#rating-result""
           hx-include=""this"" />
<div id=""rating-result"">Click a star to submit your rating...</div>";

    public void OnGet()
    {
        ViewData["Breadcrumbs"] = new List<BreadcrumbItem>
        {
            new("Home", "/"),
            new("Components", "/Docs/Components/Rating"),
            new("Rating")
        };
    }

    public IActionResult OnPostSubmitRating(string? productRating)
    {
        if (!int.TryParse(productRating, out var rating) || rating < 1 || rating > 5)
        {
            return Content("<span style=\"color: var(--rhx-color-text-muted);\">Please select a valid rating.</span>", "text/html");
        }

        var stars = new string('\u2605', rating) + new string('\u2606', 5 - rating);
        return Content($"<span style=\"color: var(--rhx-color-text-muted);\">{stars} Thank you for rating <strong>{rating}/5</strong>!</span>", "text/html");
    }
}
