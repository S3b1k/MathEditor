using MathEditor.Models;

namespace MathEditor.Services;

public class EditorState
{
    public event Action? OnChange;

    private EditorMode _mode = EditorMode.Idle;
    public EditorMode Mode
    {
        get => _mode;
        private set
        {
            _mode = value;
            NotifyStateChanged();
        }
    }

    public void SetMode(EditorMode mode)
    {
        Mode = mode;
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}