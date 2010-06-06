using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using CC.Utilities;

namespace CC.Hearts
{
    public partial class App 
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
#if DEBUG
            Settings.IsDebug = true;
#endif
            ArgumentParser argumentParser = new ArgumentParser(new[] { "/", "-" }, true, new[] { new Argument("c", ArgumentValue.Optional, true), new Argument("d", ArgumentValue.None, true), new Argument("p", ArgumentValue.Required, true), new Argument("s", ArgumentValue.None, true) });
            argumentParser.Parse(e.Args);
            ArgumentDictionary validArguments = argumentParser.ParsedArguments.GetValidArguments();

            if (validArguments.Contains("d"))
            {
                Debugger.Launch();
                Settings.IsDebug = true;
            }

            if (validArguments.Contains("c"))
            {
                OptionsWindow windowOptions = new OptionsWindow();
                windowOptions.Show();
            }
            else
            {
                IntPtr previewHandle = IntPtr.Zero;

                if (validArguments.Contains("p"))
                {
                    long tempLong;
                    if (long.TryParse(validArguments["p"].Value, out tempLong))
                    {
                        previewHandle = new IntPtr(tempLong);
                        Logging.LogMessage("Preview Handle: " + previewHandle);
                    }
                }

                if (previewHandle != IntPtr.Zero)
                {
                    MainWindow windowMain = new MainWindow(previewHandle);
                    windowMain.Show();
                }
                else
                {
                    // NOTE: Haven't tested this yet. Wondering about the CPU usage caused by multiple distinct renderings. Wondering if "mirroring" is a better option.
                    // NOTE: The above comment is obviously premature optimization...
                    foreach (Screen screen in Screen.AllScreens)
                    {
                        MainWindow windowMain = new MainWindow(screen.Primary)
                                           {
                                               WindowStartupLocation = WindowStartupLocation.Manual,
                                               Left = screen.WorkingArea.Left,
                                               Top = screen.WorkingArea.Top,
                                               Width = screen.WorkingArea.Width,
                                               Height = screen.WorkingArea.Height,
                                           };

                        windowMain.Show();
                        windowMain.WindowState = WindowState.Maximized;
                    }
                }
            }
        }
    }
}
