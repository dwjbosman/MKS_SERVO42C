using ReactiveUI;

namespace Vice.Models
{
    public class ViceControl : ReactiveObject
    {
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
