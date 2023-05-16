namespace Vice.ViewModels;
using System;
using Vice.Models;
using ReactiveUI;
using System.Windows.Input;

public class CalibrateViewModel : AbstractDialogViewModel {
        
        
        public MainWindowViewModel MainViewModel { get; set; }

        public CalibrateViewModel(MainWindowViewModel main) {
                MainViewModel = main;
        }


        public void CloseDialog() {
            /** if (RequestCloseDialogCommand != null) {
                RequestCloseDialogCommand.Execute(null);
            }**/
            Result = true;
        }

        public void Start() {
                MainViewModel.ViceControlViewModel.Control.StartCalibration();
        }
}