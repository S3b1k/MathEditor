using Microsoft.AspNetCore.Components;

namespace MathEditor.Models;

public class TextField : Field
{
    public string Text { get; set; } = "New text";

    // TODO - Auto resizing to fit text
}
