using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Vice.Views;

public partial class MainSingleView : UserControl
{
    public MainSingleView()
    {
        InitializeComponent();
    }

    public void ShowOverlay() {
        Grid OverlayGrid = this.FindControl<Grid>("OverlayGrid");
        OverlayGrid.ZIndex = 1000;
    }

    public void HideOverlay() {
        Grid OverlayGrid = this.FindControl<Grid>("OverlayGrid");
        OverlayGrid.ZIndex = -2;
    }



}

