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

        public string Status { get; set; }

        private readonly ObservableAsPropertyHelper<bool> _powerEnabled;
        public bool PowerEnabled => _powerEnabled.Value;

        public ViceControl() {
            servoController = new ServoControllerSimulator();
            Position = 30;
            Status = "Idle";

            _powerEnabled = this.WhenAnyValue(x => x.PowerEnable)
            .Select(powerEnableValue => TrySetPowerEnable(powerEnableValue))
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.PowerEnabled);

        }

    private bool TrySetPowerEnable(bool powerEnableValue)
    {
                    servoController.PowerEnable(powerEnableValue); 
                    bool nw = false; 
                    servoController.GetPowerEnable(out nw);
                    return nw;
    }

 


        public void startCalibrate() {
            Status = "Calibrating";
            
            servoController.PowerEnable(true);

        }

        

    }
}
