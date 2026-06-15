using System.Text.RegularExpressions;
using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

// rhx-signalr now emits htmx SSE-extension attributes; the demo streams real Server-Sent
// Events (clock + chat broadcast) from minimal-API endpoints. No SignalR, no custom JS.
public sealed class SignalRTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/SignalR";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clock_connector_wires_the_sse_extension(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var connector = page.Locator("#panel-clock-preview div.rhx-signalr");
        await Assertions.Expect(connector).ToHaveCountAsync(1);
        await Assertions.Expect(connector).ToHaveAttributeAsync("hx-ext", "sse");
        await Assertions.Expect(connector).ToHaveAttributeAsync("sse-connect", "/demo-sse/clock");
        await Assertions.Expect(connector).ToHaveAttributeAsync("sse-swap", "ServerClock");
        await Assertions.Expect(connector).ToHaveAttributeAsync("aria-live", "polite");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clock_connector_receives_server_time_update(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var connector = page.Locator("#panel-clock-preview div.rhx-signalr");
        // The SSE endpoint pushes a <span> with HH:mm:ss.ff every second.
        await Assertions.Expect(connector)
            .ToContainTextAsync(new Regex(@"\d{2}:\d{2}:\d{2}"), new() { Timeout = 15_000 });
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Chat_connector_targets_messages_container(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var connector = page.Locator("#panel-chat-preview #chat-connector");
        await Assertions.Expect(connector).ToHaveAttributeAsync("sse-connect", "/demo-sse/chat");
        await Assertions.Expect(connector).ToHaveAttributeAsync("sse-swap", "ReceiveMessage");
        await Assertions.Expect(connector).ToHaveAttributeAsync("hx-target", "#chat-messages");
        await Assertions.Expect(connector).ToHaveAttributeAsync("hx-swap", "beforeend");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Sending_chat_message_broadcasts_over_sse(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var messages = page.Locator("#panel-chat-preview #chat-messages");
        await page.Locator("#panel-chat-preview input[name='message']").FillAsync("Hello SSE");
        await page.Locator("#panel-chat-preview button[type='submit']").ClickAsync();

        // The POST broadcasts the message; it arrives back over the SSE connection.
        await Assertions.Expect(messages).ToContainTextAsync("Hello SSE", new() { Timeout = 15_000 });
    }
}
