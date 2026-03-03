using MathEditor.Pages;

namespace MathEditor.Models;

public class ImageField : Field
{
    public byte[] ImageData { get; set; }

    public ImageField(double x, double y, byte[] imageData) : base(x, y)
    {
        ImageData = imageData;
        
        MinWidth = Canvas.BaseCellSize * 8;
        MinHeight = Canvas.BaseCellSize * 8;
        Width = MinWidth;
        Height = MinHeight;
    }


    public override FieldSaveData ToSaveData() => new()
    {
        Type = "image",
        PosX = PosX,
        PosY = PosY,
        Width = Width,
        Height = Height,
        Content = Convert.ToBase64String(ImageData)
    };
}
