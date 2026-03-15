namespace MathEditor.Models;

public class ImageField : Field
{
    public string ImageSource { get; set; } = "";

    
    public ImageField(double x, double y) : base(x, y)
    {
        ResizeAxis = ResizableAxis.Diagonal;
    }
    

    public override FieldSaveData ToSaveData() => new()
    {
        Type = "image",
        PosX = PosX,
        PosY = PosY,
        Width = Width,
        Height = Height,
        Content = ImageSource
    };
}