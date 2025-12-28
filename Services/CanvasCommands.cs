namespace MathEditor.Services;

public class CanvasCommands
{
    public event Action? OnCreateTextField;
    
    public void CreateTextField() => OnCreateTextField?.Invoke();
}