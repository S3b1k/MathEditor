namespace MathEditor.Models;

public class TextField(double x, double y) : Field(x, y)
{
    public string Text { get; set; } = "New text";
    
    
    public override FieldSaveData ToSaveData() => new()
    {
        Type = "text",
        PosX = PosX,
        PosY = PosY,
        Width = Width,
        Height = Height,
        Content = Text
    };
}
