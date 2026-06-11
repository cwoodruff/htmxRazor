using System.Globalization;
using System.Text;
using htmxRazor.Components.Imagery;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Forms;

/// <summary>
/// Compound control: a rectangular trigger button flush-left against a dropdown.
/// The trigger opens a circular SVG pie popup whose wedges (categories, each with a
/// color + icon) drive the dropdown's option set via an htmx cascade. Selecting a wedge
/// echoes its color + icon onto the trigger, swaps the dropdown's option set, and
/// auto-selects the first option.
/// </summary>
/// <example>
/// <code>
/// &lt;rhx-radial-select rhx-for="FoodItem" rhx-category-name="Category"
///                    rhx-placeholder="Choose an item…" aria-label="Food category"&gt;
///     &lt;rhx-radial-option rhx-value="fruit" rhx-label="Fruit" rhx-icon="apple"
///                        rhx-color="danger" hx-get="/Menu?handler=Items&amp;cat=fruit" /&gt;
///     &lt;rhx-radial-option rhx-value="meat"  rhx-label="Meat"  rhx-icon="drumstick"
///                        rhx-color="success" hx-get="/Menu?handler=Items&amp;cat=meat" /&gt;
/// &lt;/rhx-radial-select&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-radial-select")]
[RestrictChildren("rhx-radial-option")]
public sealed class RadialSelectTagHelper : FormControlTagHelperBase
{
    /// <inheritdoc/>
    protected override string BlockName => "radial-select";

    /// <summary>Placeholder shown before any option is selected / when a category is empty.</summary>
    [HtmlAttributeName("rhx-placeholder")] public string? Placeholder { get; set; }

    /// <summary>Optional form field name for submitting the active category value.</summary>
    [HtmlAttributeName("rhx-category-name")] public string? CategoryName { get; set; }

    /// <summary>Optional rhx-value of the wedge to activate on initial render.</summary>
    [HtmlAttributeName("rhx-default-category")] public string? DefaultCategory { get; set; }

    // Ordered cycle for wedges that omit rhx-color. Only tokens that exist.
    private static readonly string[] ColorCycle =
        { "brand", "success", "warning", "danger", "neutral" };

    private static readonly HashSet<string> AllowedColors =
        new(ColorCycle, StringComparer.OrdinalIgnoreCase);

    // Pie geometry (SVG user units).
    private const double PieCx = 100, PieCy = 100, PieR = 92, IconR = 58;

    /// <summary>Creates a new <see cref="RadialSelectTagHelper"/> instance.</summary>
    public RadialSelectTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    /// <inheritdoc/>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // ── Collect child wedges ──
        // Reuse a list already present on the context (e.g. seeded by a test) so the same
        // instance receives the children; otherwise seed a fresh one.
        List<RadialOptionData> options;
        if (context.Items.TryGetValue(RadialOptionTagHelper.ItemsKey, out var existing)
            && existing is List<RadialOptionData> seeded)
        {
            options = seeded;
        }
        else
        {
            options = new List<RadialOptionData>();
            context.Items[RadialOptionTagHelper.ItemsKey] = options;
        }
        await output.GetChildContentAsync();

        var resolvedName = ResolveName();
        var resolvedId = ResolveId();
        if (string.IsNullOrEmpty(resolvedId))
            resolvedId = "rhx-radial-" + context.UniqueId;
        var resolvedValue = ResolveValue();
        var size = Size.ToLowerInvariant();
        var listboxId = $"{resolvedId}-listbox";
        var pieId = $"{resolvedId}-pie";

        // ── Resolve per-wedge colors (explicit or cycle) ──
        var colored = ResolveColors(options);

        // Active wedge (rhx-default-category) for the initial trigger echo.
        var active = colored.FirstOrDefault(c =>
            !string.IsNullOrEmpty(DefaultCategory) &&
            string.Equals(DefaultCategory, c.Opt.Value, StringComparison.OrdinalIgnoreCase));

        // ── Wrapper ──
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var css = CreateCssBuilder()
            .AddIf(GetModifierClass(size), size != "medium")
            .AddIf(GetModifierClass("disabled"), Disabled);
        ApplyWrapperAttributes(output, css);
        output.Attributes.SetAttribute("data-rhx-radial-select", "");

        var sb = new StringBuilder();
        sb.Append($"<div class=\"{GetElementClass("group")}\">");

        // ── Trigger button ──
        sb.Append($"<button type=\"button\" class=\"{GetElementClass("trigger")}\"");
        sb.Append($" id=\"{Enc(resolvedId)}-trigger\"");
        sb.Append(" aria-haspopup=\"menu\" aria-expanded=\"false\"");
        sb.Append($" aria-controls=\"{Enc(pieId)}\"");
        if (!string.IsNullOrEmpty(AriaLabel)) sb.Append($" aria-label=\"{Enc(AriaLabel)}\"");
        if (active.Opt != null) sb.Append($" data-rhx-active-color=\"{Enc(active.Color)}\"");
        if (active.Opt != null) sb.Append($" style=\"--rhx-radial-active: var(--rhx-color-{Enc(active.Color)}-500)\"");
        if (Disabled) sb.Append(" disabled");
        sb.Append('>');
        if (active.Opt != null && !string.IsNullOrWhiteSpace(active.Opt.Icon))
        {
            var triggerIcon = IconRegistry.Get(active.Opt.Icon!);
            if (triggerIcon != null)
                sb.Append($"<span class=\"{GetElementClass("trigger-icon")}\" aria-hidden=\"true\"><svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\">{triggerIcon}</svg></span>");
        }
        sb.Append("</button>");

        // ── Dropdown shell (listbox populated by the htmx cascade) ──
        sb.Append($"<div class=\"{GetElementClass("dropdown")}\">");
        sb.Append($"<div class=\"{GetElementClass("listbox")}\" id=\"{Enc(listboxId)}\" role=\"listbox\"");
        if (!string.IsNullOrEmpty(AriaLabel)) sb.Append($" aria-label=\"{Enc(AriaLabel)}\"");
        sb.Append('>');
        if (!string.IsNullOrEmpty(Placeholder))
            sb.Append($"<div class=\"{GetElementClass("placeholder")}\">{Enc(Placeholder)}</div>");
        sb.Append("</div>"); // listbox
        sb.Append("</div>"); // dropdown
        sb.Append("</div>"); // group

        // ── Pie popup ──
        sb.Append($"<div class=\"{GetElementClass("pie")}\" id=\"{Enc(pieId)}\" role=\"menu\"");
        if (!string.IsNullOrEmpty(AriaLabel)) sb.Append($" aria-label=\"{Enc(AriaLabel)}\"");
        sb.Append(" hidden>");
        sb.Append(BuildPie(colored));
        sb.Append("</div>");

        // ── Hidden value input (dropdown value) ──
        sb.Append($"<input type=\"hidden\" data-rhx-radial-value name=\"{Enc(resolvedName)}\" value=\"{Enc(resolvedValue ?? "")}\"");
        sb.Append(BuildValidationAttributeString());
        sb.Append(" />");

        // ── Hidden category input ──
        if (!string.IsNullOrWhiteSpace(CategoryName))
        {
            var cat = DefaultCategory ?? "";
            sb.Append($"<input type=\"hidden\" data-rhx-radial-category name=\"{Enc(CategoryName)}\" value=\"{Enc(cat)}\" />");
        }

        output.Content.SetHtmlContent(sb.ToString());
    }

    // ──────────────────────────────────────────────
    //  Color resolution (§3.3)
    // ──────────────────────────────────────────────

    private List<(RadialOptionData Opt, string Color)> ResolveColors(List<RadialOptionData> options)
    {
        var result = new List<(RadialOptionData, string)>();
        var cycleIndex = 0;
        foreach (var opt in options)
        {
            string color;
            if (!string.IsNullOrWhiteSpace(opt.Color) && AllowedColors.Contains(opt.Color))
            {
                color = opt.Color.ToLowerInvariant();
            }
            else
            {
                color = ColorCycle[cycleIndex % ColorCycle.Length];
                cycleIndex++;
            }
            result.Add((opt, color));
        }
        return result;
    }

    /// <summary>Test seam for color resolution. Not part of the public API.</summary>
    internal List<(RadialOptionData Opt, string Color)> ResolveColorsForTest(List<RadialOptionData> options)
        => ResolveColors(options);

    // ──────────────────────────────────────────────
    //  SVG pie rendering (§4.2)
    // ──────────────────────────────────────────────

    private string BuildPie(List<(RadialOptionData Opt, string Color)> colored)
    {
        if (colored.Count == 0) return "";

        var n = colored.Count;
        var slice = 360.0 / n;
        var sb = new StringBuilder();

        sb.Append("<svg class=\"" + GetElementClass("wheel") + "\" viewBox=\"0 0 200 200\" ");
        sb.Append("xmlns=\"http://www.w3.org/2000/svg\" aria-hidden=\"false\">");

        for (var i = 0; i < n; i++)
        {
            var (opt, color) = colored[i];
            var start = -90 + i * slice;          // start at 12 o'clock
            var end = start + slice;
            var (x1, y1) = Polar(PieR, start);
            var (x2, y2) = Polar(PieR, end);
            var largeArc = slice > 180 ? 1 : 0;

            var isChecked = !string.IsNullOrEmpty(DefaultCategory)
                && string.Equals(DefaultCategory, opt.Value, StringComparison.OrdinalIgnoreCase);

            // Single-wedge (n==1) degenerates to a full circle.
            var d = n == 1
                ? $"M {F(PieCx - PieR)} {F(PieCy)} a {F(PieR)} {F(PieR)} 0 1 0 {F(PieR * 2)} 0 a {F(PieR)} {F(PieR)} 0 1 0 {F(-PieR * 2)} 0"
                : $"M {F(PieCx)} {F(PieCy)} L {F(x1)} {F(y1)} A {F(PieR)} {F(PieR)} 0 {largeArc} 1 {F(x2)} {F(y2)} Z";

            sb.Append("<g class=\"" + GetElementClass("wedge") + "\"");
            sb.Append($" data-rhx-radial-option-value=\"{Enc(opt.Value)}\"");
            sb.Append(" role=\"menuitemradio\"");
            sb.Append($" aria-checked=\"{(isChecked ? "true" : "false")}\"");
            sb.Append($" aria-label=\"{Enc(opt.Label)}\"");
            if (opt.Disabled) sb.Append(" aria-disabled=\"true\"");
            if (!string.IsNullOrWhiteSpace(opt.HxGet)) sb.Append($" data-rhx-radial-hx-get=\"{Enc(opt.HxGet)}\"");
            sb.Append($" data-rhx-radial-color=\"{Enc(color)}\"");
            sb.Append(" tabindex=\"-1\">");

            sb.Append($"<path d=\"{d}\" fill=\"var(--rhx-color-{color}-500)\" />");

            // Icon at wedge centroid angle.
            var mid = start + slice / 2.0;
            var (ix, iy) = Polar(IconR, mid);
            var iconSvg = !string.IsNullOrWhiteSpace(opt.Icon) ? IconRegistry.Get(opt.Icon!) : null;
            if (iconSvg != null)
            {
                sb.Append($"<g transform=\"translate({F(ix - 8)} {F(iy - 8)})\" class=\"{GetElementClass("wedge-icon")}\" aria-hidden=\"true\">");
                sb.Append("<svg viewBox=\"0 0 24 24\" width=\"16\" height=\"16\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\">");
                sb.Append(iconSvg);
                sb.Append("</svg>");
                sb.Append("</g>");
            }
            sb.Append("</g>");
        }

        sb.Append("</svg>");
        return sb.ToString();
    }

    private static (double X, double Y) Polar(double radius, double angleDeg)
    {
        var rad = angleDeg * Math.PI / 180.0;
        return (PieCx + radius * Math.Cos(rad), PieCy + radius * Math.Sin(rad));
    }

    private static string F(double v) =>
        v.ToString("0.###", CultureInfo.InvariantCulture);
}
