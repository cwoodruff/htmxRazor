using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class SignalRTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/SignalR";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clock_connector_renders_with_hub_attributes(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var connector = page.Locator("#panel-clock-preview div.rhx-signalr[data-rhx-signalr]");
        await Assertions.Expect(connector).ToHaveCountAsync(1);
        await Assertions.Expect(connector).ToHaveAttributeAsync("data-rhx-hub-url", "/demoHub");
        await Assertions.Expect(connector).ToHaveAttributeAsync("data-rhx-method", "ServerClock");
        await Assertions.Expect(connector).ToHaveAttributeAsync("aria-live", "polite");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clock_connector_exposes_connection_state_indicator(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var connector = page.Locator("#panel-clock-preview div.rhx-signalr");
        await Assertions.Expect(connector).ToHaveAttributeAsync("data-rhx-connection-state", "");
        await Assertions.Expect(connector).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("rhx-signalr--has-state"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clock_connector_receives_server_time_update(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var connector = page.Locator("#panel-clock-preview div.rhx-signalr");
        // Hub pushes a <span> with HH:mm:ss.ff every 2s; wait for an update to replace
        // the initial spinner.
        await Assertions.Expect(connector)
            .ToContainTextAsync(new System.Text.RegularExpressions.Regex(@"\d{2}:\d{2}:\d{2}"),
                new() { Timeout = 15_000 });
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Chat_connector_targets_messages_container(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var connector = page.Locator("#panel-chat-preview #chat-connector");
        await Assertions.Expect(connector).ToHaveAttributeAsync("data-rhx-target", "#chat-messages");
        await Assertions.Expect(connector).ToHaveAttributeAsync("data-rhx-swap", "beforeend");
        await Assertions.Expect(connector).ToHaveAttributeAsync("data-rhx-method", "ReceiveMessage");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Sending_chat_message_appends_to_message_list(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var messages = page.Locator("#panel-chat-preview #chat-messages");

        // The hub takes a moment to connect; retry briefly if the first send is
        // dropped because the connection isn't ready yet.
        var messageInput = page.Locator("#panel-chat-preview #chat-message");
        await messageInput.FillAsync("Hello world");

        var sendBtn = page.Locator("#panel-chat-preview #chat-send-btn");
        await sendBtn.ClickAsync();

        await Assertions.Expect(messages).ToContainTextAsync("Hello world", new() { Timeout = 15_000 });
    }
}
