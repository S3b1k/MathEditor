using MathEditor.Models;
using MathEditor.Pages;
using Microsoft.JSInterop;

namespace MathEditor.Services;

public class Editor
{
    public event Action? OnStateChanged;
    public EditorMode Mode
    {
        get;
        private set
        {
            field = value;
            NotifyStateChanged();
        }
    } = EditorMode.Idle;
    
    public List<Field> Fields { get; } = [];
    
    public event Action? OnFieldClicked;
    public List<Field> SelectedFields { get; } = [];
    public int SelectionCount => SelectedFields.Count;
    

    public void SetMode(EditorMode mode) => Mode = mode;
    
    
    #region Field Factory
    
    public TextField CreateTextField(double posX, double posY)
    {
        var field = new TextField(posX, posY);
        Fields.Add(field);
        SelectField(field);
        return field;
    }

    public MathField CreateMathField(double posX, double posY)
    {
        var field = new MathField(posX, posY);
        Fields.Add(field);
        return field;
    }

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
    #endregion
    
    
    
    [JSInvokable]
    public void OnKeypress(string key, bool ctrl, bool shift, bool alt)
    {
        if (SelectionCount == 0)
        {
            switch (key)
            {
                case "escape":
                    SetMode(EditorMode.Idle);
                    break;
                case "t":
                    SetMode(EditorMode.CreateTextField);
                    break;
                case "m":
                    SetMode(EditorMode.CreateMathField);
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
                        if (field.ContentSelected)
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