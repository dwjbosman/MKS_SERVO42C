using System;
using Vice.Models;
using ReactiveUI;
using System.Windows.Input;

namespace Vice.ViewModels;

public abstract class AbstractDialogViewModel : ViewModelBase {

        private object? result = null;
        public object? Result { 
                get => result; 
                set {
                        this.RaiseAndSetIfChanged(ref result, value);
                } 
        }
}