using System.Text.Json;
using Blazored.LocalStorage;
using MathEditor.Components.DialogViews;
using MathEditor.Models;
using MathEditor.Models.Actions;
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

    private static Camera _cam;
    private static ILocalStorageService? _localStorage;
    private static IJSRuntime? _js;
    
    private static JsonSerializerOptions _serializerOptions = new () { WriteIndented = true };
    
    
    #region State Handling
    public static event Action? OnStateChanged;
    public static EditorMode Mode
    {
        get;
        private set
        {
            field = value;
            NotifyStateChanged();
        }
    } = EditorMode.Idle;
    
    public static void SetMode(EditorMode mode) => Mode = mode;
    #endregion
    
    #region Field Properties
    public static List<Field> Fields { get; } = [];
    
    public static event Action? OnFieldClicked;
    public static List<Field> SelectedFields { get; } = [];
    public static int SelectionCount => SelectedFields.Count;
    #endregion
    
    public static BaseDialogView? Dialog { get; set; }
    public static event Action<Type, DialogParams?>? OnDialogOpen;

    public static ElementReference FileInput;
    
    public static bool IsDark { get; private set; }
    
    

    public Editor(Camera cam, IJSRuntime js, ILocalStorageService localStorage)
    {
        _cam = cam;
        _localStorage = localStorage;
        _js = js;

        RetrieveData();
    }
    

    #region Field Factory

    public static void CreateNewField(Field field) =>
        EditorController.ExecuteAction(new CreateFieldAction(field));
    
    public static void RegisterField(Field field, bool selectField = true, bool suppressModeSwitch = false)
    {
        Fields.Add(field);
        
        if (selectField)
            SelectField(field);
        
        if (!suppressModeSwitch)
            SetMode(EditorMode.Idle);
    }
    
    public static void CreateTextField(double posX, double posY) 
        => CreateNewField(new TextField(posX, posY));
    public static void CreateMathField(double posX, double posY) 
        => CreateNewField(new MathField(posX, posY));

    #endregion
 
    
    #region Field Handling
    
    public static void SelectField(Field field) => SelectField(field, true);
    public static void SelectField(Field field, bool shift)
    {
        NotifyFieldClicked();
        
        if (field.IsSelected) return;

        if (!shift && !field.IsSelected)
            DeselectAllFields();
        
        field.IsSelected = true;
        SelectedFields.Add(field);
    }

    public static void DeselectField(Field field)
    {
        if (!field.IsSelected) return;
        
        field.IsSelected = false;
        SelectedFields.Remove(field);

        if (field.IsEditing)
            SaveCachedFile();
        
        field.NotifyFieldDeselected();
    }
    public static void DeselectAllFields()
    {
        var saved = false;
        foreach (var field in SelectedFields)
        {
            if (field.IsEditing && !saved)
            {
                SaveCachedFile();
                saved = true;
            }
            
            field.IsSelected = false;
            field.NotifyFieldDeselected();            
        } 
        
        SelectedFields.Clear();
    }
    
    public static void DeleteField(Field field)
    {
        if (field.IsSelected)
            DeselectField(field);
        Fields.Remove(field);
    }


    public static void BeginFieldDrag(Field field, double mouseX, double mouseY)
    {
        foreach (var selected in SelectedFields)
        {
            var dragOffset = _cam.ComputeDragOffset(selected, mouseX, mouseY);
            selected.DragOffsetX = dragOffset.offsetX;
            selected.DragOffsetY = dragOffset.offsetY;
            
            selected.IsDragging = true;
        }
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


    public static async Task OpenNewTab() =>
        await _js!.InvokeAsync<IJSObjectReference>("window.open", "/", "_blank");
    
    
    public static async Task ClearGrid()
    {
        Fields.Clear();
        SelectedFields.Clear();
        
        SaveCachedFile();
        await StoreDataAsync("fileName", "");
        
        await _js!.InvokeVoidAsync("mathEditor.setTitle", "");
    }
    #endregion
    
    
    #region Editor Save/Load
    
    private static async void RetrieveData()
    {
        try
        {
            if (_localStorage == null) return;
            IsDark = await _localStorage.GetItemAsync<bool>("darkTheme");

            var file = await _localStorage.GetItemAsync<List<FieldSaveData>>("file");
            if (file is { Count: > 0 })
            {
                ReadSaveList(file);

                var fileName = await _localStorage.GetItemAsStringAsync("fileName");
                if(fileName != null && !string.IsNullOrEmpty(fileName))
                    await _js!.InvokeVoidAsync("mathEditor.setTitle", fileName);
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
        }
    }

    private static async void StoreData(string key, object value)
    {
        try
        {
            await StoreDataAsync(key, value);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
        }
    }
    public static async Task StoreDataAsync(string key, object value)
    {
        if (_localStorage == null) return;
        await _localStorage.SetItemAsync(key, value);
    }


    public static void SaveCachedFile() =>
        StoreData("file", GetSaveList());
    
    #endregion
    
    #region Saving & Loading

    private static List<FieldSaveData> GetSaveList() => 
        Fields.Select(f => f.ToSaveData()).ToList();

    private static void ReadSaveList(List<FieldSaveData> saveList)
    {
        foreach (var f in saveList)
        {
            Field field = f.Type switch
            {
                "text" => new TextField(f.PosX, f.PosY) { Text = f.Content },
                "math" => new MathField(f.PosX, f.PosY) { Latex = f.Content },
                _ => throw new Exception($"Unknown field type: {f.Type}")
            };

            field.Width = f.Width;
            field.Height = f.Height;

            RegisterField(field, selectField: false);
        }
    }
    
    public static string SerializeFields() => 
        JsonSerializer.Serialize(GetSaveList(), _serializerOptions);
    
    private static void DeserializeFields(string json) =>
        ReadSaveList(JsonSerializer.Deserialize<List<FieldSaveData>>(json)!);
    
    
    public static async Task SaveFile(string fileName)
    {
        if (_js == null) return;
        
        await _js.InvokeVoidAsync(
                "mathEditor.saveFile",
                $"{fileName}.mxe",
                SerializeFields()
            );

        await _js.InvokeVoidAsync("mathEditor.setTitle", fileName);
        StoreData("fileName", fileName);
    }

    public static async Task OpenFilePicker()
    {
        if (_js == null) return;
        await _js.InvokeVoidAsync("mathEditor.openFilePicker", FileInput);
    }
    
    public static async Task LoadFile(ChangeEventArgs e)
    {
        if (_js == null) return;
        
        await ClearGrid();
        
        var fileData = await _js.InvokeAsync<FileData>("mathEditor.readFile", FileInput);

        DeserializeFields(fileData.Content);
        
        SaveCachedFile();
        await StoreDataAsync("fileName", fileData.Name);
        await _js.InvokeVoidAsync("mathEditor.setTitle", fileData.Name);
    }
    
    #endregion
    
    
    #region dialogs
    
    public static void ShowDialog(Type dialogType, DialogParams? dialogParams = null)
    {
        OnDialogOpen?.Invoke(dialogType, dialogParams);
        DialogManager.OpenDialog();
    }
    
    
    public static void ShowSaveDialog()
    {
        var parameters = new DialogParams { ["OnSave"] = SaveFile };
        ShowDialog(typeof(SaveDialogView), parameters);
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
        await StoreDataAsync("darkTheme", IsDark);
    }
    
    #endregion
    
    
    
    [JSInvokable]
    public void OnKeypress(string key, bool ctrl, bool shift, bool alt)
    {
        if (DialogManager.DialogOpen)
        {
            if (key == "escape")
                DialogManager.CloseDialog();
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
                case "a":
                    if (ctrl)
                    {
                        foreach (var field in Fields)
                            SelectField(field);
                    }
                    break;
                case "o":
                    if (ctrl)
                    {
                        var parameters = new DialogParams
                        {
                            ["TitleText"] = "Warning",
                            ["Text"] = "Loading a file will clear the grid. Any unsaved " +
                                       "changes will not be recoverable. Do you want to continue?",
                            ["OnConfirm"] = OpenFilePicker
                        };
                        ShowDialog(typeof(ConfirmationDialogView), parameters);
                    }
                    break;
                case "s":
                    if (ctrl)
                        ShowSaveDialog();
                    break;
                case "z":
                    EditorController.Undo();
                    break;
                case "y":
                    EditorController.Redo();
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
                    SaveCachedFile();
                    break;
            }
        }
    }
    
    
    #region Event Notifications
    
    private static void NotifyStateChanged() => OnStateChanged?.Invoke();
    public static void NotifyFieldClicked() => OnFieldClicked?.Invoke();
    
    #endregion

    
    
    private class FileData
    {
        public string Content { get; set; } = "";
        public string Name { get; set; } = "";
    }
}