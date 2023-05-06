using Avalonia.Controls;
using Avalonia.LogicalTree;

namespace Vice.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }


    public void ShowOverlay() {
        Grid parentGrid = this.FindLogicalDescendantOfType<Grid>();
        Grid overlayGrid = parentGrid.FindControl<Grid>("OverlayGrid");
        overlayGrid.ZIndex = 1000;
    }

    public void HideOverlay() {
        Grid parentGrid = this.FindLogicalDescendantOfType<Grid>();
        Grid overlayGrid = parentGrid.FindControl<Grid>("OverlayGrid");
        overlayGrid.ZIndex = -2;
    }

}
