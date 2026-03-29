using MathEditor.Models;
using MathEditor.Services;
using Microsoft.JSInterop;

namespace MathEditor.Components.FieldViews;

public partial class MathFieldView : BaseFieldView<MathField>
{
    #region events
    protected override void OnInitialized()
    {
        Field.OnFieldDeselected += StopEditing;
        Field.OnValueUpdated += UpdateLatex;
        Field.OnFieldDeleted += OnDelete;
        Field.OnStopEditing += StopEditing;
        Field.OnKeyPress += OnKeyPressed;
    }

    private void OnDelete()
    {
        Field.OnFieldDeselected -= StopEditing;
        Field.OnValueUpdated -= UpdateLatex;
        Field.OnFieldDeleted -= OnDelete;
        Field.OnStopEditing -= StopEditing;
        Field.OnKeyPress -= OnKeyPressed;
    }
    
    private async Task OnInput()
    {
        await UpdateFieldView();
    }
    private async void OnKeyPressed(string key, bool[] mod)
    {
        
    }
    #endregion
    
    
    private async Task UpdateFieldView()
    {
        try
        {
            // var width = await JS.InvokeAsync<double>("mathField.getWidth", ContentRef);
            // width = Canvas.SnapCeil(width);
            // width += Canvas.BaseCellSize * 4;
        
            // Field.Width = Math.Max(Field.Width, width);
            
            
        }
        catch (Exception e) { Console.Error.WriteLine(e); }
    }
    

    private async void UpdateLatex()
    {
        try
        {
            await JS.InvokeVoidAsync("mathField.setValue", ContentRef, Field.Latex);
            await UpdateFieldView(); 
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

    protected override async void StopEditing()
    {
        if (!Field.IsEditing)
            return;

        try
        {
            var val = await JS.InvokeAsync<string>("mathField.getValue", ContentRef);
            var result = MathParser.Parse(val);
            var declaredNames = new List<string>();

            foreach (var error in result.Errors)
                await JS.InvokeVoidAsync("alert", error);

            if (result.Assignment != null)
            {
                var (varName, varVal) = result.Assignment;
                Editor.Variables.Declare(Field.Id, varName, varVal, out var warning);
                declaredNames.Add(varName);

                if (warning != null)
                    await JS.InvokeVoidAsync("alert", warning);
            }

            // if (result.Evaluation != null)
            // {
            //     var varName = result.Evaluation.VarName;
            //     if (Editor.Variables.TryGet(varName, out var varVal))
            //         await JS.InvokeVoidAsync("alert", $"{varName} = {varVal}");
            //     else
            //         await JS.InvokeVoidAsync("alert", $"Variable '{varName}' not found");
            // }

            Editor.Variables.Reconcile(Field.Id, declaredNames);
            
            base.StopEditing(val);
        }
        catch (Exception e) { Console.Error.WriteLine(e); }
    }

    #endregion


    [JSInvokable]
    public string? EvaluateLatex(string latex)
    {
        // Append a fake '=' so the parser sees a complete evaluation expression
        var result = MathParser.Parse(latex + "=");

        if (result.Evaluation == null)
            return null;

        return Editor.Variables.TryGet(result.Evaluation.VarName, out var val)
            ? val
            : null;
    }
    
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            UpdateLatex();
            await JS.InvokeVoidAsync("mathField.registerHandler", ContentRef, DotNetObjectReference.Create(this));            
        }
    }
}