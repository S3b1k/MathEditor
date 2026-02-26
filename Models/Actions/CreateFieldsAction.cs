using MathEditor.Services;

namespace MathEditor.Models.Actions;

public class CreateFieldsAction(Field[] fields) : IUndoableAction
{
    public CreateFieldsAction(Field field) : this([field]) { }
    
    
    public void Execute()
    {
        foreach (var field in fields)
            Editor.RegisterField(field, suppressModeSwitch: true);
    }
    
    public void Undo()
    {
        foreach (var field in fields)
            Editor.UnregisterField(field);
    }
}