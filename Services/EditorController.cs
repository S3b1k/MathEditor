using MathEditor.Models;

namespace MathEditor.Services;

public class EditorController
{
    
}

public class Action
{
    public ActionType Type;
    public Field Before;
    public Field After;

    public Action(ActionType type, Field before, Field after)
    {
        Type = type;
        Before = before;
        After = after;
    }
}

public enum ActionType
{
    Created,
    Removed,
    TransformChange
}