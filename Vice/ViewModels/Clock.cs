namespace Vice.ViewModels;
using System;
using Vice.Models;
using ReactiveUI;
using Avalonia.Threading;

public class Clock : ReactiveObject
{
    private DispatcherTimer _disTimer = new DispatcherTimer();

    //public event PropertyChangedEventHandler? PropertyChanged;
    public EventHandler<DateTime> OnEveryHour = (s , e) => { };

    //public string CurrentTime { get; set; } = "Current Time";
    //public string NextDownloadCycle { get; set; } = "Downloading in: ";

    //private DateTime _nextCycle = DateTime.Now.AddHours(1);

    private string currentTime = "Current Time";
    public string CurrentTime {
        get => currentTime;
        set => this.RaiseAndSetIfChanged(ref currentTime, value);
    }

    public Clock()
    {
        _disTimer.Interval = TimeSpan.FromSeconds(1);
        _disTimer.Tick += DispatcherTimer_Tick;
        _disTimer.Start();
    }

    private void DispatcherTimer_Tick(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        /** if (now >= _nextCycle)
        {
            _nextCycle = DateTime.Now.AddHours(1);
            OnEveryHour(this, now);
        } 
        TimeSpan ts = _nextCycle - now;
        NextDownloadCycle = String.Format(
            "Next crawl session at {0}(in {1}m{2}s)",
            _nextCycle.ToString("HH:mm:ss"),
            Math.Round(ts.TotalMinutes) - 1,
            ts.Seconds
        ); **/
        CurrentTime = now.ToString("HH:mm:ss");
        //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentTime)));
        //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NextDownloadCycle)));
    }
}