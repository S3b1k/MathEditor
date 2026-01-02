using Microsoft.AspNetCore.Components;
using MathEditor.Models;
using MathEditor.Pages;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace MathEditor.Components;

public partial class MathFieldView : BaseFieldView<MathField>
{
    private ElementReference _contentRef;
    private DotNetObjectReference<MathFieldView> _dotnetRef;
    
    
    protected override void OnInitialized()
    {
        Field.OnFieldDeselected += OnDeselected;
    }
    private async void OnDeselected()
    {
        try
        {
            Field.ContentSelected = false;
        
            var text = await JS.InvokeAsync<string>("mathFieldInterop.getValue", _contentRef);
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
            _dotnetRef = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("mathFieldInterop.init", _contentRef, _dotnetRef, Field.Latex);
        }
        Field.ContentSelected = await JS.InvokeAsync<bool>("mathEditor.isElementFocused", _contentRef);
    }

    [JSInvokable]
    public async Task OnMathChanged(string latex)
    {
        Field.Latex = latex;

        var width = Canvas.ExpandSnap(await JS.InvokeAsync<double>("mathFieldInterop.getWidth", _contentRef));
        width += Canvas.BaseCellSize * 4;
        
        Field.Width = Math.Max(Field.Width, width);
    }
}