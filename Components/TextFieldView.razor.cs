using MathEditor.Models;
using MathEditor.Pages;
using Microsoft.JSInterop;

namespace MathEditor.Components;

public partial class TextFieldView : BaseFieldView<TextField>
{
    protected override void OnInitialized()
    {
        Field.OnFieldDeselected += OnDeselected;
    }
    
    
    #region events
    private async Task OnInput()
    {
        Field.Text = await JS.InvokeAsync<string>("textField.getText", ContentRef);
        
        var newHeight = Canvas.ExpandSnap(await JS.InvokeAsync<double>("field.getHeight", ContentRef));
        var newWidth = Canvas.ExpandSnap(await JS.InvokeAsync<double>("field.getWidth", ContentRef));
        
        Field.Height = newHeight > Field.Height ? newHeight : Field.Height;
        Field.Width = newWidth > Field.Width ? newWidth : Field.Width;
    }
    
    private async void OnDeselected()
    {
        try
        {
            Field.IsEditing = false;
            await JS.InvokeVoidAsync("textField.clearSelection");
            await JS.InvokeVoidAsync("textField.toggleSpellCheck", ContentRef, false);
        
            var text = await JS.InvokeAsync<string>("textField.getText", ContentRef);
            if(string.IsNullOrWhiteSpace(text))
                Editor.DeleteField(Field);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    #endregion

    
    #region overrides
    protected override async void StartEditing()
    {
        try
        {
            base.StartEditing();
            await JS.InvokeVoidAsync("mathEditor.focusElement", ContentRef);
            await JS.InvokeVoidAsync("textField.toggleSpellCheck", ContentRef, true);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
        }
    }
    #endregion
    
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await JS.InvokeVoidAsync("textField.setText", ContentRef, Field.Text);
    }
}