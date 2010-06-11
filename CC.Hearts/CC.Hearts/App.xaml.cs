using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using CC.Utilities;
using CC.Utilities.Interop;

namespace CC.Hearts
{
    public partial class App
    {
        #region Private Fields
        private HwndSource _HwndSource;  
        private ScreenSaverWindow _ScreenSaverWindow;
        #endregion

        #region Private Event Handlers
        protected override void OnStartup(StartupEventArgs e)
        {
            //base.OnStartup(e);

#if DEBUG
            Settings.IsDebug = true;
#endif
            Settings.Tier = (RenderCapability.Tier >> 16);

            ArgumentParser argumentParser = new ArgumentParser(new[] { "/", "-" }, true, new[] { new Argument("c", ArgumentValue.Optional, true), new Argument("d", ArgumentValue.None, true), new Argument("p", ArgumentValue.Required, true), new Argument("s", ArgumentValue.None, true) });
            argumentParser.Parse(e.Args);
            ArgumentDictionary validArguments = argumentParser.ParsedArguments.GetValidArguments();

            if (validArguments.Contains("d"))
            {
                //Debugger.Launch(); // NOTE: The only time I use /d is when I want to profile a Release build so launching the debugger is pointless
                Settings.IsDebug = true;
            }

            if (validArguments.Contains("c"))
            {
                OptionsWindow windowOptions = new OptionsWindow();
                windowOptions.Show();
            }
            else
            {
                Settings.PreviewHandle = IntPtr.Zero;

                if (validArguments.Contains("p"))
                {
                    long tempLong;
                    if (long.TryParse(validArguments["p"].Value, out tempLong))
                    {
                        Settings.PreviewHandle = new IntPtr(tempLong);
                    }
                }

                _ScreenSaverWindow = new ScreenSaverWindow(true);

                if (Settings.IsPreview)
                {
                    RECT parentRectangle = new RECT();
                    User32.GetClientRect(Settings.PreviewHandle, parentRectangle);

                    HwndSourceParameters hwndSourceParameters = new HwndSourceParameters
                                                                    {
                                                                        Height = parentRectangle.bottom - parentRectangle.top,
                                                                        Width = parentRectangle.right - parentRectangle.left,
                                                                        ParentWindow = Settings.PreviewHandle,
                                                                        PositionX = 0,
                                                                        PositionY = 0,
                                                                        WindowStyle = (int) (WS.VISIBLE | WS.CHILD | WS.CLIPCHILDREN)
                                                                    };

                    _ScreenSaverWindow.Height = hwndSourceParameters.Height;
                    _ScreenSaverWindow.Width = hwndSourceParameters.Width;
                    _ScreenSaverWindow.Visibility = Visibility.Hidden;

                    _HwndSource = new HwndSource(hwndSourceParameters) { RootVisual = _ScreenSaverWindow.LayoutRoot };
                    _HwndSource.Disposed += _HwndSource_Disposed;
                }
                else
                {
                    _ScreenSaverWindow.Show();
                }
            }
        }

        // ReSharper disable InconsistentNaming
        private void _HwndSource_Disposed(object sender, EventArgs e)
        // ReSharper restore InconsistentNaming
        {
            _ScreenSaverWindow.Close();
        }
        #endregion
    }
}
