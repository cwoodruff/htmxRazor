using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace htmxRazor.Infrastructure;

/// <summary>
/// Extension methods for triggering toast notifications from server-side handlers.
/// Supports both event-based (JS creates the toast) and OOB-swap (server renders the toast) patterns.
/// </summary>
public static class HtmxToastExtensions
{
    /// <summary>
    /// Triggers a toast notification on the client by setting the HX-Trigger-After-Settle header.
    /// The toast container's JavaScript listens for the <c>rhx:toast</c> event and creates the toast dynamically.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="message">The toast message text.</param>
    /// <param name="variant">The color variant: neutral, brand, success, warning, danger. Default: success.</param>
    /// <param name="duration">Auto-dismiss duration in milliseconds. Null uses container default. Default: null.</param>
    public static void HxToast(this HttpResponse response, string message, string variant = "success", int? duration = null)
    {
        var detail = duration.HasValue
            ? (object)new { message, variant, duration = duration.Value }
            : new { message, variant };

        response.HxTriggerAfterSettle("rhx:toast", detail);
    }

    /// <summary>
    /// Returns a <see cref="ContentResult"/> containing a fully-rendered toast fragment with
    /// <c>hx-swap-oob="beforeend:#rhx-toasts"</c> for out-of-band insertion into the toast
    /// container. This is the zero-JavaScript way to push a toast from the server: htmx places
    /// the markup into the container, CSS handles auto-dismiss (the delayed animation) and the
    /// close control (a checkbox toggled via <c>:has(:checked)</c>).
    /// </summary>
    /// <param name="page">The Razor Page model.</param>
    /// <param name="message">The toast message text (HTML-encoded).</param>
    /// <param name="variant">The color variant: neutral, brand, success, warning, danger. Default: success.</param>
    /// <param name="duration">Auto-dismiss duration in milliseconds. 0 = no auto-dismiss. Default: 5000.</param>
    /// <param name="closable">Whether to show a close control. Default: true.</param>
    /// <param name="containerId">The id of the toast container. Default: rhx-toasts.</param>
    /// <returns>A <see cref="ContentResult"/> with the toast HTML fragment.</returns>
    public static ContentResult HxToastOob(
        this PageModel page,
        string message,
        string variant = "success",
        int duration = 5000,
        bool closable = true,
        string containerId = "rhx-toasts")
    {
        static string Enc(string? v) => System.Net.WebUtility.HtmlEncode(v ?? "") ?? "";

        var v = (variant ?? "neutral").ToLowerInvariant();
        var styleAttr = duration > 0
            ? $" style=\"animation: rhx-toast-auto-dismiss 300ms ease {duration}ms forwards\""
            : "";

        var dismiss = "";
        if (closable)
        {
            var id = "rhx-toast-dismiss-" + System.Guid.NewGuid().ToString("N");
            dismiss =
                $"<input type=\"checkbox\" id=\"{id}\" class=\"rhx-toast__dismiss rhx-sr-only\" aria-label=\"Close\" />" +
                $"<label class=\"rhx-toast__close\" for=\"{id}\" title=\"Close\" aria-hidden=\"true\">" +
                "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\">" +
                "<line x1=\"18\" y1=\"6\" x2=\"6\" y2=\"18\"></line><line x1=\"6\" y1=\"6\" x2=\"18\" y2=\"18\"></line>" +
                "</svg></label>";
        }

        // The OOB `beforeend:#selector` form appends the wrapper's INNER content and discards the
        // wrapper, so the toast lives inside an outer OOB carrier div (which htmx throws away).
        var html =
            $"<div hx-swap-oob=\"beforeend:#{Enc(containerId)}\">" +
            $"<div class=\"rhx-toast rhx-toast--{Enc(v)}\"{styleAttr}>" +
            $"<span class=\"rhx-toast__icon\" aria-hidden=\"true\">{ToastIcon(v)}</span>" +
            $"<div class=\"rhx-toast__content\">{Enc(message)}</div>" +
            dismiss +
            "</div>" +
            "</div>";

        return new ContentResult
        {
            Content = html,
            ContentType = "text/html",
            StatusCode = 200
        };
    }

    private static string ToastIcon(string variant) => variant switch
    {
        "success" => "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"20\" height=\"20\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path d=\"M22 11.08V12a10 10 0 1 1-5.93-9.14\"></path><polyline points=\"22 4 12 14.01 9 11.01\"></polyline></svg>",
        "warning" => "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"20\" height=\"20\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path d=\"M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z\"></path><line x1=\"12\" y1=\"9\" x2=\"12\" y2=\"13\"></line><line x1=\"12\" y1=\"17\" x2=\"12.01\" y2=\"17\"></line></svg>",
        "danger" => "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"20\" height=\"20\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><circle cx=\"12\" cy=\"12\" r=\"10\"></circle><line x1=\"12\" y1=\"8\" x2=\"12\" y2=\"12\"></line><line x1=\"12\" y1=\"16\" x2=\"12.01\" y2=\"16\"></line></svg>",
        _ => "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"20\" height=\"20\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><circle cx=\"12\" cy=\"12\" r=\"10\"></circle><line x1=\"12\" y1=\"16\" x2=\"12\" y2=\"12\"></line><line x1=\"12\" y1=\"8\" x2=\"12.01\" y2=\"8\"></line></svg>",
    };
}
