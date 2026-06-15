using htmxRazor.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;

namespace htmxRazor.Tests;

/// <summary>
/// Tests for HtmxToastExtensions server-side helpers.
/// </summary>
public class HtmxToastExtensionsTests
{
    [Fact]
    public void HxToast_Sets_Trigger_Header()
    {
        var response = new DefaultHttpContext().Response;

        response.HxToast("Saved!", "success");

        Assert.True(response.Headers.ContainsKey("HX-Trigger-After-Settle"));
        var header = response.Headers["HX-Trigger-After-Settle"].ToString();
        Assert.Contains("rhx:toast", header);
        Assert.Contains("Saved!", header);
        Assert.Contains("success", header);
    }

    [Fact]
    public void HxToast_Includes_Duration_When_Specified()
    {
        var response = new DefaultHttpContext().Response;

        response.HxToast("Done", "brand", 3000);

        var header = response.Headers["HX-Trigger-After-Settle"].ToString();
        Assert.Contains("3000", header);
    }

    [Fact]
    public void HxToastOob_Returns_ContentResult()
    {
        var page = CreatePageModel();

        var result = page.HxToastOob("Item deleted", "danger");

        Assert.IsType<ContentResult>(result);
        Assert.Equal("text/html", result.ContentType);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public void HxToastOob_Contains_Toast_Markup()
    {
        var page = CreatePageModel();

        var result = page.HxToastOob("Item deleted", "danger");

        // Fully-rendered toast markup (no <rhx-toast> tag helper to process client-side).
        Assert.Contains("class=\"rhx-toast rhx-toast--danger\"", result.Content);
        Assert.Contains("Item deleted", result.Content);
        Assert.Contains("hx-swap-oob=\"beforeend:#rhx-toasts\"", result.Content);
        Assert.Contains("rhx-toast__content", result.Content);
    }

    [Fact]
    public void HxToastOob_Custom_ContainerId()
    {
        var page = CreatePageModel();

        var result = page.HxToastOob("Test", containerId: "my-container");

        Assert.Contains("hx-swap-oob=\"beforeend:#my-container\"", result.Content);
    }

    [Fact]
    public void HxToastOob_Encodes_Message()
    {
        var page = CreatePageModel();

        var result = page.HxToastOob("<script>alert('xss')</script>");

        Assert.DoesNotContain("<script>", result.Content);
        Assert.Contains("&lt;script&gt;", result.Content);
    }

    private static PageModel CreatePageModel()
    {
        var httpContext = new DefaultHttpContext();
        var modelState = new ModelStateDictionary();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), modelState);
        var pageContext = new PageContext(actionContext);
        var page = new Mock<PageModel>();
        page.Object.PageContext = pageContext;
        return page.Object;
    }
}
