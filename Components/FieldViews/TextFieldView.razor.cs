using MathEditor.Models;
using MathEditor.Pages;
using Microsoft.JSInterop;

namespace MathEditor.Components.FieldViews;

public partial class TextFieldView : BaseFieldView<TextField>
{
    #region events
    protected override void OnInitialized()
    {
        Field.OnFieldDeselected += StopEditing;
        Field.OnValueUpdated += UpdateText;
        Field.OnFieldDeleted += OnDelete;
        Field.OnStopEditing += StopEditing;
    }

    private void OnDelete()
    {
        Field.OnFieldDeselected -= StopEditing;
        Field.OnValueUpdated -= UpdateText;
        Field.OnFieldDeleted -= OnDelete;
        Field.OnStopEditing -= StopEditing;
    }
    
    private async Task OnInput()
    {
        var newHeight = Canvas.SnapCeil(await JS.InvokeAsync<double>("field.getHeight", ContentRef));
        var newWidth = Canvas.SnapCeil(await JS.InvokeAsync<double>("field.getWidth", ContentRef));
        
        Field.Height = newHeight > Field.Height ? newHeight : Field.Height;
        Field.Width = newWidth > Field.Width ? newWidth : Field.Width;
    }
    #endregion

    
    private async void UpdateText()
    {
        try { await JS.InvokeVoidAsync("textField.setText", ContentRef, Field.Text); }
        catch (Exception e) { Console.Error.WriteLine(e); }
    }

    
    #region overrides
    protected override async void StartEditing()
    {
        try
        {
            base.StartEditing();
            await JS.InvokeVoidAsync("mathEditor.focusElement", ContentRef);
            await JS.InvokeVoidAsync("textField.toggleSpellCheck", ContentRef, true);
        }
        catch (Exception e) { Console.Error.WriteLine(e); }
    }

    protected override async void StopEditing()
    {
        if (!Field.IsEditing)
            return;
        
        try
        {
            await JS.InvokeVoidAsync("field.clearSelection");
            await JS.InvokeVoidAsync("textField.toggleSpellCheck", ContentRef, false);
            
            base.StopEditing(await JS.InvokeAsync<string>("textField.getText", ContentRef));
        }
        catch (Exception e) { Console.Error.WriteLine(e); }
    }

    #endregion
    
    
    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            UpdateText();
        return Task.CompletedTask;
    }
}