using Microsoft.AspNetCore.Mvc.RazorPages;
using htmxRazor.Components.Navigation;
using htmxRazor.Demo.Models;

namespace htmxRazor.Demo.Pages.Docs.Components;

public class AnimatedImageModel : PageModel
{
    public List<ComponentProperty> Properties { get; } = new()
    {
        new("rhx-src", "string", "-", "URL of the animated image (GIF/APNG/WebP)"),
        new("rhx-alt", "string", "-", "Alt text for the image"),
        new("rhx-poster", "string", "-", "Optional still image; when set the animation pauses (poster shown) and plays on hover/focus — pure CSS, no JS"),
    };

    public string PlaysCode => @"<rhx-animated-image
    rhx-src=""https://media.giphy.com/media/xT9IgzoKnwFNmISR8I/giphy.gif""
    rhx-alt=""Animated loading"" />";

    public string HoverCode => @"<!-- With a poster it stays paused (poster shown) and plays on hover/focus -->
<rhx-animated-image
    rhx-src=""https://media.giphy.com/media/xT9IgzoKnwFNmISR8I/giphy.gif""
    rhx-poster=""https://media.giphy.com/media/xT9IgzoKnwFNmISR8I/giphy_s.gif""
    rhx-alt=""Hover to play"" />";

    public void OnGet()
    {
        ViewData["Breadcrumbs"] = new List<BreadcrumbItem>
        {
            new("Home", "/"),
            new("Components", "/Docs/Components/AnimatedImage"),
            new("Animated Image")
        };
    }
}
