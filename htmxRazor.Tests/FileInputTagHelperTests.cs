using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using htmxRazor.Components.Forms;
using Xunit;

namespace htmxRazor.Tests;

// rhx-file-input renders a native <input type="file"> (no JS). The browser supplies the file
// chooser and selected-file display; htmx uploads get multipart encoding automatically.
public class FileInputTagHelperTests : TagHelperTestBase
{
    private FileInputTagHelper CreateHelper() => new(CreateUrlHelperFactory());

    private string Render(FileInputTagHelper helper)
    {
        var context = CreateContext("rhx-file-input");
        var output = CreateOutput("rhx-file-input");
        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);
        return output.Content.GetContent();
    }

    [Fact]
    public void Renders_Div_Wrapper_With_Block_Class()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-file-input");
        var output = CreateOutput("rhx-file-input");
        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.Equal("div", output.TagName);
        Assert.True(HasClass(output, "rhx-file-input"));
    }

    [Fact]
    public void Renders_Native_File_Input()
    {
        var helper = CreateHelper();
        helper.Name = "avatar";
        var html = Render(helper);
        Assert.Contains("type=\"file\"", html);
        Assert.Contains("rhx-file-input__control", html);
        Assert.Contains("name=\"avatar\"", html);
    }

    [Fact]
    public void Id_Sets_On_Input()
    {
        var helper = CreateHelper();
        helper.Id = "my-file";
        helper.Name = "avatar";
        var html = Render(helper);
        Assert.Contains("id=\"my-file\"", html);
    }

    [Fact]
    public void Accept_Sets_Attribute()
    {
        var helper = CreateHelper();
        helper.Accept = "image/*";
        var html = Render(helper);
        Assert.Contains("accept=\"image/*\"", html);
    }

    [Fact]
    public void No_Accept_Omits_Attribute()
    {
        var helper = CreateHelper();
        var html = Render(helper);
        Assert.DoesNotContain("accept=", html);
    }

    [Fact]
    public void Multiple_Sets_Attribute()
    {
        var helper = CreateHelper();
        helper.Multiple = true;
        var html = Render(helper);
        Assert.Contains(" multiple", html);
    }

    [Fact]
    public void No_Multiple_By_Default()
    {
        var helper = CreateHelper();
        var html = Render(helper);
        Assert.DoesNotContain(" multiple", html);
    }

    [Fact]
    public void Label_Renders_For_The_Input()
    {
        var helper = CreateHelper();
        helper.Label = "Upload Photo";
        helper.Name = "photo";
        var html = Render(helper);
        Assert.Contains("rhx-file-input__label", html);
        Assert.Contains("Upload Photo", html);
        Assert.Contains("for=\"photo\"", html);
    }

    [Fact]
    public void Hint_Renders()
    {
        var helper = CreateHelper();
        helper.Hint = "Max 5MB";
        helper.Name = "photo";
        var html = Render(helper);
        Assert.Contains("rhx-file-input__hint", html);
        Assert.Contains("Max 5MB", html);
    }

    [Fact]
    public void Error_Adds_Modifier_And_Message()
    {
        var helper = CreateHelper();
        helper.Name = "photo";
        var context = CreateContext("rhx-file-input");
        var output = CreateOutput("rhx-file-input");
        var vc = CreateViewContext();
        vc.ModelState.AddModelError("photo", "File is required");
        helper.ViewContext = vc;
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-file-input--error"));
        var content = output.Content.GetContent();
        Assert.Contains("File is required", content);
        Assert.Contains("aria-invalid=\"true\"", content);
    }

    [Fact]
    public void Disabled_Adds_Modifier_And_Native_Attribute()
    {
        var helper = CreateHelper();
        helper.Disabled = true;
        var context = CreateContext("rhx-file-input");
        var output = CreateOutput("rhx-file-input");
        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-file-input--disabled"));
        Assert.Contains(" disabled", output.Content.GetContent());
    }

    [Fact]
    public void Required_Adds_Native_Required()
    {
        var helper = CreateHelper();
        helper.Required = true;
        var html = Render(helper);
        Assert.Contains(" required", html);
    }

    [Fact]
    public void Small_Size_Adds_Modifier()
    {
        var helper = CreateHelper();
        helper.Size = "small";
        var context = CreateContext("rhx-file-input");
        var output = CreateOutput("rhx-file-input");
        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);
        Assert.True(HasClass(output, "rhx-file-input--small"));
    }

    [Fact]
    public void Large_Size_Adds_Modifier()
    {
        var helper = CreateHelper();
        helper.Size = "large";
        var context = CreateContext("rhx-file-input");
        var output = CreateOutput("rhx-file-input");
        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);
        Assert.True(HasClass(output, "rhx-file-input--large"));
    }

    [Fact]
    public void Htmx_On_Input()
    {
        var helper = CreateHelper();
        helper.HxPost = "/upload";
        helper.HxTrigger = "change";
        var html = Render(helper);
        Assert.Contains("hx-post=\"/upload\"", html);
        Assert.Contains("hx-trigger=\"change\"", html);
    }

    [Fact]
    public void Htmx_Auto_Sets_Multipart_Encoding()
    {
        var helper = CreateHelper();
        helper.HxPost = "/upload";
        var html = Render(helper);
        Assert.Contains("hx-encoding=\"multipart/form-data\"", html);
    }

    [Fact]
    public void No_Htmx_No_Multipart_Encoding()
    {
        var helper = CreateHelper();
        var html = Render(helper);
        Assert.DoesNotContain("hx-encoding", html);
    }

    [Fact]
    public void Custom_Css_Class_Merged()
    {
        var helper = CreateHelper();
        helper.CssClass = "my-upload";
        var context = CreateContext("rhx-file-input");
        var output = CreateOutput("rhx-file-input");
        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);
        Assert.True(HasClass(output, "rhx-file-input"));
        Assert.True(HasClass(output, "my-upload"));
    }

    [Fact]
    public void Aria_Label_Sets_Attribute()
    {
        var helper = CreateHelper();
        helper.AriaLabel = "Upload file";
        var html = Render(helper);
        Assert.Contains("aria-label=\"Upload file\"", html);
    }
}
