using MathEditor.Models;
using MathEditor.Pages;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace MathEditor.Components;

public partial class MathFieldView : BaseFieldView<MathField>
{
    private DotNetObjectReference<MathFieldView>? _dotnetRef;
    
    
    protected override void OnInitialized()
    {
        Field.OnFieldDeselected += OnDeselected;
    }


    private async Task UpdateLatex(string latex)
    {
        try
        {
            Field.Latex = latex;
            await JS.InvokeVoidAsync("mathFieldInterop.setValue", ContentRef, latex);
            
            var width = Canvas.ExpandSnap(await JS.InvokeAsync<double>("mathFieldInterop.getWidth", ContentRef));
            width += Canvas.BaseCellSize * 4;
        
            Field.Width = Math.Max(Field.Width, width);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
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
        if (latex.Contains('='))
        {
            var result = await EvaluateNumericAsync(); 
            latex += result.HasValue ? result.Value : "err";
        }
        
        await UpdateLatex(latex);
    }

    private async void OnKeyDown(KeyboardEventArgs e)
    {
        try
        {
            if (e.Key == "Backspace" && Field.Latex.Contains('='))
                await UpdateLatex(Field.Latex.Split('=')[0]);
        }
        catch (Exception _)
        {
            Console.WriteLine("Error");
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