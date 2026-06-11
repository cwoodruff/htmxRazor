using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class RadialSelectTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/RadialSelect";
    private const string Scope = "#panel-basic-preview ";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Renders_trigger_group_and_hidden_pie_menu(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator(Scope + "div.rhx-radial-select");
        await Assertions.Expect(wrapper).ToHaveAttributeAsync("data-rhx-radial-select", "");

        var trigger = wrapper.Locator("button.rhx-radial-select__trigger");
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-haspopup", "menu");
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");

        var pie = wrapper.Locator(".rhx-radial-select__pie");
        await Assertions.Expect(pie).Not.ToBeVisibleAsync();
        await Assertions.Expect(pie.Locator("[role='menuitemradio']")).ToHaveCountAsync(4);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Default_category_wedge_is_checked_initially(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var fruit = page.Locator(Scope + "[role='menuitemradio'][data-rhx-radial-option-value='fruit']");
        await Assertions.Expect(fruit).ToHaveAttributeAsync("aria-checked", "true");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_trigger_opens_pie(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var trigger = page.Locator(Scope + "button.rhx-radial-select__trigger");
        var pie = page.Locator(Scope + ".rhx-radial-select__pie");

        await trigger.ClickAsync();
        await Assertions.Expect(pie).ToBeVisibleAsync();
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Pie_opens_centered_over_trigger(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var trigger = page.Locator(Scope + "button.rhx-radial-select__trigger");
        await trigger.ScrollIntoViewIfNeededAsync();

        await trigger.ClickAsync();
        var pie = page.Locator(Scope + ".rhx-radial-select__pie");
        await Assertions.Expect(pie).ToBeVisibleAsync();
        await page.WaitForTimeoutAsync(200); // let the fade-in settle

        var pieBox = await pie.BoundingBoxAsync();
        var trigBox = await trigger.BoundingBoxAsync();
        Assert.NotNull(pieBox);
        Assert.NotNull(trigBox);

        var pieCenterX = pieBox!.X + pieBox.Width / 2;
        var pieCenterY = pieBox.Y + pieBox.Height / 2;
        var trigCenterX = trigBox!.X + trigBox.Width / 2;
        var trigCenterY = trigBox.Y + trigBox.Height / 2;

        // Pie is centered OVER the trigger: its center aligns with the trigger's center
        // on both axes (overlapping it), not floating above or off to the right.
        Assert.True(System.Math.Abs(pieCenterX - trigCenterX) < 4,
            $"Pie center X {pieCenterX} should align with trigger center X {trigCenterX}.");
        Assert.True(System.Math.Abs(pieCenterY - trigCenterY) < 4,
            $"Pie center Y {pieCenterY} should align with trigger center Y {trigCenterY}.");

        // The trigger sits inside the pie's bounds (the pie covers it).
        Assert.True(trigCenterX >= pieBox.X && trigCenterX <= pieBox.X + pieBox.Width
                 && trigCenterY >= pieBox.Y && trigCenterY <= pieBox.Y + pieBox.Height,
            "Trigger center should fall within the pie bounds (pie covers the trigger).");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Selecting_wedge_cascades_and_autoselects_first(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var trigger = page.Locator(Scope + "button.rhx-radial-select__trigger");
        await trigger.ClickAsync();

        var meat = page.Locator(Scope + "[role='menuitemradio'][data-rhx-radial-option-value='meat']");
        await meat.ClickAsync();

        // Listbox repopulates with the meat category's options...
        var listbox = page.Locator(Scope + ".rhx-radial-select__listbox");
        var firstOption = listbox.Locator("[role='option']").First;
        await Assertions.Expect(firstOption).ToContainTextAsync("Chicken");
        // ...and the first option is auto-selected.
        await Assertions.Expect(firstOption).ToHaveAttributeAsync("aria-selected", "true");

        // ...the chosen category is reflected in the hidden category input + on the trigger...
        await Assertions.Expect(page.Locator(Scope + "[data-rhx-radial-category]")).ToHaveValueAsync("meat");
        await Assertions.Expect(trigger).ToHaveAttributeAsync("data-rhx-active-color", "warning");
        // ...and the combobox value display shows the auto-selected first item.
        await Assertions.Expect(page.Locator(Scope + "[data-rhx-radial-display]")).ToContainTextAsync("Chicken");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Dropdown_opens_as_vertical_popup_and_selecting_updates_display(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        // Load a category so the dropdown has items.
        await page.Locator(Scope + "button.rhx-radial-select__trigger").ClickAsync();
        await page.Locator(Scope + "[role='menuitemradio'][data-rhx-radial-option-value='fruit']").ClickAsync();

        var combobox = page.Locator(Scope + "button.rhx-radial-select__combobox");
        var listbox = page.Locator(Scope + ".rhx-radial-select__listbox");

        // Listbox is a popup — hidden until the combobox is clicked.
        await Assertions.Expect(listbox).Not.ToBeVisibleAsync();
        await combobox.ClickAsync();
        await Assertions.Expect(listbox).ToBeVisibleAsync();
        await Assertions.Expect(combobox).ToHaveAttributeAsync("aria-expanded", "true");

        // Options stack vertically (each below the previous), not in a horizontal row.
        var options = listbox.Locator("[role='option']");
        await Assertions.Expect(options).ToHaveCountAsync(4);
        var firstBox = await options.Nth(0).BoundingBoxAsync();
        var secondBox = await options.Nth(1).BoundingBoxAsync();
        Assert.NotNull(firstBox);
        Assert.NotNull(secondBox);
        Assert.True(secondBox!.Y >= firstBox!.Y + firstBox.Height - 1,
            "Second option should sit below the first (vertical stack), not beside it.");
        Assert.True(System.Math.Abs(secondBox.X - firstBox.X) < 1,
            "Options should share the same left edge (column), not flow horizontally.");

        // Selecting an option updates the value display and closes the popup.
        await options.Nth(2).ClickAsync();
        await Assertions.Expect(listbox).Not.ToBeVisibleAsync();
        await Assertions.Expect(page.Locator(Scope + "[data-rhx-radial-display]")).ToContainTextAsync("Cherry");
        await Assertions.Expect(page.Locator(Scope + "[data-rhx-radial-value]")).ToHaveValueAsync("cherry");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Escape_closes_pie_and_restores_focus(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var trigger = page.Locator(Scope + "button.rhx-radial-select__trigger");
        var pie = page.Locator(Scope + ".rhx-radial-select__pie");

        await trigger.ClickAsync();
        await Assertions.Expect(pie).ToBeVisibleAsync();

        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(pie).Not.ToBeVisibleAsync();
        await Assertions.Expect(trigger).ToBeFocusedAsync();
    }
}
