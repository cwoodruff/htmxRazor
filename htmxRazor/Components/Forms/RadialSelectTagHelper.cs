using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Forms;

/// <summary>
/// A cascading category → item picker built from two native <c>&lt;select&gt;</c> elements —
/// no JavaScript. Changing the category <c>&lt;select&gt;</c> fires the wrapper's htmx request
/// (with the chosen category included automatically) to load the item <c>&lt;select&gt;</c>'s
/// options. The item select carries the bound field value; the category select carries the
/// category value (<c>rhx-category-name</c>).
/// </summary>
/// <remarks>
/// The radial SVG pie and its pointer/keyboard interaction are replaced by native selects
/// (Decision #2). The cascade endpoint receives <c>{rhx-category-name}={category}</c> and
/// <c>selected={current item}</c>, and returns the item <c>&lt;option&gt;</c> set.
/// </remarks>
/// <example>
/// <code>
/// &lt;rhx-radial-select name="FoodItem" rhx-category-name="Category"
///                    rhx-default-category="fruit" hx-get="/Menu?handler=Items"
///                    rhx-placeholder="Choose an item…" aria-label="Food"&gt;
///     &lt;rhx-radial-option rhx-value="fruit" rhx-label="Fruit" /&gt;
///     &lt;rhx-radial-option rhx-value="meat"  rhx-label="Meat" /&gt;
/// &lt;/rhx-radial-select&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-radial-select")]
[RestrictChildren("rhx-radial-option")]
public sealed class RadialSelectTagHelper : FormControlTagHelperBase
{
    /// <inheritdoc/>
    protected override string BlockName => "radial-select";

    /// <summary>Placeholder shown in the item select before a category's options load.</summary>
    [HtmlAttributeName("rhx-placeholder")] public string? Placeholder { get; set; }

    /// <summary>Form field name for the category <c>&lt;select&gt;</c>.</summary>
    [HtmlAttributeName("rhx-category-name")] public string? CategoryName { get; set; }

    /// <summary>rhx-value of the category selected on initial render.</summary>
    [HtmlAttributeName("rhx-default-category")] public string? DefaultCategory { get; set; }

    /// <summary>Creates a new <see cref="RadialSelectTagHelper"/> instance.</summary>
    public RadialSelectTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    /// <inheritdoc/>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // ── Collect child category options ──
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
        var itemsId = $"{resolvedId}-items";
        var catId = $"{resolvedId}-category";

        // ── Wrapper ──
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var css = CreateCssBuilder()
            .AddIf(GetModifierClass(size), size != "medium")
            .AddIf(GetModifierClass("disabled"), Disabled);
        ApplyWrapperAttributes(output, css);

        var sb = new StringBuilder();

        // ── Category select (drives the cascade) ──
        var (verbAttr, verbVal) = ResolveVerb();
        sb.Append($"<select class=\"{GetElementClass("category")}\" id=\"{Enc(catId)}\"");
        if (!string.IsNullOrWhiteSpace(CategoryName))
            sb.Append($" name=\"{Enc(CategoryName)}\"");
        if (!string.IsNullOrEmpty(AriaLabel))
            sb.Append($" aria-label=\"{Enc(AriaLabel)} category\"");
        if (Disabled) sb.Append(" disabled");
        if (verbAttr != null && !string.IsNullOrWhiteSpace(verbVal))
        {
            // Changing the category (and on initial load) fetches the item options. htmx sends
            // the category select's own value; hx-vals carries the current item for pre-select.
            sb.Append($" {verbAttr}=\"{Enc(verbVal)}\"");
            sb.Append(" hx-trigger=\"change, load\"");
            sb.Append($" hx-target=\"#{Enc(itemsId)}\"");
            sb.Append(" hx-swap=\"innerHTML\"");
            sb.Append($" hx-vals='{{\"selected\":\"{Enc(resolvedValue ?? "")}\"}}'");
        }
        sb.Append('>');
        foreach (var opt in options)
        {
            var selected = !string.IsNullOrEmpty(DefaultCategory)
                && string.Equals(DefaultCategory, opt.Value, StringComparison.OrdinalIgnoreCase);
            sb.Append($"<option value=\"{Enc(opt.Value)}\"");
            if (selected) sb.Append(" selected");
            if (opt.Disabled) sb.Append(" disabled");
            sb.Append($">{Enc(opt.Label)}</option>");
        }
        sb.Append("</select>");

        // ── Item select (bound field; options arrive via the cascade) ──
        sb.Append($"<select class=\"{GetElementClass("item")}\" id=\"{Enc(itemsId)}\"");
        if (!string.IsNullOrEmpty(resolvedName))
            sb.Append($" name=\"{Enc(resolvedName)}\"");
        if (!string.IsNullOrEmpty(AriaLabel))
            sb.Append($" aria-label=\"{Enc(AriaLabel)}\"");
        if (Disabled) sb.Append(" disabled");
        sb.Append(BuildValidationAttributeString());
        sb.Append('>');
        // Initial placeholder option (replaced by the cascade on load). If a value is already
        // bound (edit), keep it as a selected option so the form submits it before the cascade.
        if (!string.IsNullOrEmpty(resolvedValue))
            sb.Append($"<option value=\"{Enc(resolvedValue)}\" selected>{Enc(resolvedValue)}</option>");
        else if (!string.IsNullOrEmpty(Placeholder))
            sb.Append($"<option value=\"\">{Enc(Placeholder)}</option>");
        sb.Append("</select>");

        output.Content.SetHtmlContent(sb.ToString());
    }

    private (string? attr, string? value) ResolveVerb()
    {
        if (HxGet != null) return ("hx-get", HxGet.Length == 0 ? GenerateRouteUrl() : HxGet);
        if (HxPost != null) return ("hx-post", HxPost.Length == 0 ? GenerateRouteUrl() : HxPost);
        return (null, null);
    }
}
