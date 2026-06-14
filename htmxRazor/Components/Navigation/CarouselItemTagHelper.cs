using htmxRazor.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Navigation;

/// <summary>
/// Renders a single scroll-snap slide within an <c>&lt;rhx-carousel&gt;</c>.
/// Automatically registers itself with the parent carousel for counting and
/// receives a sequential index used for ARIA labels.
/// </summary>
/// <example>
/// <code>
/// &lt;rhx-carousel-item&gt;
///     &lt;img src="photo.jpg" alt="Scenic view" /&gt;
/// &lt;/rhx-carousel-item&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-carousel-item", ParentTag = "rhx-carousel")]
public class CarouselItemTagHelper : htmxRazorTagHelperBase
{
    /// <inheritdoc/>
    protected override string BlockName => "carousel";

    /// <summary>
    /// Creates a new CarouselItemTagHelper with URL generation support.
    /// </summary>
    public CarouselItemTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        // Register with parent carousel
        var index = 1;
        if (context.Items.TryGetValue("CarouselSlideCount", out var obj) && obj is List<int> list)
        {
            list.Add(1);
            index = list.Count;
        }

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var css = new CssClassBuilder(GetElementClass("slide"));
        if (!string.IsNullOrWhiteSpace(CssClass))
            css.Add(CssClass);
        output.Attributes.SetAttribute("class", css.Build());

        if (!string.IsNullOrWhiteSpace(Id))
            output.Attributes.SetAttribute("id", Id);

        if (Hidden)
            output.Attributes.SetAttribute("hidden", "hidden");

        // ARIA slide semantics
        output.Attributes.SetAttribute("role", "group");
        output.Attributes.SetAttribute("aria-roledescription", "slide");
        output.Attributes.SetAttribute("aria-label", $"Slide {index}");
        output.Attributes.SetAttribute("data-rhx-slide-index", index.ToString());

        RenderHtmxAttributes(output);
    }
}
