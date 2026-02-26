namespace MathEditor.Models.Actions;

public class ChangeFieldAction(Field field, string before, string after) : IUndoableAction
{
    public void Execute()
    {
        switch (field)
        {
            case TextField tf:
                tf.Text = after;
                break;
            case MathField mf:
                mf.Latex = after;
                break;
        }
        field.NotifyValueUpdated();
    }
    
    public void Undo()
    {
        switch (field)
        {
            case TextField tf:
                tf.Text = before;
                break;
            case MathField mf:
                mf.Latex = before;
                break;
        }
        field.NotifyValueUpdated();
    }
}