using MathEditor.Models;

namespace MathEditor.Services;

public class EditorState
{
    public event Action? OnChange;

    public EditorMode Mode
    {
        get;
        private set
        {
            field = value;
            NotifyStateChanged();
        }
    } = EditorMode.Idle;

    public void SetMode(EditorMode mode) => Mode = mode;

    private void NotifyStateChanged() => OnChange?.Invoke();
}