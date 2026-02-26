using MathEditor.Models;
using MathEditor.Models.Actions;
using MathEditor.Pages;
using MathEditor.Services;
using Microsoft.JSInterop;

namespace MathEditor.Components.FieldViews;

public partial class TextFieldView : BaseFieldView<TextField>
{
    protected override void OnInitialized()
    {
        Field.OnFieldDeselected += OnDeselect;
        Field.OnValueUpdated += OnTextUpdated;
        Field.OnFieldDeleted += OnDelete;
    }

    private void OnDelete()
    {
        Field.OnFieldDeselected -= OnDeselect;
        Field.OnValueUpdated -= OnTextUpdated;
        Field.OnFieldDeleted -= OnDelete;
    }


    private async Task OnInput()
    {
        var newHeight = Canvas.SnapCeil(await JS.InvokeAsync<double>("field.getHeight", ContentRef));
        var newWidth = Canvas.SnapCeil(await JS.InvokeAsync<double>("field.getWidth", ContentRef));
        
        Field.Height = newHeight > Field.Height ? newHeight : Field.Height;
        Field.Width = newWidth > Field.Width ? newWidth : Field.Width;
    }

    private async void OnTextUpdated()
    {
        try { await JS.InvokeVoidAsync("textField.setText", ContentRef, Field.Text); }
        catch (Exception e) { Console.Error.WriteLine(e); }
    }
    
    private async void OnDeselect()
    {
        try
        {
            await JS.InvokeVoidAsync("textField.clearSelection");
            await JS.InvokeVoidAsync("textField.toggleSpellCheck", ContentRef, false);
        
            var text = await JS.InvokeAsync<string>("textField.getText", ContentRef);
            if (string.IsNullOrWhiteSpace(text))
            {
                Editor.DeleteField(Field);
                return;
            }
            
            if (Field.IsEditing)
                EditorController.ExecuteAction(new ChangeFieldAction(Field, Field.Text, text));
            Field.IsEditing = false;
        }
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
    #endregion
    
    
    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            OnTextUpdated();
        return Task.CompletedTask;
    }
}