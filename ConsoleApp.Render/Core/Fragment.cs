using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace ConsoleApp.Render.Core;

public static class Fragment
{
    public static RenderFragment ToFragment<Component>(Dictionary<string, object> parameters = null) where Component : IComponent
    {
        return builder =>
        {
            builder.OpenComponent<Component>(0);
            if (parameters != null)
            {
                int seq = 1;
                foreach (var kvp in parameters)
                {
                    builder.AddAttribute(seq++, kvp.Key, kvp.Value);
                }
            }

            builder.CloseComponent();
        };
    }
}
