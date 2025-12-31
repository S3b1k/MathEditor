namespace MathEditor.Models;

public class MathField(double x, double y) : Field(x, y)
{
    public string Text { get; set; } = "New text";
    public bool TextSelected;
}
