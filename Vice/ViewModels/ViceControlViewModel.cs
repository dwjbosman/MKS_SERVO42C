namespace Vice.ViewModels;
using System;
using System.Collections.Generic;
using Vice.Models;
using Vice.Views;
using ReactiveUI;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia;
using Avalonia.Controls;
using System.Windows.Input;

    public class ViceControlViewModel : ViewModelBase
    {

        public ViceControl Control { get; private set; }

        public MainWindowViewModel MainViewModel { get; private set; }

        public ViceControlViewModel(MainWindowViewModel main, ViceControl control) {
            Control = control;
            MainViewModel = main;
            gotoValue = "Test";
            /**
            var obs2 = EndlessBarrageOfEmails().ToObservable();
            var obs = this.WhenAnyValue(x => x.Control.Position);
            var obsThrottled = obs.Throttle(TimeSpan.FromSeconds(2));
            **/
            

            this.WhenAnyValue(x => x.Control.Position)
            .Sample(TimeSpan.FromSeconds(1))
            .ObserveOn( RxApp.MainThreadScheduler )
            .Subscribe(value =>
            {
                Position = value;
            });
        }

        private int position = 90;
        public int Position {
            get => position;
            set => this.RaiseAndSetIfChanged(ref position, value);
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
            Control.Position = 10;
        }


        private ICommand? requestCloseDialogCommand = null;
        public ICommand? RequestCloseDialogCommand { 
                get => requestCloseDialogCommand; 
                set {
                        this.RaiseAndSetIfChanged(ref requestCloseDialogCommand, value);
                } 
        }

        public void CloseDialog() {
            if (RequestCloseDialogCommand != null) {
                RequestCloseDialogCommand.Execute(null);
            }
        }

    }
