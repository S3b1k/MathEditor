using System.Text.Json;
using MathEditor.Models;
using MathEditor.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace MathEditor.Services;

public class Editor
{
    public enum EditorMode
    {
        Idle,
        Pan,
        CreateTextField,
        CreateMathField
    }
    
    
    public event Action? OnStateChanged;
    public EditorMode Mode
    {
        get;
        private set
        {
            field = value;
            NotifyStateChanged();
        }
    } = EditorMode.Idle;
    
    public List<Field> Fields { get; } = [];
    
    public event Action? OnFieldClicked;
    public List<Field> SelectedFields { get; } = [];
    public int SelectionCount => SelectedFields.Count;

    public static event Action? OnEditorSave;
    

    public void SetMode(EditorMode mode) => Mode = mode;
    
    
    #region Field Factory

    private void CreateField(Field field, bool suppressModeSwitch = false)
    {
        Fields.Add(field);
        SelectField(field);
        
        if (!suppressModeSwitch)
            SetMode(EditorMode.Idle);
    }
    
    public void CreateTextField(double posX, double posY, bool suppressModeSwitch = false) 
        => CreateField(new TextField(posX, posY), suppressModeSwitch);
    public void CreateMathField(double posX, double posY, bool suppressModeSwitch = false) 
        => CreateField(new MathField(posX, posY), suppressModeSwitch);

    #endregion
 
    
    #region Field Handling
    
    public void SelectField(Field field) => SelectField(field, true);
    public void SelectField(Field field, bool shift)
    {
        NotifyFieldClicked();
        
        if (field.IsSelected) return;

        if (!shift && !field.IsSelected)
            DeselectAllFields();
        
        field.IsSelected = true;
        SelectedFields.Add(field);
    }

    public void DeselectField(Field field)
    {
        if (!field.IsSelected) return;
        
        field.IsSelected = false;
        SelectedFields.Remove(field);
        
        field.NotifyFieldDeselected();
    }
    public void DeselectAllFields()
    {
        foreach (var field in SelectedFields)
        {
            field.IsSelected = false;
            field.NotifyFieldDeselected();            
        } 
        
        SelectedFields.Clear();
    }
    
    public void DeleteField(Field field)
    {
        if (field.IsSelected)
            DeselectField(field);
        Fields.Remove(field);
    }


    public static void BeginFieldDrag(Field field, (double x, double y) dragOffset)
    {
        field.IsDragging = true;
        field.DragOffsetX = dragOffset.x;
        field.DragOffsetY = dragOffset.y;
    }


    public static void BeginFieldResize(Field field, Field.ResizeDirection dir, (double x, double y) startPos)
    {
        field.IsResizing = true;
        field.ResizeStartWidth = field.Width;
        field.ResizeStartHeight = field.Height;
        field.ResizeStartMouseX = startPos.x;
        field.ResizeStartMouseY = startPos.y;
        field.ResizeStartPosX = field.PosX;
        field.ResizeStartPosY = field.PosY;
        field.ResizeDir = dir;
    }
    #endregion
    
    
    #region Saving & Loading

    public static void SaveFile() => OnEditorSave?.Invoke();
    public static async Task SaveFile(IJSRuntime js, string data)
    {
        await js.InvokeVoidAsync("mathEditor.saveFile", "New Document.mxe", data);
    }
    
    public static async Task LoadFile(ChangeEventArgs e)
    {
        Console.WriteLine("asdf");
    }

    
    #endregion
    
    
    
    [JSInvokable]
    public void OnKeypress(string key, bool ctrl, bool shift, bool alt)
    {
        if (SelectionCount == 0)
        {
            switch (key)
            {
                case "escape":
                    SetMode(EditorMode.Idle);
                    break;
                case "t":
                    SetMode(EditorMode.CreateTextField);
                    break;
                case "m":
                    SetMode(EditorMode.CreateMathField);
                    break;
            }
        }
        else
        {
            switch (key)
            {
                case "escape":
                    DeselectAllFields();
                    break;
                case "delete":
                    var selected = SelectedFields.ToArray();
                    foreach (var field in selected)
                    {
                        if (field.IsEditing)
                            continue;
                    
                        DeleteField(field);
                    }
                    break;
            }
        }
    }
    
    
    #region Event Notifications
    
    private void NotifyStateChanged() => OnStateChanged?.Invoke();
    public void NotifyFieldClicked() => OnFieldClicked?.Invoke();
    
    #endregion
}