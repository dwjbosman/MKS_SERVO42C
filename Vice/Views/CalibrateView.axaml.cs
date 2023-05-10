using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using System;
using System.Windows.Input;
using ReactiveUI;

using System.Reactive;
using System.Reactive.Linq;
using Vice.ViewModels;


using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

//using System.Reactive.Linq;

//using System.ComponentModel;

namespace Vice.Views;



public partial class CalibrateView : AbstractDialogContentsView
{
    private IDisposable? _bindingReferenceToResult = null;

    public CalibrateView()
    {
        InitializeComponent();

        //EventHandler<Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs>? evt = new EventHandler<Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs>(OnAttachedToLogicalTree);
        //AttachedToLogicalTree += evt;
        
        //evt = new EventHandler<Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs>(OnDetachedToLogicalTree);
        //DetachedFromLogicalTree += evt;
        
        //this.OnAttachedToLogicalTree();

    }
    
    protected override void OnDetachedFromLogicalTree(Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e) {
        base.OnDetachedFromLogicalTree(e);
        if (_bindingReferenceToResult!=null) {
            _bindingReferenceToResult.Dispose();
        }
    }

    protected override void OnAttachedToLogicalTree(Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);

        // You can also validate the data going into the DataContext using the event args
        DialogControl? dc = GetDialogControl();
        CalibrateViewModel? vm = ((CalibrateViewModel?)this.DataContext);
        if ((vm != null) && (dc!=null)) {

            Button b1 = new Button();
            b1.Content = "Cancel";
            b1.Command =  ReactiveCommand.Create(() => { vm.CloseDialog(); });

            dc.addResultButton(b1);

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
