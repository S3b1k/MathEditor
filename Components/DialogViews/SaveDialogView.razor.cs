using MathEditor.Services;
using Microsoft.AspNetCore.Components;

namespace MathEditor.Components.DialogViews;

public partial class SaveDialogView : BaseDialogView
{
    private const string DefaultFilename = "New Document";

    private string Filename { get; set; } = DefaultFilename;
    
    [Parameter] public Func<string, Task>? OnSave { get; set; }
    

    private void OnSaveClicked()
    {
        OnSave?.Invoke(Filename);
        DialogManager.CloseDialog();
    }
}