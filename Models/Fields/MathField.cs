using MathEditor.Pages;

namespace MathEditor.Models;

public class MathField : Field
{
    public string Latex { get; set; } = "";

    public MathField(double x, double y) : base(x, y)
    {
        ResizeAxis = ResizableAxis.Horizontal;
        MinWidth = Canvas.BaseCellSize * 8;
        MinHeight = Canvas.BaseCellSize * 2;
        Width = MinWidth;
        Height = MinHeight;
    }


    public override FieldSaveData ToSaveData() => new()
    {
        Type = "math",
        PosX = PosX,
        PosY = PosY,
        Width = Width,
        Height = Height,
        Content = Latex
    };
}
