using ConsoleApp.Render.Core.Interfaces;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using WebMarkupMin.Core;
using WebMarkupMin.Core.Loggers;

namespace ConsoleApp.Render.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRenderer(this IServiceCollection services)
        {
            services.AddLogging();

            services.AddSingleton((IServiceProvider builder) => new HtmlRenderer(builder, builder.GetRequiredService<ILoggerFactory>()));
            services.AddSingleton<IHtmlRender, RazorHtmlRenderWrapper>();
            services.AddSingleton<IMarkupMinifier>(
                new HtmlMinifier(
                    new HtmlMinificationSettings(),
                    new KristensenCssMinifier(),
                    new CrockfordJsMinifier(),
                    new NullLogger()));

            return services;
        }
    }
}
