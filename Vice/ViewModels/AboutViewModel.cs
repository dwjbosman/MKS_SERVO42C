namespace Vice.ViewModels;
using System;
using Vice.Models;
using ReactiveUI;
using System.Windows.Input;

public class AboutViewModel : AbstractDialogViewModel {
        
        
        public MainWindowViewModel MainViewModel { get; set; }

        public AboutViewModel(MainWindowViewModel main) {
                MainViewModel = main;
        }

        public void CloseDialog() {
            /** if (RequestCloseDialogCommand != null) {
                RequestCloseDialogCommand.Execute(null);
            }**/
            Result = true;

        }
}