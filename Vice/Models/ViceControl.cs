using ReactiveUI;
using ServoControl;
//using System.Reactive.Core;
//using System.Reactive.Interfaces;
using System.Reactive.Linq;
using System;

namespace Vice.Models
{
    public class ViceControl : ReactiveObject
    {
        private IServoController servoController;
        
        private bool _isCalibrated = false;
        private int _position = 0;
        
        public int Position {
            get => _position;
            set => this.RaiseAndSetIfChanged(ref _position, value);
        }

        private bool _powerEnable = false;
        
        public bool PowerEnable {
            get => _powerEnable;
            set => this.RaiseAndSetIfChanged(ref _powerEnable, value);
        }

        private string _status = "";
        public string Status { 
            get => _status; 
            set => this.RaiseAndSetIfChanged(ref _status, value);
        }

        private bool _stallDetected = false;
        
        public bool StallDetected {
            get => _stallDetected;
            private set => this.RaiseAndSetIfChanged(ref _stallDetected, value);
        }

        private readonly ObservableAsPropertyHelper<bool> _powerEnabled;
        public bool PowerEnabled => _powerEnabled.Value;



        private IDisposable _controlUpdater;

        public ViceControl() {
            servoController = new ServoControllerSimulator();
            Position = 30;
            Status = "Idle";

            _powerEnabled = this.WhenAnyValue(x => x.PowerEnable)
            .Select(powerEnableValue => TrySetPowerEnable(powerEnableValue))
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.PowerEnabled);

            _controlUpdater = Observable.Interval(TimeSpan.FromSeconds(0.250))
            .Subscribe(_ => {
                Position = servoController.absEncoderPosition;
                StallDetected = servoController.stallDetected;
            });




        }

    private bool TrySetPowerEnable(bool powerEnableValue)
    {
                    servoController.PowerEnable(powerEnableValue); 
                    bool nw = false; 
                    servoController.GetPowerEnable(out nw);

                    if (!nw) {
                        Status = "Powered off";
                    } else {
                        Status = "Idle";
                    }

                    return nw;
    }

 


        public void StartCalibration() {
            Status = "Calibrating";
            
            servoController.SetConstantSpeed(10, true);

        }

        

    }
}
