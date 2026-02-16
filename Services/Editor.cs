using System.Text.Json;
using Blazored.LocalStorage;
using MathEditor.Components;
using MathEditor.Models;
using Microsoft.AspNetCore.Components;
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
    
    private static ILocalStorageService? _localStorage;
    private static IJSRuntime? _js;
    
    
    #region State Handling
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
    
    public void SetMode(EditorMode mode) => Mode = mode;
    #endregion
    
    
    #region Field Properties
    public List<Field> Fields { get; } = [];
    
    public event Action? OnFieldClicked;
    public List<Field> SelectedFields { get; } = [];
    public int SelectionCount => SelectedFields.Count;
    #endregion
    
    
    #region Save Dialog
    public static SaveFileDialog? SaveDialog { get; set; }

    public static bool IsDialogOpen => SaveDialog?.IsOpen ?? false;

    public static bool IsDark { get; private set; }
    #endregion
    

    public Editor(IJSRuntime js, ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
        _js = js;

        RetrieveData();
    }
    


    #region Field Factory

    private void CreateField(Field field, bool selectField = true, bool suppressModeSwitch = false)
    {
        Fields.Add(field);
        
        if (selectField)
            SelectField(field);
        
        if (!suppressModeSwitch)
            SetMode(EditorMode.Idle);
    }
    
    public void CreateTextField(double posX, double posY, bool selectField = true, bool suppressModeSwitch = false) 
        => CreateField(new TextField(posX, posY), selectField, suppressModeSwitch);
    public void CreateMathField(double posX, double posY, bool selectField = true, bool suppressModeSwitch = false) 
        => CreateField(new MathField(posX, posY), selectField, suppressModeSwitch);

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
    
    
    #region Editor Save/Load

    private static async void RetrieveData()
    {
        try
        {
            if (_localStorage == null) return;
            await ToggleTheme(await _localStorage.GetItemAsync<bool>("darkTheme"));
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
        }
    }

    private static async Task StoreData(string key, object value)
    {
        if (_localStorage == null) return;
        await _localStorage.SetItemAsync(key, value);
    }
    
    #endregion
    
    #region Saving & Loading
    
    public static void ShowSaveDialog() => 
        SaveDialog?.Open(/* Saved Filename */);   
    

    public static async Task SaveFile(string data)
    {
        if (_js == null) return;
        await _js.InvokeVoidAsync(
                "mathEditor.saveFile",
                $"{SaveDialog?.Filename ?? SaveFileDialog.DefaultFilename}.mxe",
                data
            );
    }

    public static async Task OpenFilePicker(ElementReference fileInput)
    {
        if (_js == null) return;
        await _js.InvokeVoidAsync("mathEditor.openFilePicker", fileInput);
    }
    
    public async Task LoadFile(ChangeEventArgs e, ElementReference fileInput)
    {
        if (_js == null) return;
        var content = await _js.InvokeAsync<string>("mathEditor.readFile", fileInput);
        DeserializeFields(content);
    }
    
    private void DeserializeFields(string json)
    {
        var list = JsonSerializer.Deserialize<List<FieldSaveData>>(json)!;

        foreach (var f in list)
        {
            Field field = f.Type switch
            {
                "text" => new TextField(f.PosX, f.PosY) { Text = f.Content },
                "math" => new MathField(f.PosX, f.PosY) { Latex = f.Content },
                _ => throw new Exception($"Unknown field type: {f.Type}")
            };

            field.Width = f.Width;
            field.Height = f.Height;

            CreateField(field);
        }
    }

    
    #endregion
    
    
    #region Theme Toggling

    public static async Task ToggleTheme(bool? val = null)
    {
        if (_js == null) return;

        if (val.HasValue) 
            IsDark = val.Value;
        else 
            IsDark = !IsDark;
        
        await _js.InvokeVoidAsync("mathEditor.toggleTheme", IsDark ? "dark" : "light");
        await StoreData("darkTheme", IsDark);
    }
    
    #endregion
    
    
    
    [JSInvokable]
    public void OnKeypress(string key, bool ctrl, bool shift, bool alt)
    {
        if (IsDialogOpen)
        {
            if (key == "escape")
                SaveDialog?.Close();
            return;
        }
        
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