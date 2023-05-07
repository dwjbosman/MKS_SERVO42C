using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

using Avalonia.Data;
using System;
using System.Windows.Input;
using ReactiveUI;

using System.Reactive;
//using System.Reactive.Linq;

//using System.ComponentModel;


namespace Vice;

public class DialogControl : TemplatedControl
{

    public DialogControl()
    {
        //InitializeComponent();

    

        //ContentProperty.Changed.Subscribe(ContentChanged);
        //ResultProperty.Changed.Subscribe(ResultChanged);

        this.PropertyChanged += new EventHandler<AvaloniaPropertyChangedEventArgs>(
            OnPropertyChanged
        );
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);

        
        StackPanel buttons = e.NameScope.Find<StackPanel>("DialogButtons");

        Button b1 = new Button();
        b1.Content = "Ok";
        b1.HorizontalAlignment=Avalonia.Layout.HorizontalAlignment.Center;
        Button b2 = new Button();
        b2.Content = "Cancel";
        b2.HorizontalAlignment=Avalonia.Layout.HorizontalAlignment.Center;

        buttons.Children.Add(b1);
        buttons.Children.Add(b2);

    }

   public static readonly StyledProperty<object?> ContentProperty =
    AvaloniaProperty.Register<DialogControl, object?>(nameof(Content),defaultBindingMode: BindingMode.OneWay);


    public object? Content
    {
        get { 
            return GetValue(ContentProperty); 
            }
        set { 
            SetValue(ContentProperty, value);
         }
      
    }

    private void ContentChanged(AvaloniaPropertyChangedEventArgs e) {
            BindingValue<object?> nv = e.NewValue;
            if (nv.Value !=null) {
                    ShowOverlay();
                    ZIndex = 1001;
                    Result = null;
            } else {
                    HideOverlay();
                    ZIndex = -101;
            }
                
    }

  protected void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs change) {
    if (change.Property == ContentProperty) {
        ContentChanged(change);
    } else if (change.Property == ResultProperty) {
        ResultChanged(change);
    }

            //ContentProperty.Changed.Subscribe(ContentChanged);
        //ResultProperty.Changed.Subscribe(ResultChanged);

  }

   public static readonly StyledProperty<object?> OverlayProperty =
    AvaloniaProperty.Register<DialogControl, object?>(nameof(Overlay),defaultBindingMode: BindingMode.OneWay);


    public object? Overlay
    {
        get { 
            return GetValue(OverlayProperty); 
            }
        set { 
            SetValue(OverlayProperty, value);
         }
      
    }

   public static readonly StyledProperty<object?> ResultProperty =
    AvaloniaProperty.Register<DialogControl, object?>(nameof(Result),defaultBindingMode: BindingMode.TwoWay);


    public object? Result
    {
        get { 
            return GetValue(ResultProperty); 
            }
        set { 
            SetValue(ResultProperty, value);
         }
      
    }

    private void ResultChanged(AvaloniaPropertyChangedEventArgs e) {
        BindingValue<object?> nv = e.NewValue;
        if (nv.Value !=null) {
            HideOverlay();
            ZIndex = -101;

            if(Closed!=null) {
                Closed.Execute(Result);
            }
        }
    }

   public static readonly StyledProperty<ICommand?> ClosedProperty =
    AvaloniaProperty.Register<DialogControl, ICommand?>(nameof(Closed),defaultBindingMode: BindingMode.OneWay);


    public ICommand? Closed
    {
        get { 
            return GetValue(ClosedProperty); 
            }
        set { 
            SetValue(ClosedProperty, value);
         }
      
    }

    public void HideOverlay() {
            if (Overlay!=null) {
                ((IControl)Overlay).ZIndex = -100;
            }
    }

    public void ShowOverlay() {
            if (Overlay!=null) {
                ((IControl)Overlay).ZIndex = 1000;
            }
    }

    public void Close() {
        Result = true;

    }
}