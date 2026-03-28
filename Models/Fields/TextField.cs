namespace MathEditor.Models;

public class TextField(double x, double y) : Field(x, y)
{
    public string Text { get; set; } = "New text";
    public override string Value => Text;
    
    
    public override Field Clone() => new TextField(PosX, PosY)
    {
        Width = Width,
        Height = Height,
        Text = Text
    };
    
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
