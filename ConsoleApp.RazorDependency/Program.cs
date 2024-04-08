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
                { "Message", "Hello from the Render Message component!" }
            };

            var parameters = ParameterView.FromDictionary(dictionary);
            var output = await render.RenderComponentAsync<RenderMessage>(parameters);

            return output.ToHtmlString();
        });

        Console.WriteLine(Thread.CurrentThread.ManagedThreadId);

        var html2 = await render.Dispatcher.InvokeAsync(async () =>
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            var dictionary = new Dictionary<string, object>
            {
                { "Message", "Message 2" }
            };

            var parameters = ParameterView.FromDictionary(dictionary);
            var output = await render.RenderComponentAsync<RenderMessage>(parameters);

            return output.ToHtmlString();
        });

        Console.WriteLine("Hello, World!");
    }
}
