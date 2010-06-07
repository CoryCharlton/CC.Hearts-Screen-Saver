using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using CC.Hearts.Controls;
using Cursors=System.Windows.Input.Cursors;
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

            _Timer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 100), DispatcherPriority.Background, TimerTick, Dispatcher);
            _Timer.Start();
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
        private readonly DispatcherTimer _Timer;
        #endregion

        #region Private Event Handlers
        private void CompositionTargetRendering(object sender, EventArgs e)
        {
            if ((DateTime.Now - _FrameReset).TotalSeconds > 1)
            {
                _TextBlockFramesPerSecond.Text = "FPS: " + _FrameCount;
                _FrameCount = 0;
                _FrameReset = DateTime.Now;
            }
            else
            {
                _FrameCount++;                
            }
        }

        private void SecondaryScreenSaverClosed(object sender, EventArgs e)
        {
            Close();
        }

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
                    }
                }
                else
                {
                    _CanvasMain.Children.RemoveAt(i);
                }
            }

            int currentCount = _CanvasMain.Children.Count;

            if (currentCount < Settings.MaximumHearts)
            {
                int maxHearts = ((Settings.MaximumHearts - currentCount) / 20) + 1;

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
                }
            }
        }
        #endregion

        #region Private Methods
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

        private void CreateSecondaryScreenSavers()
        {
            if (Screen.AllScreens.Length > 1)
            {
                foreach (Screen screen in Screen.AllScreens)
                {
                    if (screen.Primary == false)
                    {
                        //NOTE: I'm not happy with the multi-monitor performace. FPS drops by ~50% regardless if I mirror or render seperately (rendering seperately seems slightly faster)
                        ScreenSaverWindow secondaryScreenSaver = new ScreenSaverWindow(false)
                        //SecondaryScreenSaverWindow secondaryScreenSaver = new SecondaryScreenSaverWindow()
                                                                              {
                                                                                  WindowStartupLocation = WindowStartupLocation.Manual,
                                                                                  Left = screen.WorkingArea.Left,
                                                                                  Top = screen.WorkingArea.Top,
                                                                                  Width = screen.WorkingArea.Width,
                                                                                  Height = screen.WorkingArea.Height,
                                                                              };
                        //secondaryScreenSaver.SetParent(this);
                        secondaryScreenSaver.Closed += SecondaryScreenSaverClosed;
                        secondaryScreenSaver.Show();
                        secondaryScreenSaver.WindowState = WindowState.Maximized;
                    }
                }
            }
        }

        private void SetupScreenSaver()
        {
            if (_IsPrimary && Settings.IsDebug)
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
