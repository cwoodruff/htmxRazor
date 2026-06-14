using htmxRazor.Components.Forms;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Xunit;

namespace htmxRazor.Tests;

// rhx-option renders a native <option>: for rhx-select it carries value/selected/disabled;
// for rhx-combobox (a <datalist>) the option's value IS its display text.
public class OptionTagHelperTests : TagHelperTestBase
{
    private TagHelperContext SelectCtx(params string[] selected)
    {
        var ctx = CreateContext("rhx-option");
        ctx.Items["OptionClassPrefix"] = "select";
        ctx.Items["SelectedValues"] = new HashSet<string>(selected, StringComparer.OrdinalIgnoreCase);
        return ctx;
    }

    private TagHelperContext ComboboxCtx()
    {
        var ctx = CreateContext("rhx-option");
        ctx.Items["OptionClassPrefix"] = "combobox";
        ctx.Items["SelectedValues"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        return ctx;
    }

    // ── Element ──

    [Fact]
    public async Task Renders_Option_Element()
    {
        var helper = new OptionTagHelper { Value = "us" };
        var output = CreateOutput("rhx-option", childContent: "United States");
        await helper.ProcessAsync(SelectCtx(), output);

        Assert.Equal("option", output.TagName);
    }

    // ── Value resolution ──

    [Fact]
    public async Task Select_Value_From_Attribute()
    {
        var helper = new OptionTagHelper { Value = "custom-value" };
        var output = CreateOutput("rhx-option", childContent: "Display Text");
        await helper.ProcessAsync(SelectCtx(), output);

        AssertAttribute(output, "value", "custom-value");
    }

    [Fact]
    public async Task Select_Value_From_Content_When_No_Attribute()
    {
        var helper = new OptionTagHelper();
        var output = CreateOutput("rhx-option", childContent: "United States");
        await helper.ProcessAsync(SelectCtx(), output);

        AssertAttribute(output, "value", "United States");
    }

    // ── Selected state ──

    [Fact]
    public async Task Selected_Adds_Native_Selected()
    {
        var helper = new OptionTagHelper { Value = "us" };
        var output = CreateOutput("rhx-option", childContent: "United States");
        await helper.ProcessAsync(SelectCtx("us"), output);

        AssertAttribute(output, "selected", "selected");
    }

    [Fact]
    public async Task Not_Selected_Has_No_Selected_Attribute()
    {
        var helper = new OptionTagHelper { Value = "ca" };
        var output = CreateOutput("rhx-option", childContent: "Canada");
        await helper.ProcessAsync(SelectCtx("us"), output);

        AssertNoAttribute(output, "selected");
    }

    // ── Disabled ──

    [Fact]
    public async Task Disabled_Adds_Native_Disabled()
    {
        var helper = new OptionTagHelper { Value = "mx", Disabled = true };
        var output = CreateOutput("rhx-option", childContent: "Mexico");
        await helper.ProcessAsync(SelectCtx(), output);

        AssertAttribute(output, "disabled", "disabled");
    }

    // ── Combobox (datalist) ──

    [Fact]
    public async Task Combobox_Value_Is_Display_Text()
    {
        // In a datalist the input submits its literal text, so value == text (ignores code).
        var helper = new OptionTagHelper { Value = "ca" };
        var output = CreateOutput("rhx-option", childContent: "Canada");
        await helper.ProcessAsync(ComboboxCtx(), output);

        AssertAttribute(output, "value", "Canada");
        AssertNoAttribute(output, "selected");
    }

    // ── Content ──

    [Fact]
    public async Task Content_Renders_As_Encoded_Text()
    {
        var helper = new OptionTagHelper { Value = "test" };
        var output = CreateOutput("rhx-option", childContent: "Option & Text");
        await helper.ProcessAsync(SelectCtx(), output);

        Assert.Contains("Option &amp; Text", output.Content.GetContent());
    }

    // ── Default prefix ──

    [Fact]
    public async Task Defaults_To_Select_Option_Without_Context()
    {
        var helper = new OptionTagHelper { Value = "test" };
        var context = CreateContext("rhx-option");
        var output = CreateOutput("rhx-option", childContent: "Test");

        await helper.ProcessAsync(context, output);

        Assert.Equal("option", output.TagName);
        AssertAttribute(output, "value", "test");
    }
}
