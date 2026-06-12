using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using htmxRazor.Components.Forms;
using Xunit;

namespace htmxRazor.Tests;

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

    // ── Element rendering ──

    [Fact]
    public async Task Renders_Div_Element()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        Assert.Equal("div", output.TagName);
    }

    [Fact]
    public async Task Has_Block_Class()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-select"));
    }

    [Fact]
    public async Task Has_Data_Attribute()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "data-rhx-select", "");
    }

    // ── Trigger button ──

    [Fact]
    public async Task Has_Trigger_Button()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-select__trigger", content);
        Assert.Contains("<button", content);
    }

    [Fact]
    public async Task Trigger_Has_Combobox_Role()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("role=\"combobox\"", content);
    }

    [Fact]
    public async Task Trigger_Has_Aria_Expanded_False()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("aria-expanded=\"false\"", content);
    }

    [Fact]
    public async Task Trigger_Has_Aria_Haspopup_Listbox()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("aria-haspopup=\"listbox\"", content);
    }

    [Fact]
    public async Task Trigger_Has_Aria_Controls()
    {
        var helper = CreateHelper();
        helper.Name = "country";
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("aria-controls=\"country-listbox\"", content);
    }

    // ── Listbox ──

    [Fact]
    public async Task Has_Listbox_With_Role()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("role=\"listbox\"", content);
    }

    [Fact]
    public async Task Listbox_Has_Hidden_Attribute()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("role=\"listbox\"", content);
        Assert.Contains(" hidden>", content);
    }

    [Fact]
    public async Task Listbox_Has_Id()
    {
        var helper = CreateHelper();
        helper.Name = "country";
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("id=\"country-listbox\"", content);
    }

    // ── Hidden input ──

    [Fact]
    public async Task Has_Hidden_Input()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("type=\"hidden\"", content);
        Assert.Contains("data-rhx-select-value", content);
    }

    [Fact]
    public async Task Name_On_Hidden_Input()
    {
        var helper = CreateHelper();
        helper.Name = "country";
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("name=\"country\"", content);
    }

    [Fact]
    public async Task Value_On_Hidden_Input()
    {
        var helper = CreateHelper();
        helper.Name = "country";
        helper.Value = "US";
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("value=\"US\"", content);
    }

    // ── Placeholder ──

    [Fact]
    public async Task Placeholder_Shows_When_No_Value()
    {
        var helper = CreateHelper();
        helper.Placeholder = "Choose a country";
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-select__placeholder", content);
        Assert.Contains("Choose a country", content);
    }

    [Fact]
    public async Task Value_Displays_In_Trigger()
    {
        var helper = CreateHelper();
        helper.Value = "US";
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-select__value", content);
        Assert.Contains("US", content);
        Assert.DoesNotContain("rhx-select__placeholder", content);
    }

    // ── Label ──

    [Fact]
    public async Task Label_Has_Id_For_LabelledBy()
    {
        var helper = CreateHelper();
        helper.Label = "Country";
        helper.Name = "country";
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-select__label", content);
        Assert.Contains("id=\"country-label\"", content);
        Assert.Contains("Country", content);
        Assert.Contains("aria-labelledby=\"country-label\"", content);
    }

    // ── Hint ──

    [Fact]
    public async Task Hint_Renders()
    {
        var helper = CreateHelper();
        helper.Hint = "Select your country";
        helper.Name = "country";
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-select__hint", content);
        Assert.Contains("Select your country", content);
    }

    // ── Error ──

    [Fact]
    public async Task Error_Adds_Modifier_And_Message()
    {
        var helper = CreateHelper();
        helper.Name = "country";
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        var viewContext = CreateViewContext();
        viewContext.ModelState.AddModelError("country", "Country is required");
        helper.ViewContext = viewContext;
        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-select--error"));
        var content = output.Content.GetContent();
        Assert.Contains("Country is required", content);
        Assert.Contains("aria-invalid=\"true\"", content);
    }

    // ── States ──

    [Fact]
    public async Task Disabled_Adds_Modifier_And_Attribute()
    {
        var helper = CreateHelper();
        helper.Disabled = true;
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-select--disabled"));
        var content = output.Content.GetContent();
        Assert.Contains(" disabled", content);
    }

    [Fact]
    public async Task Required_Sets_Aria()
    {
        var helper = CreateHelper();
        helper.Required = true;
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("aria-required=\"true\"", content);
    }

    [Fact]
    public async Task Required_Single_Select_Renders_A_Validatable_Required_Mirror_Input()
    {
        // Issue #14: a `type="hidden"` input is barred from HTML constraint validation,
        // so native `required` never fires on rhx-select. When required, the value-carrying
        // input must be a focusable, validatable (non-hidden) mirror that the browser enforces.
        var helper = CreateHelper();
        helper.Name = "type";
        helper.Required = true;
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        // The value input carries the native `required` attribute...
        Assert.Contains("data-rhx-select-value", content);
        Assert.Contains(" required", content);
        // ...and is NOT type="hidden" (hidden inputs are barred from constraint validation)...
        Assert.DoesNotContain("type=\"hidden\"", content);
        // ...but is hidden from layout/AT via the validatable-mirror class + scrubbed semantics.
        Assert.Contains("rhx-select__hidden--required", content);
        Assert.Contains("tabindex=\"-1\"", content);
        Assert.Contains("aria-hidden=\"true\"", content);
    }

    [Fact]
    public async Task NonRequired_Single_Select_Keeps_A_Plain_Hidden_Input()
    {
        var helper = CreateHelper();
        helper.Name = "type";
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("type=\"hidden\"", content);
        Assert.DoesNotContain(" required", content);
        Assert.DoesNotContain("rhx-select__hidden--required", content);
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
    public async Task Multiple_Adds_Modifier_And_Data_Attr()
    {
        var helper = CreateHelper();
        helper.Multiple = true;
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-select--multiple"));
        AssertAttribute(output, "data-rhx-select-multiple", "");
    }

    [Fact]
    public async Task Multiple_Listbox_Has_Multiselectable()
    {
        var helper = CreateHelper();
        helper.Multiple = true;
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("aria-multiselectable=\"true\"", content);
    }

    [Fact]
    public async Task Multiple_Creates_Values_Container()
    {
        var helper = CreateHelper();
        helper.Multiple = true;
        helper.Name = "tags";
        helper.Value = "a,b";
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("data-rhx-select-values", content);
        Assert.Contains("name=\"tags\" value=\"a\"", content);
        Assert.Contains("name=\"tags\" value=\"b\"", content);
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

    // ── Clear button ──

    [Fact]
    public async Task WithClear_Renders_Clear_Button()
    {
        var helper = CreateHelper();
        helper.WithClear = true;
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-select__clear", content);
        Assert.Contains("aria-label=\"Clear\"", content);
    }

    // ── Arrow icon ──

    [Fact]
    public async Task Has_Arrow_Icon()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-select__arrow", content);
        Assert.Contains("<svg", content);
    }

    // ── Items binding ──

    [Fact]
    public async Task Items_Generates_Options()
    {
        var helper = CreateHelper();
        helper.Items = new List<SelectListItem>
        {
            new("United States", "US"),
            new("Canada", "CA")
        };
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("data-value=\"US\"", content);
        Assert.Contains("United States", content);
        Assert.Contains("data-value=\"CA\"", content);
        Assert.Contains("Canada", content);
    }

    [Fact]
    public async Task Items_Marks_Selected()
    {
        var helper = CreateHelper();
        helper.Value = "US";
        helper.Items = new List<SelectListItem>
        {
            new("United States", "US"),
            new("Canada", "CA")
        };
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-select__option--selected\" role=\"option\" data-value=\"US\" aria-selected=\"true\"", content);
        Assert.Contains("data-value=\"CA\" aria-selected=\"false\"", content);
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
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-select__option--disabled", content);
        Assert.Contains("aria-disabled=\"true\"", content);
    }

    // ── Enum auto-generation ──

    [Fact]
    public async Task Enum_Auto_Generates_Options()
    {
        var helper = CreateHelper();
        helper.For = CreateModelExpressionFor("Priority", Priority.Medium);
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("Low Priority", content);
        Assert.Contains("data-value=\"Low\"", content);
        Assert.Contains("data-value=\"Medium\"", content);
        Assert.Contains("High Priority", content);
        Assert.Contains("data-value=\"High\"", content);
    }

    [Fact]
    public async Task Enum_Marks_Selected_Value()
    {
        var helper = CreateHelper();
        helper.For = CreateModelExpressionFor("Priority", Priority.Medium);
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("data-value=\"Medium\" aria-selected=\"true\"", content);
        Assert.Contains("data-value=\"Low\" aria-selected=\"false\"", content);
    }

    // ── htmx ──

    [Fact]
    public async Task Htmx_On_Hidden_Input()
    {
        var helper = CreateHelper();
        helper.HxGet = "/api/states";
        helper.HxTrigger = "change";
        helper.HxTarget = "#state-options";
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
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
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("name=\"Country\"", content);
        Assert.Contains("id=\"Country\"", content);
    }

    [Fact]
    public async Task For_Resolves_Value()
    {
        var helper = CreateHelper();
        helper.For = CreateModelExpressionFor("Country", "US");
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("value=\"US\"", content);
    }

    // ── Aria-label ──

    [Fact]
    public async Task AriaLabel_Sets_Attribute()
    {
        var helper = CreateHelper();
        helper.AriaLabel = "Choose country";
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("aria-label=\"Choose country\"", content);
    }

    // ── MaxOptionsVisible ──

    [Fact]
    public async Task MaxOptionsVisible_Sets_Data_Attribute()
    {
        var helper = CreateHelper();
        helper.MaxOptionsVisible = 5;
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select");

        helper.ViewContext = CreateViewContext();
        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("data-rhx-max-visible=\"5\"", content);
    }
}
