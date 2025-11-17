using ConsoleApp.Render.Core;
using ConsoleApp.Render.Core.Interfaces;
using ConsoleApp.Render.Models;
using ConsoleApp.Render.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ConsoleApp.RazorDependency;

internal sealed class Program
{
    public static async Task Main(string[] args)
    {
        IServiceCollection services = new ServiceCollection();
        services.AddRazorHtmlRenderer();

        var simpleBodyParams = new Dictionary<string, object>()
            {
                { nameof(SimpleBodyComponent.Info), "Hey simple" },
                { nameof(SimpleBodyComponent.InnerBody), Fragment.ToFragment<InnerSimpleBodyComponent>() }
            };

        var dictionary = new Dictionary<string, object>
            {
                { nameof(IndexComponent.IsInnerApplied), false },
                { nameof(IndexComponent.Message), "Hello from the External Lib Render Message component!" },
                { nameof(IndexComponent.MessageItems), new[] { "data1", "data2", "data3" } },
                { nameof(IndexComponent.InnerMessageViewModel), new InnerRenderMsgViewModel { MsgId = 9999, MsgName = "Msg Name" } },
                { nameof(IndexComponent.Header), Fragment.ToFragment<HeaderComponent>() },
                { nameof(IndexComponent.Body), Fragment.ToFragment<SimpleBodyComponent>(simpleBodyParams) }
            };

        //================= String Render =================
        var render = services.BuildServiceProvider().GetService<IHtmlRender>();
        var html = await render.RenderAsync<IndexComponent>(dictionary);
        Console.WriteLine(html);
        //================= String Render =================

        //================= Stream 1 Render =================
        var ms = new MemoryStream();
        await render.RenderStreamAsync<IndexComponent>(ms, dictionary);
        
        StreamReader reader = new StreamReader(ms);

        Console.WriteLine("==================");
        Console.WriteLine(reader.ReadToEnd());
        Console.WriteLine("==================");
        //================= Stream 1 Render =================

        //================= Stream 2 Render =================
        var dictionary2 = new Dictionary<string, object>
            {
                { nameof(IndexComponent.Message), "Message 2" },
                { nameof(IndexComponent.MessageItems), new[] { "data1", "data2", "data3" } },
                { nameof(IndexComponent.InnerMessageViewModel), new InnerRenderMsgViewModel { MsgId = 9999, MsgName = "Msg Name" } },
            };

        using var html2Stream = await render.RenderStreamAsync<IndexComponent>(dictionary2);
        using var sr = new StreamReader(html2Stream);
        var s = await sr.ReadToEndAsync();
        Console.WriteLine(s);
        //================= Stream 2 Render =================
    }
}
