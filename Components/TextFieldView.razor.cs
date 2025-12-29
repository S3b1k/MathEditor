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

    private ElementReference _content;

    private bool _clickingText;
    
    private string Style =>
        $"position:absolute;" +
        $"left:{(Field.PosX * Zoom + PanX).ToString(CultureInfo.InvariantCulture)}px;" +
        $"top:{(Field.PosY * Zoom + PanY).ToString(CultureInfo.InvariantCulture)}px;" +
        $"transform:scale({Zoom.ToString(CultureInfo.InvariantCulture)});" +
        $"transform-origin:top left;" +
        $"width:{Field.Width.ToString(CultureInfo.InvariantCulture)}px;" +
        $"height:{Field.Height.ToString(CultureInfo.InvariantCulture)}px;";

    
    private async Task OnInput(ChangeEventArgs e)
    {
        Field.Text = await JS.InvokeAsync<string>("textField.getText", _content);
        
        var height = await JS.InvokeAsync<double>("textField.getHeight", _content);
        var width = await JS.InvokeAsync<double>("textField.getWidth", _content);

        Field.Height = Canvas.ExpandSnap(height);
        Field.Width = Canvas.ExpandSnap(width);
        
        Console.WriteLine(Field.Text);
    }
    
    
    private async Task HandlePointerDown(PointerEventArgs e)
    {
        if (!_clickingText)
            Field.TextSelected = false;
        Field.IsDragging = true;
        
        Field.DragOffsetX = (e.ClientX - (Field.PosX * Zoom + PanX)) / Zoom;
        Field.DragOffsetY = (e.ClientY - (Field.PosY * Zoom + PanY)) / Zoom;

        await OnStartDrag.InvokeAsync(Field);
        await OnSelect.InvokeAsync((Field, e.ShiftKey));
    }

    private void SelectText()
    {
        _clickingText = true;
        Field.TextSelected = true;
    }
    private void StopClickText() => _clickingText = false;
    
    private void StartResize(PointerEventArgs e)
    {
        Field.IsResizing = true;
        Field.ResizeStartWidth = Field.Width;
        Field.ResizeStartHeight = Field.Height;
        Field.ResizeStartX = e.ClientX;
        Field.ResizeStartY = e.ClientY;
    }
    
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await JS.InvokeVoidAsync("textField.setText", _content, Field.Text);
    }
}