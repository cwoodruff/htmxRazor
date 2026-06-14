using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using htmxRazor.Components.Forms;
using Xunit;

namespace htmxRazor.Tests;

// rhx-combobox renders a native autocomplete: <input list> bound to a <datalist> (no JS).
// Submits the input's literal text; server-filter swaps datalist <option>s via htmx.
public class ComboboxTagHelperTests : TagHelperTestBase
{
    private ComboboxTagHelper CreateHelper() => new(CreateUrlHelperFactory());

    private enum Status
    {
        [Display(Name = "To Do")]
        Todo,
        [Display(Name = "In Progress")]
        InProgress,
        Done
    }

    private class TestModel
    {
        public Status Status { get; set; }
        public string? City { get; set; }
    }

    private static ModelExpression CreateModelExpressionFor(string propertyName, object? value = null)
    {
        var provider = new EmptyModelMetadataProvider();
        var metadata = provider.GetMetadataForProperty(typeof(TestModel), propertyName);
        var explorer = new ModelExplorer(provider, metadata, value);
        return new ModelExpression(propertyName, explorer);
    }

    private async Task<string> RenderAsync(ComboboxTagHelper helper, ViewContext? vc = null)
    {
        var context = CreateContext("rhx-combobox");
        var output = CreateOutput("rhx-combobox");
        helper.ViewContext = vc ?? CreateViewContext();
        await helper.ProcessAsync(context, output);
        return output.Content.GetContent();
    }

    // ── Element rendering ──

    [Fact]
    public async Task Renders_Div_Wrapper()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-combobox");
        var output = CreateOutput("rhx-combobox");
        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        Assert.Equal("div", output.TagName);
        Assert.True(HasClass(output, "rhx-combobox"));
    }

    // ── Native input + datalist ──

    [Fact]
    public async Task Has_Text_Input_Bound_To_Datalist()
    {
        var helper = CreateHelper();
        helper.Name = "city";
        var content = await RenderAsync(helper);

        Assert.Contains("rhx-combobox__control", content);
        Assert.Contains("type=\"text\"", content);
        Assert.Contains("list=\"city-list\"", content);
        Assert.Contains("<datalist id=\"city-list\"", content);
    }

    [Fact]
    public async Task Input_Has_Autocomplete_Off()
    {
        var content = await RenderAsync(CreateHelper());
        Assert.Contains("autocomplete=\"off\"", content);
    }

    // ── Name / value ──

    [Fact]
    public async Task Name_And_Value_On_Input()
    {
        var helper = CreateHelper();
        helper.Name = "city";
        helper.Value = "NYC";
        var content = await RenderAsync(helper);
        Assert.Contains("name=\"city\"", content);
        Assert.Contains("value=\"NYC\"", content);
    }

    // ── Placeholder ──

    [Fact]
    public async Task Placeholder_On_Input()
    {
        var helper = CreateHelper();
        helper.Placeholder = "Search cities...";
        var content = await RenderAsync(helper);
        Assert.Contains("placeholder=\"Search cities...\"", content);
    }

    // ── Label ──

    [Fact]
    public async Task Label_With_For_And_LabelledBy()
    {
        var helper = CreateHelper();
        helper.Label = "City";
        helper.Name = "city";
        var content = await RenderAsync(helper);
        Assert.Contains("rhx-combobox__label", content);
        Assert.Contains("id=\"city-label\"", content);
        Assert.Contains("for=\"city\"", content);
        Assert.Contains("aria-labelledby=\"city-label\"", content);
    }

    // ── Hint ──

    [Fact]
    public async Task Hint_Renders()
    {
        var helper = CreateHelper();
        helper.Hint = "Type to filter";
        helper.Name = "city";
        var content = await RenderAsync(helper);
        Assert.Contains("rhx-combobox__hint", content);
        Assert.Contains("Type to filter", content);
    }

    // ── Error ──

    [Fact]
    public async Task Error_Adds_Modifier_And_Message()
    {
        var helper = CreateHelper();
        helper.Name = "city";
        var vc = CreateViewContext();
        vc.ModelState.AddModelError("city", "City is required");

        var context = CreateContext("rhx-combobox");
        var output = CreateOutput("rhx-combobox");
        helper.ViewContext = vc;
        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-combobox--error"));
        var content = output.Content.GetContent();
        Assert.Contains("City is required", content);
        Assert.Contains("aria-invalid=\"true\"", content);
    }

    // ── States ──

    [Fact]
    public async Task Disabled_Adds_Modifier_And_Native_Attribute()
    {
        var helper = CreateHelper();
        helper.Disabled = true;
        var context = CreateContext("rhx-combobox");
        var output = CreateOutput("rhx-combobox");
        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-combobox--disabled"));
        Assert.Contains(" disabled", output.Content.GetContent());
    }

    [Fact]
    public async Task Readonly_Adds_Modifier_And_Native_Attribute()
    {
        var helper = CreateHelper();
        helper.Readonly = true;
        var context = CreateContext("rhx-combobox");
        var output = CreateOutput("rhx-combobox");
        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-combobox--readonly"));
        Assert.Contains(" readonly", output.Content.GetContent());
    }

    [Fact]
    public async Task Required_Adds_Native_Required()
    {
        var helper = CreateHelper();
        helper.Required = true;
        var content = await RenderAsync(helper);
        Assert.Contains(" required", content);
    }

    // ── Size ──

    [Fact]
    public async Task Small_Size_Adds_Modifier()
    {
        var helper = CreateHelper();
        helper.Size = "small";
        var context = CreateContext("rhx-combobox");
        var output = CreateOutput("rhx-combobox");
        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);
        Assert.True(HasClass(output, "rhx-combobox--small"));
    }

    [Fact]
    public async Task Large_Size_Adds_Modifier()
    {
        var helper = CreateHelper();
        helper.Size = "large";
        var context = CreateContext("rhx-combobox");
        var output = CreateOutput("rhx-combobox");
        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);
        Assert.True(HasClass(output, "rhx-combobox--large"));
    }

    // ── Filled ──

    [Fact]
    public async Task Filled_Adds_Modifier()
    {
        var helper = CreateHelper();
        helper.Filled = true;
        var context = CreateContext("rhx-combobox");
        var output = CreateOutput("rhx-combobox");
        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);
        Assert.True(HasClass(output, "rhx-combobox--filled"));
    }

    // ── Server filter ──

    [Fact]
    public async Task ServerFilter_Sets_Data_Attribute_And_Targets_Datalist()
    {
        var helper = CreateHelper();
        helper.Name = "city";
        helper.ServerFilter = true;
        var context = CreateContext("rhx-combobox");
        var output = CreateOutput("rhx-combobox");
        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "data-rhx-server-filter", "");
        var content = output.Content.GetContent();
        Assert.Contains("hx-target=\"#city-list\"", content);
        Assert.Contains("hx-swap=\"innerHTML\"", content);
    }

    // ── Items binding (datalist option value == display text) ──

    [Fact]
    public async Task Items_Generates_Datalist_Options()
    {
        var helper = CreateHelper();
        helper.Items = new List<SelectListItem> { new("New York", "NYC"), new("Los Angeles", "LA") };
        var content = await RenderAsync(helper);
        Assert.Contains("<option value=\"New York\">", content);
        Assert.Contains("<option value=\"Los Angeles\">", content);
    }

    // ── Enum auto-generation ──

    [Fact]
    public async Task Enum_Auto_Generates_Options()
    {
        var helper = CreateHelper();
        helper.For = CreateModelExpressionFor("Status", Status.InProgress);
        var content = await RenderAsync(helper);
        Assert.Contains("<option value=\"To Do\">", content);
        Assert.Contains("<option value=\"In Progress\">", content);
        Assert.Contains("<option value=\"Done\">", content);
    }

    // ── htmx on input ──

    [Fact]
    public async Task Htmx_On_Input()
    {
        var helper = CreateHelper();
        helper.HxGet = "/api/cities";
        helper.HxTrigger = "input changed delay:200ms";
        var content = await RenderAsync(helper);
        Assert.Contains("hx-get=\"/api/cities\"", content);
        Assert.Contains("hx-trigger=\"input changed delay:200ms\"", content);
    }

    // ── CSS class merging ──

    [Fact]
    public async Task Custom_Css_Class_Is_Merged()
    {
        var helper = CreateHelper();
        helper.CssClass = "my-combobox";
        var context = CreateContext("rhx-combobox");
        var output = CreateOutput("rhx-combobox");
        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);
        Assert.True(HasClass(output, "rhx-combobox"));
        Assert.True(HasClass(output, "my-combobox"));
    }

    // ── Model binding ──

    [Fact]
    public async Task For_Resolves_Name_And_Value()
    {
        var helper = CreateHelper();
        helper.For = CreateModelExpressionFor("City", "NYC");
        var content = await RenderAsync(helper);
        Assert.Contains("name=\"City\"", content);
        Assert.Contains("value=\"NYC\"", content);
    }

    // ── MaxOptionsVisible ──

    [Fact]
    public async Task MaxOptionsVisible_Sets_Data_Attribute()
    {
        var helper = CreateHelper();
        helper.MaxOptionsVisible = 10;
        var content = await RenderAsync(helper);
        Assert.Contains("data-rhx-max-visible=\"10\"", content);
    }
}
