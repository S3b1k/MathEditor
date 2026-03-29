namespace MathEditor.Models;

public class FieldSaveData
{
    public required string Type { get; init; }
    public double PosX { get; init; }
    public double PosY { get; init; }
    public double Width { get; init; }
    public double Height { get; init; }
    
    public required string Content { get; init; }
}