namespace Vice.ViewModels;
using System;
using Vice.Models;
using ReactiveUI;

    public class ViceControlViewModel : ViewModelBase
    {
        public ViceControl Control { get; private set; }

        public MainWindowViewModel MainViewModel { get; private set; }

        public ViceControlViewModel(MainWindowViewModel main, ViceControl control) {
                Control = control;
                MainViewModel = main;
                gotoValue = "Test";
        }

        private string gotoValue;
        public string GotoValue
        {
            get => gotoValue;
            set => this.RaiseAndSetIfChanged(ref gotoValue, value);
        }

        

        public void UpdateGotoValue()
        {
            GotoValue = $"updated me at {DateTime.Now}";
        }

        public void NumberClicked(string param) {
            //UpdateGotoValue();
            GotoValue = param;
        }
    }
