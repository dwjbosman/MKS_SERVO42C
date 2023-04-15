﻿using ReactiveUI;
namespace Vice.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public string Greeting => "Welcome to Avalonia!";
    ViewModelBase content;    


    public MainWindowViewModel() {
    	Content = Player = new TestViewModel();
    }

    public ViewModelBase Content {
        get => content;
        private set => this.RaiseAndSetIfChanged(ref content, value);
    }

    public TestViewModel Player { get; }
}
