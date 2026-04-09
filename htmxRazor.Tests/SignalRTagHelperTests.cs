using htmxRazor.Components.Patterns;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Xunit;

namespace htmxRazor.Tests;

public class SignalRTagHelperTests : TagHelperTestBase
{
    private SignalRTagHelper CreateHelper()
    {
        var helper = new SignalRTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        return helper;
    }

    [Fact]
    public async Task Renders_Div_With_Block_Class()
    {
        var helper = CreateHelper();
        helper.HubUrl = "/testHub";
        helper.Method = "TestMethod";

        var context = CreateContext("rhx-signalr");
        var output = CreateOutput("rhx-signalr");

        await helper.ProcessAsync(context, output);

        Assert.Equal("div", output.TagName);
        Assert.True(HasClass(output, "rhx-signalr"));
    }

    [Fact]
    public async Task Sets_Data_Attributes()
    {
        var helper = CreateHelper();
        helper.HubUrl = "/chatHub";
        helper.Method = "ReceiveMessage";
        helper.SignalRSwap = "beforeend";

        var context = CreateContext("rhx-signalr");
        var output = CreateOutput("rhx-signalr");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "data-rhx-signalr", "");
        AssertAttribute(output, "data-rhx-hub-url", "/chatHub");
        AssertAttribute(output, "data-rhx-method", "ReceiveMessage");
        AssertAttribute(output, "data-rhx-swap", "beforeend");
    }

    [Fact]
    public async Task Sets_Default_Swap_To_InnerHTML()
    {
        var helper = CreateHelper();
        helper.HubUrl = "/hub";
        helper.Method = "Test";

        var context = CreateContext("rhx-signalr");
        var output = CreateOutput("rhx-signalr");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "data-rhx-swap", "innerHTML");
    }

    [Fact]
    public async Task Sets_Target_When_Specified()
    {
        var helper = CreateHelper();
        helper.HubUrl = "/hub";
        helper.Method = "Test";
        helper.SignalRTarget = "#messages";

        var context = CreateContext("rhx-signalr");
        var output = CreateOutput("rhx-signalr");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "data-rhx-target", "#messages");
    }

    [Fact]
    public async Task Omits_Target_When_Not_Specified()
    {
        var helper = CreateHelper();
        helper.HubUrl = "/hub";
        helper.Method = "Test";

        var context = CreateContext("rhx-signalr");
        var output = CreateOutput("rhx-signalr");

        await helper.ProcessAsync(context, output);

        AssertNoAttribute(output, "data-rhx-target");
    }

    [Fact]
    public async Task Sets_Reconnect_False_When_Disabled()
    {
        var helper = CreateHelper();
        helper.HubUrl = "/hub";
        helper.Method = "Test";
        helper.Reconnect = false;

        var context = CreateContext("rhx-signalr");
        var output = CreateOutput("rhx-signalr");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "data-rhx-reconnect", "false");
    }

    [Fact]
    public async Task Omits_Reconnect_When_Default_True()
    {
        var helper = CreateHelper();
        helper.HubUrl = "/hub";
        helper.Method = "Test";

        var context = CreateContext("rhx-signalr");
        var output = CreateOutput("rhx-signalr");

        await helper.ProcessAsync(context, output);

        AssertNoAttribute(output, "data-rhx-reconnect");
    }

    [Fact]
    public async Task Sets_Transport_Attribute()
    {
        var helper = CreateHelper();
        helper.HubUrl = "/hub";
        helper.Method = "Test";
        helper.Transport = "WebSockets";

        var context = CreateContext("rhx-signalr");
        var output = CreateOutput("rhx-signalr");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "data-rhx-transport", "websockets");
    }

    [Fact]
    public async Task Omits_Transport_When_Not_Specified()
    {
        var helper = CreateHelper();
        helper.HubUrl = "/hub";
        helper.Method = "Test";

        var context = CreateContext("rhx-signalr");
        var output = CreateOutput("rhx-signalr");

        await helper.ProcessAsync(context, output);

        AssertNoAttribute(output, "data-rhx-transport");
    }

    [Fact]
    public async Task Sets_Groups_Attribute()
    {
        var helper = CreateHelper();
        helper.HubUrl = "/hub";
        helper.Method = "Test";
        helper.Groups = "room1,room2";

        var context = CreateContext("rhx-signalr");
        var output = CreateOutput("rhx-signalr");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "data-rhx-groups", "room1,room2");
    }

    [Fact]
    public async Task Sets_Connection_State_Attribute_And_Class()
    {
        var helper = CreateHelper();
        helper.HubUrl = "/hub";
        helper.Method = "Test";
        helper.ConnectionState = true;

        var context = CreateContext("rhx-signalr");
        var output = CreateOutput("rhx-signalr");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "data-rhx-connection-state", "");
        Assert.True(HasClass(output, "rhx-signalr--has-state"));
    }

    [Fact]
    public async Task Sets_Accessibility_Attributes()
    {
        var helper = CreateHelper();
        helper.HubUrl = "/hub";
        helper.Method = "Test";

        var context = CreateContext("rhx-signalr");
        var output = CreateOutput("rhx-signalr");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "aria-live", "polite");
        AssertAttribute(output, "aria-atomic", "false");
    }

    [Fact]
    public async Task Generates_Url_From_Page_Route()
    {
        var helper = CreateHelper();
        helper.Page = "/Dashboard";
        helper.PageHandler = "Hub";
        helper.Method = "StatusUpdate";

        var context = CreateContext("rhx-signalr");
        var output = CreateOutput("rhx-signalr");

        await helper.ProcessAsync(context, output);

        // The mocked URL helper factory returns "/generated-url"
        AssertAttribute(output, "data-rhx-hub-url", "/generated-url");
    }

    [Fact]
    public async Task Custom_Css_Class_Is_Appended()
    {
        var helper = CreateHelper();
        helper.HubUrl = "/hub";
        helper.Method = "Test";
        helper.CssClass = "my-custom-class";

        var context = CreateContext("rhx-signalr");
        var output = CreateOutput("rhx-signalr");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-signalr"));
        Assert.True(HasClass(output, "my-custom-class"));
    }

    [Fact]
    public async Task Preserves_Child_Content()
    {
        var helper = CreateHelper();
        helper.HubUrl = "/hub";
        helper.Method = "Test";

        var context = CreateContext("rhx-signalr");
        var output = CreateOutput("rhx-signalr", childContent: "<span>Loading...</span>");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("Loading...", content);
    }
}
