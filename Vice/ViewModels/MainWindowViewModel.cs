using ReactiveUI;
using Vice.Models;
namespace Vice.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    ViewModelBase content;    

    ViceControlViewModel viceControlViewModel;
    AboutViewModel aboutViewModel;

    public MainWindowViewModel(ViceControl model) {
    	content = viceControlViewModel = new ViceControlViewModel(this, model);
        aboutViewModel = new AboutViewModel(this);
    }

    public ViewModelBase Content {
        get => content;
        private set => this.RaiseAndSetIfChanged(ref content, value);
    }

    public void ShowViceControl() { 
        Content = viceControlViewModel;
    }

    public void ShowAbout() {
        Content = aboutViewModel;
    }
}
