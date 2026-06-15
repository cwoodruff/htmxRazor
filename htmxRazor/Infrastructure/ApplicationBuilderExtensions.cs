using System;
using htmxRazor.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace htmxRazor.Infrastructure;

/// <summary>
/// Extension methods for configuring the htmxRazor middleware pipeline.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds htmxRazor middleware to serve embedded CSS/JS assets from the /_rhx/ path prefix.
    /// </summary>
    public static IApplicationBuilder UsehtmxRazor(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetService<htmxRazorOptions>() ?? new htmxRazorOptions();

        var embeddedProvider = new EmbeddedFileProvider(
            typeof(ApplicationBuilderExtensions).Assembly,
            "htmxRazor.Assets");

        // Theme persistence (zero client JS): the <rhx-theme-toggle> checkbox POSTs here on
        // change; we flip the rhx-theme cookie. The dark tokens themselves are applied purely by
        // CSS (html:has(.rhx-theme-controller:checked)), and the toggle is rendered checked from
        // this cookie on the next request. (Must precede the static-file provider for /_rhx.)
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.Equals("/_rhx/theme", StringComparison.OrdinalIgnoreCase)
                && HttpMethods.IsPost(context.Request.Method))
            {
                var current = context.Request.Cookies["rhx-theme"];
                var updated = string.Equals(current, "dark", StringComparison.OrdinalIgnoreCase)
                    ? "light" : "dark";
                context.Response.Cookies.Append("rhx-theme", updated, new CookieOptions
                {
                    Path = "/",
                    SameSite = SameSiteMode.Lax,
                    HttpOnly = false,
                    MaxAge = TimeSpan.FromDays(365),
                });
                context.Response.StatusCode = StatusCodes.Status204NoContent;
                return;
            }
            await next();
        });

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = embeddedProvider,
            RequestPath = new PathString("/_rhx"),
            ContentTypeProvider = new FileExtensionContentTypeProvider(
                new Dictionary<string, string>
                {
                    { ".css", "text/css" },
                    { ".js", "application/javascript" },
                    { ".map", "application/json" }
                })
        });

        return app;
    }
}
