using System.Text.RegularExpressions;
using htmxRazor.Components.Forms;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Xunit;

namespace htmxRazor.Tests;

public class RadialSelectTagHelperTests : TagHelperTestBase
{
    private RadialOptionTagHelper CreateOptionHelper() => new();

    private RadialSelectTagHelper CreateRadialHelper() =>
        new(CreateUrlHelperFactory()) { ViewContext = CreateViewContext() };

    // ──────────────────────────────────────────────
    //  RadialOption (child)
    // ──────────────────────────────────────────────

    [Fact]
    public void RadialOption_Appends_Data_To_Parent_List_And_Suppresses_Output()
    {
        var helper = CreateOptionHelper();
        helper.Value = "fruit";
        helper.Label = "Fruit";
        helper.Icon = "heart";
        helper.Color = "danger";
        helper.HxGet = "/Menu?handler=Items&cat=fruit";

        var context = CreateContext("rhx-radial-option");
        context.Items[RadialOptionTagHelper.ItemsKey] = new List<RadialOptionData>();
        var output = CreateOutput("rhx-radial-option", childContent: "");

        helper.Process(context, output);

        var list = (List<RadialOptionData>)context.Items[RadialOptionTagHelper.ItemsKey];
        var opt = Assert.Single(list);
        Assert.Equal("fruit", opt.Value);
        Assert.Equal("Fruit", opt.Label);
        Assert.Equal("heart", opt.Icon);
        Assert.Equal("danger", opt.Color);
        Assert.Equal("/Menu?handler=Items&cat=fruit", opt.HxGet);
        Assert.Null(output.TagName); // SuppressOutput sets TagName null
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

    // ──────────────────────────────────────────────
    //  Wrapper skeleton
    // ──────────────────────────────────────────────

    [Fact]
    public async Task RadialSelect_Renders_Wrapper_Div_With_Block_Class_And_DataAttr()
    {
        var helper = CreateRadialHelper();
        helper.Name = "FoodItem";
        var context = CreateContext("rhx-radial-select");
        var output = CreateOutput("rhx-radial-select", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.Equal("div", output.TagName);
        Assert.True(HasClass(output, "rhx-radial-select"));
        Assert.True(output.Attributes.TryGetAttribute("data-rhx-radial-select", out _));
    }

    [Fact]
    public async Task RadialSelect_Renders_Trigger_And_Hidden_Value_Input()
    {
        var helper = CreateRadialHelper();
        helper.Name = "FoodItem";
        var context = CreateContext("rhx-radial-select");
        var output = CreateOutput("rhx-radial-select", childContent: "");

        await helper.ProcessAsync(context, output);

        var html = output.Content.GetContent();
        Assert.Contains("rhx-radial-select__trigger", html);
        Assert.Contains("aria-haspopup=\"menu\"", html);
        Assert.Contains("data-rhx-radial-value", html);
        Assert.Contains("name=\"FoodItem\"", html);
    }

    [Fact]
    public async Task RadialSelect_Renders_Combobox_And_Popup_Listbox()
    {
        var helper = CreateRadialHelper();
        helper.Name = "FoodItem";
        helper.Placeholder = "Choose…";
        var context = CreateContext("rhx-radial-select");
        var output = CreateOutput("rhx-radial-select", childContent: "");

        await helper.ProcessAsync(context, output);

        var html = output.Content.GetContent();
        // Combobox button shows the value/placeholder and controls the listbox.
        Assert.Contains("rhx-radial-select__combobox", html);
        Assert.Contains("role=\"combobox\"", html);
        Assert.Contains("aria-haspopup=\"listbox\"", html);
        Assert.Contains("data-rhx-radial-display", html);
        Assert.Contains("Choose…", html);
        // Listbox is a popup, hidden until opened.
        Assert.Contains("rhx-radial-select__listbox", html);
        Assert.Contains("role=\"listbox\"", html);
        Assert.Matches("rhx-radial-select__listbox[^>]*hidden", html);
    }

    [Fact]
    public async Task RadialSelect_Category_Name_Adds_Second_Hidden_Input()
    {
        var helper = CreateRadialHelper();
        helper.Name = "FoodItem";
        helper.CategoryName = "Category";
        var context = CreateContext("rhx-radial-select");
        var output = CreateOutput("rhx-radial-select", childContent: "");

        await helper.ProcessAsync(context, output);

        var html = output.Content.GetContent();
        Assert.Contains("data-rhx-radial-category", html);
        Assert.Contains("name=\"Category\"", html);
    }

    [Fact]
    public async Task RadialSelect_No_Category_Name_Omits_Second_Hidden_Input()
    {
        var helper = CreateRadialHelper();
        helper.Name = "FoodItem";
        var context = CreateContext("rhx-radial-select");
        var output = CreateOutput("rhx-radial-select", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.DoesNotContain("data-rhx-radial-category", output.Content.GetContent());
    }

    [Fact]
    public async Task RadialSelect_Size_Modifier_Applied()
    {
        var helper = CreateRadialHelper();
        helper.Name = "x";
        helper.Size = "large";
        var context = CreateContext("rhx-radial-select");
        var output = CreateOutput("rhx-radial-select", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-radial-select--large"));
    }

    [Fact]
    public async Task RadialSelect_Disabled_Adds_Modifier_And_Disables_Trigger()
    {
        var helper = CreateRadialHelper();
        helper.Name = "x";
        helper.Disabled = true;
        var context = CreateContext("rhx-radial-select");
        var output = CreateOutput("rhx-radial-select", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-radial-select--disabled"));
        Assert.Contains("disabled", output.Content.GetContent());
    }

    // ──────────────────────────────────────────────
    //  Color resolution (§3.3)
    // ──────────────────────────────────────────────

    [Fact]
    public void ResolveColors_Uses_Explicit_Variant_When_Valid()
    {
        var helper = CreateRadialHelper();
        var opts = new List<RadialOptionData> { new() { Value = "a", Label = "A", Color = "success" } };

        var result = helper.ResolveColorsForTest(opts);

        Assert.Equal("success", result[0].Color);
    }

    [Fact]
    public void ResolveColors_Cycles_When_Color_Omitted()
    {
        var helper = CreateRadialHelper();
        var opts = new List<RadialOptionData>
        {
            new() { Value = "a", Label = "A" },
            new() { Value = "b", Label = "B" },
            new() { Value = "c", Label = "C" },
            new() { Value = "d", Label = "D" },
            new() { Value = "e", Label = "E" },
            new() { Value = "f", Label = "F" }, // wraps
        };

        var result = helper.ResolveColorsForTest(opts);

        Assert.Equal(new[] { "brand", "success", "warning", "danger", "neutral", "brand" },
            result.Select(r => r.Color).ToArray());
    }

    [Fact]
    public void ResolveColors_Falls_Back_To_Cycle_For_Invalid_Color()
    {
        var helper = CreateRadialHelper();
        var opts = new List<RadialOptionData> { new() { Value = "a", Label = "A", Color = "#ff0000" } };

        var result = helper.ResolveColorsForTest(opts);

        Assert.Equal("brand", result[0].Color);
    }

    // ──────────────────────────────────────────────
    //  SVG pie wedges
    // ──────────────────────────────────────────────

    private RadialSelectTagHelper CreateRadialWithOptions(out TagHelperContext ctx)
    {
        var helper = CreateRadialHelper();
        helper.Name = "FoodItem";
        helper.AriaLabel = "Food category";
        ctx = CreateContext("rhx-radial-select");
        ctx.Items[RadialOptionTagHelper.ItemsKey] = new List<RadialOptionData>
        {
            new() { Value = "fruit", Label = "Fruit", Icon = "heart", Color = "danger",
                    HxGet = "/Menu?handler=Items&cat=fruit" },
            new() { Value = "meat", Label = "Meat", Icon = "grid", Color = "success",
                    HxGet = "/Menu?handler=Items&cat=meat" },
        };
        return helper;
    }

    [Fact]
    public async Task RadialSelect_Renders_One_Wedge_Per_Option_With_Menuitemradio()
    {
        var helper = CreateRadialWithOptions(out var ctx);
        var output = CreateOutput("rhx-radial-select", childContent: "");

        await helper.ProcessAsync(ctx, output);

        var html = output.Content.GetContent();
        Assert.Contains("<svg", html);
        Assert.Equal(2, Regex.Matches(html, "role=\"menuitemradio\"").Count);
        Assert.Contains("data-rhx-radial-option-value=\"fruit\"", html);
        Assert.Contains("data-rhx-radial-hx-get=\"/Menu?handler=Items&amp;cat=fruit\"", html);
    }

    [Fact]
    public async Task RadialSelect_Wedge_Uses_Resolved_Color_Token()
    {
        var helper = CreateRadialWithOptions(out var ctx);
        var output = CreateOutput("rhx-radial-select", childContent: "");

        await helper.ProcessAsync(ctx, output);

        var html = output.Content.GetContent();
        Assert.Contains("var(--rhx-color-danger-500)", html);
        Assert.Contains("var(--rhx-color-success-500)", html);
        Assert.DoesNotContain("NaN", html);
    }

    [Fact]
    public async Task RadialSelect_DefaultCategory_Marks_Wedge_Checked()
    {
        var helper = CreateRadialWithOptions(out var ctx);
        helper.DefaultCategory = "meat";
        var output = CreateOutput("rhx-radial-select", childContent: "");

        await helper.ProcessAsync(ctx, output);

        var html = output.Content.GetContent();
        Assert.Contains("data-rhx-radial-option-value=\"meat\" role=\"menuitemradio\" aria-checked=\"true\"", html);
    }

    [Fact]
    public async Task RadialSelect_DefaultCategory_Echoes_Active_Color_On_Trigger()
    {
        var helper = CreateRadialWithOptions(out var ctx);
        helper.DefaultCategory = "meat";
        var output = CreateOutput("rhx-radial-select", childContent: "");

        await helper.ProcessAsync(ctx, output);

        var html = output.Content.GetContent();
        Assert.Contains("data-rhx-active-color=\"success\"", html);
    }

    [Fact]
    public async Task RadialSelect_Disabled_Wedge_Gets_Aria_Disabled()
    {
        var helper = CreateRadialHelper();
        helper.Name = "x";
        var ctx = CreateContext("rhx-radial-select");
        ctx.Items[RadialOptionTagHelper.ItemsKey] = new List<RadialOptionData>
        {
            new() { Value = "a", Label = "A", Disabled = true },
            new() { Value = "b", Label = "B" },
        };
        var output = CreateOutput("rhx-radial-select", childContent: "");

        await helper.ProcessAsync(ctx, output);

        Assert.Contains("aria-disabled=\"true\"", output.Content.GetContent());
    }
}
