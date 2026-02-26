using System.Globalization;
using MathEditor.Models;
using MathEditor.Models.Actions;
using MathEditor.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace MathEditor.Pages;

public partial class Canvas : ComponentBase
{
    #region Properties
 
    public const double BaseCellSize = 18.9;
    
    #region styles
    
    private string CameraStyle =>
        "position: absolute;" +
        "left: 0;" +
        "top: 0;" +
        $"transform: translate({F(Cam.PanX)}px, " +
        $"                     {F(Cam.PanY)}px) " +
        $"scale({F(Zoom)});" +
        "transform-origin: 0 0;";
    
    private string SelectionBoxStyle =>
        $"left:{F(Math.Min(_selectStartX, _selectCurrentX))}px;" +
        $"top:{F(Math.Min(_selectStartY, _selectCurrentY))}px;" +
        $"width:{F(Math.Abs(_selectCurrentX - _selectStartX))}px;" +
        $"height:{F(Math.Abs(_selectCurrentY - _selectStartY))}px;";
    
    #endregion
    
    // Zooming
    private double Zoom => Cam.Zoom;
    private double TargetZoom => Cam.TargetZoom;
    private bool Zooming => Cam.IsZooming;
    
    // Panning
    private bool _panning;
    private double _lastX, _lastY;
    
    // Fields
    private bool _isInteractingWithField;
    private List<Field> Fields => Editor.Fields;
    private List<Field> SelectedFields => Editor.SelectedFields;
    
    // Selecting
    private bool _isSelecting;
    private bool _isFadingSelection;
    private double _selectStartX, _selectStartY;
    private double _selectCurrentX, _selectCurrentY;
    
    // Editor
    private Editor.EditorMode _previousMode = Editor.EditorMode.Idle;
    public required RenderFragment Dialog { get; set; }

    private double _lastTimeStamp;
    
    #endregion

    
    #region Helper Methods
    
    private static string F(double v)
        => v.ToString(CultureInfo.InvariantCulture);
    
    private static (double x, double y) Snap((double x, double y) val) => (Snap(val.x), Snap(val.y));
    private static double Snap(double value) => Math.Round(value / BaseCellSize) * BaseCellSize;
    
    public static double SnapCeil(double value) => Math.Ceiling(value / BaseCellSize) * BaseCellSize;

    private static (double x, double y) SnapFloor((double x, double y) val) => (SnapFloor(val.x), SnapFloor(val.y));
    private static double SnapFloor(double value) => Math.Floor(value / BaseCellSize) * BaseCellSize;
    
    private void StartPan(PointerEventArgs e)
    {
        Editor.SetMode(Editor.EditorMode.Pan);
        _panning = true;
            
        _lastX = e.ClientX;
        _lastY = e.ClientY;
        Cam.ResetVelocity();
    }

    #endregion
    
    
    protected override void OnInitialized()
    {
        Editor.OnFieldClicked += () => _isInteractingWithField = true;
        Editor.OnDialogOpen += ResolveDialogView;
    }

    private void ResolveDialogView(Type dialogType, DialogParams? dialogParams)
    {
        Dialog = DialogManager.Resolve(dialogType, dialogParams)!;
    }
    
    
    #region Events
    
    private void OnPointerDown(PointerEventArgs e)
    {
        if (DialogManager.DialogOpen || _isInteractingWithField)
            return;

        if (e.Button == 0)
        {
            var (instantiateX, instantiateY) = SnapFloor(Cam.ScreenToWorld(e.ClientX, e.ClientY));
            
            switch (Editor.Mode)
            {
                case Editor.EditorMode.Pan:
                    _previousMode = Editor.EditorMode.Pan;
                    StartPan(e);
                    break;
                case Editor.EditorMode.CreateTextField:
                    Editor.CreateTextField(instantiateX, instantiateY);
                    break;
                case Editor.EditorMode.CreateMathField:
                    Editor.CreateMathField(instantiateX, instantiateY);
                    break;
                default:
                    Editor.DeselectAllFields();
                    
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
        if (DialogManager.DialogOpen)
            return;
        
        if (Editor.Mode == Editor.EditorMode.Pan && _panning)
        {
            var dx = e.ClientX - _lastX;
            var dy = e.ClientY - _lastY;
        
            Cam.PanX += dx;
            Cam.PanY += dy;
            Cam.SetVelocity(dx, dy);

            _lastX = e.ClientX;
            _lastY = e.ClientY;
        }

        // Selection Rect
        if (_isSelecting)
        {
            _selectCurrentX = e.ClientX;
            _selectCurrentY = e.ClientY;
            
            var rect = GetSelectionWorldRect();
            SelectElementsInside(rect);
            
            StateHasChanged();
        }

        foreach (var field in Editor.SelectedFields)
        {
            if (field.IsResizing)
            {
                var dx = (e.ClientX - field.ResizeStartMouseX) / Zoom;
                var dy = (e.ClientY - field.ResizeStartMouseY) / Zoom;

                var startX = field.ResizeStartPosX;
                var startY = field.ResizeStartPosY;
                
                var (newX, newY, newWidth, newHeight) = field.ResizeDir switch
                {
                    Field.ResizeDirection.TopLeft => 
                        (startX + dx, startY + dy, field.ResizeStartWidth - dx, field.ResizeStartHeight - dy),
                    Field.ResizeDirection.Top => 
                        (startX, startY + dy, field.Width, field.ResizeStartHeight - dy),
                    Field.ResizeDirection.TopRight => 
                        (startX, startY + dy, field.ResizeStartWidth + dx, field.ResizeStartHeight - dy),
                    Field.ResizeDirection.Right => 
                        (startX, startY, field.ResizeStartWidth + dx, field.Height),
                    Field.ResizeDirection.BottomRight => 
                        (startX, startY, field.ResizeStartWidth + dx, field.ResizeStartHeight + dy),
                    Field.ResizeDirection.Bottom => 
                        (startX, startY, field.Width, field.ResizeStartHeight + dy),
                    Field.ResizeDirection.BottomLeft => 
                        (startX + dx, startY, field.ResizeStartWidth - dx, field.ResizeStartHeight + dy),
                    // Left
                    _ => (startX + dx, startY, field.ResizeStartWidth - dx, field.Height)
                };

                if (!e.CtrlKey)
                {
                    (newWidth, newHeight) = Snap((newWidth, newHeight));
                    (newX, newY) = Snap((newX, newY));
                }
                
                if (field.Width > field.MinWidth)
                    field.PosX = newX;
                if (field.Height > field.MinHeight)
                    field.PosY = newY;
                field.Width = Math.Max(field.MinWidth, newWidth);
                field.Height = Math.Max(field.MinHeight, newHeight);
            }
            else if (field.IsDragging)
            {
                if (field.IsEditing)
                    break;
                
                var (worldX, worldY) = Cam.ScreenToWorld(e.ClientX, e.ClientY);
                worldX -= field.DragOffsetX;
                worldY -= field.DragOffsetY;
                
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

    private async Task OnPointerUp(PointerEventArgs e)
    {
        if (DialogManager.DialogOpen)
            return;
        
        if (Editor.Mode == Editor.EditorMode.Pan)
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

        List<Field> draggedFields = new();
        Field? resizingField = null;
        
        foreach (var field in SelectedFields)
        {
            if (field is { IsDragging: true, IsResizing: false } && (
                    Math.Abs(field.StartPosX - field.PosX) > 0.1 ||
                    Math.Abs(field.StartPosY - field.PosY) > 0.1)) 
            {
                draggedFields.Add(field);
            }
            else if(field.IsResizing)
                resizingField = field;
            
            field.IsDragging = false;
            field.IsResizing = false;
        }

        if (draggedFields.Count > 0)
        {
            var field = draggedFields[0];
            var dragOffset = (field.PosX - field.StartPosX, field.PosY - field.StartPosY);
            Console.WriteLine(field.StartPosX + ", " + field.StartPosY);
            EditorController.RegisterAction(new MoveFieldsAction(draggedFields.ToArray(), dragOffset));
        }

        if (resizingField != null)
        {
            EditorController.RegisterAction(new ScaleFieldAction(
                resizingField,
                ( resizingField.Width - resizingField.StartWidth, resizingField.Height - resizingField.StartHeight), 
                (resizingField.ResizeStartPosX, resizingField.ResizeStartPosY),
                (resizingField.PosX, resizingField.PosY)));
        }

        if (_isInteractingWithField)
            Editor.SaveCachedFile();

        _isInteractingWithField = false;
    }

    private void OnWheel(WheelEventArgs e)
    {
        if (DialogManager.DialogOpen)
            return;
        
        if (e.CtrlKey)
        {
            const double zoomSpeed = 0.002;
            var zoomFactor = 1 - e.DeltaY * zoomSpeed;
            Cam.TargetZoom = Math.Clamp(TargetZoom * zoomFactor, .5, 5);

            Cam.ZoomMouseX = e.ClientX;
            Cam.ZoomMouseY = e.ClientY;

            Cam.IsZooming = true;
        }
        else
        {
            const float scrollSpeed = 0.3f;
            Cam.VelX -= e.DeltaX * scrollSpeed;
            Cam.VelY -= e.DeltaY * scrollSpeed;
        }
    }
    
    private void OnKeyDown(KeyboardEventArgs e)
    {
        if (DialogManager.DialogOpen)
            return;
        
        if (Editor.SelectionCount > 0)
        {
            var step = e.CtrlKey ? 1 : BaseCellSize;
        
            foreach (var field in Editor.SelectedFields)
            {
                if (field.IsEditing)
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
    
    
    [JSInvokable]
    public async Task OnAnimationFrame(double timeStamp)
    {
        const double smooth = 0.15;
        const double speed = 12;
        
        if (_lastTimeStamp == 0)
        {
            _lastTimeStamp = timeStamp;
            return;
        }

        var deltaTime = (timeStamp - _lastTimeStamp) / 1000;
        _lastTimeStamp = timeStamp;

        if (Cam.IsMoving)
        {
            Cam.PanX += (Cam.TargetPanX - Cam.PanX) * speed * deltaTime;
            Cam.PanY += (Cam.TargetPanY - Cam.PanY) * speed * deltaTime;

            if (Math.Abs(Cam.PanX - Cam.TargetPanX) < 5 && Math.Abs(Cam.PanY - Cam.TargetPanY) < 5)
            {
                Cam.IsMoving = false;
                Console.WriteLine("asdf");                
            }
        }
        else if (Zooming)
        {
            var oldZoom = Zoom;

            Cam.Zoom = oldZoom + (TargetZoom - oldZoom) * smooth;

            if (Math.Abs(Zoom - TargetZoom) < 0.001f)
            {
                Cam.Zoom = TargetZoom;
                Cam.IsZooming = false;
            }
            
            Cam.ApplyZoomAtCursor(oldZoom, Zoom);
        }

        if (Editor.Mode != Editor.EditorMode.Pan && !_isInteractingWithField)
        {
            const double friction = 0.7f;

            Cam.PanX += Cam.VelX;
            Cam.PanY += Cam.VelY;

            Cam.VelX *= friction;
            Cam.VelY *= friction;

            if (Math.Abs(Cam.VelX) < 0.01) Cam.VelX = 0;
            if (Math.Abs(Cam.VelY) < 0.01) Cam.VelY = 0;
        }
        
        await InvokeAsync(StateHasChanged);
    }

    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var editorReference = DotNetObjectReference.Create(Editor);
            await JS.InvokeVoidAsync("mathEditor.startRenderLoop", DotNetObjectReference.Create(this));
            await JS.InvokeVoidAsync("keyboardActions.register", editorReference);
            await JS.InvokeVoidAsync("mathEditor.registerPasteHandler", editorReference);
        }
    }


    #region Selection Rect
    private (double x, double y, double w, double h) GetSelectionWorldRect()
    {
        var x1 = Math.Min(_selectStartX, _selectCurrentX);
        var y1 = Math.Min(_selectStartY, _selectCurrentY);
        var x2 = Math.Max(_selectStartX, _selectCurrentX);
        var y2 = Math.Max(_selectStartY, _selectCurrentY);

        var (worldX1, worldY1) = Cam.ScreenToWorld(x1, y1);
        var (worldX2, worldY2) = Cam.ScreenToWorld(x2, y2);
        
        return (worldX1, worldY1, worldX2 - worldX1, worldY2 - worldY1);
    }

    private void SelectElementsInside((double x, double y, double w, double h) rect)
    {
        var (x, y, w, h) = rect;

        foreach (var field in Fields)
        {
            var inside = 
                field.PosX >= x &&
                field.PosY >= y &&
                field.PosX + field.Width <= x + w &&
                field.PosY + field.Height <= y + h;
            
            if (inside)
                Editor.SelectField(field);
            else
                Editor.DeselectField(field);
        }
    }
    #endregion
}