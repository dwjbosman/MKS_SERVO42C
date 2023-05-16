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

    public CalibrateView()
    {
        InitializeComponent();

        //EventHandler<Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs>? evt = new EventHandler<Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs>(OnAttachedToLogicalTree);
        //AttachedToLogicalTree += evt;
        
        //evt = new EventHandler<Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs>(OnDetachedToLogicalTree);
        //DetachedFromLogicalTree += evt;
        
        //this.OnAttachedToLogicalTree();

    }
    


    protected override void OnAttachedToLogicalTree(Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);

        DialogControl? dc = GetDialogControl();
        CalibrateViewModel? vm = ((CalibrateViewModel?)this.DataContext);
        if ((vm != null) && (dc!=null)) {

            Button b0 = new Button();
            b0.Content = "Start";
            b0.Command =  ReactiveCommand.Create(() => { vm.Start(); });

            dc.addResultButton(b0);

            Button b1 = new Button();
            b1.Content = "Cancel";
            b1.Command =  ReactiveCommand.Create(() => { vm.CloseDialog(); });

            dc.addResultButton(b1);
            
        }

    }

}
