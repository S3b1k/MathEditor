using MathEditor.Services;

namespace MathEditor.Models.Actions;

public class CreateFieldsAction(Field[] fields, bool suppressModeSwitch, bool select = true) : IUndoableAction
{
    public CreateFieldsAction(Field field, bool suppressModeSwitch, bool select = true) 
        : this([field], suppressModeSwitch, select) { }
    
    
    public void Execute()
    {
        foreach (var field in fields)
            Editor.RegisterField(field, selectField: select, suppressModeSwitch: suppressModeSwitch);
    }
    
    public void Undo()
    {
        foreach (var field in fields)
            Editor.UnregisterField(field);
    }
}