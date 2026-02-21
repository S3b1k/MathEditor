using Microsoft.AspNetCore.Components;

namespace MathEditor.Components.DialogViews;

public partial class BaseDialogView : ComponentBase
{
    [Parameter] public required string Title { get; set; }
    [Parameter] public string CloseBtn { get; set; } = "Ok";
    [Parameter] public RenderFragment? Body { get; set; }
    [Parameter] public RenderFragment? Buttons { get; set; }
}