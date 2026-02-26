namespace MathEditor.Models.Actions;

public class ScaleFieldAction(
        Field field, 
        (double x, double y) offset, 
        (double x, double y) startPos, 
        (double x, double y) endPos) 
    : IUndoableAction
{
    
    public void Execute()
    {
        field.Width += offset.x;
        field.Height += offset.y;
        field.PosX = endPos.x;
        field.PosY = endPos.y;
    }
    
    public void Undo()
    {
        field.Width -= offset.x;
        field.Height -= offset.y;
        field.PosX = startPos.x;
        field.PosY = startPos.y;
    }
}