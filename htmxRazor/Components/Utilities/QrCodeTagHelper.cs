using System.Globalization;
using System.Text;
using htmxRazor.Infrastructure;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Utilities;

/// <summary>
/// Renders a QR code as an inline <c>&lt;svg&gt;</c> element. The QR code is generated
/// entirely server-side by a lightweight built-in algorithm (no external dependency,
/// no JavaScript). Supports versions 1–10, byte mode, and all EC levels (L/M/Q/H).
/// </summary>
/// <example>
/// <code>
/// &lt;rhx-qr-code rhx-value="https://example.com" rhx-size="200" /&gt;
///
/// &lt;rhx-qr-code rhx-value="@Model.ShareUrl"
///               rhx-label="Share link QR code"
///               rhx-error-correction="H" /&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-qr-code")]
public class QrCodeTagHelper : htmxRazorTagHelperBase
{
    // The previous canvas implementation drew modules edge-to-edge with no quiet
    // zone, so the SVG keeps the same behaviour for visual parity.
    private const int QuietZone = 0;

    private const string DefaultFill = "var(--rhx-color-text)";
    private const string DefaultBackground = "var(--rhx-color-surface)";

    /// <inheritdoc/>
    protected override string BlockName => "qr-code";

    /// <summary>
    /// The data to encode in the QR code.
    /// </summary>
    [HtmlAttributeName("rhx-value")]
    public string Value { get; set; } = "";

    /// <summary>
    /// Accessible label for the QR code image.
    /// </summary>
    [HtmlAttributeName("rhx-label")]
    public string? Label { get; set; }

    /// <summary>
    /// Pixel dimensions (width and height) of the rendered SVG.
    /// Default: 128.
    /// </summary>
    [HtmlAttributeName("rhx-size")]
    public int Size { get; set; } = 128;

    /// <summary>
    /// Foreground (module) color.
    /// Defaults to the current theme's text color when not set.
    /// </summary>
    [HtmlAttributeName("rhx-fill")]
    public string? Fill { get; set; }

    /// <summary>
    /// Background color.
    /// Defaults to the current theme's surface color when not set.
    /// </summary>
    [HtmlAttributeName("rhx-background")]
    public string? Background { get; set; }

    /// <summary>
    /// Corner radius for each module dot, as a fraction of the cell size (0–0.5).
    /// Default: 0.
    /// </summary>
    [HtmlAttributeName("rhx-radius")]
    public double Radius { get; set; }

    /// <summary>
    /// Reed-Solomon error correction level.
    /// Options: L, M, Q, H.
    /// Default: M.
    /// </summary>
    [HtmlAttributeName("rhx-error-correction")]
    public string ErrorCorrection { get; set; } = "M";

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "svg";
        output.TagMode = TagMode.StartTagAndEndTag;

        // ── CSS classes & base attributes ──
        var css = CreateCssBuilder();
        ApplyBaseAttributes(output, css);
        RenderHtmxAttributes(output);

        var ec = ErrorCorrection.ToUpperInvariant();
        var modules = QrCodeGenerator.Generate(Value, ec);
        var moduleCount = modules?.Length ?? 0;
        var viewSize = moduleCount + 2 * QuietZone;
        if (viewSize <= 0) viewSize = 1;

        // ── SVG attributes ──
        output.Attributes.SetAttribute("xmlns", "http://www.w3.org/2000/svg");
        output.Attributes.SetAttribute("width", Size.ToString(CultureInfo.InvariantCulture));
        output.Attributes.SetAttribute("height", Size.ToString(CultureInfo.InvariantCulture));
        output.Attributes.SetAttribute("viewBox", $"0 0 {viewSize} {viewSize}");
        output.Attributes.SetAttribute("role", "img");

        if (!string.IsNullOrWhiteSpace(Label))
            AriaAttributeHelper.AriaLabel(output, Label);

        // ── SVG content ──
        var fill = string.IsNullOrEmpty(Fill) ? DefaultFill : Fill;
        var background = string.IsNullOrEmpty(Background) ? DefaultBackground : Background;

        output.Content.Clear();

        // Background rectangle fills the whole viewBox.
        output.Content.AppendHtml(
            $"<rect width=\"{viewSize}\" height=\"{viewSize}\" fill=\"{background}\" />");

        if (modules == null || moduleCount == 0)
            return;

        output.Content.AppendHtml(BuildModulesMarkup(modules, moduleCount, fill));
    }

    private string BuildModulesMarkup(bool[][] modules, int moduleCount, string fill)
    {
        // With a quiet zone of 0 the module grid maps 1:1 onto the viewBox.
        const int offset = QuietZone;

        if (Radius > 0)
        {
            // Rounded modules: one <rect> per dark module with rx = Radius * cellSize.
            // cellSize is 1 in viewBox units, so rx is simply Radius.
            var rx = Radius.ToString("0.####", CultureInfo.InvariantCulture);
            var sb = new StringBuilder();
            for (var r = 0; r < moduleCount; r++)
            {
                for (var c = 0; c < moduleCount; c++)
                {
                    if (!modules[r][c]) continue;
                    var x = c + offset;
                    var y = r + offset;
                    sb.Append($"<rect x=\"{x}\" y=\"{y}\" width=\"1\" height=\"1\" rx=\"{rx}\" fill=\"{fill}\" />");
                }
            }
            return sb.ToString();
        }

        // Square modules: a single compact <path> covering all dark modules.
        var path = new StringBuilder();
        for (var r = 0; r < moduleCount; r++)
        {
            for (var c = 0; c < moduleCount; c++)
            {
                if (!modules[r][c]) continue;
                var x = c + offset;
                var y = r + offset;
                path.Append($"M{x} {y}h1v1h-1z");
            }
        }

        return $"<path d=\"{path}\" fill=\"{fill}\" />";
    }
}
