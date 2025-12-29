using MathEditor.Pages;

namespace MathEditor.Models;

public abstract class Field
{
    public Guid Id { get; } = Guid.NewGuid();
    
    public double PosX { get; set; }
    public double PosY { get; set; }
    
    public double Width { get; set; } = Canvas.BaseCellSize * 6;
    public double Height { get; set; } = Canvas.BaseCellSize;

    public bool IsSelected { get; set; }
    
    // Dragging
    public bool IsDragging { get; set; }
    public double DragOffsetX { get; set; }
    public double DragOffsetY { get; set; }

    // Resizing
    public bool IsResizing { get; set; }
    public double ResizeStartWidth { get; set; }
    public double ResizeStartHeight { get; set; }
    public double ResizeStartX { get; set; }
    public double ResizeStartY { get; set; }
}