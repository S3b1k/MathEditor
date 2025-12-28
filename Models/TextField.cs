using Microsoft.AspNetCore.Components;

namespace MathEditor.Models;

public class TextField : Field
{
    public string Text { get; set; } = "New text";

    // TODO - Auto resizing to fit text
    
    public override RenderFragment Render(double zoom, double panX, double panY)
    {
        return builder =>
        {
            var screenX = PosX * zoom + panX;
            var screenY = PosY * zoom + panY;

            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "text-field");
            builder.AddAttribute(2, "style",
                $"position:absolute;" +
                $"left:{screenX}px;" +
                $"top:{screenY}px;" +
                $"transform:scale({zoom});" +
                $"transform-origin: top left;" +
                $"width:{Width}px;" +
                $"height:{Height}px;"
            );

            builder.OpenElement(3, "input");
            builder.AddAttribute(4, "value", Text);
            builder.AddAttribute(5, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(this, e =>
            {
                Text = e.Value?.ToString() ?? "";
            }));
            builder.CloseElement();

            builder.CloseElement();
        };
    }
}
