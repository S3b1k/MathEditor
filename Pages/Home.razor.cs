using MathEditor.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace MathEditor.Pages;

public partial class Home : ComponentBase
{
    #region Properties
    private const double BaseCellSize = 37.8;

    private double _zoom = 1.0;
    private double _targetZoom = 1.0;
    private bool _zooming;
    private double _zoomMouseX, _zoomMouseY;
    
    private double _panX;
    private double _panY;
    private bool _panning;
    private double _lastX, _lastY;
    private double _velX; 
    private double _velY;
    
    private bool _isInteractingWithField;
    #endregion
    
    private List<Field> Fields { get; } = [];

    
    #region Helper Methods
    private static double Snap(double value, double gridSize) => Math.Round(value / gridSize) * gridSize;

    public void AddDefaultTextField() => Fields.Add(new TextField {PosX = 100, PosY = 100, Text = "Hello World"});
    
    private void DeselectFields()
    {
        foreach (var f in Fields)
            f.IsSelected = false;
    }
    #endregion
    
    // Start
    protected override void OnInitialized()
    {
        Commands.OnCreateTextField += AddDefaultTextField;
        
        AddDefaultTextField();
    }

    #region Events
    
    private void OnPointerDown(PointerEventArgs e)
    {
        if (_isInteractingWithField)
            return;
        
        if (e.Button == 1 || e.PointerType == "touch")
        {
            _panning = true;
            _lastX = e.ClientX;
            _lastY = e.ClientY;

            _velX = 0;
            _velY = 0;
        }
        else if (e.Button == 0)
        {
            DeselectFields();
        }
    }

    private void OnPointerMove(PointerEventArgs e)
    {
        if (_panning)
        {
            var dx = e.ClientX - _lastX;
            var dy = e.ClientY - _lastY;
        
            _panX += dx;
            _panY += dy;

            _velX = dx;
            _velY = dy;

            _lastX = e.ClientX;
            _lastY = e.ClientY;
        }

        foreach (var field in Fields)
        {
            if (field.IsResizing)
            {
                var dx = (e.ClientX - field.ResizeStartX) / _zoom;
                var dy = (e.ClientY - field.ResizeStartY) / _zoom;

                var newWidth = field.ResizeStartWidth + dx;
                var newHeight = field.ResizeStartHeight + dy;

                if (!e.CtrlKey)
                {
                    newWidth = Snap(newWidth, BaseCellSize);
                    newHeight = Snap(newHeight, BaseCellSize);
                }
                
                field.Width = Math.Max(50, newWidth);
                field.Height = Math.Max(20, newHeight);
            }
            else if (field.IsDragging)
            {
                var worldX = (e.ClientX - _panX) / _zoom - field.DragOffsetX;
                var worldY = (e.ClientY - _panY) / _zoom - field.DragOffsetY;

                if (e.ShiftKey)
                {
                    var dx = Math.Abs(worldX - field.PosX);
                    var dy = Math.Abs(worldY - field.PosY);

                    if (dx > dy)
                        worldY = field.PosY;
                    else
                        worldX = field.PosX;
                }
                
                if (!e.CtrlKey)
                {
                    worldX = Snap(worldX, BaseCellSize);
                    worldY = Snap(worldY, BaseCellSize);
                }

                field.PosX = worldX;
                field.PosY = worldY;
            }
        }
    }

    private void OnPointerUp(PointerEventArgs e)
    {
        _panning = false;

        foreach (var field in Fields)
        {
            field.IsDragging = false;
            field.IsResizing = false;
        }

        _isInteractingWithField = false;
    }

    private void OnWheel(WheelEventArgs e)
    {
        if (e.CtrlKey)
        {
            const double zoomSpeed = 0.002;
            var zoomFactor = 1 - e.DeltaY * zoomSpeed;
            _targetZoom = Math.Clamp(_targetZoom * zoomFactor, 0.2, 5.0);

            _zoomMouseX = e.ClientX;
            _zoomMouseY = e.ClientY;

            _zooming = true;
        }
        else
        {
            const float scrollSpeed = 0.3f;
            _velX -= e.DeltaX * scrollSpeed;
            _velY -= e.DeltaY * scrollSpeed;
        }
    }
    
    private void OnKeyDown(KeyboardEventArgs e)
    {
        var selected = Fields.Where(f => f.IsSelected).ToList();
        if (selected.Count > 0)
        {
            var step = e.CtrlKey ? 1 : BaseCellSize;

            foreach (var field in selected)
            {
                switch (e.Key)
                {
                    case "ArrowUp": field.PosY -= step; break;
                    case "ArrowDown": field.PosY += step; break;
                    case "ArrowLeft": field.PosX -= step; break;
                    case "ArrowRight": field.PosX += step; break;
                    case "Delete": Fields.Remove(field); break;
                }
            }   
        }
        
        if(e.Key == "1")
            Fields.Add(new TextField());
    }
    
    #endregion
    
    
    private void ApplyZoomAtCursor(double oldZoom, double newZoom)
    {
        var worldX = (_zoomMouseX - _panX) / oldZoom;
        var worldY = (_zoomMouseY - _panY) / oldZoom;

        _panX = _zoomMouseX - worldX * newZoom;
        _panY = _zoomMouseY - worldY * newZoom;
    }
    private void HandleFieldSelect(Field field)
    {
        _isInteractingWithField = true;
        DeselectFields();
        field.IsSelected = true;
    }
    private void HandleFieldDragStart(Field field)
    {
        _isInteractingWithField = true;
        _panning = false;
        _velX = 0;
        _velY = 0;
    }
    
    
    [JSInvokable]
    public void OnAnimationFrame()
    {
        const double speed = 0.15;
        if (_zooming)
        {
            var oldZoom = _zoom;

            _zoom = oldZoom + (_targetZoom - oldZoom) * speed;

            if (Math.Abs(_zoom - _targetZoom) < 0.001f)
            {
                _zoom = _targetZoom;
                _zooming = false;
            }
            
            ApplyZoomAtCursor(oldZoom, _zoom);
        }

        if (!_panning && !_isInteractingWithField)
        {
            const double friction = 0.7f;

            _panX += _velX;
            _panY += _velY;

            _velX *= friction;
            _velY *= friction;

            if (Math.Abs(_velX) < 0.01) _velX = 0;
            if (Math.Abs(_velY) < 0.01) _velY = 0;
        }
        
        StateHasChanged();
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync(
                "mathEditor.startRenderLoop",
                DotNetObjectReference.Create(this)
            );
        }
    }
}