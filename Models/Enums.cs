namespace MathEditor.Models;

public enum Enums
{
    Idle,
    Pan,
    CreateTextField,
    CreateMathField
}

public enum ResizeDirection
{
    TopLeft,
    Top,
    TopRight,
    Right,
    BottomRight,
    Bottom,
    BottomLeft,
    Left
}

[Flags]
public enum ResizableAxis
{
    None = 0,
    Horizontal = 1,
    Vertical = 2,
    All = Horizontal | Vertical
}