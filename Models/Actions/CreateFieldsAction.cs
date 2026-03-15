using MathEditor.Services;

namespace MathEditor.Models.Actions;

public class CreateFieldsAction(Field[] fields, bool suppressModeSwitch) : IUndoableAction
{
    public CreateFieldsAction(Field field, bool suppressModeSwitch) : this([field], suppressModeSwitch) { }
    
    
    public void Execute()
    {
        foreach (var field in fields)
            Editor.RegisterField(field, suppressModeSwitch: suppressModeSwitch);
    }
    
    public void Undo()
    {
        foreach (var field in fields)
            Editor.UnregisterField(field);
    }
}