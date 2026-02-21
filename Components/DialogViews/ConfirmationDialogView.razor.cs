using MathEditor.Services;
using Microsoft.AspNetCore.Components;

namespace MathEditor.Components.DialogViews;

public partial class ConfirmationDialogView : BaseDialogView
{
    [Parameter] public required string TitleText { get; set; }
    [Parameter] public required string Text { get; set; }
    [Parameter] public required Func<Task> OnConfirm { get; set; }


    private void Confirm()
    {
        OnConfirm.Invoke();
        DialogManager.CloseDialog();
    }
}