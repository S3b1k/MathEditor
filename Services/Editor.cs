using MathEditor.Models;
using Microsoft.JSInterop;

namespace MathEditor.Services;

public class Editor
{
    public event Action? OnStateChanged;
    public Enums Mode
    {
        get;
        private set
        {
            field = value;
            NotifyStateChanged();
        }
    } = Enums.Idle;
    
    public List<Field> Fields { get; } = [];
    
    public event Action? OnFieldClicked;
    public List<Field> SelectedFields { get; } = [];
    public int SelectionCount => SelectedFields.Count;
    

    public void SetMode(Enums mode) => Mode = mode;
    
    
    #region Field Factory

    private void CreateField(Field field)
    {
        Fields.Add(field);
        SelectField(field);
    }
    
    public void CreateTextField(double posX, double posY) => CreateField(new TextField(posX, posY));
    public void CreateMathField(double posX, double posY) => CreateField(new MathField(posX, posY));

    #endregion
 
    
    #region Field Handling
    
    public void SelectField(Field field) => SelectField(field, true);
    public void SelectField(Field field, bool shift)
    {
        NotifyFieldClicked();
        
        if (field.IsSelected) return;

        if (!shift && !field.IsSelected)
            DeselectAllFields();
        
        field.IsSelected = true;
        SelectedFields.Add(field);
    }

    public void DeselectField(Field field)
    {
        if (!field.IsSelected) return;
        
        field.IsSelected = false;
        SelectedFields.Remove(field);
        
        field.NotifyFieldDeselected();
    }
    public void DeselectAllFields()
    {
        foreach (var field in SelectedFields)
        {
            field.IsSelected = false;
            field.NotifyFieldDeselected();            
        } 
        
        SelectedFields.Clear();
    }
    
    public void DeleteField(Field field)
    {
        if (field.IsSelected)
            DeselectField(field);
        Fields.Remove(field);
    }


    public static void BeginFieldDrag(Field field, (double x, double y) dragOffset)
    {
        field.IsDragging = true;
        field.DragOffsetX = dragOffset.x;
        field.DragOffsetY = dragOffset.y;
    }


    public static void BeginFieldResize(Field field, ResizeDirection dir, (double x, double y) startPos)
    {
        field.IsResizing = true;
        field.ResizeStartWidth = field.Width;
        field.ResizeStartHeight = field.Height;
        field.ResizeStartMouseX = startPos.x;
        field.ResizeStartMouseY = startPos.y;
        field.ResizeStartPosX = field.PosX;
        field.ResizeStartPosY = field.PosY;
        field.ResizeDir = dir;
    }
    #endregion
    
    
    
    [JSInvokable]
    public void OnKeypress(string key, bool ctrl, bool shift, bool alt)
    {
        if (SelectionCount == 0)
        {
            switch (key)
            {
                case "escape":
                    SetMode(Enums.Idle);
                    break;
                case "t":
                    SetMode(Enums.CreateTextField);
                    break;
                case "m":
                    SetMode(Enums.CreateMathField);
                    break;
            }
        }
        else
        {
            switch (key)
            {
                case "escape":
                    DeselectAllFields();
                    break;
                case "delete":
                    var selected = SelectedFields.ToArray();
                    foreach (var field in selected)
                    {
                        if (field.IsEditing)
                            continue;
                    
                        DeleteField(field);
                    }
                    break;
            }
        }
    }
    
    
    #region Event Notifications
    
    private void NotifyStateChanged() => OnStateChanged?.Invoke();
    public void NotifyFieldClicked() => OnFieldClicked?.Invoke();
    
    #endregion
}