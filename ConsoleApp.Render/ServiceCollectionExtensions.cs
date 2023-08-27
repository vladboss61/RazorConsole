using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.Render
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRazor(this IServiceCollection services)
        {
            services.AddLogging();
            services.AddSingleton((builder) => new HtmlRenderer(builder, builder.GetRequiredService<ILoggerFactory>()));
            return services;
        }
    }
}
