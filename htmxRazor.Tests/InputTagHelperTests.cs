using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using htmxRazor.Components.Forms;
using Xunit;

namespace htmxRazor.Tests;

public class InputTagHelperTests : TagHelperTestBase
{
    private InputTagHelper CreateHelper() => new(CreateUrlHelperFactory());

    // ── Test model ──

    private class TestModel
    {
        [Required]
        [Display(Name = "Email Address", Description = "We'll never share your email")]
        [EmailAddress]
        public string Email { get; set; } = "";

        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = "";

        [Url]
        public string? Website { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [RegularExpression(@"^[A-Z]{2}\d{4}$")]
        public string? Code { get; set; }

        public int Quantity { get; set; }

        public DateTime? BirthDate { get; set; }
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
    public void Renders_Div_Element()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.Equal("div", output.TagName);
    }

    [Fact]
    public void Has_Block_Class()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-input"));
    }

    [Fact]
    public void Renders_Native_Input()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.Contains("rhx-input__native", output.Content.GetContent());
    }

    // ── Inner HTML structure ──

    [Fact]
    public void Contains_Control_Wrapper()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-input__control", content);
    }

    [Fact]
    public void Contains_Native_Input()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-input__native", content);
        Assert.Contains("<input", content);
    }

    [Fact]
    public void Default_Type_Is_Text()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("type=\"text\"", content);
    }

    // ── Name, Id, Value ──

    [Fact]
    public void Name_Sets_Name_Attribute()
    {
        var helper = CreateHelper();
        helper.Name = "email";
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("name=\"email\"", content);
    }

    [Fact]
    public void Id_Sets_Id_On_Native_Input()
    {
        var helper = CreateHelper();
        helper.Id = "my-email";
        helper.Name = "email";
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("id=\"my-email\"", content);
        // Id should NOT be on wrapper
        AssertNoAttribute(output, "id");
    }

    [Fact]
    public void Value_Sets_Value_Attribute()
    {
        var helper = CreateHelper();
        helper.Value = "test@example.com";
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("value=\"test@example.com\"", content);
    }

    // ── Type ──

    [Fact]
    public void Custom_Type()
    {
        var helper = CreateHelper();
        helper.Type = "email";
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("type=\"email\"", content);
    }

    [Fact]
    public void Password_Type()
    {
        var helper = CreateHelper();
        helper.Type = "password";
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("type=\"password\"", content);
    }

    // ── Label ──

    [Fact]
    public void Label_Renders_Label_Element()
    {
        var helper = CreateHelper();
        helper.Label = "Email";
        helper.Name = "email";
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("<label", content);
        Assert.Contains("rhx-input__label", content);
        Assert.Contains("Email", content);
        Assert.Contains("for=\"email\"", content);
    }

    [Fact]
    public void No_Label_Omits_Label_Element()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.DoesNotContain("<label", content);
    }

    // ── Hint ──

    [Fact]
    public void Hint_Renders_Hint_Span()
    {
        var helper = CreateHelper();
        helper.Hint = "Enter your email";
        helper.Name = "email";
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-input__hint", content);
        Assert.Contains("Enter your email", content);
        Assert.Contains("id=\"email-hint\"", content);
    }

    [Fact]
    public void Hint_Sets_Aria_Describedby()
    {
        var helper = CreateHelper();
        helper.Hint = "Enter your email";
        helper.Name = "email";
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("aria-describedby=\"email-hint\"", content);
    }

    // ── Placeholder ──

    [Fact]
    public void Placeholder_Sets_Attribute()
    {
        var helper = CreateHelper();
        helper.Placeholder = "you@example.com";
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("placeholder=\"you@example.com\"", content);
    }

    // ── Required ──

    [Fact]
    public void Required_Sets_Required_And_Aria()
    {
        var helper = CreateHelper();
        helper.Required = true;
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains(" required", content);
        Assert.Contains("aria-required=\"true\"", content);
    }

    // ── Disabled ──

    [Fact]
    public void Disabled_Adds_Modifier_And_Attribute()
    {
        var helper = CreateHelper();
        helper.Disabled = true;
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-input--disabled"));
        var content = output.Content.GetContent();
        Assert.Contains(" disabled", content);
    }

    // ── Readonly ──

    [Fact]
    public void Readonly_Adds_Modifier_And_Attribute()
    {
        var helper = CreateHelper();
        helper.Readonly = true;
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-input--readonly"));
        var content = output.Content.GetContent();
        Assert.Contains(" readonly", content);
    }

    // ── Size ──

    [Fact]
    public void Default_Size_No_Modifier()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.False(HasClass(output, "rhx-input--medium"));
    }

    [Fact]
    public void Small_Size_Adds_Modifier()
    {
        var helper = CreateHelper();
        helper.Size = "small";
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-input--small"));
    }

    [Fact]
    public void Large_Size_Adds_Modifier()
    {
        var helper = CreateHelper();
        helper.Size = "large";
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-input--large"));
    }

    // ── Filled ──

    [Fact]
    public void Filled_Adds_Modifier()
    {
        var helper = CreateHelper();
        helper.Filled = true;
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-input--filled"));
    }

    // ── Constraints ──

    [Fact]
    public void Minlength_Sets_Attribute()
    {
        var helper = CreateHelper();
        helper.Minlength = 3;
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("minlength=\"3\"", content);
    }

    [Fact]
    public void Maxlength_Sets_Attribute()
    {
        var helper = CreateHelper();
        helper.Maxlength = 100;
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("maxlength=\"100\"", content);
    }

    [Fact]
    public void Pattern_Sets_Attribute()
    {
        var helper = CreateHelper();
        helper.Pattern = @"^[A-Z]+$";
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("pattern=", content);
    }

    [Fact]
    public void Min_Max_Step_Set_Attributes()
    {
        var helper = CreateHelper();
        helper.Min = "0";
        helper.Max = "100";
        helper.Step = "5";
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("min=\"0\"", content);
        Assert.Contains("max=\"100\"", content);
        Assert.Contains("step=\"5\"", content);
    }

    // ── Autocomplete and Autofocus ──

    [Fact]
    public void Autocomplete_Sets_Attribute()
    {
        var helper = CreateHelper();
        helper.Autocomplete = "email";
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("autocomplete=\"email\"", content);
    }

    [Fact]
    public void Autofocus_Sets_Attribute()
    {
        var helper = CreateHelper();
        helper.Autofocus = true;
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains(" autofocus", content);
    }

    // ── Clear button (native type="search") ──

    [Fact]
    public void WithClear_Uses_Native_Search_Type()
    {
        // The native search input renders the browser's own clear "✕" — no custom button/JS.
        var helper = CreateHelper();
        helper.WithClear = true;
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("type=\"search\"", content);
        Assert.DoesNotContain("rhx-input__clear", content);
    }

    [Fact]
    public void No_WithClear_Stays_Text_Type()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("type=\"text\"", content);
        Assert.DoesNotContain("type=\"search\"", content);
    }

    // ── Password toggle ──

    [Fact]
    public void Password_Toggle_Renders_When_Type_Is_Password()
    {
        var helper = CreateHelper();
        helper.Type = "password";
        helper.PasswordToggle = true;
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-input__toggle", content);
        Assert.Contains("data-rhx-input-toggle", content);
        Assert.Contains("aria-label=\"Show password\"", content);
    }

    [Fact]
    public void Password_Toggle_Not_Rendered_For_Non_Password()
    {
        var helper = CreateHelper();
        helper.Type = "text";
        helper.PasswordToggle = true;
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.DoesNotContain("rhx-input__toggle", content);
    }

    // ── Error state ──

    [Fact]
    public void Error_Adds_Error_Modifier()
    {
        var helper = CreateHelper();
        helper.Name = "email";
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        var viewContext = CreateViewContext();
        viewContext.ModelState.AddModelError("email", "Email is invalid");
        helper.ViewContext = viewContext;
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-input--error"));
    }

    [Fact]
    public void Error_Renders_Error_Message()
    {
        var helper = CreateHelper();
        helper.Name = "email";
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        var viewContext = CreateViewContext();
        viewContext.ModelState.AddModelError("email", "Email is invalid");
        helper.ViewContext = viewContext;
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-input__error", content);
        Assert.Contains("Email is invalid", content);
        Assert.Contains("aria-live=\"polite\"", content);
    }

    [Fact]
    public void Error_Sets_Aria_Invalid()
    {
        var helper = CreateHelper();
        helper.Name = "email";
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        var viewContext = CreateViewContext();
        viewContext.ModelState.AddModelError("email", "Email is invalid");
        helper.ViewContext = viewContext;
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("aria-invalid=\"true\"", content);
    }

    [Fact]
    public void No_Error_Hides_Error_Span()
    {
        var helper = CreateHelper();
        helper.Name = "email";
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-input__error", content);
        Assert.Contains("hidden", content);
    }

    // ── ARIA label ──

    [Fact]
    public void AriaLabel_Sets_Attribute()
    {
        var helper = CreateHelper();
        helper.AriaLabel = "Search query";
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("aria-label=\"Search query\"", content);
    }

    // ── CSS class merging ──

    [Fact]
    public void Custom_Css_Class_Is_Merged()
    {
        var helper = CreateHelper();
        helper.CssClass = "my-input";
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-input"));
        Assert.True(HasClass(output, "my-input"));
    }

    // ── Model binding ──

    [Fact]
    public void For_Resolves_Name_And_Id()
    {
        var helper = CreateHelper();
        helper.For = CreateModelExpressionFor("Email", "test@example.com");
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("name=\"Email\"", content);
        Assert.Contains("id=\"Email\"", content);
    }

    [Fact]
    public void For_Resolves_Value()
    {
        var helper = CreateHelper();
        helper.For = CreateModelExpressionFor("Email", "test@example.com");
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("value=\"test@example.com\"", content);
    }

    [Fact]
    public void For_Int_Property_Infers_Number_Type()
    {
        var helper = CreateHelper();
        helper.For = CreateModelExpressionFor("Quantity", 5);
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("type=\"number\"", content);
    }

    [Fact]
    public void For_DateTime_Property_Infers_DatetimeLocal_Type()
    {
        var helper = CreateHelper();
        helper.For = CreateModelExpressionFor("BirthDate", null);
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("type=\"datetime-local\"", content);
    }

    [Fact]
    public void Explicit_Type_Overrides_Inferred_Type()
    {
        var helper = CreateHelper();
        helper.For = CreateModelExpressionFor("Quantity", 5);
        helper.Type = "text";
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("type=\"text\"", content);
    }

    // ── htmx attributes on native input ──

    [Fact]
    public void Htmx_Attributes_On_Native_Input()
    {
        var helper = CreateHelper();
        helper.HxGet = "/search";
        helper.HxTarget = "#results";
        helper.HxTrigger = "input changed delay:300ms";
        var context = CreateContext("rhx-input");
        var output = CreateOutput("rhx-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("hx-get=\"/search\"", content);
        Assert.Contains("hx-target=\"#results\"", content);
        Assert.Contains("hx-trigger=\"input changed delay:300ms\"", content);
    }
}
