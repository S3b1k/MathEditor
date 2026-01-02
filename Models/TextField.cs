namespace MathEditor.Models;

public class TextField(double x, double y) : Field(x, y)
{
    public string Text { get; set; } = "New text";
}
