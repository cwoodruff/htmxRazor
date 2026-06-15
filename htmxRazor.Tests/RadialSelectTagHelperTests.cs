using htmxRazor.Components.Forms;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Xunit;

namespace htmxRazor.Tests;

// rhx-radial-select is now a cascading pair of native <select>s (category → item), no JS.
// Changing the category select fires the wrapper's htmx request to load the item options.
public class RadialSelectTagHelperTests : TagHelperTestBase
{
    private RadialOptionTagHelper CreateOptionHelper() => new();

    private RadialSelectTagHelper CreateRadialHelper() =>
        new(CreateUrlHelperFactory()) { ViewContext = CreateViewContext() };

    private RadialSelectTagHelper CreateRadialWithOptions(out TagHelperContext ctx)
    {
        var helper = CreateRadialHelper();
        helper.Name = "FoodItem";
        helper.CategoryName = "Category";
        helper.HxGet = "/Menu?handler=Items";
        helper.AriaLabel = "Food";
        ctx = CreateContext("rhx-radial-select");
        ctx.Items[RadialOptionTagHelper.ItemsKey] = new List<RadialOptionData>
        {
            new() { Value = "fruit", Label = "Fruit" },
            new() { Value = "meat", Label = "Meat" },
        };
        return helper;
    }

    // ── RadialOption (child) ──

    [Fact]
    public void RadialOption_Appends_Data_To_Parent_List_And_Suppresses_Output()
    {
        var helper = CreateOptionHelper();
        helper.Value = "fruit";
        helper.Label = "Fruit";

        var context = CreateContext("rhx-radial-option");
        context.Items[RadialOptionTagHelper.ItemsKey] = new List<RadialOptionData>();
        var output = CreateOutput("rhx-radial-option", childContent: "");

        helper.Process(context, output);

        var list = (List<RadialOptionData>)context.Items[RadialOptionTagHelper.ItemsKey];
        var opt = Assert.Single(list);
        Assert.Equal("fruit", opt.Value);
        Assert.Equal("Fruit", opt.Label);
        Assert.Null(output.TagName);
    }

    [Fact]
    public void RadialOption_No_Throw_When_Parent_List_Missing()
    {
        var helper = CreateOptionHelper();
        helper.Value = "fruit";
        var context = CreateContext("rhx-radial-option");
        var output = CreateOutput("rhx-radial-option", childContent: "");

        helper.Process(context, output);

        Assert.Null(output.TagName);
    }

    // ── Wrapper / native selects ──

    [Fact]
    public async Task Renders_Wrapper_With_Two_Native_Selects()
    {
        var helper = CreateRadialWithOptions(out var ctx);
        var output = CreateOutput("rhx-radial-select", childContent: "");

        await helper.ProcessAsync(ctx, output);

        Assert.Equal("div", output.TagName);
        Assert.True(HasClass(output, "rhx-radial-select"));
        var html = output.Content.GetContent();
        Assert.Contains("rhx-radial-select__category", html);
        Assert.Contains("rhx-radial-select__item", html);
        Assert.Equal(2, System.Text.RegularExpressions.Regex.Matches(html, "<select").Count);
    }

    [Fact]
    public async Task Category_Select_Has_Category_Name_And_Options()
    {
        var helper = CreateRadialWithOptions(out var ctx);
        var output = CreateOutput("rhx-radial-select", childContent: "");

        await helper.ProcessAsync(ctx, output);

        var html = output.Content.GetContent();
        Assert.Contains("name=\"Category\"", html);
        Assert.Contains("<option value=\"fruit\">Fruit</option>", html);
        Assert.Contains("<option value=\"meat\">Meat</option>", html);
    }

    [Fact]
    public async Task Item_Select_Carries_Bound_Field_Name()
    {
        var helper = CreateRadialWithOptions(out var ctx);
        var output = CreateOutput("rhx-radial-select", childContent: "");

        await helper.ProcessAsync(ctx, output);

        Assert.Contains("name=\"FoodItem\"", output.Content.GetContent());
    }

    [Fact]
    public async Task Category_Select_Wires_The_Htmx_Cascade()
    {
        var helper = CreateRadialWithOptions(out var ctx);
        var output = CreateOutput("rhx-radial-select", childContent: "");

        await helper.ProcessAsync(ctx, output);

        var html = output.Content.GetContent();
        Assert.Contains("hx-get=\"/Menu?handler=Items\"", html);
        Assert.Contains("hx-trigger=\"change, load\"", html);
        Assert.Contains("hx-target=\"#FoodItem-items\"", html);
        Assert.Contains("hx-swap=\"innerHTML\"", html);
    }

    [Fact]
    public async Task DefaultCategory_Marks_Category_Option_Selected()
    {
        var helper = CreateRadialWithOptions(out var ctx);
        helper.DefaultCategory = "meat";
        var output = CreateOutput("rhx-radial-select", childContent: "");

        await helper.ProcessAsync(ctx, output);

        Assert.Contains("<option value=\"meat\" selected>Meat</option>", output.Content.GetContent());
    }

    [Fact]
    public async Task Bound_Value_Becomes_Preselected_Item_Option()
    {
        var helper = CreateRadialWithOptions(out var ctx);
        helper.Value = "Apple";
        var output = CreateOutput("rhx-radial-select", childContent: "");

        await helper.ProcessAsync(ctx, output);

        var html = output.Content.GetContent();
        Assert.Contains("<option value=\"Apple\" selected>Apple</option>", html);
        // hx-vals forwards the current item so the cascade endpoint can pre-select it.
        Assert.Contains("\"selected\":\"Apple\"", html);
    }

    [Fact]
    public async Task Placeholder_Used_When_No_Value()
    {
        var helper = CreateRadialWithOptions(out var ctx);
        helper.Placeholder = "Pick an item…";
        var output = CreateOutput("rhx-radial-select", childContent: "");

        await helper.ProcessAsync(ctx, output);

        Assert.Contains("Pick an item…", output.Content.GetContent());
    }

    [Fact]
    public async Task Size_Modifier_Applied()
    {
        var helper = CreateRadialWithOptions(out var ctx);
        helper.Size = "large";
        var output = CreateOutput("rhx-radial-select", childContent: "");

        await helper.ProcessAsync(ctx, output);

        Assert.True(HasClass(output, "rhx-radial-select--large"));
    }

    [Fact]
    public async Task Disabled_Adds_Modifier_And_Disables_Both_Selects()
    {
        var helper = CreateRadialWithOptions(out var ctx);
        helper.Disabled = true;
        var output = CreateOutput("rhx-radial-select", childContent: "");

        await helper.ProcessAsync(ctx, output);

        Assert.True(HasClass(output, "rhx-radial-select--disabled"));
        Assert.Equal(2, System.Text.RegularExpressions.Regex.Matches(output.Content.GetContent(), " disabled").Count);
    }

    [Fact]
    public async Task Disabled_Category_Option_Gets_Disabled()
    {
        var helper = CreateRadialHelper();
        helper.Name = "x";
        helper.HxGet = "/items";
        var ctx = CreateContext("rhx-radial-select");
        ctx.Items[RadialOptionTagHelper.ItemsKey] = new List<RadialOptionData>
        {
            new() { Value = "a", Label = "A", Disabled = true },
            new() { Value = "b", Label = "B" },
        };
        var output = CreateOutput("rhx-radial-select", childContent: "");

        await helper.ProcessAsync(ctx, output);

        Assert.Contains("<option value=\"a\" disabled>A</option>", output.Content.GetContent());
    }
}
