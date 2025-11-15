using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Collections.Generic;

namespace ConsoleApp.Render.Core;

public static class Fragment
{
    public static RenderFragment ToFragment<Component>(Dictionary<string, object> parameters = null) where Component : IComponent
    {
        return builder =>
        {
            builder.OpenComponent<Component>(0);
            if (parameters is not null)
            {
                int seq = 1;
                ApplyAttributes(builder, ref seq, parameters);
            }

            builder.CloseComponent();
        };
    }

    private static void ApplyAttributes(RenderTreeBuilder builder, ref int seq, Dictionary<string, object> parameters)
    {
        foreach (var kvp in parameters)
        {
            builder.AddAttribute(seq++, kvp.Key, kvp.Value);
        }
    }
}
