namespace Vice.ViewModels;
using System;
using Vice.Models;
    public class ViceControlViewModel : ViewModelBase
    {
        public ViceControl Control { get; set; }
        public ViceControlViewModel(ViceControl control) {
                Control = control;
        }
    }
