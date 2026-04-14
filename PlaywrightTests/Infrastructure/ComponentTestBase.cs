using Microsoft.Playwright;

namespace PlaywrightTests.Infrastructure;

/// <summary>
/// Shared base class for component interaction tests.
/// Each derived test exposes a <see cref="Browsers"/> data source so every
/// [Theory] runs headless on Chromium, Firefox, and WebKit.
/// </summary>
[Collection(DemoAppCollection.Name)]
public abstract class ComponentTestBase : IAsyncLifetime
{
    private readonly List<IBrowserContext> _contexts = new();
    private IPlaywright? _playwright;
    private IBrowser? _chromium;
    private IBrowser? _firefox;
    private IBrowser? _webkit;

    protected ComponentTestBase(DemoAppFactory app)
    {
        App = app;
    }

    protected DemoAppFactory App { get; }

    public static IEnumerable<object[]> Browsers()
    {
        yield return new object[] { "chromium" };
        yield return new object[] { "firefox" };
        yield return new object[] { "webkit" };
    }

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
    }

    public async Task DisposeAsync()
    {
        foreach (var ctx in _contexts)
        {
            await ctx.CloseAsync();
        }

        if (_chromium is not null) await _chromium.CloseAsync();
        if (_firefox is not null) await _firefox.CloseAsync();
        if (_webkit is not null) await _webkit.CloseAsync();

        _playwright?.Dispose();
    }

    /// <summary>
    /// Opens a new page in the requested browser pointing at <paramref name="path"/>.
    /// The page's context is tracked and closed on test teardown.
    /// </summary>
    protected async Task<IPage> OpenAsync(string browserName, string path)
    {
        var browser = await GetBrowserAsync(browserName);
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new ViewportSize { Width = 1280, Height = 900 },
        });
        _contexts.Add(context);

        var page = await context.NewPageAsync();
        var url = App.ServerAddress + (path.StartsWith('/') ? path : "/" + path);
        await page.GotoAsync(url, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded,
            Timeout = 60_000,
        });
        return page;
    }

    private async Task<IBrowser> GetBrowserAsync(string browserName)
    {
        var options = new BrowserTypeLaunchOptions { Headless = true };
        return browserName switch
        {
            "chromium" => _chromium ??= await _playwright!.Chromium.LaunchAsync(options),
            "firefox" => _firefox ??= await _playwright!.Firefox.LaunchAsync(options),
            "webkit" => _webkit ??= await _playwright!.Webkit.LaunchAsync(options),
            _ => throw new ArgumentException($"Unknown browser: {browserName}", nameof(browserName)),
        };
    }
}
