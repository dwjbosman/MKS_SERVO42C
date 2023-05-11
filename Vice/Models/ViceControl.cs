using ReactiveUI;
using ServoControl;

namespace Vice.Models
{
    public class ViceControl : ReactiveObject
    {
        private IServoController servoController;
        private int position = 0;
        public string Status { get; set; }

        public ViceControl() { 
            Position = 30;
            Status = "Unknown";
        }


        public int Position {
            get => position;
            set => this.RaiseAndSetIfChanged(ref position, value);
        }
    }
}
