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
    public double MinWidth = Canvas.BaseCellSize;
    public double MinHeight = Canvas.BaseCellSize;

    public bool IsSelected { get; set; }
    public event Action? OnFieldDeselected;

    public bool IsEditing { get; set; }
    
    // Dragging
    public bool IsDragging { get; set; }
    public double DragOffsetX { get; set; }
    public double DragOffsetY { get; set; }

    // Resizing
    public ResizableAxis ResizeAxis { get; set; } = ResizableAxis.All;
    public ResizeDirection ResizeDir { get; set; }
    public bool IsResizing { get; set; }
    public double ResizeStartWidth { get; set; }
    public double ResizeStartHeight { get; set; }
    public double ResizeStartMouseX { get; set; }
    public double ResizeStartMouseY { get; set; }
    public double ResizeStartPosX { get; set; }
    public double ResizeStartPosY { get; set; }


    public void NotifyFieldDeselected() => OnFieldDeselected?.Invoke();
}