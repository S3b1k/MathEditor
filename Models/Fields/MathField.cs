using MathEditor.Pages;

namespace MathEditor.Models;

public class MathField : Field
{
    public string Latex { get; set; } = "";
    public override string Value => Latex;
    

    
    public MathField(double x, double y) : base(x, y)
    {
        MinWidth = Canvas.BaseCellSize * 4;
        MinHeight = Canvas.BaseCellSize * 2;
        Width = Canvas.BaseCellSize * 6;
        Height = MinHeight;
    }


    public override Field Clone() => new MathField(PosX, PosY)
    {
        Width = Width,
        Height = Height,
        Latex = Latex
    };

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
