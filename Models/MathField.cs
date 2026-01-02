using MathEditor.Pages;

namespace MathEditor.Models;

public class MathField : Field
{
    public string Latex { get; set; } = "";

    public MathField(double x, double y) : base(x, y)
    {
        IsResizable = false;
        Width = Canvas.BaseCellSize * 12;
        Height = Canvas.BaseCellSize * 2;
    }
}
