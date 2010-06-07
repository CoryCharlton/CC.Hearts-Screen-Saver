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

#if USEVISUAL
            _HeartsHost.HorizontalAlignment = HorizontalAlignment.Stretch;
            _HeartsHost.VerticalAlignment = VerticalAlignment.Stretch;
            _CanvasMain.Children.Add(_HeartsHost);
#else
            _Timer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 250), DispatcherPriority.Background, TimerTick, Dispatcher);
            _Timer.Start();
#endif
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

#if USEVISUAL
        private readonly HeartsVisualHost _HeartsHost = new HeartsVisualHost();
#else
        private readonly DispatcherTimer _Timer;
#endif
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

        private void SecondaryScreenSaverClosed(object sender, EventArgs e)
        {
            Close();
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

#if !USEVISUAL
        private void TimerTick(object sender, EventArgs e)
        {
            double actualHeight = ActualHeight;
            double actualWidth = ActualWidth;

            for (int i = _CanvasMain.Children.Count - 1; i >= 0; i--)
            {
                Heart currentHeart = _CanvasMain.Children[i] as Heart;

                if (currentHeart != null)
                {
                    if (!currentHeart.IsReallyVisible(actualHeight, actualWidth))
                    {
                        _CanvasMain.Children.RemoveAt(i);

                        Settings.DecreaseHeartCount();
                    }
                }
                else
                {
                    _CanvasMain.Children.RemoveAt(i);
                }
            }

            int currentCount = Settings.HeartCount;

            if (currentCount < Settings.MaximumHearts)
            {
                int maxHearts = ((Settings.MaximumHearts - currentCount) / 10) + 1;

                if (maxHearts == 1)
                {
                    maxHearts = 2;
                }

                int minHeight = (int)(actualHeight * (Settings.Scale / 150.0));
                int maxHeight = minHeight * 2;

                int minWidth = (int)(actualWidth * (Settings.Scale / 150.0));
                int maxWidth = minWidth * 2;

                int heartsToCreate = Utilities.RandomNext(1, maxHearts);

                for (int i = 0; i < heartsToCreate; i++)
                {
                    _CanvasMain.Children.Add(CreateHeart(minHeight, maxHeight, minWidth, maxWidth));

                    Settings.IncreaseHeartCount();
                }
            }
        }
#endif
        #endregion

        #region Private Methods
#if !USEVISUAL
        private Heart CreateHeart(int minHeight, int maxHeight, int minWidth, int maxWidth)
        {
            Utilities.FixMinMax(ref minHeight, ref maxHeight);
            Utilities.FixMinMax(ref minWidth, ref maxWidth);

            Heart newHeart = new Heart(Utilities.RandomGradientBrush<RadialGradientBrush>(), Utilities.RandomGradientBrush<LinearGradientBrush>())
                                 {
                                     Height = Utilities.RandomNext(minHeight, maxHeight),
                                     Left = Utilities.RandomNext(0, (int) ActualWidth),
                                     Width = Utilities.RandomNext(minWidth, maxWidth),
                                 };

            newHeart.Top = newHeart.Height * -1;
            newHeart.Start();

            return newHeart;
        }
#endif

        private void CreateSecondaryScreenSavers()
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

                        secondaryScreenSaver.Closed += SecondaryScreenSaverClosed;
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
                    Close();
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
                    Close();
                }
            }

            base.OnMouseMove(e);
        }
        #endregion
    }
}
