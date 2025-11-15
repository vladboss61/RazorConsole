using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WebMarkupMin.Core;

namespace ConsoleApp.Render.Core;

public interface IHtmlRender
{
    public Task<string> RenderAsync<TComponent>(Dictionary<string, object> parameters) where TComponent : IComponent;

    public Task<Stream> RenderStreamAsync<TComponent>(Dictionary<string, object> parameters) where TComponent : IComponent;
}

public sealed class HtmlRender : IHtmlRender
{
    private readonly HtmlRenderer _htmlRenderer;
    private readonly IMarkupMinifier _markupMinifier;

    public HtmlRender(HtmlRenderer htmlRenderer, IMarkupMinifier markupMinifier)
    {
        _htmlRenderer = htmlRenderer;
        _markupMinifier = markupMinifier;
    }

    public async Task<string> RenderAsync<TComponent>(Dictionary<string, object> parameters)
        where TComponent : IComponent
    {
        var htmlStr = await _htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await _htmlRenderer.RenderComponentAsync<TComponent>(ParameterView.FromDictionary(parameters));
            return output.ToHtmlString();
        });

        return _markupMinifier.Minify(htmlStr).MinifiedContent;
    }

    public async Task<Stream> RenderStreamAsync<TComponent>(Dictionary<string, object> parameters)
        where TComponent : IComponent
    {
        var sb = new StringBuilder();
        using var stringWriter = new StringWriter(sb);

        await _htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await _htmlRenderer.RenderComponentAsync<TComponent>(
                ParameterView.FromDictionary(parameters)
            );

            output.WriteHtmlTo(stringWriter);
        });

        var minified = _markupMinifier.Minify(sb.ToString()).MinifiedContent;

        var stream = new MemoryStream();
        using (var streamWriter = new StreamWriter(stream, leaveOpen: true))
        {
            await streamWriter.WriteAsync(minified);
            await streamWriter.FlushAsync();
        }

        stream.Position = 0;

        return stream;
    }
}