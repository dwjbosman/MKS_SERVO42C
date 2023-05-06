namespace Vice.ViewModels;
using System;
using Vice.Models;
using ReactiveUI;
using System.Windows.Input;

public class AboutViewModel : ViewModelBase {
        
        
        public MainWindowViewModel MainViewModel { get; set; }

        public AboutViewModel(MainWindowViewModel main) {
                MainViewModel = main;
        }

        private object? result = null;
        public object? Result { 
                get => result; 
                set {
                        this.RaiseAndSetIfChanged(ref result, value);
                } 
        }

        public void CloseDialog() {
            /** if (RequestCloseDialogCommand != null) {
                RequestCloseDialogCommand.Execute(null);
            }**/
            Result = true;

        }
}