using ReactiveUI;
namespace Vice.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    ViewModelBase content;    


    public MainWindowViewModel() {
    	content = new ViceControlViewModel();
    }

    public ViewModelBase Content {
        get => content;
        private set => this.RaiseAndSetIfChanged(ref content, value);
    }
}
