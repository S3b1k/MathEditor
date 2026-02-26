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

    
    public override Field Copy()
    {
        return new TextField(PosX, PosY)
        {
            Id = Id,
            Width = Width,
            Height = Height,
            Text = Text,
        };
    }
}
