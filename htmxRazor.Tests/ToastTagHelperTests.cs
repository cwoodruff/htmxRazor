using htmxRazor.Components.Feedback;
using Xunit;

namespace htmxRazor.Tests;

/// <summary>
/// Tests for ToastTagHelper and ToastContainerTagHelper components.
/// </summary>
public class ToastTagHelperTests : TagHelperTestBase
{
    // ──────────────────────────────────────────────
    //  Toast Container
    // ──────────────────────────────────────────────

    private ToastContainerTagHelper CreateContainerHelper()
    {
        var helper = new ToastContainerTagHelper();
        helper.ViewContext = CreateViewContext();
        return helper;
    }

    [Fact]
    public async Task Container_Renders_Div()
    {
        var helper = CreateContainerHelper();
        var context = CreateContext("rhx-toast-container");
        var output = CreateOutput("rhx-toast-container");

        await helper.ProcessAsync(context, output);

        Assert.Equal("div", output.TagName);
    }

    [Fact]
    public async Task Container_Has_Block_Class()
    {
        var helper = CreateContainerHelper();
        var context = CreateContext("rhx-toast-container");
        var output = CreateOutput("rhx-toast-container");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-toast-container"));
    }

    [Fact]
    public async Task Container_Has_Position_Modifier()
    {
        var helper = CreateContainerHelper();
        helper.Position = "bottom-center";
        var context = CreateContext("rhx-toast-container");
        var output = CreateOutput("rhx-toast-container");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-toast-container--bottom-center"));
    }

    [Fact]
    public async Task Container_Has_Default_Id()
    {
        var helper = CreateContainerHelper();
        var context = CreateContext("rhx-toast-container");
        var output = CreateOutput("rhx-toast-container");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "id", "rhx-toasts");
    }

    [Fact]
    public async Task Container_Respects_Custom_Id()
    {
        var helper = CreateContainerHelper();
        helper.Id = "my-toasts";
        var context = CreateContext("rhx-toast-container");
        var output = CreateOutput("rhx-toast-container");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "id", "my-toasts");
    }

    [Fact]
    public async Task Container_Has_AriaLive()
    {
        var helper = CreateContainerHelper();
        var context = CreateContext("rhx-toast-container");
        var output = CreateOutput("rhx-toast-container");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "role", "status");
        AssertAttribute(output, "aria-live", "polite");
        AssertAttribute(output, "aria-relevant", "additions");
    }

    [Fact]
    public async Task Container_Has_Data_Attributes()
    {
        var helper = CreateContainerHelper();
        helper.MaxToasts = 3;
        helper.DefaultDuration = 8000;
        var context = CreateContext("rhx-toast-container");
        var output = CreateOutput("rhx-toast-container");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "data-rhx-toast-container", "");
        AssertAttribute(output, "data-rhx-max", "3");
        AssertAttribute(output, "data-rhx-duration", "8000");
    }

    // ──────────────────────────────────────────────
    //  Toast
    // ──────────────────────────────────────────────

    private ToastTagHelper CreateToastHelper()
    {
        var helper = new ToastTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        return helper;
    }

    [Fact]
    public async Task Toast_Renders_Div()
    {
        var helper = CreateToastHelper();
        var context = CreateContext("rhx-toast");
        var output = CreateOutput("rhx-toast", childContent: "Hello");

        await helper.ProcessAsync(context, output);

        Assert.Equal("div", output.TagName);
    }

    [Fact]
    public async Task Toast_Has_Block_Class()
    {
        var helper = CreateToastHelper();
        var context = CreateContext("rhx-toast");
        var output = CreateOutput("rhx-toast", childContent: "Hello");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-toast"));
    }

    [Theory]
    [InlineData("success")]
    [InlineData("danger")]
    [InlineData("warning")]
    [InlineData("brand")]
    [InlineData("neutral")]
    public async Task Toast_Has_Variant_Modifier(string variant)
    {
        var helper = CreateToastHelper();
        helper.Variant = variant;
        var context = CreateContext("rhx-toast");
        var output = CreateOutput("rhx-toast", childContent: "Hello");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, $"rhx-toast--{variant}"));
    }

    [Fact]
    public async Task Toast_Renders_Close_Dismiss_Control()
    {
        // JS-free dismiss: a sr-only checkbox + label that CSS :has(:checked) hides.
        var helper = CreateToastHelper();
        var context = CreateContext("rhx-toast");
        var output = CreateOutput("rhx-toast", childContent: "Hello");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-toast__dismiss", content);
        Assert.Contains("type=\"checkbox\"", content);
    }

    [Fact]
    public async Task Toast_Duration_Sets_Auto_Dismiss_Animation()
    {
        // Auto-dismiss is a delayed CSS animation (no JS, no data attribute).
        var helper = CreateToastHelper();
        helper.Duration = 3000;
        var context = CreateContext("rhx-toast");
        var output = CreateOutput("rhx-toast", childContent: "Hello");

        await helper.ProcessAsync(context, output);

        var style = output.Attributes["style"]?.Value?.ToString() ?? "";
        Assert.Contains("rhx-toast-auto-dismiss", style);
        Assert.Contains("3000ms", style);
    }

    [Fact]
    public async Task Toast_Duration_Zero_Disables_Auto_Dismiss()
    {
        var helper = CreateToastHelper();
        helper.Duration = 0;
        var context = CreateContext("rhx-toast");
        var output = CreateOutput("rhx-toast", childContent: "Hello");

        await helper.ProcessAsync(context, output);

        var style = output.Attributes["style"]?.Value?.ToString() ?? "";
        Assert.DoesNotContain("rhx-toast-auto-dismiss", style);
    }

    [Fact]
    public async Task Toast_Has_Close_Button_By_Default()
    {
        var helper = CreateToastHelper();
        var context = CreateContext("rhx-toast");
        var output = CreateOutput("rhx-toast", childContent: "Hello");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-toast__close", content);
        Assert.Contains("aria-label=\"Close\"", content);
    }

    [Fact]
    public async Task Toast_No_Close_Button_When_Not_Closable()
    {
        var helper = CreateToastHelper();
        helper.Closable = false;
        var context = CreateContext("rhx-toast");
        var output = CreateOutput("rhx-toast", childContent: "Hello");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.DoesNotContain("rhx-toast__close", content);
    }

    [Fact]
    public async Task Toast_Has_Icon()
    {
        var helper = CreateToastHelper();
        var context = CreateContext("rhx-toast");
        var output = CreateOutput("rhx-toast", childContent: "Hello");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-toast__icon", content);
        Assert.Contains("aria-hidden=\"true\"", content);
    }

    [Fact]
    public async Task Toast_Has_Content_Wrapper()
    {
        var helper = CreateToastHelper();
        var context = CreateContext("rhx-toast");
        var output = CreateOutput("rhx-toast", childContent: "Hello World");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-toast__content", content);
        Assert.Contains("Hello World", content);
    }
}
