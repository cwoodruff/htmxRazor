/**
 * htmxRazor SignalR Connector
 * Establishes SignalR hub connections and swaps received HTML fragments
 * into the DOM using htmx's programmatic swap API.
 *
 * Requires @microsoft/signalr client library to be loaded (window.signalR).
 */
(function () {
  "use strict";

  var TRANSPORT_MAP = {
    "websockets": 1,     // signalR.HttpTransportType.WebSockets
    "sse": 2,            // signalR.HttpTransportType.ServerSentEvents
    "longpolling": 4     // signalR.HttpTransportType.LongPolling
  };

  function initSignalR(root) {
    var containers = root.querySelectorAll("[data-rhx-signalr]");
    containers.forEach(function (container) {
      if (container._rhxSignalRInit) return;
      container._rhxSignalRInit = true;

      if (typeof signalR === "undefined") {
        console.warn("[rhx-signalr] @microsoft/signalr client library not found. Load it before rhx-signalr.js.");
        return;
      }

      var hubUrl = container.getAttribute("data-rhx-hub-url");
      if (!hubUrl) {
        console.warn("[rhx-signalr] Missing data-rhx-hub-url attribute.");
        return;
      }

      var method = container.getAttribute("data-rhx-method");
      var swapMode = container.getAttribute("data-rhx-swap") || "innerHTML";
      var targetSel = container.getAttribute("data-rhx-target");
      var reconnect = container.getAttribute("data-rhx-reconnect") !== "false";
      var transport = container.getAttribute("data-rhx-transport");
      var groups = container.getAttribute("data-rhx-groups");
      var showState = container.hasAttribute("data-rhx-connection-state");

      // Build connection
      var builder = new signalR.HubConnectionBuilder().withUrl(hubUrl);

      if (transport && TRANSPORT_MAP[transport]) {
        builder = new signalR.HubConnectionBuilder()
          .withUrl(hubUrl, { transport: TRANSPORT_MAP[transport] });
      }

      if (reconnect) {
        builder = builder.withAutomaticReconnect();
      }

      var connection = builder.build();

      // Connection state management
      function setState(state) {
        container.classList.remove("rhx-signalr--connected", "rhx-signalr--disconnected", "rhx-signalr--reconnecting");
        container.classList.add("rhx-signalr--" + state);

        if (showState) {
          container.setAttribute("data-rhx-state", state);
        }
      }

      // Listen for the specified method
      if (method) {
        connection.on(method, function (htmlContent) {
          var target = targetSel ? document.querySelector(targetSel) : container;
          if (!target) return;

          // Swap HTML content into the target element
          switch (swapMode) {
            case "outerHTML":
              target.outerHTML = htmlContent;
              break;
            case "beforeend":
              target.insertAdjacentHTML("beforeend", htmlContent);
              break;
            case "afterbegin":
              target.insertAdjacentHTML("afterbegin", htmlContent);
              break;
            case "beforebegin":
              target.insertAdjacentHTML("beforebegin", htmlContent);
              break;
            case "afterend":
              target.insertAdjacentHTML("afterend", htmlContent);
              break;
            default: // innerHTML
              target.innerHTML = htmlContent;
              break;
          }

          // Process any htmx attributes in the swapped content
          if (typeof htmx !== "undefined") {
            htmx.process(target);
          }

          container.dispatchEvent(new CustomEvent("rhx:signalr:message", {
            bubbles: true,
            detail: { method: method, content: htmlContent }
          }));
        });
      }

      // Connection lifecycle events
      connection.onreconnecting(function () {
        setState("reconnecting");
        container.dispatchEvent(new CustomEvent("rhx:signalr:reconnecting", { bubbles: true }));
      });

      connection.onreconnected(function () {
        setState("connected");
        joinGroups();
        container.dispatchEvent(new CustomEvent("rhx:signalr:reconnected", { bubbles: true }));
      });

      connection.onclose(function () {
        setState("disconnected");
        container.dispatchEvent(new CustomEvent("rhx:signalr:disconnected", { bubbles: true }));
      });

      // Join groups
      function joinGroups() {
        if (!groups) return;
        groups.split(",").forEach(function (group) {
          var g = group.trim();
          if (g) {
            connection.invoke("JoinGroup", g).catch(function (err) {
              console.warn("[rhx-signalr] Failed to join group '" + g + "':", err);
            });
          }
        });
      }

      // Start connection
      connection.start()
        .then(function () {
          setState("connected");
          joinGroups();
          container.dispatchEvent(new CustomEvent("rhx:signalr:connected", { bubbles: true }));
        })
        .catch(function (err) {
          setState("disconnected");
          console.error("[rhx-signalr] Connection failed:", err);
        });

      // Expose connection for programmatic use
      container._rhxSignalRConnection = connection;
    });
  }

  if (window.RHX) {
    window.RHX.register("signalr", initSignalR);
  }
})();
