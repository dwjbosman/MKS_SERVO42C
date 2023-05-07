using ReactiveUI;
using Vice.Models;
using Avalonia.Controls;
using Vice.Views;

namespace Vice.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    ViewModelBase content;    

    ViewModelBase? dialogContent;

    ViceControlViewModel viceControlViewModel;
    AboutViewModel aboutViewModel;
    Clock clock;

    Control mainView;

    public MainWindowViewModel(ViceControl model, Control view) {
    	content = viceControlViewModel = new ViceControlViewModel(this, model);
        dialogContent = null;
        aboutViewModel = new AboutViewModel(this);
        clock = new Clock();
        mainView = view;
    }

    public ViewModelBase Content {
        get => content;
        private set => this.RaiseAndSetIfChanged(ref content, value);
    }

    public ViewModelBase? DialogContent {
        get => dialogContent;
        private set => this.RaiseAndSetIfChanged(ref dialogContent, value);
    }

    public Clock Clock {
        get => clock;
    }

    public Control MainView {
        get => mainView;
    }



    public void ShowViceControl() { 
        ((MainWindow)MainView).HideOverlay();

    }

    public void ShowAbout() {
        DialogContent = null;
        //aboutViewModel.Result = null;
        DialogContent = aboutViewModel;
    }
    public void DialogClosed(object? result) {
        //DialogContent = null;     
    }


}
