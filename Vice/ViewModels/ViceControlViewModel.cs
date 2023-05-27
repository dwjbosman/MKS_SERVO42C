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

using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;


    public class ViceControlViewModel : ViewModelBase
    {

        public ViceControl Control { get; private set; }

        public MainWindowViewModel MainViewModel { get; private set; }

        public ReactiveCommand<Unit, Unit> EnablePowerCommand { get; }
        public ReactiveCommand<Unit, Unit> DisablePowerCommand { get; }
        public ReactiveCommand<Unit, Unit> CalibrateCommand { get; }


        private readonly ObservableAsPropertyHelper<string> _stallDetectedMessage;
        public string StallDetectedMessage => _stallDetectedMessage.Value;


        private readonly ObservableAsPropertyHelper<string> _statusMessage;
        public string StatusMessage => _statusMessage.Value;


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

            _stallDetectedMessage = this.WhenAnyValue(x => x.Control.StallDetected)
            .Select(stalled => { 
                if (stalled) {
                    return "Found";
                } else {
                    return "-";
                }
            })
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.StallDetectedMessage);

            _statusMessage = this.WhenAnyValue(x => x.Control.Status)
            .Select(status => { return status.ToString("G");})
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.StatusMessage);


            IObservable<bool> ce = this.WhenAnyValue(x => x.Control.PowerEnabled).Select(x => Debug("disable",x));

            EnablePowerCommand = ReactiveCommand.Create(
                EnablePower, 
                canExecute : this.WhenAnyValue(x => x.Control.PowerEnabled).Select(x => !x).Select(x => Debug("enable", x)));
            DisablePowerCommand = ReactiveCommand.Create(
               DisablePower, 
               canExecute : ce);
            CalibrateCommand = ReactiveCommand.Create(
               StartCalibration, 
               canExecute : ce);
            
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


        public void StartCalibration() {
            this.MainViewModel.ShowCalibrationDialog();
        }

        private void EnablePower() {
            Console.WriteLine("Enable power");
            this.Control.PowerEnable = true;
        }

        private void DisablePower() {
            Console.WriteLine("Disable power");
            this.Control.PowerEnable = false;
        }
        private bool Debug(string msg, bool x) {
            Console.WriteLine("Debug: " + msg + ":" + x);
            return x;
        }

    }
