using MathEditor.Models.Actions;

namespace MathEditor.Services;

public static class EditorController
{
    private static Stack<IUndoableAction> UndoStack { get; } = new();
    private static Stack<IUndoableAction> RedoStack { get; } = new();


    public static void ExecuteAction(IUndoableAction action)
    {
        action.Execute();
        RegisterAction(action);
        Editor.SaveCachedFile();
    }

    public static void RegisterAction(IUndoableAction action)
    {
        UndoStack.Push(action);
        RedoStack.Clear();
    }
    

    public static void Undo()
    {
        if (UndoStack.Count == 0)
            return;
        
        var action = UndoStack.Pop();
        action.Undo();
        RedoStack.Push(action);
        Editor.SaveCachedFile();
    }


    public static void Redo()
    {
        if (RedoStack.Count == 0)
            return;
        
        var action = RedoStack.Pop();
        action.Execute();
        UndoStack.Push(action);
        Editor.SaveCachedFile();
    }
}