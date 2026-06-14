using System.Text;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Forms;

/// <summary>
/// A file input backed by the native <c>&lt;input type="file"&gt;</c> — no JavaScript. The
/// browser supplies the file chooser, selected-file display, and accessibility. When htmx verbs
/// are present, multipart encoding is set automatically for uploads.
/// </summary>
/// <remarks>
/// Drag-and-drop highlighting, client-side previews, and client-side size checks are dropped with
/// the custom widget; <c>rhx-accept</c> still constrains selectable types and server-side checks
/// remain authoritative. <c>rhx-max-file-size</c> is retained for source compatibility (no-op).
/// </remarks>
/// <example>
/// <code>
/// &lt;rhx-file-input name="avatar" rhx-label="Profile Photo" rhx-accept="image/*" /&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-file-input")]
public class FileInputTagHelper : FormControlTagHelperBase
{
    /// <inheritdoc/>
    protected override string BlockName => "file-input";

    /// <summary>Accepted file types (e.g., "image/*", ".pdf,.doc"). Maps to the native accept attribute.</summary>
    [HtmlAttributeName("rhx-accept")]
    public string? Accept { get; set; }

    /// <summary>Whether to allow multiple file selection. Default: false.</summary>
    [HtmlAttributeName("rhx-multiple")]
    public bool Multiple { get; set; }

    /// <summary>Retained for source compatibility; client-side size checks are dropped (validate on the server).</summary>
    [HtmlAttributeName("rhx-max-file-size")]
    public long? MaxFileSize { get; set; }

    public FileInputTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var resolvedName = ResolveName();
        var resolvedId = ResolveId();
        var resolvedRequired = ResolveRequired();
        var hasError = HasError();
        var size = Size.ToLowerInvariant();

        var hintId = $"{resolvedId}-hint";
        var errorId = $"{resolvedId}-error";

        var css = CreateCssBuilder()
            .AddIf(GetModifierClass(size), size != "medium")
            .AddIf(GetModifierClass("disabled"), Disabled)
            .AddIf(GetModifierClass("error"), hasError);
        ApplyWrapperAttributes(output, css);

        var sb = new StringBuilder();

        sb.Append(BuildLabelHtml(resolvedId));

        sb.Append($"<input type=\"file\" class=\"{GetElementClass("control")}\"");
        sb.Append($" id=\"{Enc(resolvedId)}\"");
        if (!string.IsNullOrEmpty(resolvedName))
            sb.Append($" name=\"{Enc(resolvedName)}\"");
        if (!string.IsNullOrEmpty(Accept))
            sb.Append($" accept=\"{Enc(Accept)}\"");
        if (Multiple) sb.Append(" multiple");
        if (Disabled) sb.Append(" disabled");
        if (resolvedRequired) sb.Append(" required");
        if (!string.IsNullOrEmpty(AriaLabel))
            sb.Append($" aria-label=\"{Enc(AriaLabel)}\"");
        var describedBy = BuildAriaDescribedBy(hintId, errorId);
        if (describedBy != null)
            sb.Append($" aria-describedby=\"{Enc(describedBy)}\"");
        if (hasError) sb.Append(" aria-invalid=\"true\"");

        // htmx on the input — auto-set multipart encoding when htmx verbs are present.
        var htmxStr = BuildHtmxAttributeString();
        sb.Append(htmxStr);
        if (!string.IsNullOrEmpty(htmxStr) && HxEncoding == null)
            sb.Append(" hx-encoding=\"multipart/form-data\"");

        sb.Append(BuildValidationAttributeString());
        sb.Append(" />");

        sb.Append(BuildHintHtml(hintId));
        sb.Append(BuildErrorHtml(errorId));

        output.Content.SetHtmlContent(sb.ToString());
    }
}
