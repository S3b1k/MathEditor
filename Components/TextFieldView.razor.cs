using Microsoft.AspNetCore.Components;
using MathEditor.Models;
using MathEditor.Pages;
using Microsoft.JSInterop;

namespace MathEditor.Components;

public partial class TextFieldView : BaseFieldView<TextField>
{
    private ElementReference _contentRef;
    
    
    private async Task OnInput()
    {
        Field.Text = await JS.InvokeAsync<string>("textField.getText", _contentRef);
        
        var newHeight = Canvas.ExpandSnap(await JS.InvokeAsync<double>("field.getHeight", _contentRef));
        var newWidth = Canvas.ExpandSnap(await JS.InvokeAsync<double>("field.getWidth", _contentRef));
        
        Field.Height = newHeight > Field.Height ? newHeight : Field.Height;
        Field.Width = newWidth > Field.Width ? newWidth : Field.Width;
    }


    protected override void OnInitialized()
    {
        Field.OnFieldDeselected += OnDeselected;
    }
    

    private async void OnDeselected()
    {
        try
        {
            Field.ContentSelected = false;
            await JS.InvokeVoidAsync("textField.clearSelection");
        
            var text = await JS.InvokeAsync<string>("textField.getText", _contentRef);
            if(string.IsNullOrWhiteSpace(text))
                Editor.DeleteField(Field);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("textField.setText", _contentRef, Field.Text);
            
            if (Field.FirstInstantiation)
                await JS.InvokeVoidAsync("mathEditor.setElementFocus", _contentRef);
        }
        Field.ContentSelected = await JS.InvokeAsync<bool>("mathEditor.isElementFocused", _contentRef);
    }
}