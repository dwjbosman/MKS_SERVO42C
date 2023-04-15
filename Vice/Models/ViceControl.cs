namespace Vice.Models
{
    public class ViceControl
    {
        public int Position { get; set; }
        public string Status { get; set; }

        public ViceControl() { 
            Position = 50;
            Status = "Unknown";
        }
    }
}
