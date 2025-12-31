using System.Globalization;
using MathEditor.Models;
using MathEditor.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace MathEditor.Components;

public partial class BaseFieldView<TField> : ComponentBase where TField : Field
{
    [Inject] public required IJSRuntime JS { get; set; }
    [Inject] public required Camera Cam { get; set; }
    [Inject] public required Editor Editor { get; set; }
    
    [Parameter] public required TField Field { get; set; }
    [Parameter] public RenderFragment? Body { get; set; }

    [Parameter] public EventCallback<PointerEventArgs> OnPointerDown { get; set; }
    
    private string Style =>
        $"position:absolute;" +
        $"left:{Field.PosX.ToString(CultureInfo.InvariantCulture)}px;" +
        $"top:{Field.PosY.ToString(CultureInfo.InvariantCulture)}px;" +
        $"width:{Field.Width.ToString(CultureInfo.InvariantCulture)}px;" +
        $"height:{Field.Height.ToString(CultureInfo.InvariantCulture)}px;";

    
    protected virtual void PointerDown(PointerEventArgs e)
    {
        Field.IsDragging = true;

        var (dx, dy) = Cam.ComputeDragOffset(Field, e.ClientX, e.ClientY);
        Field.DragOffsetX = dx;
        Field.DragOffsetY = dy;
        
        Editor.SelectField(Field, e.ShiftKey);
    }
    
    private Task HandlePointerDown(PointerEventArgs e)
    {
        Editor.NotifyFieldClicked();
        return OnPointerDown.InvokeAsync(e);
    }

    private void StartResize(PointerEventArgs e)
    {
        Field.IsResizing = true;
        Field.ResizeStartWidth = Field.Width;
        Field.ResizeStartHeight = Field.Height;
        Field.ResizeStartX = e.ClientX;
        Field.ResizeStartY = e.ClientY;
    }
}