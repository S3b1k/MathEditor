using MathEditor.Services;

namespace MathEditor.Models.Actions;

public class DeleteFieldsAction(Field[] fields) : IUndoableAction
{
    public DeleteFieldsAction(Field field) : this([field]) { }
    
    
    public void Execute()
    {
        foreach (var field in fields)
            Editor.UnregisterField(field);
    }
    
    public void Undo()
    {
        foreach (var field in fields)
            Editor.RegisterField(field);
    }
}