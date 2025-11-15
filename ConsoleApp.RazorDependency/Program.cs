using ConsoleApp.Render;
using ConsoleApp.Render.Core;
using ConsoleApp.Render.Views;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp.RazorDependency;

internal class Program
{
    static async Task Main(string[] args)
    {
        IServiceCollection services = new ServiceCollection();
        services.AddRazor();

        var render = services.BuildServiceProvider().GetService<IHtmlRender>();

        var dictionary = new Dictionary<string, object>
            {
                { nameof(RenderMessage.Message), "Hello from the External Lib Render Message component!" },
                { nameof(RenderMessage.MessageItems), new[] { "data1", "data2", "data3" } },
                { nameof(RenderMessage.InnerMessageViewModel), new InnerRenderMsgViewModel { MsgId = 9999, MsgName = "Msg Name" } },
            };

        var html = await render.RenderAsync<RenderMessage>(dictionary);

        Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
        Console.WriteLine(html);

        Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
        var dictionary2 = new Dictionary<string, object>
            {
                { nameof(RenderMessage.Message), "Message 2" },
                { nameof(RenderMessage.MessageItems), new[] { "data1", "data2", "data3" } },
                { nameof(RenderMessage.InnerMessageViewModel), new InnerRenderMsgViewModel { MsgId = 9999, MsgName = "Msg Name" } },
            };

        var html2Stream = await render.RenderStreamAsync<RenderMessage>(dictionary2);
        
        var s = await new StreamReader(html2Stream).ReadToEndAsync();

        Console.WriteLine(s);
    }
}
