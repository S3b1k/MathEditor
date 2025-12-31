using Microsoft.AspNetCore.Components;
using MathEditor.Models;
using MathEditor.Pages;
using Microsoft.JSInterop;

namespace MathEditor.Components;

public partial class MathFieldView : BaseFieldView<MathField>
{
    private ElementReference _contentRef;
    
    
    private async Task OnInput()
    {
        Field.Text = await JS.InvokeAsync<string>("textField.getText", _contentRef);
        
        var newHeight = Canvas.ExpandSnap(await JS.InvokeAsync<double>("textField.getHeight", _contentRef));
        var newWidth = Canvas.ExpandSnap(await JS.InvokeAsync<double>("textField.getWidth", _contentRef));
        
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
            Field.TextSelected = false;
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
            await JS.InvokeVoidAsync("textField.setText", _contentRef, Field.Text);
        Field.TextSelected = await JS.InvokeAsync<bool>("field.hasContentFocus", _contentRef);
    }
}