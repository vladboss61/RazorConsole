using ConsoleApp.Render.Core;
using ConsoleApp.Render.Core.Interfaces;
using ConsoleApp.Render.Models;
using ConsoleApp.Render.Views;
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
        services.AddRenderer();

        var simpleBodyParams = new Dictionary<string, object>()
            {
                { nameof(SimpleBody.Info), "Hey simple" },
                { nameof(SimpleBody.InnerBody), Fragment.ToFragment<InnerSimpleBody>() }
            };

        var dictionary = new Dictionary<string, object>
            {
                { nameof(IndexComponent.Message), "Hello from the External Lib Render Message component!" },
                { nameof(IndexComponent.MessageItems), new[] { "data1", "data2", "data3" } },
                { nameof(IndexComponent.InnerMessageViewModel), new InnerRenderMsgViewModel { MsgId = 9999, MsgName = "Msg Name" } },
                { nameof(IndexComponent.Header), Fragment.ToFragment<Header>() },
                { nameof(IndexComponent.Body), Fragment.ToFragment<SimpleBody>(simpleBodyParams) }
            };

        var render = services.BuildServiceProvider().GetService<IHtmlRender>();
        var html = await render.RenderAsync<IndexComponent>(dictionary);

        Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
        Console.WriteLine(html);

        Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
        var dictionary2 = new Dictionary<string, object>
            {
                { nameof(IndexComponent.Message), "Message 2" },
                { nameof(IndexComponent.MessageItems), new[] { "data1", "data2", "data3" } },
                { nameof(IndexComponent.InnerMessageViewModel), new InnerRenderMsgViewModel { MsgId = 9999, MsgName = "Msg Name" } },
            };

        var html2Stream = await render.RenderStreamAsync<IndexComponent>(dictionary2);
        
        var s = await new StreamReader(html2Stream).ReadToEndAsync();

        Console.WriteLine(s);
    }
}
