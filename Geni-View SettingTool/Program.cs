using Avalonia;
using System;

namespace Geni_View_SettingTool
{
    class Program
    {
        // Avalonia requires a dedicated [STAThread] Main for desktop targets
        [STAThread]
        public static void Main(string[] args)
        {
            App.BuildAvaloniaApp()
               .StartWithClassicDesktopLifetime(args);
        }
    }
}
