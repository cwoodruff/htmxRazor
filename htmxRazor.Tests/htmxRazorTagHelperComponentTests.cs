using System.Threading.Tasks;
using htmxRazor.Configuration;
using htmxRazor.Infrastructure;
using Xunit;

namespace htmxRazor.Tests;

/// <summary>
/// Tests for the head-injection tag helper component, with emphasis on the
/// Content-Security-Policy contract: the library must not emit any inline
/// <c>&lt;style&gt;</c> or <c>&lt;script&gt;</c>, so consumers can run under a
/// strict policy (style-src/script-src 'self') without 'unsafe-inline'.
/// </summary>
public class htmxRazorTagHelperComponentTests : TagHelperTestBase
{
    private static async Task<string> InjectIntoAsync(string tagName, htmxRazorOptions? options = null)
    {
        var component = new htmxRazorTagHelperComponent(options ?? new htmxRazorOptions());
        var context = CreateContext(tagName);
        var output = CreateOutput(tagName);

        await component.ProcessAsync(context, output);

        return output.PreContent.GetContent() + output.PostContent.GetContent();
    }

    [Fact]
    public async Task Establishes_cascade_layer_order_via_external_stylesheet()
    {
        var injected = await InjectIntoAsync("head");

        Assert.Contains("<link rel=\"stylesheet\" href=\"/_rhx/css/rhx-layers.css\">", injected);
    }

    [Fact]
    public async Task Does_not_emit_any_inline_style_element()
    {
        var injected = await InjectIntoAsync("head");

        // The lone inline <style> (the @layer order declaration) was the one thing
        // forcing consumers to loosen their CSP. It must now be an external <link>.
        Assert.DoesNotContain("<style", injected);
        Assert.DoesNotContain("@layer", injected);
    }

    [Fact]
    public async Task Layer_order_stylesheet_precedes_component_stylesheets_in_document_order()
    {
        // PreContent (very top of <head>) renders before PostContent, and cascade
        // layer order follows document order — so the order declaration must come
        // before the component stylesheets that populate those layers (issue #13).
        var injected = await InjectIntoAsync("head");

        var layersIndex = injected.IndexOf("rhx-layers.css", System.StringComparison.Ordinal);
        var resetIndex = injected.IndexOf("rhx-reset.css", System.StringComparison.Ordinal);

        Assert.True(layersIndex >= 0, "layer order stylesheet was not injected");
        Assert.True(resetIndex >= 0, "reset stylesheet was not injected");
        Assert.True(layersIndex < resetIndex,
            "the @layer order stylesheet must precede component stylesheets");
    }

    [Fact]
    public async Task Injects_nothing_for_non_head_elements()
    {
        var injected = await InjectIntoAsync("body");

        Assert.Equal(string.Empty, injected);
    }
}
