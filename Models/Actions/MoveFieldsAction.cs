namespace MathEditor.Models.Actions;

public class MoveFieldsAction(Field[] fields, (double x, double y) offset) : IUndoableAction
{
    public MoveFieldsAction(Field field, (double x, double y) offset) : this([field], offset) { }
    
    
    public void Execute()
    {
        foreach (var field in fields)
        {
            field.PosX += offset.x;
            field.PosY += offset.y;
        }
    }
    
    public void Undo()
    {
        foreach (var field in fields)
        {
            field.PosX -= offset.x;
            field.PosY -= offset.y;
        }
    }
}