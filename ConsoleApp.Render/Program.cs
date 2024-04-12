using ConsoleApp.Render.Views;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleApp.Render;

internal class Program
{
    static async Task Main(string[] args)
    {
        IServiceCollection services = new ServiceCollection();
        services.AddLogging();

        IServiceProvider serviceProvider = services.BuildServiceProvider();
        ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

        await using var htmlRenderer = new HtmlRenderer(serviceProvider, loggerFactory);

        var html = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            var dictionary = new Dictionary<string, object>
            {
                { nameof(RenderMessage.Message), "Hello from the Render Message component!" },
                { nameof(RenderMessage.MessageItems), new[] { "data1", "data2", "data3" } },
                { nameof(RenderMessage.InnerMessageViewModel), new InnerRenderMsgViewModel { MsgId = 9999, MsgName = "Msg Name" } },
            };

            var parameters = ParameterView.FromDictionary(dictionary);
            var output = await htmlRenderer.RenderComponentAsync<RenderMessage>(parameters);
            return output.ToHtmlString();
        });

        Console.WriteLine(html);
    }
}
