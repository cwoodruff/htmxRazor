using Microsoft.AspNetCore.Razor.TagHelpers;
using htmxRazor.Components.Forms;
using Xunit;

namespace htmxRazor.Tests;

public class HtmxFormTagHelperTests : TagHelperTestBase
{
    private HtmxFormTagHelper CreateHelper(string? generatedUrl = "/generated-url")
    {
        var helper = new HtmxFormTagHelper(CreateUrlHelperFactory(generatedUrl));
        helper.ViewContext = CreateViewContext();
        return helper;
    }

    // ── Element ──

    [Fact]
    public async Task Renders_Form_Element()
    {
        var helper = CreateHelper();
        helper.Page = "/Contact";

        var context = CreateContext("rhx-htmx-form");
        var output = CreateOutput("rhx-htmx-form");

        await helper.ProcessAsync(context, output);

        Assert.Equal("form", output.TagName);
    }

    [Fact]
    public async Task Has_Block_Class()
    {
        var helper = CreateHelper();
        helper.Page = "/Contact";

        var context = CreateContext("rhx-htmx-form");
        var output = CreateOutput("rhx-htmx-form");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-htmx-form"));
    }

    // ── HTTP method ──

    [Fact]
    public async Task Default_Method_Post()
    {
        var helper = CreateHelper();
        helper.Page = "/Contact";

        var context = CreateContext("rhx-htmx-form");
        var output = CreateOutput("rhx-htmx-form");

        await helper.ProcessAsync(context, output);

        var attr = GetAttribute(output, "hx-post");
        Assert.NotNull(attr);
    }

    [Fact]
    public async Task Put_Method()
    {
        var helper = CreateHelper();
        helper.Page = "/Contact";
        helper.Method = "put";

        var context = CreateContext("rhx-htmx-form");
        var output = CreateOutput("rhx-htmx-form");

        await helper.ProcessAsync(context, output);

        var attr = GetAttribute(output, "hx-put");
        Assert.NotNull(attr);
    }

    [Fact]
    public async Task Patch_Method()
    {
        var helper = CreateHelper();
        helper.Page = "/Contact";
        helper.Method = "patch";

        var context = CreateContext("rhx-htmx-form");
        var output = CreateOutput("rhx-htmx-form");

        await helper.ProcessAsync(context, output);

        var attr = GetAttribute(output, "hx-patch");
        Assert.NotNull(attr);
    }

    [Fact]
    public async Task Delete_Method()
    {
        var helper = CreateHelper();
        helper.Page = "/Contact";
        helper.Method = "delete";

        var context = CreateContext("rhx-htmx-form");
        var output = CreateOutput("rhx-htmx-form");

        await helper.ProcessAsync(context, output);

        var attr = GetAttribute(output, "hx-delete");
        Assert.NotNull(attr);
    }

    // ── URL generation ──

    [Fact]
    public async Task Generates_Url_From_Page()
    {
        var helper = CreateHelper("/Contact?handler=Submit");
        helper.Page = "/Contact";
        helper.PageHandler = "Submit";

        var context = CreateContext("rhx-htmx-form");
        var output = CreateOutput("rhx-htmx-form");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "hx-post", "/Contact?handler=Submit");
    }

    // ── Response targets extension ──

    [Fact]
    public async Task Sets_HxExt_ResponseTargets()
    {
        var helper = CreateHelper();
        helper.Page = "/Contact";

        var context = CreateContext("rhx-htmx-form");
        var output = CreateOutput("rhx-htmx-form");

        await helper.ProcessAsync(context, output);

        var ext = GetAttribute(output, "hx-ext");
        Assert.Contains("response-targets", ext);
    }

    [Fact]
    public async Task Sets_Target422()
    {
        var helper = CreateHelper();
        helper.Page = "/Contact";
        helper.Target422 = "#form-errors";

        var context = CreateContext("rhx-htmx-form");
        var output = CreateOutput("rhx-htmx-form");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "hx-target-422", "#form-errors");
    }

    [Fact]
    public async Task Sets_Target4xx()
    {
        var helper = CreateHelper();
        helper.Page = "/Contact";
        helper.Target4xx = "#client-error";

        var context = CreateContext("rhx-htmx-form");
        var output = CreateOutput("rhx-htmx-form");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "hx-target-4*", "#client-error");
    }

    [Fact]
    public async Task Sets_Target5xx()
    {
        var helper = CreateHelper();
        helper.Page = "/Contact";
        helper.Target5xx = "#server-error";

        var context = CreateContext("rhx-htmx-form");
        var output = CreateOutput("rhx-htmx-form");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "hx-target-5*", "#server-error");
    }

    [Fact]
    public async Task ErrorTarget_Sets_All_Targets()
    {
        var helper = CreateHelper();
        helper.Page = "/Contact";
        helper.ErrorTarget = "#errors";

        var context = CreateContext("rhx-htmx-form");
        var output = CreateOutput("rhx-htmx-form");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "hx-target-422", "#errors");
        AssertAttribute(output, "hx-target-4*", "#errors");
        AssertAttribute(output, "hx-target-5*", "#errors");
    }

    [Fact]
    public async Task Specific_Target_Overrides_ErrorTarget()
    {
        var helper = CreateHelper();
        helper.Page = "/Contact";
        helper.ErrorTarget = "#generic";
        helper.Target422 = "#validation";

        var context = CreateContext("rhx-htmx-form");
        var output = CreateOutput("rhx-htmx-form");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "hx-target-422", "#validation");
        AssertAttribute(output, "hx-target-4*", "#generic");
        AssertAttribute(output, "hx-target-5*", "#generic");
    }

    // ── Defaults ──

    [Fact]
    public async Task Default_HxTarget_This()
    {
        var helper = CreateHelper();
        helper.Page = "/Contact";

        var context = CreateContext("rhx-htmx-form");
        var output = CreateOutput("rhx-htmx-form");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "hx-target", "this");
    }

    [Fact]
    public async Task Default_HxSwap_InnerHTML()
    {
        var helper = CreateHelper();
        helper.Page = "/Contact";

        var context = CreateContext("rhx-htmx-form");
        var output = CreateOutput("rhx-htmx-form");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "hx-swap", "innerHTML");
    }

    // ── DisableOnSubmit ──

    [Fact]
    public async Task DisableOnSubmit_Sets_HxDisabledElt()
    {
        var helper = CreateHelper();
        helper.Page = "/Contact";
        // DisableOnSubmit defaults to true

        var context = CreateContext("rhx-htmx-form");
        var output = CreateOutput("rhx-htmx-form");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "hx-disabled-elt", "find button[type='submit']");
    }

    [Fact]
    public async Task DisableOnSubmit_False_No_Attribute()
    {
        var helper = CreateHelper();
        helper.Page = "/Contact";
        helper.DisableOnSubmit = false;

        var context = CreateContext("rhx-htmx-form");
        var output = CreateOutput("rhx-htmx-form");

        await helper.ProcessAsync(context, output);

        AssertNoAttribute(output, "hx-disabled-elt");
    }

    // ── Indicator ──

    [Fact]
    public async Task Indicator_Forwarded()
    {
        var helper = CreateHelper();
        helper.Page = "/Contact";
        helper.Indicator = "#spinner";

        var context = CreateContext("rhx-htmx-form");
        var output = CreateOutput("rhx-htmx-form");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "hx-indicator", "#spinner");
    }

    // ── Error container ──

    [Fact]
    public async Task Renders_Error_Container()
    {
        var helper = CreateHelper();
        helper.Page = "/Contact";

        var context = CreateContext("rhx-htmx-form");
        var output = CreateOutput("rhx-htmx-form");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-htmx-form__error-container", content);
    }

    [Fact]
    public async Task Error_Container_Has_AriaLive()
    {
        var helper = CreateHelper();
        helper.Page = "/Contact";

        var context = CreateContext("rhx-htmx-form");
        var output = CreateOutput("rhx-htmx-form");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("aria-live=\"polite\"", content);
    }

    // ── Common attributes ──

    [Fact]
    public async Task Custom_CssClass_Appended()
    {
        var helper = CreateHelper();
        helper.Page = "/Contact";
        helper.CssClass = "my-form";

        var context = CreateContext("rhx-htmx-form");
        var output = CreateOutput("rhx-htmx-form");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-htmx-form"));
        Assert.True(HasClass(output, "my-form"));
    }

    // ── Reset on success (htmx hx-on, no custom JS) ──

    [Fact]
    public async Task ResetOnSuccess_Emits_HxOn_AfterRequest()
    {
        var helper = CreateHelper();
        helper.Page = "/Contact";
        helper.ResetOnSuccess = true;

        var context = CreateContext("rhx-htmx-form");
        var output = CreateOutput("rhx-htmx-form");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "hx-on::after-request", "if(event.detail.successful)this.reset()");
    }

    [Fact]
    public async Task Without_ResetOnSuccess_No_HxOn()
    {
        var helper = CreateHelper();
        helper.Page = "/Contact";

        var context = CreateContext("rhx-htmx-form");
        var output = CreateOutput("rhx-htmx-form");

        await helper.ProcessAsync(context, output);

        AssertNoAttribute(output, "hx-on::after-request");
    }

    [Fact]
    public async Task Error_Container_Has_No_Hidden_Attribute()
    {
        var helper = CreateHelper();
        helper.Page = "/Contact";

        var context = CreateContext("rhx-htmx-form");
        var output = CreateOutput("rhx-htmx-form");

        await helper.ProcessAsync(context, output);

        // The container is CSS-hidden while :empty; it must not carry a `hidden` attribute.
        var content = output.Content.GetContent();
        Assert.Contains("rhx-htmx-form__error-container", content);
        Assert.DoesNotContain("rhx-htmx-form__error-container\" aria-live=\"polite\" hidden", content);
    }
}
