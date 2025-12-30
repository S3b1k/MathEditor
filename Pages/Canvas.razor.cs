using System.Globalization;
using MathEditor.Models;
using MathEditor.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace MathEditor.Pages;

public partial class Canvas : ComponentBase
{
    #region Properties
    
    #region styles

    private string GridStyle
    {
        get
        {
            var scaledCell = BaseCellSize * _zoom;

            var offsetX = _panX % scaledCell;
            var offsetY = _panY % scaledCell;

            if (offsetX < 0) offsetX += scaledCell;
            if (offsetY < 0) offsetY += scaledCell;
            
            return 
                $"--cell-size: {scaledCell.ToString(CultureInfo.InvariantCulture)}px;" +
                $"--pan-x: {_panX.ToString(CultureInfo.InvariantCulture)}px; " +
                $"--pan-y: {_panY.ToString(CultureInfo.InvariantCulture)}px; ";
        }
    }

    
    // ***
    private string CameraStyle =>
        "position: absolute;" +
        "left: 0;" +
        "top: 0;" +
        $"transform: translate({_panX}px, {_panY}px) scale({_zoom});" +
        "transform-origin: 0 0;";
    
    private string SelectionBoxStyle =>
        $"left:{Math.Min(_selectStartX, _selectCurrentX).ToString(CultureInfo.InvariantCulture)}px;" +
        $"top:{Math.Min(_selectStartY, _selectCurrentY).ToString(CultureInfo.InvariantCulture)}px;" +
        $"width:{Math.Abs(_selectCurrentX - _selectStartX).ToString(CultureInfo.InvariantCulture)}px;" +
        $"height:{Math.Abs(_selectCurrentY - _selectStartY).ToString(CultureInfo.InvariantCulture)}px;";
    #endregion
    
    public const double BaseCellSize = 18.9;
    
    // Zooming
    private double _zoom = 0.5;
    private double _targetZoom = 1.0;
    private bool _zooming;
    private double _zoomMouseX, _zoomMouseY;
    
    // Panning
    private double _panX;
    private double _panY;
    private bool _panning;
    private double _lastX, _lastY;
    private double _velX; 
    private double _velY;
    
    // Fields
    private bool _isInteractingWithField;
    private List<Field> Fields { get; } = [];
    private readonly List<Field> _selectedFields = [];
    
    // Selecting
    private bool _isSelecting;
    private bool _isFadingSelection;
    private double _selectStartX, _selectStartY;
    private double _selectCurrentX, _selectCurrentY;
    
    // Editor
    private EditorMode _previousMode = EditorMode.Idle;
    
    #endregion

    
    #region Helper Methods
    
    private static double Snap(double value) => Math.Round(value / BaseCellSize) * BaseCellSize;
    public static double ExpandSnap(double value) => Math.Ceiling(value / BaseCellSize) * BaseCellSize;

    private void StartPan(PointerEventArgs e)
    {
        Editor.SetMode(EditorMode.Pan);
        _panning = true;
            
        _lastX = e.ClientX;
        _lastY = e.ClientY;
        _velX = 0;
        _velY = 0;
    }
    #endregion
    
    
    #region Editor

    private void CreateTextField(double x, double y) => Fields.Add(new TextField(x, y));

    private void CreateMathField(double x, double y)
    {
        CreateTextField(x, y);
    }
    
    #endregion
    
    
    // protected override void OnInitialized()
    // {
    //     
    // }
    
    #region Events
    
    private void OnPointerDown(PointerEventArgs e)
    {
        if (_isInteractingWithField)
            return;

        if (e.Button == 0)
        {
            var worldX = (e.ClientX - _panX) / _zoom;
            var worldY =  (e.ClientY - _panY) / _zoom;
            
            switch (Editor.Mode)
            {
                case EditorMode.Pan:
                    _previousMode = EditorMode.Pan;
                    StartPan(e);
                    break;
                case EditorMode.CreateTextField:
                    CreateTextField(worldX, worldY);
                    Editor.SetMode(EditorMode.Idle);
                    break;
                case EditorMode.CreateMathField:
                    CreateMathField(worldX, worldY);
                    Editor.SetMode(EditorMode.Idle);
                    break;
                default: 
                    DeselectAllFields();
                    
                    _isSelecting = true;
                    _selectStartX = e.ClientX;
                    _selectStartY = e.ClientY;
                    _selectCurrentX = e.ClientX;
                    _selectCurrentY = e.ClientY;
                    break;
            }
            
        }
        else if (e.Button == 1 || e.PointerType == "touch")
        {
            _previousMode = Editor.Mode;
            StartPan(e);
        }
    }

    private void OnPointerMove(PointerEventArgs e)
    {
        if (Editor.Mode == EditorMode.Pan && _panning)
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

        if (_isSelecting)
        {
            _selectCurrentX = e.ClientX;
            _selectCurrentY = e.ClientY;
            
            var rect = GetSelectionWorldRect();
            SelectElementsInside(rect);
            
            StateHasChanged();
        }
        
        foreach (var field in _selectedFields)
        {
            if (field.IsResizing)
            {
                var dx = (e.ClientX - field.ResizeStartX) / _zoom;
                var dy = (e.ClientY - field.ResizeStartY) / _zoom;

                var newWidth = field.ResizeStartWidth + dx;
                var newHeight = field.ResizeStartHeight + dy;

                if (!e.CtrlKey)
                {
                    newWidth = Snap(newWidth);
                    newHeight = Snap(newHeight);
                }
                
                field.Width = Math.Max(BaseCellSize, newWidth);
                field.Height = Math.Max(BaseCellSize, newHeight);
            }
            else if (field.IsDragging)
            {
                if (field is TextField { TextSelected: true })
                    continue;
                
                var worldX = (e.ClientX - _panX) / _zoom - field.DragOffsetX;
                var worldY = (e.ClientY - _panY) / _zoom - field.DragOffsetY;
                
                if (!e.CtrlKey)
                {
                    worldX = Snap(worldX);
                    worldY = Snap(worldY);
                }

                field.PosX = worldX;
                field.PosY = worldY;
            }
        }
    }

    private async void OnPointerUp(PointerEventArgs e)
    {
        if (Editor.Mode == EditorMode.Pan)
            Editor.SetMode(_previousMode);
        _panning = false;

        if (_isSelecting)
        {
            _isSelecting = false;
            _isFadingSelection = true;
            
            await Task.Delay(200);

            _isFadingSelection = false;
            StateHasChanged();
        }

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
            _targetZoom = Math.Clamp(_targetZoom * zoomFactor, .5, 5);

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
        if (_selectedFields.Count > 0)
        {
            var step = e.CtrlKey ? 1 : BaseCellSize;

            foreach (var field in _selectedFields)
            {
                if (field is TextField { TextSelected: true })
                    break;
                
                switch (e.Key)
                {
                    case "ArrowUp": field.PosY -= step; break;
                    case "ArrowDown": field.PosY += step; break;
                    case "ArrowLeft": field.PosX -= step; break;
                    case "ArrowRight": field.PosX += step; break;
                }
            }   
        }
    }
    
    #endregion
    
    
    #region field handling
    private void SelectField(Field field)
    {
        if (field.IsSelected) return;
        field.IsSelected = true;
        _selectedFields.Add(field);
    }
    private void DeselectField(Field field)
    {
        field.IsSelected = false;
        _selectedFields.Remove(field);

        if (field is TextField tf)
            tf.TextSelected = false;
    }
    private void DeselectAllFields()
    {
        var fields = _selectedFields.ToArray();
        foreach (var f in fields)
            DeselectField(f);
    }
    
    
    private void HandleFieldSelect((Field field, bool shift) data)
    {
        var (field, shift) = data;
        
        _isInteractingWithField = true;
        
        if (!shift && !field.IsSelected)
            DeselectAllFields();
        
        SelectField(field);
    }
    
    private void HandleFieldDragStart(Field field)
    {
        if (field is TextField { TextSelected: true })
            return;
        
        _isInteractingWithField = true;
        _velX = 0;
        _velY = 0;
    }
    #endregion
    
    
    private void ApplyZoomAtCursor(double oldZoom, double newZoom)
    {
        var worldX = (_zoomMouseX - _panX) / oldZoom;
        var worldY = (_zoomMouseY - _panY) / oldZoom;

        _panX = _zoomMouseX - worldX * newZoom;
        _panY = _zoomMouseY - worldY * newZoom;
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

        if (Editor.Mode != EditorMode.Pan && !_isInteractingWithField)
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
    
    [JSInvokable]
    public void OnKeypress(string key, bool ctrl, bool shift, bool alt)
    {
        if (_selectedFields.Count == 0)
        {
            switch (key)
            {
                case "escape":
                    Editor.SetMode(EditorMode.Idle);
                    break;
                case "t":
                    Editor.SetMode(EditorMode.CreateTextField);
                    break;
            }
        }
        else
        {
            var selected = _selectedFields.ToArray();
            switch (key)
            {
                case "delete":
                    foreach (var field in selected)
                    {
                        if (field is TextField { TextSelected: true })
                            continue;
                        
                        _selectedFields.Remove(field);
                        Fields.Remove(field);
                    }
                    break;
            }
        }
    }
    
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var dotnetRef = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("mathEditor.startRenderLoop", dotnetRef);
            await JS.InvokeVoidAsync("keyboardActions.register", dotnetRef);
        }
    }


    private (double x, double y, double w, double h) GetSelectionWorldRect()
    {
        var x1 = Math.Min(_selectStartX, _selectCurrentX);
        var y1 = Math.Min(_selectStartY, _selectCurrentY);
        var x2 = Math.Max(_selectStartX, _selectCurrentX);
        var y2 = Math.Max(_selectStartY, _selectCurrentY);

        // Convert screen → world
        var worldX1 = (x1 - _panX) / _zoom;
        var worldY1 = (y1 - _panY) / _zoom;
        var worldX2 = (x2 - _panX) / _zoom;
        var worldY2 = (y2 - _panY) / _zoom;

        return (worldX1, worldY1, worldX2 - worldX1, worldY2 - worldY1);
    }

    private void SelectElementsInside((double x, double y, double w, double h) rect)
    {
        var (x, y, w, h) = rect;

        foreach (var field in Fields)
        {
            if (field.PosX >= x &&
                field.PosY >= y &&
                field.PosX + field.Width <= x + w &&
                field.PosY + field.Height <= y + h)
            {
                SelectField(field);
            }
            else
                DeselectField(field);
        }
    }

}