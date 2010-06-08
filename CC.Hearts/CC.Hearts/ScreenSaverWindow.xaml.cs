using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using CC.Hearts.Controls;
using Application = System.Windows.Application;
using Cursors=System.Windows.Input.Cursors;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using KeyEventArgs=System.Windows.Input.KeyEventArgs;
using MouseEventArgs=System.Windows.Input.MouseEventArgs;

namespace CC.Hearts
{
    public partial class ScreenSaverWindow
    {
        #region Constructor
        public ScreenSaverWindow()
        {
            InitializeComponent();

            _HeartsVisualHost.Start();
        }

        public ScreenSaverWindow(bool primaryScreen): this()
        {
            _IsPrimary = primaryScreen;

            SetupScreenSaver();
        }
        #endregion

        #region Private Fields
        private Point _FirstMousePosition;
        private int _FrameCount;
        private DateTime _FrameReset;
        private readonly bool _IsPrimary;
        #endregion

        #region Private Event Handlers
        private readonly Collection<double> _FrameHistory = new Collection<double>();

        private void CompositionTargetRendering(object sender, EventArgs e)
        {
            double totalSeconds = (DateTime.Now - _FrameReset).TotalSeconds;
            if (totalSeconds > 1)
            {
                double framesPerSecond = (_FrameCount/totalSeconds);
                _FrameHistory.Add(framesPerSecond);

                while (_FrameHistory.Count > 180)
                {
                    _FrameHistory.RemoveAt(0);    
                }

                _TextBlockFramesPerSecond.Text = "FPS: " + framesPerSecond.ToString("F") + " (" + (_FrameHistory.Aggregate((totalValue, nextValue) => totalValue += nextValue) / _FrameHistory.Count).ToString("F") + " " + _FrameHistory.Count + ")";
                _FrameCount = 0;
                _FrameReset = DateTime.Now;
            }
            else
            {
                _FrameCount++;                
            }

            _TextBlockHeartCount.Text = "(" + Settings.HeartCount + "/" + Settings.MaximumHearts + ")";
        }

        private void SettingChanged(object sender, SettingChangedEventArgs e)
        {
            if (!e.IsLoadEvent)
            {
                switch (e.Setting)
                {
                    case Setting.FramesPerSecond:
                        {
                            //TODO: How can I undo this? From the documentation it looks like I can't since it's already in use...
                            //Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = Settings.FramesPerSecond });
                            break;
                        }
                    case Setting.ShowStatus:
                        {
                            if (Settings.ShowStatus)// || Settings.IsDebug)
                            {
                                CompositionTarget.Rendering -= CompositionTargetRendering;
                                CompositionTarget.Rendering += CompositionTargetRendering;
                                _RowDebug.Height = new GridLength(22);
                            }
                            else
                            {
                                CompositionTarget.Rendering -= CompositionTargetRendering;
                                _RowDebug.Height = new GridLength(0);                                
                            }

                            break;
                        }
                }
            }
        }
        #endregion

        #region Private Methods
        private static void CreateSecondaryScreenSavers()
        {
            if (Screen.AllScreens.Length > 1)
            {
                foreach (Screen screen in Screen.AllScreens)
                {
                    if (screen.Primary == false)
                    {
                        ScreenSaverWindow secondaryScreenSaver = new ScreenSaverWindow(false)
                                                                              {
                                                                                  WindowStartupLocation = WindowStartupLocation.Manual,
                                                                                  Left = screen.WorkingArea.Left,
                                                                                  Top = screen.WorkingArea.Top,
                                                                                  Width = screen.WorkingArea.Width,
                                                                                  Height = screen.WorkingArea.Height,
                                                                              };

                        secondaryScreenSaver.Show();
                        secondaryScreenSaver.WindowState = WindowState.Maximized;
                    }
                }
            }
        }

        private void SetupScreenSaver()
        {
            if (_IsPrimary && (Settings.IsDebug || Settings.ShowStatus))
            {
                CompositionTarget.Rendering += CompositionTargetRendering;
                _RowDebug.Height = new GridLength(22);
            }

            if (!Settings.IsDebug)
            {
                Cursor = Cursors.None;
            }

            if (!Settings.IsPreview)
            {
                Topmost = !Settings.IsDebug;

                if (_IsPrimary)
                {
                    Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = Settings.FramesPerSecond });
                    Settings.SettingChanged += SettingChanged;

                    Show();
                    WindowState = WindowState.Maximized;

                    CreateSecondaryScreenSavers();
                }
            }
            else
            {
                Topmost = false;
            }
        }
        #endregion

        #region Protected Methods
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!Settings.IsPreview)
            {
                if (!Settings.IsDebug || e.Key == Key.Escape)
                {
                    Application.Current.Shutdown();
                }
                else if (e.KeyboardDevice.Modifiers == ModifierKeys.Alt && e.KeyboardDevice.IsKeyDown(Key.O))
                {
                    OptionsWindow windowOptions = new OptionsWindow {Opacity = 0.85, ShowInTaskbar = false, Topmost = Topmost};
                    windowOptions.ShowDialog();
                }
            }

            base.OnKeyDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!Settings.IsDebug && !Settings.IsPreview)
            {
                Point currentPosition = e.GetPosition(this);

                if (_FirstMousePosition.X <= 0 && _FirstMousePosition.Y <= 0)
                {
                    _FirstMousePosition = currentPosition;
                }

                if (Point.Subtract(_FirstMousePosition, currentPosition).Length > 20)
                {
                    Application.Current.Shutdown();
                }
            }

            base.OnMouseMove(e);
        }
        #endregion
    }
}
