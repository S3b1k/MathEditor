using MathEditor.Models;
using MathEditor.Models.Actions;

namespace MathEditor.Services;

public class EditorController
{
    public static Stack<IUndoableAction> UndoStack { get; } = new();
    public static Stack<IUndoableAction> RedoStack { get; } = new();


    public static void ExecuteAction(IUndoableAction action)
    {
        action.Execute();
        UndoStack.Push(action);
        RedoStack.Clear();
        Editor.SaveCachedFile();
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