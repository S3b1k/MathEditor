using MathEditor.Pages;

namespace MathEditor.Models;

public abstract class Field(double x, double y)
{

    public Guid Id { get; protected init; } = Guid.NewGuid();
    
    #region Transform
    public double PosX { get; set; } = x;
    public double PosY { get; set; } = y;
    public double Width { get; set; } = Canvas.BaseCellSize * 6;
    public double Height { get; set; } = Canvas.BaseCellSize;
    public double MinWidth = Canvas.BaseCellSize;
    public double MinHeight = Canvas.BaseCellSize;
    #endregion

    public bool IsSelected { get; set; }
    public event Action? OnFieldDeselected;
    public event Action? OnFieldDeleted;

    public bool IsEditing { get; set; }
    
    #region Dragging
    public bool IsDragging { get; set; }
    public double DragOffsetX { get; set; }
    public double DragOffsetY { get; set; }
    
    public double StartPosX;
    public double StartPosY;
    #endregion

    #region Resizing
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
    
    
    public ResizableAxis ResizeAxis { get; set; } = ResizableAxis.All;
    public ResizeDirection ResizeDir { get; set; }
    public bool IsResizing { get; set; }
    public double ResizeStartWidth { get; set; }
    public double ResizeStartHeight { get; set; }
    public double ResizeStartMouseX { get; set; }
    public double ResizeStartMouseY { get; set; }
    public double ResizeStartPosX { get; set; }
    public double ResizeStartPosY { get; set; }
    
    public double StartWidth;
    public double StartHeight;
    #endregion

    public event Action? OnValueUpdated;
    

    public void NotifyFieldDeselected() => OnFieldDeselected?.Invoke();
    public void NotifyFieldDeleted() => OnFieldDeleted?.Invoke();
    public void NotifyValueUpdated() => OnValueUpdated?.Invoke();

    public abstract FieldSaveData ToSaveData();
    public abstract Field Copy();
}