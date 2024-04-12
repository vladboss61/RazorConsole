using ConsoleApp.Render;
using ConsoleApp.Render.Views;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp.RazorDependency;

internal class Program
{
    static async Task Main(string[] args)
    {
        IServiceCollection services = new ServiceCollection();
        services.AddRazor();

        var render = services.BuildServiceProvider().GetService<HtmlRenderer>();

        var html = await render.Dispatcher.InvokeAsync(async () =>
        {
            var dictionary = new Dictionary<string, object>
            {
                { nameof(RenderMessage.Message), "Hello from the External Lib Render Message component!" },
                { nameof(RenderMessage.MessageItems), new[] { "data1", "data2", "data3" } },
                { nameof(RenderMessage.InnerMessageViewModel), new InnerRenderMsgViewModel { MsgId = 9999, MsgName = "Msg Name" } },
            };

            var parameters = ParameterView.FromDictionary(dictionary);
            var output = await render.RenderComponentAsync<RenderMessage>(parameters);

            return output.ToHtmlString();
        });

        Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
        Console.WriteLine(html);

        var html2 = await render.Dispatcher.InvokeAsync(async () =>
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            var dictionary = new Dictionary<string, object>
            {
                { nameof(RenderMessage.Message), "Message 2" },
                { nameof(RenderMessage.MessageItems), new[] { "data1", "data2", "data3" } },
                { nameof(RenderMessage.InnerMessageViewModel), new InnerRenderMsgViewModel { MsgId = 9999, MsgName = "Msg Name" } },
            };

            var parameters = ParameterView.FromDictionary(dictionary);
            var output = await render.RenderComponentAsync<RenderMessage>(parameters);

            return output.ToHtmlString();
        });

        Console.WriteLine(html2);
    }
}
