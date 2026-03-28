using MathEditor.Models;
using Microsoft.JSInterop;

namespace MathEditor.Components.FieldViews;

public partial class MathFieldView : BaseFieldView<MathField>
{
    private string _currentLatex;
    
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
        
        try { base.StopEditing(await JS.InvokeAsync<string>("mathField.getValue", ContentRef)); }
        catch (Exception e) { Console.Error.WriteLine(e); }
    }

    #endregion
    
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            UpdateLatex();
            await JS.InvokeVoidAsync("mathField.registerHandler", ContentRef, DotNetObjectReference.Create(this));            
        }
    }
}