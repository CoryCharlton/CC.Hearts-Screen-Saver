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
using CC.Hearts.Controls;
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

            //_HelpCanvas = new HelpCanvas()
            //                  {
            //                      HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            //                      VerticalAlignment = VerticalAlignment.Center
            //                  };
            //_RowStatus.Height = new GridLength(0); // TODO: Look here...
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
        private int _FrameCount;
        private readonly List<double> _FrameHistory = new List<double>();
        private DateTime _FrameReset;
        //private readonly HelpCanvas _HelpCanvas;
        private readonly bool _IsPrimary;
        private bool _IsStatusVisible;
        private Point _MousePosition;
        #endregion

        #region Public Properties
        public double FramesPerSecond
        {
            get { return (double)GetValue(FramesPerSecondProperty); }
            set { SetValue(FramesPerSecondProperty, value); }
        }
        #endregion

        #region Private Event Handlers
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //_HeartsCanvas.ShowHelp(_IsPrimary && !Settings.IsPreview && Settings.Instance.ShowHelp);
        }
        #endregion

        #region Private Methods
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

                        secondaryScreenSaver.Show();
                        secondaryScreenSaver.Owner = this;
                        secondaryScreenSaver.WindowState = WindowState.Maximized;
                    }
                }
            }
        }

        private void EnableStatus(bool enable)
        {
            if (enable && !_TextBlockStatus.IsVisible)
            {
                MultiBinding multiBinding = new MultiBinding {Mode = BindingMode.OneWay};
                multiBinding.Bindings.Add(new Binding
                                              {
                                                  Mode = BindingMode.OneWay,
                                                  Path = new PropertyPath(FramesPerSecondProperty),
                                                  Source = this
                                              });
                multiBinding.Bindings.Add(new Binding
                                              {
                                                  Path = new PropertyPath(Settings.ScaleProperty),
                                                  Source = Settings.Instance
                                              });
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
                multiBinding.StringFormat = "{0:N} FPS - {1}% - {2}/{3}";
                
                BindingOperations.SetBinding(_TextBlockStatus, TextBlock.TextProperty, multiBinding);
                CompositionTarget.Rendering += CompositionTargetRendering;

                _FrameCount = 0;
                _FrameHistory.Clear();
                _FrameReset = DateTime.MinValue;
                _TextBlockStatus.Visibility = Visibility.Visible;
            }
            else if (!enable)
            {
                BindingOperations.ClearBinding(_TextBlockStatus, TextBlock.TextProperty);
                CompositionTarget.Rendering -= CompositionTargetRendering;
                _TextBlockStatus.Visibility = Visibility.Hidden;
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

                    _HeartsCanvas.ShowHelp(Settings.Instance.ShowHelp);
                    
                    CreateSecondaryScreenSavers();
                }
            }
            else
            {
                Topmost = false;
            }
        }

        private void ShowOptions()
        {
            _HeartsCanvas.ShowHelp(false);
            OptionsWindow optionsWindow = new OptionsWindow { ShowInTaskbar = false, Topmost = Topmost };
            optionsWindow.ShowDialog();

            _MousePosition = Mouse.GetPosition(this);

            foreach (ScreenSaverWindow screenSaverWindow in OwnedWindows.OfType<ScreenSaverWindow>())
            {
                screenSaverWindow._MousePosition = Mouse.GetPosition(screenSaverWindow);
            }
        }
        #endregion

        #region Protected Methods
        protected override void OnKeyDown(KeyEventArgs e)
        {
            ScreenSaverWindow screenSaverWindow;
            if (!_IsPrimary && Owner != null && (screenSaverWindow = Owner as ScreenSaverWindow) != null)
            {
                screenSaverWindow.OnKeyDown(e);
            }
            else
            {
                if (!Settings.IsPreview)
                {
                    switch (e.Key)
                    {
                        case Key.LeftShift:
                        case Key.RightShift:
                            {
                                break;
                            }
                        case Key.OemQuestion: 
                            {
                                _HeartsCanvas.ShowHelp(null);
                                break;
                            }
                        case Key.OemComma:
                            {
                                EnableStatus(true);
                                Settings.Instance.MaximumHearts -= 1;
                                break;
                            }
                        case Key.OemCloseBrackets:
                            {
                                EnableStatus(true);
                                Settings.Instance.Scale += 1;
                                break;
                            }
                        case Key.OemOpenBrackets:
                            {
                                EnableStatus(true);
                                Settings.Instance.Scale -= 1;
                                break;
                            }
                        case Key.OemPeriod:
                            {
                                EnableStatus(true);
                                Settings.Instance.MaximumHearts += 1;
                                break;
                            }
                        case Key.O:
                            {
                                ShowOptions();
                                break;
                            }
                        case Key.S:
                            {
                                Settings.Instance.ShowStatus = !Settings.Instance.ShowStatus;
                                break;
                            }
                        default:
                            {
                                if (!Settings.IsDebug || e.Key == Key.Escape)
                                {
                                    Application.Current.Shutdown();
                                }
                                break;
                            }
                    }
                }
            }

            base.OnKeyDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!Settings.IsPreview && !Settings.IsDebug)
            {
                Point currentPosition = e.GetPosition(this);

                if (_MousePosition.X <= 0 && _MousePosition.Y <= 0)
                {
                    _MousePosition = currentPosition;
                }

                if (Point.Subtract(_MousePosition, currentPosition).Length > 30)
                {
                    Application.Current.Shutdown();
                }
            }

            base.OnMouseMove(e);
        }
        #endregion
    }
}
