using MathEditor.Models;
using MathEditor.Models.Actions;
using MathEditor.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace MathEditor.Components.FieldViews;

public partial class ImageFieldView : BaseFieldView<ImageField>
{
    private bool _isDragOver;
    private ElementReference _dropZoneRef;
    
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && string.IsNullOrEmpty(Field.ImageSource))
            await JS.InvokeVoidAsync("mathEditor.registerImageDropHandler", _dropZoneRef, DotNetObjectReference.Create(this));
    }
    
    [JSInvokable]
    public void ApplyImage(string dataUrl)
    {
        _isDragOver = false;
        EditorController.ExecuteAction(new ChangeFieldAction(Field, Field.ImageSource, dataUrl));
        Editor.SaveCachedFile();
        StateHasChanged();
    }
}