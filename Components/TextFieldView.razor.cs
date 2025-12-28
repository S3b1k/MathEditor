using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MathEditor.Models;

namespace MathEditor.Components;

public partial class TextFieldView : ComponentBase
{
    [Parameter] public required TextField Field { get; set; }
    [Parameter] public double Zoom { get; set; }
    [Parameter] public double PanX { get; set; }
    [Parameter] public double PanY { get; set; }
    [Parameter] public EventCallback<Field> OnSelect { get; set; }
    [Parameter] public EventCallback<Field> OnStartDrag { get; set; }

    private string Style =>
        $"position:absolute;" +
        $"left:{(Field.PosX * Zoom + PanX).ToString(CultureInfo.InvariantCulture)}px;" +
        $"top:{(Field.PosY * Zoom + PanY).ToString(CultureInfo.InvariantCulture)}px;" +
        $"transform:scale({Zoom.ToString(CultureInfo.InvariantCulture)});" +
        $"transform-origin:top left;" +
        $"width:{Field.Width.ToString(CultureInfo.InvariantCulture)}px;" +
        $"height:{Field.Height.ToString(CultureInfo.InvariantCulture)}px;";

    
    private void OnInput(ChangeEventArgs e)
    {
        Field.Text = e.Value?.ToString() ?? "";
    }

    
    private void HandlePointerDown(PointerEventArgs e)
    {
        Field.IsDragging = true;
        Field.DragOffsetX = (e.ClientX - (Field.PosX * Zoom + PanX)) / Zoom;
        Field.DragOffsetY = (e.ClientY - (Field.PosY * Zoom + PanY)) / Zoom;

        OnStartDrag.InvokeAsync(Field);
        
        OnSelect.InvokeAsync(Field);
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