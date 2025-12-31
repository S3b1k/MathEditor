using MathEditor.Pages;

namespace MathEditor.Models;

public abstract class Field(double x, double y)
{
    public Guid Id { get; } = Guid.NewGuid();
    
    // Transform
    public double PosX { get; set; } = x;
    public double PosY { get; set; } = y;

    public double Width { get; set; } = Canvas.BaseCellSize * 6;
    public double Height { get; set; } = Canvas.BaseCellSize;

    public bool IsSelected { get; set; }
    public event Action? OnFieldDeselected;
    
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


    public void NotifyFieldDeselected() => OnFieldDeselected?.Invoke();
}