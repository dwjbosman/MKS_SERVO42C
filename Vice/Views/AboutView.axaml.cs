using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using System;
using System.Windows.Input;
using ReactiveUI;

using System.Reactive;
using System.Reactive.Linq;
using Vice.ViewModels;
namespace Vice.Views;

public partial class AboutView : AbstractDialogContentsView
{

    public AboutView()
    {
        InitializeComponent();

        EventHandler<Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs>? evt = new EventHandler<Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs>(UserControl1_DataContextChanged);
        AttachedToLogicalTree += evt;

    }

    

    void UserControl1_DataContextChanged(object? sender, Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e)
    {
        // You can also validate the data going into the DataContext using the event args
        DialogControl? dc = GetDialogControl();
        AboutViewModel? vm = ((AboutViewModel?)this.DataContext);
        if ((vm != null) && (dc!=null)) {
            /**
            var result = dc.GetObservable(DialogControl.ResultProperty).Skip(1);
            result.Subscribe(
                value => 
                vm.Result = value); //value => Console.WriteLine(value + " Changed"));
            **/

            DialogControl.ResultProperty.Changed.Subscribe(ResultChanged);

var binding = new Binding 
{ 
    Source = vm, 
    Path = nameof(vm.Result),
    Mode = BindingMode.TwoWay
}; 



    dc.Bind(DialogControl.ResultProperty, binding);

        }

    }

    private void ResultChanged(AvaloniaPropertyChangedEventArgs<object?> e) {
    
    }


}
