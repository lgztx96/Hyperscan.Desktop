﻿using Avalonia;
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
    }
    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            //.UsePlatformDetect()
            .UseWin32()
            .UseSkia()
            .With(new Win32PlatformOptions
            {
                RenderingMode = [
                    Win32RenderingMode.AngleEgl, 
                    //Win32RenderingMode.Vulkan, 
                    Win32RenderingMode.Software
                    ]
            })
            //.With(new X11PlatformOptions() 
            //{ 
            //    RenderingMode = [X11RenderingMode.Vulkan, X11RenderingMode.Glx, X11RenderingMode.Software] 
            //})
            .LogToTextWriter(File.CreateText("1.txt"), Avalonia.Logging.LogEventLevel.Verbose);

}
