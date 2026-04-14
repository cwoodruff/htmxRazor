using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PlaywrightTests.Infrastructure;

/// <summary>
/// Boots htmxRazor.Demo via WebApplicationFactory with a real Kestrel server
/// bound to 127.0.0.1:0 so Playwright can drive it over HTTP.
/// </summary>
public sealed class DemoAppFactory : WebApplicationFactory<Program>
{
    private IHost? _kestrelHost;

    public string ServerAddress
    {
        get
        {
            EnsureStarted();
            return ClientOptions.BaseAddress.ToString().TrimEnd('/');
        }
    }

    private void EnsureStarted()
    {
        if (_kestrelHost is not null) return;
        using var _ = CreateDefaultClient();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        var testHost = builder.Build();

        builder.ConfigureWebHost(webHostBuilder =>
            webHostBuilder.UseKestrel().UseUrls("http://127.0.0.1:0"));

        _kestrelHost = builder.Build();
        _kestrelHost.Start();

        var server = _kestrelHost.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>()
            ?? throw new InvalidOperationException("Kestrel did not expose an addresses feature.");

        ClientOptions.BaseAddress = new Uri(addresses.Addresses.First());

        testHost.Start();
        return testHost;
    }

    protected override void Dispose(bool disposing)
    {
        _kestrelHost?.Dispose();
        base.Dispose(disposing);
    }
}
