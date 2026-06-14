using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using htmxRazor.Components.Navigation;
using htmxRazor.Demo.Models;

namespace htmxRazor.Demo.Pages.Docs.Components;

public class SliderModel : PageModel
{
    public int Volume { get; set; } = 50;

    public List<ComponentProperty> Properties { get; } = new()
    {
        new("rhx-for", "ModelExpression", "-", "ASP.NET Core model expression for two-way binding"),
        new("name", "string", "-", "The form field name"),
        new("value", "string", "-", "The current value"),
        new("rhx-label", "string", "-", "Label text displayed above the slider"),
        new("rhx-hint", "string", "-", "Hint text displayed below the slider"),
        new("rhx-min", "string", "0", "Minimum value"),
        new("rhx-max", "string", "100", "Maximum value"),
        new("rhx-step", "string", "1", "Step increment"),
        new("rhx-show-value", "bool", "false", "Show a static output of the initial value (does not live-update without JS)"),
        new("rhx-size", "string", "medium", "Slider size: small, medium, large"),
        new("rhx-disabled", "bool", "false", "Whether the slider is disabled"),
    };

    public string BasicCode => @"<rhx-slider name=""volume"" rhx-label=""Volume"" value=""50"" />";

    public string CustomRangeCode => @"<rhx-slider name=""custom"" rhx-label=""Custom Range""
           rhx-min=""10"" rhx-max=""200"" rhx-step=""10""
           value=""80"" />";

    public string TooltipCode => @"<rhx-slider name=""brightness"" rhx-label=""Brightness""
           value=""75"" rhx-show-value=""true"" />
<rhx-slider name=""contrast"" rhx-label=""Contrast""
           value=""60"" rhx-show-value=""true"" />";

    public string HintCode => @"<rhx-slider name=""opacity"" rhx-label=""Opacity""
           value=""100""
           rhx-hint=""0 = fully transparent, 100 = fully opaque"" />";

    public string SizesCode => @"<rhx-slider name=""sl-sm"" rhx-label=""Small slider"" rhx-size=""small"" value=""30"" />
<rhx-slider name=""sl-md"" rhx-label=""Medium slider (default)"" value=""50"" />
<rhx-slider name=""sl-lg"" rhx-label=""Large slider"" rhx-size=""large"" value=""70"" />";

    public string StatesCode => @"<rhx-slider name=""sl-dis"" rhx-label=""Disabled slider""
           rhx-disabled=""true"" value=""40"" />";

    public string ModelBindingCode => @"<rhx-slider rhx-for=""Volume"" rhx-label=""Volume"" />";

    public string HtmxCode => @"<rhx-slider name=""fontSize"" rhx-label=""Font Size""
           value=""16"" rhx-min=""8"" rhx-max=""48"" rhx-step=""1""
           hx-get=""/Docs/Components/Slider?handler=FontPreview""
           hx-trigger=""input changed delay:200ms""
           hx-target=""#slider-preview""
           hx-include=""this"" />
<div id=""slider-preview"">
    <span style=""font-size: 16px;"">The quick brown fox...</span>
</div>";

    public void OnGet()
    {
        ViewData["Breadcrumbs"] = new List<BreadcrumbItem>
        {
            new("Home", "/"),
            new("Components", "/Docs/Components/Slider"),
            new("Slider")
        };
    }

    public IActionResult OnGetFontPreview(int fontSize)
    {
        if (fontSize < 8) fontSize = 8;
        if (fontSize > 48) fontSize = 48;
        return Content($"<span style=\"font-size: {fontSize}px;\">The quick brown fox jumps over the lazy dog. ({fontSize}px)</span>", "text/html");
    }
}
