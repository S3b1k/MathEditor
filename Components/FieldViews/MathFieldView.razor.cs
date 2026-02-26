using MathEditor.Models;
using MathEditor.Models.Actions;
using MathEditor.Pages;
using MathEditor.Services;
using Microsoft.JSInterop;

namespace MathEditor.Components.FieldViews;

public partial class MathFieldView : BaseFieldView<MathField>
{
    protected override void OnInitialized()
    {
        Field.OnFieldDeselected += OnDeselect;
        Field.OnValueUpdated += OnLatexUpdated;
        Field.OnFieldDeleted += OnDelete;
    }

    private void OnDelete()
    {
        Field.OnFieldDeselected -= OnDeselect;
        Field.OnValueUpdated -= OnLatexUpdated;
        Field.OnFieldDeleted -= OnDelete;
    }
    

    [JSInvokable]
    public async Task OnMathChanged(string latex) =>
        await UpdateFieldView();

    private async void OnLatexUpdated()
    {
        try
        {
            await JS.InvokeVoidAsync("mathField.setValue", ContentRef, Field.Latex);
            await UpdateFieldView(); 
        }
        catch (Exception e) { Console.Error.WriteLine(e); }
    }
    private async Task UpdateFieldView()
    {
        try
        {
            var width = await JS.InvokeAsync<double>("mathField.getWidth", ContentRef);
            width = Canvas.SnapCeil(width);
            width += Canvas.BaseCellSize * 4;
        
            Field.Width = Math.Max(Field.Width, width);
        }
        catch (Exception e) { Console.Error.WriteLine(e); }
    }
    
    
    private async void OnDeselect()
    {
        try
        {
            var latex = await JS.InvokeAsync<string>("mathField.getValue", ContentRef);
            if (string.IsNullOrWhiteSpace(latex))
            {
                Editor.DeleteField(Field);
                return;
            }
            
            if (Field.IsEditing)
                EditorController.ExecuteAction(new ChangeFieldAction(Field, Field.Latex, latex));
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
        }
        catch (Exception e) { Console.Error.WriteLine(e); }
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
            await JS.InvokeVoidAsync("mathField.init", 
                                     ContentRef, 
                                     DotNetObjectReference.Create(this), 
                                     Field.Latex);
        }
    }
}