using MathEditor.Services;

namespace MathEditor.Models.Actions;

public class ChangeFieldAction(Field field, string before, string after) : IUndoableAction
{
    public void Execute()
    {
    }
    
    public void Undo()
    {
        
    }
}