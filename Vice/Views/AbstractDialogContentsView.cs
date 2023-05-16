using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using System;
using System.Windows.Input;
using ReactiveUI;

using System.Reactive;
//using System.Reactive.Linq;
using Vice.ViewModels;

namespace Vice.Views;

public abstract class AbstractDialogContentsView : UserControl
{

    private IDisposable? _bindingReferenceToResult = null;

    public AbstractDialogContentsView()
    {

        //ResultProperty.Changed.Subscribe(ResultChanged);

    }
    protected override void OnDetachedFromLogicalTree(Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e) {
        base.OnDetachedFromLogicalTree(e);
        if (_bindingReferenceToResult!=null) {
            _bindingReferenceToResult.Dispose();
        }
    }
/**
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
        DialogControl? d = GetDialogControl();
        if (d != null) {
            d.Result = nv.Value;
        }

    }
**/

    protected DialogControl? GetDialogControl() {
        IControl? cur = this;
        while (cur != null) {
            //Console.WriteLine("Parent class:",cur.GetType());
            if (cur.GetType() == typeof(DialogControl)) {
                return (DialogControl)cur;
            }
            cur = cur.Parent;
            if (cur == null) {
                break;
            }
        }
        return null;

    }

    protected override void OnAttachedToLogicalTree(Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);

        DialogControl? dc = GetDialogControl();
        AbstractDialogViewModel? vm = ((AbstractDialogViewModel?)this.DataContext);
        if ((vm != null) && (dc!=null)) {

            var binding = new Binding 
            { 
                Source = vm, 
                Path = nameof(vm.Result),
                Mode = BindingMode.TwoWay
            }; 
            
            if (_bindingReferenceToResult!=null) {
                _bindingReferenceToResult.Dispose();
            }

            _bindingReferenceToResult = dc.Bind(DialogControl.ResultProperty, binding);
            
        }

    }



}
