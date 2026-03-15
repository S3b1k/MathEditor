using MathEditor.Pages;

namespace MathEditor.Models;

public class ImageField : Field
{
    public string ImageSource { get; set; } = "";

    
    public ImageField(double x, double y) : base(x, y)
    {
        ResizeAxis = ResizableAxis.Diagonal;
        MinWidth = Canvas.BaseCellSize * 2;
        MinHeight = Canvas.BaseCellSize * 2;
        Width = Canvas.BaseCellSize * 12;
        Height = Canvas.BaseCellSize * 12;
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