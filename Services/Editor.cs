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
        Idle, Pan,
        CreateTextField,
        CreateMathField
    }

    private static Camera? _cam;
    private static ILocalStorageService? _localStorage;
    private static IJSRuntime? _js;
    
    private static readonly JsonSerializerOptions SerializerOptions = new () { WriteIndented = true };
    
    
    #region Editor Mode
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
    
    #region Fields
    public static List<Field> Fields { get; } = [];
    
    public static event Action? OnFieldClicked;
    public static List<Field> SelectedFields { get; } = [];
    public static int SelectionCount => SelectedFields.Count;
    #endregion
    
    // Variables
    public static readonly VariableStore Variables = new();
    
    #region Dialog
    public static BaseDialogView? Dialog { get; set; }
    public static event Action<Type, DialogParams?>? OnDialogOpen;
    
    public static ElementReference FileInput;
    #endregion
    
    // Dark Mode
    public static bool IsDark { get; private set; }
    
    
    public Editor(Camera cam, IJSRuntime js, ILocalStorageService localStorage)
    {
        _cam = cam;
        _localStorage = localStorage;
        _js = js;

        LoadSettings();
    }
    

    #region Field Factory

    /// <summary>
    /// Creates a new field on the canvas
    /// </summary>
    /// <param name="field">Field to register</param>
    /// <param name="deselectFields">Deselect all currently selected fields</param>
    /// <param name="suppressModeSwitch">Stay in current editor mode</param>
    public static void CreateNewField(Field field, bool deselectFields = false, bool suppressModeSwitch = true)
    {
        if (deselectFields) 
            DeselectAllFields();
        EditorController.ExecuteAction(new CreateFieldsAction(field, suppressModeSwitch));
    }

    /// <summary>
    /// Creates new fields on the canvas
    /// </summary>
    /// <param name="fields">Fields to register</param>
    /// <param name="suppressModeSwitch">Stay in current editor mode</param>
    /// <param name="selectFields">Select created fields</param>
    public static void CreateNewFields(Field[] fields, bool suppressModeSwitch = true, bool selectFields = true) =>
        EditorController.ExecuteAction(new CreateFieldsAction(fields, suppressModeSwitch, selectFields));
    
    /// <summary>
    /// Directly registers a field onto the canvas
    /// </summary>
    /// <param name="field">Field to register</param>
    /// <param name="selectField">Select the registered field</param>
    /// <param name="suppressModeSwitch">Stay in current editor mode</param>
    public static void RegisterField(Field field, bool selectField = true, bool suppressModeSwitch = false)
    {
        Fields.Add(field);
        
        if (selectField) 
            SelectField(field);
        
        if (!suppressModeSwitch)
            SetMode(EditorMode.Idle);
    }

    #endregion
 
    
    #region Field Handling
    
    public static void SelectField(Field field) => SelectField(field, true, false);
    public static void SelectField(Field field, bool shift, bool clicked)
    {
        if (clicked)
            NotifyFieldClicked();

        if (field.IsSelected)
        {
            if (shift && clicked)
                DeselectField(field);
            return;
        }

        if (!shift && !field.IsSelected)
            DeselectAllFields();
        
        field.IsSelected = true;
        SelectedFields.Add(field);
    }
    public static void SelectAllFields() =>
        Fields.ForEach(SelectField);
    
    public static void DeselectField(Field field)
    {
        if (!field.IsSelected) return;
        
        field.IsSelected = false;
        field.IsDragging = false;
        field.IsResizing = false;
        SelectedFields.Remove(field);

        if (field.IsEditing)
            SaveCanvas();
        
        field.NotifyFieldDeselected();
    }
    public static void DeselectAllFields()
    {
        var saved = false;
        foreach (var field in SelectedFields)
        {
            if (field.IsEditing && !saved)
            {
                SaveCanvas();
                saved = true;
            }
            
            field.IsSelected = false;
            field.IsDragging = false;
            field.IsResizing = false;
            field.NotifyFieldDeselected();            
        } 
        
        SelectedFields.Clear();
    }
    
    public static void DeleteField(Field field)
        => EditorController.ExecuteAction(new DeleteFieldsAction(field));

    public static void DeleteSelectedFields()
    {
        var fields = SelectedFields.Where(f => !f.IsEditing).ToArray();
        EditorController.ExecuteAction(new DeleteFieldsAction(fields));
    }

    public static void UnregisterField(Field field)
    {
        if (field.IsSelected)
            DeselectField(field);
        Fields.Remove(field);
        field.NotifyFieldDeleted();
    }


    public static void BeginFieldDrag(double mouseX, double mouseY)
    {
        foreach (var selected in SelectedFields)
        {
            var dragOffset = _cam!.ComputeDragOffset(selected, mouseX, mouseY);
            selected.DragOffsetX = dragOffset.offsetX;
            selected.DragOffsetY = dragOffset.offsetY;
            selected.StartPosX = selected.PosX;
            selected.StartPosY = selected.PosY;
            
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
        field.StartWidth = field.Width;
        field.StartHeight = field.Height;
        field.ResizeDir = dir;
    }


    public static async Task OpenNewTab() =>
        await _js!.InvokeAsync<IJSObjectReference>("window.open", "/", "_blank");
    
    
    public static async Task ClearCanvas()
    {
        Fields.Clear();
        SelectedFields.Clear();
        
        SaveCanvas();
        await StoreDataAsync("fileName", "");
        
        await _js!.InvokeVoidAsync("mathEditor.setTitle", "");
    }
    #endregion
    
    
    #region Editor Save/Load
    /// <summary> Loads saved settings from local storage </summary>
    private static async void LoadSettings()
    {
        try
        {
            if (_localStorage == null) return;
            IsDark = await _localStorage.GetItemAsync<bool>("darkTheme");

            var file = await _localStorage.GetItemAsync<List<FieldSaveData>>("file");
            if (file is { Count: > 0 })
            {
                LoadSaveData(file);

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

    /// <summary> Saves a setting into local storage </summary>
    private static async void SaveSetting(string key, object value)
    {
        try { await StoreDataAsync(key, value); }
        catch (Exception e) { Console.Error.WriteLine(e); }
    }
    public static async Task StoreDataAsync(string key, object value)
    {
        if (_localStorage == null) return;
        await _localStorage.SetItemAsync(key, value);
    }


    /// <summary> Saves the current canvas into local storage </summary>
    public static void SaveCanvas() =>
        SaveSetting("file", GetSaveList());
    
    #endregion
    
    #region Saving & Loading

    private static List<FieldSaveData> GetSaveList() => 
        Fields.Select(f => f.ToSaveData()).ToList();

    private static void LoadSaveData(List<FieldSaveData> saveList) =>
        LoadSaveData(saveList, f => RegisterField(f, selectField: false));

    private static void LoadSaveData(List<FieldSaveData> saveList, Action<Field> callback)
    {
        foreach (var f in saveList)
        {
            if (string.IsNullOrWhiteSpace(f.Content))
                continue;

            (double x, double y) pos = (f.PosX, f.PosY);
            Field field = f.Type switch
            {
                "text" => new TextField(pos.x, pos.y) { Text = f.Content },
                "math" => new MathField(pos.x, pos.y) { Latex = f.Content },
                "image" => new ImageField(pos.x, pos.y) { ImageSource = f.Content },
                _ => throw new Exception($"Unknown field type: {f.Type}")
            };

            field.Width = f.Width;
            field.Height = f.Height;

            callback.Invoke(field);
        }
    }
    
    public static string SerializeFields() => 
        JsonSerializer.Serialize(GetSaveList(), SerializerOptions);
    
    public static void DeserializeFields(string json) =>
        LoadSaveData(JsonSerializer.Deserialize<List<FieldSaveData>>(json)!);
    
    
    public static async Task SaveToFile(string fileName)
    {
        if (_js == null) return;
        
        await _js.InvokeVoidAsync(
                "mathEditor.saveFile",
                $"{fileName}.mxe",
                SerializeFields()
            );

        await _js.InvokeVoidAsync("mathEditor.setTitle", fileName);
        SaveSetting("fileName", fileName);
    }

    public static async Task OpenFilePicker()
    {
        if (_js == null) return;
        await _js.InvokeVoidAsync("mathEditor.openFilePicker", FileInput);
    }
    
    public static async Task LoadFromFile(ChangeEventArgs e)
    {
        if (_js == null) return;
        
        await ClearCanvas();
        
        var fileData = await _js.InvokeAsync<FileData>("mathEditor.readFile", FileInput);

        DeserializeFields(fileData.Content);
        
        SaveCanvas();
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
        var parameters = new DialogParams { ["OnSave"] = SaveToFile };
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
    public async void OnKeypress(string key, bool ctrl, bool shift, bool alt)
    {
        bool editingField = SelectedFields.Any(f => f.IsEditing); 
        if (editingField)
            SelectedFields.First(f => f.IsEditing).NotifyKeyPressed(key, [ctrl, shift, alt]);
        
        if (DialogManager.DialogOpen)
        {
            if (key == "escape")
                DialogManager.CloseDialog();
            return;
        }
        
        if (ctrl && !editingField)
        {
            switch (key)
            {
                case "a":
                    if (shift)
                        DeselectAllFields();
                    else
                        SelectAllFields();
                    return;
                case "z":
                    EditorController.Undo();
                    return;
                case "y":
                    EditorController.Redo();
                    return;
                case "s":
                    ShowSaveDialog();
                    return;
                case "o":
                    var parameters = new DialogParams
                    {
                        ["TitleText"] = "Warning",
                        ["Text"] = "Loading a file will clear the grid. Any unsaved " +
                                   "changes will not be recoverable. Do you want to continue?",
                        ["OnConfirm"] = OpenFilePicker
                    };
                    ShowDialog(typeof(ConfirmationDialogView), parameters);
                    return;
            }
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
                case "1":
                    MoveCam(0, 0);
                    return;
            }
        }
        else
        {
            switch (key)
            {
                case "escape":
                    if (SelectedFields.Any(f => f.IsEditing))
                        SelectedFields.First(f => f.IsEditing).StopEditing();
                    else
                        DeselectAllFields();
                    break;
                case "delete":
                    DeleteSelectedFields();
                    SaveCanvas();
                    break;
                case "c":
                    if (ctrl && !editingField)
                    {
                        var fields = SelectedFields.Select(f => f.ToSaveData()).ToList();
                        var copied = JsonSerializer.Serialize(fields, SerializerOptions);
                        await _js!.InvokeVoidAsync("clipBoard.copyToClipboard", copied);
                    }
                    break;
            }
        }
    }
    
    [JSInvokable]
    public void OnPaste(string content)
    {
        if (SelectedFields.Any(f => f.IsEditing))
            return;
        
        var center = _cam!.GetScreenCenter();
        
        if (content.StartsWith("data:image/"))
        {
            var selectedImage = SelectedFields.OfType<ImageField>().FirstOrDefault();
            if (selectedImage != null)
            {
                EditorController.ExecuteAction(
                    new ChangeFieldAction(selectedImage, selectedImage.ImageSource, content));
                return;
            }

            var field = Field.Create<ImageField>(center.x, center.y);
            field.ImageSource = content;
            field.PosX = center.x - field.Width / 2;
            field.PosY = center.y - field.Height / 2;
            return;
        }
        
        DeselectAllFields();
        
        try
        {
            var data = JsonSerializer.Deserialize<List<FieldSaveData>>(content);
            List<Field> fields = [];
            if (data != null)
                LoadSaveData(data, f => fields.Add(f));

            CreateNewFields(fields.ToArray());
        }
        catch (Exception)
        {
            var field = Field.Create<TextField>(0, 0);
            field.PosX = center.x - field.Width / 2;
            field.PosY = center.y - field.Height / 2;
        }
        finally { SaveCanvas(); }
    }

    private static void MoveCam(double posX, double posY) 
        => _cam!.MoveToWorldPoint((posX, posY));
    
    
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