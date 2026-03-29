using MathEditor.Pages;
using MathEditor.Services;

namespace MathEditor.Models;

public abstract class Field(double x, double y)
{
    public Guid Id { get; protected init; } = Guid.NewGuid();
    
    public abstract string? Value { get; }
    
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
    public event Action<string, bool[]>? OnKeyPress;
    public event Action? OnStopEditing;
    
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
        TopLeft, Top, TopRight,
        Right, BottomRight, Bottom,
        BottomLeft, Left
    }

    [Flags]
    public enum ResizableAxis
    {
        None = 0,
        Horizontal = 1, Vertical = 2,
        Diagonal = 4,
        All = Horizontal | Vertical | Diagonal
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
    public void NotifyKeyPressed(string key, bool[] mod) => OnKeyPress?.Invoke(key, mod);
    public void StopEditing() => OnStopEditing?.Invoke();
    
    
    /// <summary> Creates a new instance with the same values of this field </summary>
    public abstract Field Clone();
    /// <summary> Converts the fields into saving data </summary>
    public abstract FieldSaveData ToSaveData();


    /// <summary>
    /// Creates a new field of a given type
    /// </summary>
    /// <param name="posX">X position of the new field</param>
    /// <param name="posY">Y position of the new field</param>
    /// <param name="suppressModeSwitch">Stay in current editor mode</param>
    /// <typeparam name="T">Any type of field that should be created</typeparam>
    /// <returns>Instance of the new field</returns>
    public static T Create<T>(double posX, double posY, bool suppressModeSwitch = false) where T : Field
    {
        var field = (T)Activator.CreateInstance(typeof(T), posX, posY)!;
        Editor.CreateNewField(field, suppressModeSwitch: suppressModeSwitch);
        return field;
    }
}