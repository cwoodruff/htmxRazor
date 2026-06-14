using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Forms;

/// <summary>
/// Renders a native <c>&lt;select&gt;</c> control (single or multiple) with label,
/// hint, and error slots. Built entirely on the native element — no JavaScript —
/// so listbox behavior, keyboard navigation, type-ahead, and accessibility come
/// from the browser. Supports enum auto-generation, <see cref="SelectListItem"/>
/// binding, child <c>&lt;rhx-option&gt;</c> elements, and htmx integration.
/// </summary>
/// <example>
/// <code>
/// &lt;rhx-select rhx-for="Country" rhx-placeholder="Choose a country" /&gt;
///
/// &lt;rhx-select name="priority" rhx-label="Priority" rhx-items="Model.Priorities"
///              hx-get="/api/tasks" hx-trigger="change" hx-target="#task-list" /&gt;
///
/// &lt;rhx-select name="tags" rhx-label="Tags" rhx-multiple="true"&gt;
///     &lt;rhx-option value="bug"&gt;Bug&lt;/rhx-option&gt;
///     &lt;rhx-option value="feature"&gt;Feature&lt;/rhx-option&gt;
/// &lt;/rhx-select&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-select")]
public class SelectTagHelper : FormControlTagHelperBase
{
    /// <inheritdoc/>
    protected override string BlockName => "select";

    // ──────────────────────────────────────────────
    //  Select-specific properties
    // ──────────────────────────────────────────────

    /// <summary>Placeholder text shown as a non-value first option when nothing is selected.</summary>
    [HtmlAttributeName("rhx-placeholder")]
    public string? Placeholder { get; set; }

    /// <summary>Enable multi-select mode (renders <c>&lt;select multiple&gt;</c>).</summary>
    [HtmlAttributeName("rhx-multiple")]
    public bool Multiple { get; set; }

    /// <summary>Number of rows shown for a multi-select before scrolling. Default: 8.</summary>
    [HtmlAttributeName("rhx-max-visible")]
    public int MaxOptionsVisible { get; set; } = 8;

    /// <summary>
    /// Retained for source compatibility. Native selects offer no clear button; this is a no-op.
    /// </summary>
    [HtmlAttributeName("rhx-with-clear")]
    public bool WithClear { get; set; }

    /// <summary>Use the filled appearance variant.</summary>
    [HtmlAttributeName("rhx-filled")]
    public bool Filled { get; set; }

    /// <summary>
    /// Collection of SelectListItem to generate options from.
    /// When set, child rhx-option elements are ignored.
    /// </summary>
    [HtmlAttributeName("rhx-items")]
    public IEnumerable<SelectListItem>? Items { get; set; }

    // ──────────────────────────────────────────────
    //  Constructor
    // ──────────────────────────────────────────────

    public SelectTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

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

        var selectedValues = ParseSelectedValues(resolvedValue);

        // Store context for child <rhx-option> elements
        context.Items["OptionClassPrefix"] = "select";
        context.Items["SelectedValues"] = selectedValues;

        // Process children (needed for <rhx-option> elements)
        var childContent = await output.GetChildContentAsync();

        // ── CSS classes on wrapper ──
        var css = CreateCssBuilder()
            .AddIf(GetModifierClass(size), size != "medium")
            .AddIf(GetModifierClass("filled"), Filled)
            .AddIf(GetModifierClass("multiple"), Multiple)
            .AddIf(GetModifierClass("disabled"), Disabled)
            .AddIf(GetModifierClass("readonly"), Readonly)
            .AddIf(GetModifierClass("error"), hasError);

        ApplyWrapperAttributes(output, css);

        var sb = new StringBuilder();

        // ── Label ──
        var labelText = ResolveLabelText();
        if (!string.IsNullOrEmpty(labelText))
        {
            sb.Append($"<label class=\"{GetElementClass("label")}\" id=\"{Enc(labelId)}\" for=\"{Enc(resolvedId)}\">{Enc(labelText)}</label>");
        }

        // ── Native <select> ──
        sb.Append($"<select class=\"{GetElementClass("control")}\" id=\"{Enc(resolvedId)}\"");
        if (!string.IsNullOrEmpty(resolvedName))
            sb.Append($" name=\"{Enc(resolvedName)}\"");
        if (Multiple)
        {
            sb.Append(" multiple");
            sb.Append($" size=\"{MaxOptionsVisible}\"");
        }
        if (resolvedRequired)
            sb.Append(" required");
        // A native <select> has no readonly; emulate by disabling and submitting via a mirror input.
        if (Disabled || Readonly)
            sb.Append(" disabled");
        if (!string.IsNullOrEmpty(labelText))
            sb.Append($" aria-labelledby=\"{Enc(labelId)}\"");
        if (!string.IsNullOrEmpty(AriaLabel))
            sb.Append($" aria-label=\"{Enc(AriaLabel)}\"");
        var describedBy = BuildAriaDescribedBy(hintId, errorId);
        if (describedBy != null)
            sb.Append($" aria-describedby=\"{Enc(describedBy)}\"");
        if (hasError)
            sb.Append(" aria-invalid=\"true\"");
        sb.Append(BuildHtmxAttributeString());
        sb.Append(BuildValidationAttributeString());
        sb.Append(">");

        // Placeholder option (single-select only): an empty-value option so `required`
        // can fail until a real choice is made. Selected/hidden when no value is set.
        if (!Multiple && !string.IsNullOrEmpty(Placeholder))
        {
            sb.Append("<option value=\"\"");
            if (resolvedRequired) sb.Append(" disabled");
            if (string.IsNullOrEmpty(resolvedValue)) sb.Append(" selected hidden");
            sb.Append($">{Enc(Placeholder)}</option>");
        }

        // Generate options from Items, enum, or child <rhx-option> content
        var generatedOptions = GenerateOptions(selectedValues);
        if (!string.IsNullOrEmpty(generatedOptions))
            sb.Append(generatedOptions);
        else
            sb.Append(childContent.GetContent());

        sb.Append("</select>");

        // ── Readonly mirror ── a disabled <select> doesn't submit, so carry the value.
        if (Readonly && !Disabled && !string.IsNullOrEmpty(resolvedName))
        {
            if (Multiple)
            {
                foreach (var v in selectedValues)
                    sb.Append($"<input type=\"hidden\" name=\"{Enc(resolvedName)}\" value=\"{Enc(v)}\" />");
            }
            else
            {
                sb.Append($"<input type=\"hidden\" name=\"{Enc(resolvedName)}\" value=\"{Enc(resolvedValue ?? "")}\" />");
            }
        }

        // ── Hint ──
        sb.Append(BuildHintHtml(hintId));

        // ── Error ──
        sb.Append(BuildErrorHtml(errorId));

        output.Content.SetHtmlContent(sb.ToString());
    }

    // ──────────────────────────────────────────────
    //  Option generation
    // ──────────────────────────────────────────────

    private HashSet<string> ParseSelectedValues(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (Multiple)
        {
            return new HashSet<string>(
                value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                StringComparer.OrdinalIgnoreCase);
        }

        return new HashSet<string>(StringComparer.OrdinalIgnoreCase) { value };
    }

    private string? GenerateOptions(HashSet<string> selected)
    {
        if (Items != null)
            return GenerateOptionsFromItems(selected);

        if (For != null)
        {
            var modelType = Nullable.GetUnderlyingType(For.Metadata.ModelType) ?? For.Metadata.ModelType;
            if (modelType.IsEnum)
                return GenerateOptionsFromEnum(modelType, selected);
        }

        return null;
    }

    private string GenerateOptionsFromItems(HashSet<string> selected)
    {
        var sb = new StringBuilder();

        foreach (var item in Items!)
        {
            var isSelected = selected.Contains(item.Value ?? "") || item.Selected;
            sb.Append($"<option value=\"{Enc(item.Value)}\"");
            if (isSelected) sb.Append(" selected");
            if (item.Disabled) sb.Append(" disabled");
            sb.Append(">");
            sb.Append(Enc(item.Text));
            sb.Append("</option>");
        }

        return sb.ToString();
    }

    private string GenerateOptionsFromEnum(Type enumType, HashSet<string> selected)
    {
        var sb = new StringBuilder();

        foreach (var val in Enum.GetValues(enumType))
        {
            var name = val.ToString()!;
            var member = enumType.GetMember(name).FirstOrDefault();
            var displayAttr = member?.GetCustomAttribute<DisplayAttribute>();
            var text = displayAttr?.Name ?? name;

            var isSelected = selected.Contains(name);
            sb.Append($"<option value=\"{Enc(name)}\"");
            if (isSelected) sb.Append(" selected");
            sb.Append(">");
            sb.Append(Enc(text));
            sb.Append("</option>");
        }

        return sb.ToString();
    }
}
