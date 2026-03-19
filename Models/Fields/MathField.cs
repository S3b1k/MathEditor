using MathEditor.Pages;

namespace MathEditor.Models;

public class MathField : Field
{
    public string Latex { get; set; } = "";
    

    public MathField(double x, double y) : base(x, y)
    {
        MinWidth = Canvas.BaseCellSize * 4;
        MinHeight = Canvas.BaseCellSize * 2;
        Width = Canvas.BaseCellSize * 6;
        Height = Canvas.BaseCellSize * 3;
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
