using MathEditor.Components.FieldViews;
using MathEditor.Models;
using Microsoft.AspNetCore.Components;

namespace MathEditor.Components;

public static class FieldViewResolver
{
    public static RenderFragment Resolve(Field field) => field switch
    {
        TextField tf => builder =>
        {
            builder.OpenComponent(0, typeof(TextFieldView));
            builder.AddAttribute(1, "Field", tf);
            builder.CloseComponent();
        },
        MathField mf => builder =>
        {
            builder.OpenComponent(0, typeof(MathFieldView));
            builder.AddAttribute(1, "Field", mf);
            builder.CloseComponent();
        },
        ImageField imf => builder =>
        {
            builder.OpenComponent(0, typeof(ImageFieldView));
            builder.AddAttribute(1, "Field", imf);
            builder.CloseComponent();
        },

        _ => builder => { }
    };
}