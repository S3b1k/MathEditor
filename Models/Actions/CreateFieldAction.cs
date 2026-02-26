using MathEditor.Services;

namespace MathEditor.Models.Actions;

public class CreateFieldAction(Field field) : IUndoableAction
{
    public void Execute()
    {
        Editor.RegisterField(field);
    }
    
    public void Undo()
    {
        Editor.DeleteField(field);
    }
}