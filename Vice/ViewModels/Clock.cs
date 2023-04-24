namespace Vice.ViewModels;
using System;
using Vice.Models;
using ReactiveUI;
using Avalonia.Threading;

public class Clock : ReactiveObject
{
    private DispatcherTimer _disTimer = new DispatcherTimer();

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
        CurrentTime = now.ToString("HH:mm:ss");
    }
}