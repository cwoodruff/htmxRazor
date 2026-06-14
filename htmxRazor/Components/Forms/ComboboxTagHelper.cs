using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Forms;

/// <summary>
/// Renders a native autocomplete combobox: a text <c>&lt;input list&gt;</c> bound to a
/// <c>&lt;datalist&gt;</c> of suggestions. Built entirely on native elements — no
/// JavaScript — so filtering, keyboard navigation, and accessibility come from the
/// browser. With <c>rhx-server-filter="true"</c>, htmx attributes on the input fetch a
/// fresh set of <c>&lt;option&gt;</c> suggestions into the datalist as the user types.
/// </summary>
/// <remarks>
/// Native autocomplete submits the input's literal text, so there is no hidden value/code
/// separate from the displayed suggestion (the value/label distinction of the old custom
/// widget is dropped).
/// </remarks>
/// <example>
/// <code>
/// &lt;rhx-combobox rhx-for="City" rhx-placeholder="Search cities..."&gt;
///     &lt;rhx-option&gt;Austin&lt;/rhx-option&gt;
///     &lt;rhx-option&gt;Boston&lt;/rhx-option&gt;
/// &lt;/rhx-combobox&gt;
///
/// &lt;rhx-combobox name="city" rhx-label="City" rhx-server-filter="true"
///                hx-get="/api/cities" hx-trigger="input changed delay:200ms" /&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-combobox")]
public class ComboboxTagHelper : FormControlTagHelperBase
{
    /// <inheritdoc/>
    protected override string BlockName => "combobox";

    // ──────────────────────────────────────────────
    //  Combobox-specific properties
    // ──────────────────────────────────────────────

    /// <summary>Placeholder text for the text input.</summary>
    [HtmlAttributeName("rhx-placeholder")]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Retained for source compatibility. The native datalist controls its own visible
    /// suggestion count; this value is emitted as a data attribute only.
    /// </summary>
    [HtmlAttributeName("rhx-max-visible")]
    public int MaxOptionsVisible { get; set; } = 8;

    /// <summary>Use the filled appearance variant.</summary>
    [HtmlAttributeName("rhx-filled")]
    public bool Filled { get; set; }

    /// <summary>
    /// Collection of SelectListItem to generate suggestions from.
    /// When set, child rhx-option elements are ignored.
    /// </summary>
    [HtmlAttributeName("rhx-items")]
    public IEnumerable<SelectListItem>? Items { get; set; }

    /// <summary>
    /// When true, datalist suggestions are fetched server-side via htmx as the user types.
    /// Set htmx attributes (hx-get, hx-trigger) on this component; the server returns a set
    /// of <c>&lt;option&gt;</c> elements that replace the datalist contents.
    /// </summary>
    [HtmlAttributeName("rhx-server-filter")]
    public bool ServerFilter { get; set; }

    /// <summary>
    /// Retained for source compatibility. With native autocomplete the input's own
    /// <c>name</c> carries the typed text to the server, so a separate search param is unused.
    /// </summary>
    [HtmlAttributeName("rhx-search-param")]
    public string SearchParam { get; set; } = "q";

    // ──────────────────────────────────────────────
    //  Constructor
    // ──────────────────────────────────────────────

    public ComboboxTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    // ──────────────────────────────────────────────
    //  Rendering
    // ──────────────────────────────────────────────

    /// <inheritdoc/>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var resolvedName = ResolveName();
        var resolvedId = ResolveId();
        var resolvedValue = ResolveValue();
        var resolvedRequired = ResolveRequired();
        var hasError = HasError();
        var size = Size.ToLowerInvariant();

        var hintId = $"{resolvedId}-hint";
        var errorId = $"{resolvedId}-error";
        var labelId = $"{resolvedId}-label";
        var listId = $"{resolvedId}-list";

        // Store context for child <rhx-option> elements (datalist mode).
        context.Items["OptionClassPrefix"] = "combobox";
        context.Items["SelectedValues"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var childContent = await output.GetChildContentAsync();

        // ── CSS classes on wrapper ──
        var css = CreateCssBuilder()
            .AddIf(GetModifierClass(size), size != "medium")
            .AddIf(GetModifierClass("filled"), Filled)
            .AddIf(GetModifierClass("disabled"), Disabled)
            .AddIf(GetModifierClass("readonly"), Readonly)
            .AddIf(GetModifierClass("error"), hasError);

        ApplyWrapperAttributes(output, css);
        if (ServerFilter)
            output.Attributes.SetAttribute("data-rhx-server-filter", "");

        var sb = new StringBuilder();

        // ── Label ──
        var labelText = ResolveLabelText();
        if (!string.IsNullOrEmpty(labelText))
        {
            sb.Append($"<label class=\"{GetElementClass("label")}\" id=\"{Enc(labelId)}\" for=\"{Enc(resolvedId)}\">{Enc(labelText)}</label>");
        }

        // ── Native autocomplete input ──
        sb.Append($"<input class=\"{GetElementClass("control")}\" type=\"text\"");
        sb.Append($" id=\"{Enc(resolvedId)}\"");
        sb.Append($" list=\"{Enc(listId)}\"");
        if (!string.IsNullOrEmpty(resolvedName))
            sb.Append($" name=\"{Enc(resolvedName)}\"");
        sb.Append(" autocomplete=\"off\"");
        if (!string.IsNullOrEmpty(Placeholder))
            sb.Append($" placeholder=\"{Enc(Placeholder)}\"");
        if (!string.IsNullOrEmpty(resolvedValue))
            sb.Append($" value=\"{Enc(resolvedValue)}\"");
        if (!string.IsNullOrEmpty(labelText))
            sb.Append($" aria-labelledby=\"{Enc(labelId)}\"");
        if (!string.IsNullOrEmpty(AriaLabel))
            sb.Append($" aria-label=\"{Enc(AriaLabel)}\"");
        var describedBy = BuildAriaDescribedBy(hintId, errorId);
        if (describedBy != null)
            sb.Append($" aria-describedby=\"{Enc(describedBy)}\"");
        if (hasError)
            sb.Append(" aria-invalid=\"true\"");
        if (resolvedRequired)
            sb.Append(" required");
        if (Disabled)
            sb.Append(" disabled");
        if (Readonly)
            sb.Append(" readonly");
        // htmx attributes on the input (server-side suggestion fetching). The server returns
        // <option> elements; default to targeting the datalist so they replace its contents
        // (unless the author specified their own target/swap).
        if (ServerFilter)
        {
            if (string.IsNullOrWhiteSpace(HxTarget))
                sb.Append($" hx-target=\"#{Enc(listId)}\"");
            if (string.IsNullOrWhiteSpace(HxSwap))
                sb.Append(" hx-swap=\"innerHTML\"");
        }
        sb.Append(BuildHtmxAttributeString());
        sb.Append(BuildValidationAttributeString());
        sb.Append(" />");

        // ── Datalist of suggestions ──
        sb.Append($"<datalist id=\"{Enc(listId)}\" data-rhx-max-visible=\"{MaxOptionsVisible}\">");

        var generatedOptions = GenerateOptions();
        if (!string.IsNullOrEmpty(generatedOptions))
            sb.Append(generatedOptions);
        else
            sb.Append(childContent.GetContent());

        sb.Append("</datalist>");

        // ── Hint ──
        sb.Append(BuildHintHtml(hintId));

        // ── Error ──
        sb.Append(BuildErrorHtml(errorId));

        output.Content.SetHtmlContent(sb.ToString());
    }

    // ──────────────────────────────────────────────
    //  Suggestion generation (datalist <option>s; value == display text)
    // ──────────────────────────────────────────────

    private string? GenerateOptions()
    {
        if (Items != null)
            return GenerateOptionsFromItems();

        if (For != null)
        {
            var modelType = Nullable.GetUnderlyingType(For.Metadata.ModelType) ?? For.Metadata.ModelType;
            if (modelType.IsEnum)
                return GenerateOptionsFromEnum(modelType);
        }

        return null;
    }

    private string GenerateOptionsFromItems()
    {
        var sb = new StringBuilder();
        foreach (var item in Items!)
            sb.Append($"<option value=\"{Enc(item.Text)}\"></option>");
        return sb.ToString();
    }

    private string GenerateOptionsFromEnum(Type enumType)
    {
        var sb = new StringBuilder();
        foreach (var val in Enum.GetValues(enumType))
        {
            var name = val.ToString()!;
            var member = enumType.GetMember(name).FirstOrDefault();
            var displayAttr = member?.GetCustomAttribute<DisplayAttribute>();
            var text = displayAttr?.Name ?? name;
            sb.Append($"<option value=\"{Enc(text)}\"></option>");
        }
        return sb.ToString();
    }
}
