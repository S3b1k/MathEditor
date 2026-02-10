using Microsoft.AspNetCore.Components;

namespace MathEditor.Components;

public partial class SaveFileDialog : ComponentBase
{
    public const string DefaultFilename = "New Document";
 
    [Parameter] public EventCallback<string> OnSave { get; set; }
    
    public bool IsOpen { get; set; }
    public string Filename { get; set; } = DefaultFilename;
    


    public void Open(string filename = DefaultFilename)
    {
        Filename = filename;
        IsOpen = true;
        StateHasChanged();
    }

    public void Close()
    {
        IsOpen = false;
        StateHasChanged();
    }

    private async Task OnSaveClicked()
    {
        await OnSave.InvokeAsync(Filename);
        Close();
    }
}