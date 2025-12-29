namespace MathEditor.Models;

public class TextField : Field
{
    public string Text { get; set; } = "New text";
    public bool TextSelected;

    public TextField(double x, double y)
    {
        PosX = x;
        PosY = y;
    }
}
