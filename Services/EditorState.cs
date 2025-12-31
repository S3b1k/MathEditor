using MathEditor.Models;

namespace MathEditor.Services;

public class EditorState
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
    
    public event Action? OnFieldClicked;
    public List<Field> SelectedFields { get; } = [];
    public int SelectionCount => SelectedFields.Count;
    public event Action<Field>? OnFieldDeleteRequest;
    

    public void SetMode(EditorMode mode) => Mode = mode;
    
    
    #region Field Factory
    
    public TextField CreateTextField(double posX, double posY) => new(posX, posY);

    // TODO - Implement math fields
    public TextField CreateMathField(double posX, double posY) => CreateTextField(posX, posY);
    
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


    public void DeleteField(Field field) => OnFieldDeleteRequest?.Invoke(field);
    #endregion
    
    
    #region Event Notifications
    
    private void NotifyStateChanged() => OnStateChanged?.Invoke();
    public void NotifyFieldClicked() => OnFieldClicked?.Invoke();
    
    #endregion
}