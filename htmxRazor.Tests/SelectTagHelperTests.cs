using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using htmxRazor.Components.Forms;
using Xunit;

namespace htmxRazor.Tests;

// rhx-select renders a native <select> (no JS). Listbox behavior, keyboard, type-ahead,
// and accessibility come from the browser; the wrapper carries label/hint/error.
public class SelectTagHelperTests : TagHelperTestBase
{
    private SelectTagHelper CreateHelper() => new(CreateUrlHelperFactory());

    // ── Test model ──

    private enum Priority
    {
        [Display(Name = "Low Priority")]
        Low,
        Medium,
        [Display(Name = "High Priority")]
        High
    }

    private class TestModel
    {
        public Priority Priority { get; set; }
        public string? Country { get; set; }
    }

    private static ModelExpression CreateModelExpressionFor(string propertyName, object? value = null)
    {
        var provider = new EmptyModelMetadataProvider();
        var metadata = provider.GetMetadataForProperty(typeof(TestModel), propertyName);
        var explorer = new ModelExplorer(provider, metadata, value);
        return new ModelExpression(propertyName, explorer);
    }

    private async Task<string> RenderAsync(SelectTagHelper helper, ViewContext? vc = null)
    {
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");
        helper.ViewContext = vc ?? CreateViewContext();
        await helper.ProcessAsync(context, output);
        return output.Content.GetContent();
    }

    // ── Element rendering ──

    [Fact]
    public async Task Renders_Div_Wrapper()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");
        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        Assert.Equal("div", output.TagName);
        Assert.True(HasClass(output, "rhx-select"));
    }

    [Fact]
    public async Task Renders_Native_Select_Control()
    {
        var content = await RenderAsync(CreateHelper());
        Assert.Contains("<select", content);
        Assert.Contains("rhx-select__control", content);
        Assert.Contains("</select>", content);
    }

    // ── Name / id ──

    [Fact]
    public async Task Name_And_Id_On_Select()
    {
        var helper = CreateHelper();
        helper.Name = "country";
        var content = await RenderAsync(helper);
        Assert.Contains("name=\"country\"", content);
        Assert.Contains("id=\"country\"", content);
    }

    // ── Placeholder ──

    [Fact]
    public async Task Placeholder_Renders_Empty_Option_Selected_When_No_Value()
    {
        var helper = CreateHelper();
        helper.Placeholder = "Choose a country";
        var content = await RenderAsync(helper);
        Assert.Contains("<option value=\"\"", content);
        Assert.Contains("selected hidden", content);
        Assert.Contains("Choose a country", content);
    }

    [Fact]
    public async Task Required_Placeholder_Option_Is_Disabled()
    {
        var helper = CreateHelper();
        helper.Required = true;
        helper.Placeholder = "Choose";
        var content = await RenderAsync(helper);
        // The empty placeholder option is disabled so `required` fails until a real choice.
        Assert.Contains("<option value=\"\" disabled", content);
    }

    // ── Value → selected option ──

    [Fact]
    public async Task Value_Marks_Item_Selected()
    {
        var helper = CreateHelper();
        helper.Value = "US";
        helper.Items = new List<SelectListItem> { new("United States", "US"), new("Canada", "CA") };
        var content = await RenderAsync(helper);
        Assert.Contains("<option value=\"US\" selected>United States</option>", content);
        Assert.Contains("<option value=\"CA\">Canada</option>", content);
    }

    // ── Label ──

    [Fact]
    public async Task Label_Has_Id_And_For()
    {
        var helper = CreateHelper();
        helper.Label = "Country";
        helper.Name = "country";
        var content = await RenderAsync(helper);
        Assert.Contains("rhx-select__label", content);
        Assert.Contains("id=\"country-label\"", content);
        Assert.Contains("for=\"country\"", content);
        Assert.Contains("aria-labelledby=\"country-label\"", content);
    }

    // ── Hint ──

    [Fact]
    public async Task Hint_Renders()
    {
        var helper = CreateHelper();
        helper.Hint = "Select your country";
        helper.Name = "country";
        var content = await RenderAsync(helper);
        Assert.Contains("rhx-select__hint", content);
        Assert.Contains("Select your country", content);
    }

    // ── Error ──

    [Fact]
    public async Task Error_Adds_Modifier_And_Message()
    {
        var helper = CreateHelper();
        helper.Name = "country";
        var vc = CreateViewContext();
        vc.ModelState.AddModelError("country", "Country is required");

        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");
        helper.ViewContext = vc;
        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-select--error"));
        var content = output.Content.GetContent();
        Assert.Contains("Country is required", content);
        Assert.Contains("aria-invalid=\"true\"", content);
    }

    // ── States ──

    [Fact]
    public async Task Disabled_Adds_Modifier_And_Native_Attribute()
    {
        var helper = CreateHelper();
        helper.Disabled = true;
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");
        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-select--disabled"));
        Assert.Contains(" disabled", output.Content.GetContent());
    }

    [Fact]
    public async Task Required_Adds_Native_Required_On_Select()
    {
        var helper = CreateHelper();
        helper.Required = true;
        var content = await RenderAsync(helper);
        // Native <select> is focusable + validatable, so plain `required` works (issue #14 fixed).
        Assert.Contains(" required", content);
        Assert.Contains("<select", content);
    }

    [Fact]
    public async Task Readonly_Disables_Select_And_Mirrors_Value()
    {
        var helper = CreateHelper();
        helper.Name = "country";
        helper.Value = "US";
        helper.Readonly = true;
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");
        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-select--readonly"));
        var content = output.Content.GetContent();
        Assert.Contains(" disabled", content);
        // A disabled select doesn't submit, so a hidden mirror carries the value.
        Assert.Contains("type=\"hidden\" name=\"country\" value=\"US\"", content);
    }

    // ── Size ──

    [Fact]
    public async Task Small_Size_Adds_Modifier()
    {
        var helper = CreateHelper();
        helper.Size = "small";
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");
        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);
        Assert.True(HasClass(output, "rhx-select--small"));
    }

    [Fact]
    public async Task Large_Size_Adds_Modifier()
    {
        var helper = CreateHelper();
        helper.Size = "large";
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");
        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);
        Assert.True(HasClass(output, "rhx-select--large"));
    }

    // ── Multiple ──

    [Fact]
    public async Task Multiple_Adds_Modifier_And_Native_Multiple()
    {
        var helper = CreateHelper();
        helper.Multiple = true;
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");
        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-select--multiple"));
        var content = output.Content.GetContent();
        Assert.Contains(" multiple", content);
    }

    [Fact]
    public async Task Multiple_Selects_Each_Value()
    {
        var helper = CreateHelper();
        helper.Multiple = true;
        helper.Name = "tags";
        helper.Value = "a,b";
        helper.Items = new List<SelectListItem> { new("A", "a"), new("B", "b"), new("C", "c") };
        var content = await RenderAsync(helper);
        Assert.Contains("<option value=\"a\" selected>A</option>", content);
        Assert.Contains("<option value=\"b\" selected>B</option>", content);
        Assert.Contains("<option value=\"c\">C</option>", content);
    }

    // ── Filled ──

    [Fact]
    public async Task Filled_Adds_Modifier()
    {
        var helper = CreateHelper();
        helper.Filled = true;
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");
        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);
        Assert.True(HasClass(output, "rhx-select--filled"));
    }

    // ── Items binding ──

    [Fact]
    public async Task Items_Generates_Options()
    {
        var helper = CreateHelper();
        helper.Items = new List<SelectListItem> { new("United States", "US"), new("Canada", "CA") };
        var content = await RenderAsync(helper);
        Assert.Contains("<option value=\"US\">United States</option>", content);
        Assert.Contains("<option value=\"CA\">Canada</option>", content);
    }

    [Fact]
    public async Task Items_Disabled_Option()
    {
        var helper = CreateHelper();
        helper.Items = new List<SelectListItem>
        {
            new("Active", "A"),
            new SelectListItem { Text = "Disabled", Value = "D", Disabled = true }
        };
        var content = await RenderAsync(helper);
        Assert.Contains("<option value=\"D\" disabled>Disabled</option>", content);
    }

    // ── Enum auto-generation ──

    [Fact]
    public async Task Enum_Auto_Generates_Options()
    {
        var helper = CreateHelper();
        helper.For = CreateModelExpressionFor("Priority", Priority.Medium);
        var content = await RenderAsync(helper);
        Assert.Contains("<option value=\"Low\">Low Priority</option>", content);
        Assert.Contains("<option value=\"Medium\" selected>Medium</option>", content);
        Assert.Contains("<option value=\"High\">High Priority</option>", content);
    }

    // ── htmx ──

    [Fact]
    public async Task Htmx_On_Select()
    {
        var helper = CreateHelper();
        helper.HxGet = "/api/states";
        helper.HxTrigger = "change";
        helper.HxTarget = "#state-options";
        var content = await RenderAsync(helper);
        Assert.Contains("hx-get=\"/api/states\"", content);
        Assert.Contains("hx-trigger=\"change\"", content);
        Assert.Contains("hx-target=\"#state-options\"", content);
    }

    // ── CSS class merging ──

    [Fact]
    public async Task Custom_Css_Class_Is_Merged()
    {
        var helper = CreateHelper();
        helper.CssClass = "my-select";
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");
        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);
        Assert.True(HasClass(output, "rhx-select"));
        Assert.True(HasClass(output, "my-select"));
    }

    // ── Model binding ──

    [Fact]
    public async Task For_Resolves_Name_And_Id()
    {
        var helper = CreateHelper();
        helper.For = CreateModelExpressionFor("Country", "US");
        var content = await RenderAsync(helper);
        Assert.Contains("name=\"Country\"", content);
        Assert.Contains("id=\"Country\"", content);
    }

    // ── Aria-label ──

    [Fact]
    public async Task AriaLabel_Sets_Attribute()
    {
        var helper = CreateHelper();
        helper.AriaLabel = "Choose country";
        var content = await RenderAsync(helper);
        Assert.Contains("aria-label=\"Choose country\"", content);
    }
}
