using htmxRazor.Components.Imagery;
using htmxRazor.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Navigation;

/// <summary>
/// Renders an accessible CSS scroll-snap carousel/slider with optional
/// previous/next navigation buttons. Scrolling, swiping (trackpad/touch),
/// and keyboard navigation are pure CSS — no per-component JavaScript.
/// The prev/next buttons are handled by the shared scroll-button shim
/// (<c>rhx-scroll-buttons.js</c>) via the <c>data-rhx-scroll-*</c> contract.
/// </summary>
/// <example>
/// <code>
/// &lt;rhx-carousel&gt;
///     &lt;rhx-carousel-item&gt;Slide 1&lt;/rhx-carousel-item&gt;
///     &lt;rhx-carousel-item&gt;Slide 2&lt;/rhx-carousel-item&gt;
/// &lt;/rhx-carousel&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-carousel")]
public class CarouselTagHelper : htmxRazorTagHelperBase
{
    /// <summary>Context key used to share the slides-per-page value with child slides.</summary>
    internal const string SlidesPerPageKey = "CarouselSlidesPerPage";

    /// <inheritdoc/>
    protected override string BlockName => "carousel";

    /// <summary>
    /// Whether to show previous/next navigation buttons. Default: true.
    /// </summary>
    [HtmlAttributeName("rhx-navigation")]
    public bool Navigation { get; set; } = true;

    /// <summary>
    /// Number of slides visible at once. Default: 1.
    /// </summary>
    [HtmlAttributeName("rhx-slides-per-page")]
    public int SlidesPerPage { get; set; } = 1;

    /// <summary>
    /// Accessible label for the carousel region.
    /// </summary>
    [HtmlAttributeName("aria-label")]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Creates a new CarouselTagHelper with URL generation support.
    /// </summary>
    public CarouselTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    /// <inheritdoc/>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // Set up slide counter for child items (tests may pre-populate)
        if (!context.Items.ContainsKey("CarouselSlideCount"))
            context.Items["CarouselSlideCount"] = new List<int>();
        var slideList = (List<int>)context.Items["CarouselSlideCount"];

        context.Items[typeof(CarouselTagHelper)] = this;

        // Share slides-per-page with child slides so they can size their flex-basis.
        var perPage = SlidesPerPage < 1 ? 1 : SlidesPerPage;
        context.Items[SlidesPerPageKey] = perPage;

        // Process children — CarouselItemTagHelpers register themselves
        var childContent = await output.GetChildContentAsync();
        var slideCount = slideList.Count;

        // Render container
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var css = CreateCssBuilder();
        ApplyBaseAttributes(output, css);

        // Scroll-snap shim contract hook
        output.Attributes.SetAttribute("data-rhx-scrollable", "");
        output.Attributes.SetAttribute("data-rhx-slide-count", slideCount.ToString());

        // ARIA
        output.Attributes.SetAttribute("role", "region");
        output.Attributes.SetAttribute("aria-roledescription", "carousel");
        if (!string.IsNullOrWhiteSpace(AriaLabel))
            output.Attributes.SetAttribute("aria-label", AriaLabel);

        RenderHtmxAttributes(output);

        // Assemble inner HTML
        output.Content.Clear();

        var showNav = Navigation && slideCount > 1;

        // Prev button (handled by the shared scroll-button shim)
        if (showNav)
        {
            var chevronLeft = IconRegistry.Get("chevron-left") ?? "";
            output.Content.AppendHtml(
                $"<button class=\"{GetElementClass("nav")} {GetElementClass("nav")}--prev\" " +
                "type=\"button\" data-rhx-scroll-prev aria-label=\"Previous slide\">" +
                "<svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" " +
                $"stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\">{chevronLeft}</svg>" +
                "</button>");
        }

        // Scroll-snap viewport (directly contains the slides)
        var perPageStyle = perPage > 1 ? $" style=\"--rhx-carousel-per-page: {perPage};\"" : "";
        output.Content.AppendHtml(
            $"<div class=\"{GetElementClass("viewport")}\" data-rhx-scroll-viewport aria-live=\"polite\"{perPageStyle}>");
        output.Content.AppendHtml(childContent);
        output.Content.AppendHtml("</div>");

        // Next button (handled by the shared scroll-button shim)
        if (showNav)
        {
            var chevronRight = IconRegistry.Get("chevron-right") ?? "";
            output.Content.AppendHtml(
                $"<button class=\"{GetElementClass("nav")} {GetElementClass("nav")}--next\" " +
                "type=\"button\" data-rhx-scroll-next aria-label=\"Next slide\">" +
                "<svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" " +
                $"stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\">{chevronRight}</svg>" +
                "</button>");
        }
    }
}
