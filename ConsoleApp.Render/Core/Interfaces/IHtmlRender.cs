using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ConsoleApp.Render.Core.Interfaces;

public interface IHtmlRender
{
    /// <summary>
    ///     Renderes HTML and outputs string.
    /// </summary>
    /// <typeparam name="TComponent">Component for rendering.</typeparam>
    /// <param name="parameters">Component parameters.</param>
    /// <returns>Rendered HTML.</returns>
    public Task<string> RenderAsync<TComponent>(Dictionary<string, object> parameters) 
        where TComponent : IComponent;

    /// <summary>
    ///     Renderes HTML and outputs steam.
    /// </summary>
    /// <typeparam name="TComponent">Component for rendering.</typeparam>
    /// <param name="parameters">Component parameters.</param>
    /// <returns>Rendered HTML stream.</returns>
    public Task<Stream> RenderStreamAsync<TComponent>(Dictionary<string, object> parameters)
        where TComponent : IComponent;


    /// <summary>
    ///     Renderes HTML and outputs steam.
    /// </summary>
    /// <typeparam name="TComponent">Component for rendering.</typeparam>
    /// <param name="parameters">Component parameters.</param>
    /// <returns>Rendered HTML stream.</returns>
    public Task RenderStreamAsync<TComponent>(Stream stream, Dictionary<string, object> parameters)
        where TComponent : IComponent;
}