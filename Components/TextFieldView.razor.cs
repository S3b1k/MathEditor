using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MathEditor.Models;
using MathEditor.Pages;
using Microsoft.JSInterop;

namespace MathEditor.Components;

public partial class TextFieldView : ComponentBase
{
    [Parameter] public required TextField Field { get; set; }
    [Parameter] public double Zoom { get; set; }
    [Parameter] public double PanX { get; set; }
    [Parameter] public double PanY { get; set; }
    [Parameter] public EventCallback<(Field field, bool shift)> OnSelect { get; set; }
    [Parameter] public EventCallback<Field> OnStartDrag { get; set; }
    
    private string Style =>
        $"position:absolute;" +
        $"left:{(Field.PosX * Zoom + PanX).ToString(CultureInfo.InvariantCulture)}px;" +
        $"top:{(Field.PosY * Zoom + PanY).ToString(CultureInfo.InvariantCulture)}px;" +
        $"transform:scale({Zoom.ToString(CultureInfo.InvariantCulture)});" +
        $"transform-origin:top left;" +
        $"width:{Field.Width.ToString(CultureInfo.InvariantCulture)}px;" +
        $"height:{Field.Height.ToString(CultureInfo.InvariantCulture)}px;";

    private ElementReference _textarea;
    
    private async void OnInput(ChangeEventArgs e)
    {
        Field.Text = e.Value?.ToString() ?? "";

        var pxHeight = await JS.InvokeAsync<double>("measureTextArea", _textarea);
        
        var worldHeight = pxHeight / Zoom;
        
        worldHeight = Math.Ceiling(worldHeight / Canvas.BaseCellSize) * Canvas.BaseCellSize;
        
        Field.Height = Math.Max(Canvas.BaseCellSize, worldHeight);
        
        await InvokeAsync(StateHasChanged);
    }

    
    private async Task HandlePointerDown(PointerEventArgs e)
    {
        Field.IsDragging = true;
        
        Field.DragOffsetX = (e.ClientX - (Field.PosX * Zoom + PanX)) / Zoom;
        Field.DragOffsetY = (e.ClientY - (Field.PosY * Zoom + PanY)) / Zoom;

        await OnStartDrag.InvokeAsync(Field);
        await OnSelect.InvokeAsync((Field, e.ShiftKey));
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