using MathEditor.Models;
using MathEditor.Pages;
using Microsoft.JSInterop;

namespace MathEditor.Components.FieldViews;

public partial class MathFieldView : BaseFieldView<MathField>
{
    private DotNetObjectReference<MathFieldView>? _dotnetRef;
    
    
    protected override void OnInitialized()
    {
        Field.OnFieldDeselected += OnDeselect;
    }


    private async Task UpdateLatex(string latex)
    {
        try
        {
            Field.Latex = latex;
            await JS.InvokeVoidAsync("mathFieldInterop.setValue", ContentRef, latex);
            
            var width = Canvas.SnapCeil(await JS.InvokeAsync<double>("mathFieldInterop.getWidth", ContentRef));
            width += Canvas.BaseCellSize * 4;
        
            Field.Width = Math.Max(Field.Width, width);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    
    #region events
    private async void OnDeselect()
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
        await UpdateLatex(latex);
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


    #region ComputeEngine

    public async Task<double?> EvaluateNumericAsync()
    {
        try
        {
            var latex = Field.Latex;
            latex = latex.Replace("=", "");
            
            var result = await JS.InvokeAsync<double?>("mathComputeEngine.evaluateNumeric", latex);

            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
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