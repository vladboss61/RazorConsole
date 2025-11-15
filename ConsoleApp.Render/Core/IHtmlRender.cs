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

public sealed class HtmlRender(HtmlRenderer htmlRenderer, IMarkupMinifier markupMinifier) : IHtmlRender
{
    private readonly HtmlRenderer _htmlRenderer = htmlRenderer;
    private readonly IMarkupMinifier _markupMinifier = markupMinifier;

    public async Task<string> RenderAsync<TComponent>(Dictionary<string, object> parameters)
        where TComponent : IComponent
    {
        string renderedHtml = await _htmlRenderer.Dispatcher.InvokeAsync(async () => 
            (await _htmlRenderer.RenderComponentAsync<TComponent>(
                ParameterView.FromDictionary(parameters))
            ).ToHtmlString());

        return _markupMinifier.Minify(renderedHtml).MinifiedContent;
    }

    public async Task<Stream> RenderStreamAsync<TComponent>(Dictionary<string, object> parameters)
        where TComponent : IComponent
    {
        var stringBuilder = new StringBuilder();
        using var renderedStringWriter = new StringWriter(stringBuilder);

        await _htmlRenderer.Dispatcher.InvokeAsync(async () =>
            (await _htmlRenderer.RenderComponentAsync<TComponent>(
                ParameterView.FromDictionary(parameters))
            ).WriteHtmlTo(renderedStringWriter));

        string minifiedHtml = _markupMinifier.Minify(stringBuilder.ToString()).MinifiedContent;

        var stream = new MemoryStream();
        using (var streamWriter = new StreamWriter(stream, leaveOpen: true))
        {
            await streamWriter.WriteAsync(minifiedHtml);
            await streamWriter.FlushAsync();
        }

        stream.Position = 0;

        return stream;
    }
}