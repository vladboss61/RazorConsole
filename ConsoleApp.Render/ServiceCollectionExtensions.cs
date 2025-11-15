using ConsoleApp.Render.Core;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebMarkupMin.Core;
using WebMarkupMin.Core.Loggers;

namespace ConsoleApp.Render
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRazor(this IServiceCollection services)
        {
            services.AddLogging();
            services.AddSingleton((builder) => new HtmlRenderer(builder, builder.GetRequiredService<ILoggerFactory>()));

            services.AddSingleton<IMarkupMinifier>(
                new HtmlMinifier(
                    new HtmlMinificationSettings(),
                    new KristensenCssMinifier(),
                    new CrockfordJsMinifier(),
                    new NullLogger()));

            services.AddSingleton<IHtmlRender, HtmlRender>();

            return services;
        }
    }
}
