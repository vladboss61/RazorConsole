using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace ConsoleApp.Render;

internal static class ViewExtensions
{
    private const string ViewParameter = "ViewParameter";

    public static ParameterView ToParameterView<TView>(this TView parameter)
    {
        return ParameterView.FromDictionary(new Dictionary<string, object> { { ViewParameter, parameter } });
    }
}
