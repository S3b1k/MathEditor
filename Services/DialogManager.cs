using MathEditor.Components.DialogViews;
using Microsoft.AspNetCore.Components;

namespace MathEditor.Services;

public class DialogManager
{
    private static BaseDialogView? Dialog { get; set; }
    public static bool DialogOpen;

    private Dictionary<string, object> dialogParams;


    public static void OpenDialog()
    {
        DialogOpen = true;
    }

    public static void CloseDialog()
    {
        DialogOpen = false;
        Dialog = null;
    }
    
    
    public static RenderFragment? Resolve(Type? dialog, DialogParams? dialogParams)
    {
        if (dialog == null)
            return null;
        
        return builder =>
        {
            builder.OpenComponent(0, dialog);

            int i;
            for (i = 0; i < dialogParams?.Count; i++)
            {
                var parameters = dialogParams.ToArray();
                builder.AddAttribute(i + 1, parameters[i].Key, parameters[i].Value);
            }
            
            builder.AddComponentReferenceCapture(
                i + 2, c => Dialog = (BaseDialogView)c);
            builder.CloseComponent();
        };
    }
}

public class DialogParams() : Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);