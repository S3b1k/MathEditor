using MathEditor.Services;

namespace MathEditor.Models.Actions;

public interface IUndoableAction
{
    void Execute();
    void Undo();
}