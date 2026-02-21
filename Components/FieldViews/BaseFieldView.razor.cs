using System.Globalization;
using MathEditor.Models;
using MathEditor.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace MathEditor.Components.FieldViews;

public partial class BaseFieldView<TField> : ComponentBase where TField : Field
{
    [Inject] public required IJSRuntime JS { get; set; }
    [Inject] public required Camera Cam { get; set; }
    [Inject] public required Editor Editor { get; set; }
    
    [Parameter] public required TField Field { get; set; }
    [Parameter] public RenderFragment? Body { get; set; }
    [Parameter] public bool HideOverflow { get; set; }
    
    private ElementReference _fieldRef;
    protected ElementReference ContentRef;

    [Parameter] public EventCallback<PointerEventArgs> OnPointerDown { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnDoubleClick { get; set; }
    [Parameter] public EventCallback OnStartEditing { get; set; }
    
    private string Style =>
        $"position:absolute;" +
        $"left:{Field.PosX.ToString(CultureInfo.InvariantCulture)}px;" +
        $"top:{Field.PosY.ToString(CultureInfo.InvariantCulture)}px;" +
        $"width:{Field.Width.ToString(CultureInfo.InvariantCulture)}px;" +
        $"height:{Field.Height.ToString(CultureInfo.InvariantCulture)}px;";

    
    #region events
    private Task HandlePointerDown(PointerEventArgs e)
    {
        Editor.NotifyFieldClicked();
        return OnPointerDown.InvokeAsync(e);
    }
    
    
    private Task StartEditing(MouseEventArgs e) => 
        OnStartEditing.InvokeAsync(e);
    
    private void StartResize(PointerEventArgs e, Field.ResizeDirection direction) =>
        Editor.BeginFieldResize(Field, direction, (e.ClientX, e.ClientY));
    
    private void StartDrag(PointerEventArgs e) =>
        Editor.BeginFieldDrag(Field, e.ClientX, e.ClientY);
    #endregion
    
    
    #region overridables
    protected virtual void PointerDown(PointerEventArgs e)
    {
        Editor.SelectField(Field, e.ShiftKey);
        StartDrag(e);
    }
    

    protected virtual void StartEditing()
    {
        Field.IsEditing = true;
    }

    #endregion
    
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(firstRender && HideOverflow) 
            await JS.InvokeVoidAsync("field.hideOverflow", _fieldRef, HideOverflow);
    }
}