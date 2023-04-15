using ReactiveUI;
using Vice.Models;
namespace Vice.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    ViewModelBase content;    


    public MainWindowViewModel(ViceControl model) {
    	content = new ViceControlViewModel(model);
    }

    public ViewModelBase Content {
        get => content;
        private set => this.RaiseAndSetIfChanged(ref content, value);
    }
}
