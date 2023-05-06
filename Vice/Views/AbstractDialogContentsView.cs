using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using System;
using System.Windows.Input;
using ReactiveUI;

using System.Reactive;
//using System.Reactive.Linq;

namespace Vice.Views;

public abstract class AbstractDialogContentsView : UserControl
{

    public AbstractDialogContentsView()
    {

        ResultProperty.Changed.Subscribe(ResultChanged);

    }


    public static readonly StyledProperty<object?> ResultProperty =
        AvaloniaProperty.Register<AboutView, object?>(nameof(Result),defaultBindingMode: BindingMode.OneWay);


    public object? Result
    {
        get { 
            return GetValue(ResultProperty); 
            }
        set { 
            SetValue(ResultProperty, value);
         }
      
    }

    private void ResultChanged(AvaloniaPropertyChangedEventArgs<object?> e) {
        BindingValue<object?> nv = e.NewValue;
        IControl? cur = this;
        while (cur != null) {
            //Console.WriteLine("Parent class:",cur.GetType());
            if (cur.GetType() == typeof(DialogControl)) {
                ((DialogControl)cur).Result = nv.Value;
                break;
            }
            cur = cur.Parent;
            if (cur == null) {
                break;
            }
        }

    }



}
