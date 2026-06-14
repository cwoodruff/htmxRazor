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
