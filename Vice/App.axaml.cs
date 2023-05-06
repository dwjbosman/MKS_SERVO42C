using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Vice.ViewModels;
using Vice.Views;
using Vice.Models;

namespace Vice;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        ViceControl control = new ViceControl();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
            desktop.MainWindow.DataContext = new MainWindowViewModel(control,desktop.MainWindow);
            
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView) {
            singleView.MainView = new MainSingleView();
            singleView.MainView.DataContext = new MainWindowViewModel(control,singleView.MainView);
	}


        base.OnFrameworkInitializationCompleted();
    }
}
