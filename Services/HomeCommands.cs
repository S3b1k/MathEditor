namespace MathEditor.Services;

public class HomeCommands
{
    public event Action? OnCreateTextField;
    
    public void CreateTextField() => OnCreateTextField?.Invoke();
}