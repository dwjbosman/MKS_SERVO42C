namespace Vice.ViewModels;
using System;
using Vice.Models;
using ReactiveUI;
using System.Windows.Input;

public class CalibrateViewModel : AbstractDialogViewModel {
        
        
        public MainWindowViewModel MainViewModel { get; set; }
        public ViceControl Control { get; set; }
        public ViceControlViewModel ControlViewModel { get; set; }
        

        public CalibrateViewModel(MainWindowViewModel main) {
                MainViewModel = main;
                ControlViewModel = MainViewModel.ViceControlViewModel; 
                Control = MainViewModel.ViceControlViewModel.Control;
        }


        public void CloseDialog() {
            /** if (RequestCloseDialogCommand != null) {
                RequestCloseDialogCommand.Execute(null);
            }**/
            Result = true;
        }

        public void Start() {
                Control.StartCalibration();
        }

        public void Stop() {
                Control.Stop();
        }

        public void Cancel() {
                CloseDialog();
        }

        public void Done() {
                CloseDialog();
        }

}