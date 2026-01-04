using MathEditor.Pages;

namespace MathEditor.Models;

public class MathField : Field
{
    public string Latex { get; set; } = "";

    public MathField(double x, double y) : base(x, y)
    {
        ResizeAxis = ResizableAxis.Horizontal;
        Width = Canvas.BaseCellSize * 8;
        Height = Canvas.BaseCellSize * 2;
        MinWidth = Canvas.BaseCellSize * 8;
        MinHeight = Canvas.BaseCellSize * 2;
    }
}
