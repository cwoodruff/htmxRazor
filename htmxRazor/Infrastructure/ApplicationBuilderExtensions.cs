using System;
using htmxRazor.Components.Forms.Calendar;
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

        // Dynamic calendar grid endpoint (must precede the static-file provider for /_rhx).
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.Equals("/_rhx/calendar", StringComparison.OrdinalIgnoreCase)
                && HttpMethods.IsGet(context.Request.Method))
            {
                var html = CalendarEndpoint.Render(context.Request.Query, DateOnly.FromDateTime(DateTime.Today));
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync(html);
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
