using ConsoleApp.Render.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WebMarkupMin.Core;

namespace ConsoleApp.Render.Core;

public sealed class RazorHtmlRenderWrapper(HtmlRenderer htmlRenderer, IMarkupMinifier markupMinifier) : IHtmlRender
{
    /// <summary>
    ///     HtmlRenderer renders string-oriented HTML.
    /// </summary>
    private readonly HtmlRenderer _htmlRenderer = htmlRenderer;

    /// <summary>
    ///     Minifier is used to shrink HTML.
    /// </summary>
    private readonly IMarkupMinifier _markupMinifier = markupMinifier;

    /// <inheritdoc/>
    public async Task<string> RenderAsync<TComponent>(Dictionary<string, object> parameters)
        where TComponent : IComponent
    {
        string renderedHtml = await _htmlRenderer.Dispatcher.InvokeAsync(async () =>
            (await _htmlRenderer.RenderComponentAsync<TComponent>(
                ParameterView.FromDictionary(parameters))
            ).ToHtmlString());

        return _markupMinifier.Minify(renderedHtml).MinifiedContent;
    }

    /// <inheritdoc/>
    public async Task<Stream> RenderStreamAsync<TComponent>(Dictionary<string, object> parameters)
        where TComponent : IComponent
    {
        var minifiedHtml = await GetMinifiedHtmlAsync<TComponent>(parameters);
        
        var stream = new MemoryStream();
        using (var streamWriter = new StreamWriter(stream, leaveOpen: true))
        {
            await streamWriter.WriteAsync(minifiedHtml);
            await streamWriter.FlushAsync();
        }

        stream.Seek(0, SeekOrigin.Begin);

        return stream;
    }

    public async Task RenderStreamAsync<TComponent>(Stream stream, Dictionary<string, object> parameters)
        where TComponent : IComponent
    {
        var minifiedHtml = await GetMinifiedHtmlAsync<TComponent>(parameters);

        using var streamWriter = new StreamWriter(stream, leaveOpen: true);

        await streamWriter.WriteAsync(minifiedHtml);
        await streamWriter.FlushAsync();

        stream.Seek(0, SeekOrigin.Begin);
    }

    private async Task<string> GetMinifiedHtmlAsync<TComponent>(Dictionary<string, object> parameters)
        where TComponent : IComponent
    {
        var stringBuilder = new StringBuilder();
        using var renderedStringWriter = new StringWriter(stringBuilder);

        await _htmlRenderer.Dispatcher.InvokeAsync(async () =>
            (await _htmlRenderer.RenderComponentAsync<TComponent>(
                ParameterView.FromDictionary(parameters))
            ).WriteHtmlTo(renderedStringWriter));

        string minifiedHtml = _markupMinifier.Minify(stringBuilder.ToString()).MinifiedContent;
        return minifiedHtml;
    }
}