using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Application = System.Windows.Application;
using Binding = System.Windows.Data.Binding;
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
        }

        public ScreenSaverWindow(bool primaryScreen): this()
        {
            _IsPrimary = primaryScreen;

            SetupScreenSaver();
            
            _HeartsCanvas.Start();
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty FramesPerSecondProperty = DependencyProperty.Register("FramesPerSecond", typeof(double), typeof(ScreenSaverWindow), new PropertyMetadata(0.00));
        #endregion

        #region Private Fields
        private Point _FirstMousePosition;
        private int _FrameCount;
        private DateTime _FrameReset;
        private readonly bool _IsPrimary;
        #endregion

        #region Public Properties
        public double FramesPerSecond
        {
            get { return (double)GetValue(FramesPerSecondProperty); }
            set { SetValue(FramesPerSecondProperty, value); }
        }
        #endregion

        #region Private Event Handlers
        private readonly List<double> _FrameHistory = new List<double>();

        private void CompositionTargetRendering(object sender, EventArgs e)
        {
            double totalSeconds = (DateTime.Now - _FrameReset).TotalSeconds;
            if (totalSeconds > 1)
            {
                double framesPerSecond = (_FrameCount / totalSeconds);
                _FrameHistory.Add(framesPerSecond);

                while (_FrameHistory.Count > 180)
                {
                    _FrameHistory.RemoveAt(0);
                }

                // ReSharper disable RedundantAssignment
                FramesPerSecond = _FrameHistory.Aggregate((totalValue, nextValue) => totalValue += nextValue) / _FrameHistory.Count;
                //_TextBlockHeartCount.Text = "(" + Settings.HeartCount + "/" + Settings.MaximumHearts + ")";

                _FrameCount = 0;
                _FrameReset = DateTime.Now;
            }
            else
            {
                _FrameCount++;
            }

        }

        private void SettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ShowStatus":
                    {
                        EnableStatus(Settings.Instance.ShowStatus);
                        break;
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

        private void EnableStatus(bool enable)
        {
            if (enable)
            {
                MultiBinding multiBinding = new MultiBinding();
                multiBinding.Bindings.Add(new Binding
                                      {
                                          Path = new PropertyPath(Settings.HeartCountProperty),
                                          Source = Settings.Instance
                                      });

                multiBinding.Bindings.Add(new Binding
                                      {
                                          Path = new PropertyPath(Settings.MaximumHeartsProperty),
                                          Source = Settings.Instance
                                      });
                multiBinding.StringFormat = "({0}/{1})";
                BindingOperations.SetBinding(_TextBlockHeartCount, TextBlock.TextProperty, multiBinding);
                BindingOperations.SetBinding(_TextBlockFramesPerSecond, TextBlock.TextProperty, new Binding
                                                                                                    {
                                                                                                        Path = new PropertyPath(FramesPerSecondProperty), 
                                                                                                        StringFormat = "{0:N} FPS", 
                                                                                                        Source = this
                                                                                                    });
                CompositionTarget.Rendering += CompositionTargetRendering;
                _FrameCount = 0;
                _FrameHistory.Clear();
                _FrameReset = DateTime.MinValue;
                _RowStatus.Height = new GridLength(22);
            }
            else
            {
                BindingOperations.ClearBinding(_TextBlockFramesPerSecond, TextBlock.TextProperty);
                BindingOperations.ClearBinding(_TextBlockHeartCount, TextBlock.TextProperty);
                CompositionTarget.Rendering -= CompositionTargetRendering;
                _RowStatus.Height = new GridLength(0);                                
            }
        }

        private void SetupScreenSaver()
        {
            EnableStatus(_IsPrimary && (Settings.IsDebug || Settings.Instance.ShowStatus));

            if (!Settings.IsDebug)
            {
                Cursor = Cursors.None;
            }

            if (!Settings.IsPreview)
            {
                Topmost = !Settings.IsDebug;

                if (_IsPrimary)
                {
                    Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = Settings.Instance.FramesPerSecond });
                    Settings.Instance.PropertyChanged += SettingsPropertyChanged;
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
