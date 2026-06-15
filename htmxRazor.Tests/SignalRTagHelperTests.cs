using htmxRazor.Components.Patterns;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Xunit;

namespace htmxRazor.Tests;

// rhx-signalr now emits htmx SSE-extension attributes (hx-ext="sse", sse-connect, sse-swap) —
// the sse.js extension does server→client push with no custom JavaScript.
public class SignalRTagHelperTests : TagHelperTestBase
{
    private SignalRTagHelper CreateHelper()
    {
        var helper = new SignalRTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        return helper;
    }

    private async Task<TagHelperOutput> RenderAsync(SignalRTagHelper helper)
    {
        var context = CreateContext("rhx-signalr");
        var output = CreateOutput("rhx-signalr");
        await helper.ProcessAsync(context, output);
        return output;
    }

    [Fact]
    public async Task Renders_Div_With_Block_Class()
    {
        var helper = CreateHelper();
        helper.HubUrl = "/events";
        helper.Method = "TestMethod";

        var output = await RenderAsync(helper);

        Assert.Equal("div", output.TagName);
        Assert.True(HasClass(output, "rhx-signalr"));
    }

    [Fact]
    public async Task Wires_The_Htmx_Sse_Extension()
    {
        var helper = CreateHelper();
        helper.HubUrl = "/chat-events";
        helper.Method = "ReceiveMessage";
        helper.SignalRSwap = "beforeend";

        var output = await RenderAsync(helper);

        AssertAttribute(output, "hx-ext", "sse");
        AssertAttribute(output, "sse-connect", "/chat-events");
        AssertAttribute(output, "sse-swap", "ReceiveMessage");
        AssertAttribute(output, "hx-swap", "beforeend");
    }

    [Fact]
    public async Task Default_Swap_Is_InnerHTML()
    {
        var helper = CreateHelper();
        helper.HubUrl = "/events";
        helper.Method = "Test";

        AssertAttribute(await RenderAsync(helper), "hx-swap", "innerHTML");
    }

    [Fact]
    public async Task Sets_Target_When_Specified()
    {
        var helper = CreateHelper();
        helper.HubUrl = "/events";
        helper.Method = "Test";
        helper.SignalRTarget = "#messages";

        AssertAttribute(await RenderAsync(helper), "hx-target", "#messages");
    }

    [Fact]
    public async Task Omits_Target_When_Not_Specified()
    {
        var helper = CreateHelper();
        helper.HubUrl = "/events";
        helper.Method = "Test";

        AssertNoAttribute(await RenderAsync(helper), "hx-target");
    }

    [Fact]
    public async Task ConnectionState_Adds_Modifier_Class()
    {
        var helper = CreateHelper();
        helper.HubUrl = "/events";
        helper.Method = "Test";
        helper.ConnectionState = true;

        Assert.True(HasClass(await RenderAsync(helper), "rhx-signalr--has-state"));
    }

    [Fact]
    public async Task Sets_Accessibility_Attributes()
    {
        var helper = CreateHelper();
        helper.HubUrl = "/events";
        helper.Method = "Test";

        var output = await RenderAsync(helper);
        AssertAttribute(output, "aria-live", "polite");
        AssertAttribute(output, "aria-atomic", "false");
    }

    [Fact]
    public async Task Generates_Sse_Connect_From_Page_Route()
    {
        var helper = CreateHelper();
        helper.Page = "/Dashboard";
        helper.PageHandler = "Events";
        helper.Method = "StatusUpdate";

        // The mocked URL helper factory returns "/generated-url".
        AssertAttribute(await RenderAsync(helper), "sse-connect", "/generated-url");
    }

    [Fact]
    public async Task Custom_Css_Class_Is_Appended()
    {
        var helper = CreateHelper();
        helper.HubUrl = "/events";
        helper.Method = "Test";
        helper.CssClass = "my-custom-class";

        var output = await RenderAsync(helper);
        Assert.True(HasClass(output, "rhx-signalr"));
        Assert.True(HasClass(output, "my-custom-class"));
    }

    [Fact]
    public async Task Preserves_Child_Content()
    {
        var helper = CreateHelper();
        helper.HubUrl = "/events";
        helper.Method = "Test";

        var context = CreateContext("rhx-signalr");
        var output = CreateOutput("rhx-signalr", childContent: "<span>Loading...</span>");
        await helper.ProcessAsync(context, output);

        Assert.Contains("Loading...", output.Content.GetContent());
    }
}
