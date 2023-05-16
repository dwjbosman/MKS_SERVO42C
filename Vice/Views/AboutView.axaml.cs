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



public partial class AboutView : AbstractDialogContentsView
{

    public AboutView()
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

        // You can also validate the data going into the DataContext using the event args
        DialogControl? dc = GetDialogControl();
        AboutViewModel? vm = ((AboutViewModel?)this.DataContext);
        if ((vm != null) && (dc!=null)) {

            Button b1 = new Button();
            b1.Content = "Close";
            b1.Command =  ReactiveCommand.Create(() => { vm.CloseDialog(); });

            dc.addResultButton(b1);
                        
        }

    }

}
