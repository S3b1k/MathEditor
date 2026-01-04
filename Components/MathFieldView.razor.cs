using MathEditor.Models;
using MathEditor.Pages;
using Microsoft.JSInterop;

namespace MathEditor.Components;

public partial class MathFieldView : BaseFieldView<MathField>
{
    private DotNetObjectReference<MathFieldView>? _dotnetRef;
    
    
    protected override void OnInitialized()
    {
        Field.OnFieldDeselected += OnDeselected;
    }

    
    #region events
    private async void OnDeselected()
    {
        try
        {
            Field.IsEditing = false;
            
            var text = await JS.InvokeAsync<string>("mathFieldInterop.getValue", ContentRef);
            if(string.IsNullOrWhiteSpace(text))
                Editor.DeleteField(Field);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
        }
    }
    
    [JSInvokable]
    public async Task OnMathChanged(string latex)
    {
        Field.Latex = latex;

        var width = Canvas.ExpandSnap(await JS.InvokeAsync<double>("mathFieldInterop.getWidth", ContentRef));
        width += Canvas.BaseCellSize * 4;
        
        Field.Width = Math.Max(Field.Width, width);

        Console.WriteLine(Field.Latex);
    }
    #endregion


    #region overrides
    protected override async void StartEditing()
    {
        try
        {
            base.StartEditing();
            await JS.InvokeVoidAsync("mathEditor.focusElement", ContentRef);
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
        {
            _dotnetRef = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("mathFieldInterop.init", ContentRef, _dotnetRef, Field.Latex);
        }
    }
}