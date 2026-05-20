using Avalonia;
using System;
using System.IO;

namespace Hyperscan.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp()
           .StartWithClassicDesktopLifetime(args);

        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            Exception ex = (Exception)e.ExceptionObject;
            string logPath = Path.Combine(AppContext.BaseDirectory, "error.log");
            File.WriteAllText(logPath, $"Unhandled exception: {ex}");
        };
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
        .UseHarfBuzz()
        .UseWin32()
        .UseSkia();
}
