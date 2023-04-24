namespace Vice.ViewModels;
using System;
using Vice.Models;
using ReactiveUI;

public class AboutViewModel : ViewModelBase {
        
        
        public MainWindowViewModel MainViewModel { get; set; }

        public AboutViewModel(MainWindowViewModel main) {
                MainViewModel = main;
        }

}