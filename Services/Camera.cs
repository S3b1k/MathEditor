using MathEditor.Models;

namespace MathEditor.Services;

public class Camera
{
    public double PanX { get; set; }
    public double PanY { get; set; }
    
    public bool IsMoving { get; set; }
    public double TargetPanX { get; set; }
    public double TargetPanY { get; set; }
    
    public double Zoom { get; set; } = 0.5;
    public double TargetZoom { get; set; } = 0.5;
    public bool IsZooming { get; set; }
    public double ZoomMouseX { get; set; }
    public double ZoomMouseY { get; set; }
    
    public double VelX { get; set; }
    public double VelY { get; set; }


    public void GoToWorldPoint((double x, double y) pos, double screenWidth, double screenHeight)
    {
        var cx = screenWidth / 2;
        var cy = screenHeight / 2;
        
        TargetPanX = cx - pos.x * Zoom;
        TargetPanY = cy - pos.y * Zoom;
        IsMoving = true;
    }
    
    
    public (double worldX, double worldY) ScreenToWorld(double screenX, double screenY) 
        => ((screenX - PanX) / Zoom, (screenY - PanY) / Zoom);

    public (double screenX, double screenY) WorldToScreen(double worldX, double worldY)
        => (worldX * Zoom + PanX, worldY * Zoom + PanY);
    
    public (double offsetX, double offsetY) ComputeDragOffset(Field field, double screenX, double screenY)
    {
        var (fx, fy) = WorldToScreen(field.PosX, field.PosY);
        return ((screenX - fx) / Zoom, (screenY - fy) / Zoom);
    }
    

    public void ApplyZoomAtCursor(double oldZoom, double newZoom)
    {
        var worldX = (ZoomMouseX - PanX) / oldZoom;
        var worldY = (ZoomMouseY - PanY) / oldZoom;

        PanX = ZoomMouseX - worldX * newZoom;
        PanY = ZoomMouseY - worldY * newZoom;
    }

    public void SetVelocity(double velX, double velY)
    {
        VelX = velX;
        VelY = velY;
    }
    public void ResetVelocity() => SetVelocity(0, 0);
}
