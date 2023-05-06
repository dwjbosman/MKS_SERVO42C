using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.Collections;
using System.Threading;


namespace Vice;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static int Main(string[] args)
    {
	    Console.WriteLine("Starting");

    	var builder = BuildAvaloniaApp();
        if (((IList)args).Contains("--drm"))
        {
            // SilenceConsole();
            return builder.StartLinuxDrm(args,"/dev/dri/card1");
        }

        return builder.StartWithClassicDesktopLifetime(args);
    }

    private static void SilenceConsole()
    {
        new Thread(() =>
            {
                Console.CursorVisible = false;
                while (true)
                    Console.ReadKey(true);
            })
            { IsBackground = true }.Start();
    }


    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
}
