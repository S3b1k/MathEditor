using MathEditor.Pages;

namespace MathEditor.Models;

public class ImageField : Field
{
    public string ImageSource { get; set; } = "";
    public override string Value => ImageSource;

    
    public ImageField(double x, double y) : base(x, y)
    {
        ResizeAxis = ResizableAxis.Diagonal;
        MinWidth = Canvas.BaseCellSize * 2;
        MinHeight = Canvas.BaseCellSize * 2;
        Width = Canvas.BaseCellSize * 12;
        Height = Canvas.BaseCellSize * 12;
    }


    public override Field Clone() => new ImageField(PosX, PosY)
    {
        Width = Width,
        Height = Height,
        ImageSource = ImageSource
    };
    
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