using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;
using CC.Hearts.Controls;
using CC.Hearts.Utilities;
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

            BindingOperations.SetBinding(_HeartsCanvas, HeartsCanvas.AnimationSpeedProperty, new Binding
                                                                                                 {
                                                                                                     Mode = BindingMode.OneWay,
                                                                                                     Path = new PropertyPath(Settings.AnimationSpeedProperty),
                                                                                                     Source = Settings.Instance
                                                                                                 });
            SetupScreenSaver();
            
            _HeartsCanvas.Start();
        }
        #endregion

        #region Private Fields
        private FramesRateMonitor _FrameRateMonitor;
        private readonly bool _IsPrimary;
        private Point _MousePosition;
        #endregion

        #region Private Event Handlers
        private void SettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "FramesPerSecond":
                    {
                        MessageBoxHelper.Show("Some of your settings will not take effect until you restart.", MessageBoxImage.Information);
                        break;
                    }
                case "ShowStatus":
                    {
                        EnableStatus(Settings.Instance.ShowStatus);
                        break;
                    }
            }
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
                if (_FrameRateMonitor == null)
                {
                    _FrameRateMonitor = new FramesRateMonitor(15);    
                }

                MultiBinding multiBinding = new MultiBinding {Mode = BindingMode.OneWay};
                multiBinding.Bindings.Add(new Binding
                                              {
                                                  Mode = BindingMode.OneWay,
                                                  Path = new PropertyPath(FramesRateMonitor.FramesPerSecondProperty),
                                                  Source = _FrameRateMonitor
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
                multiBinding.Bindings.Add(new Binding
                                              {
                                                  Path = new PropertyPath(Settings.AnimationSpeedProperty),
                                                  Source = Settings.Instance
                                              });
                multiBinding.StringFormat = Settings.IsPreview ? "{0:N} FPS - {1}% - {2}/{3} - {4:D}" : "{0:N} FPS - {1}% - {2}/{3} - {4}";
                
                BindingOperations.SetBinding(_TextBlockStatus, TextBlock.TextProperty, multiBinding);

                _FrameRateMonitor.Start();
                _TextBlockStatus.Visibility = Visibility.Visible;
            }
            else if (!enable)
            {
                BindingOperations.ClearBinding(_TextBlockStatus, TextBlock.TextProperty);
             
                if (_FrameRateMonitor != null)
                {
                    _FrameRateMonitor.Stop();
                }

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

                    if (Settings.Instance.ShowHelp)
                    {
                        _HeartsCanvas.ShowHelp(15);
                    }
                    
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
            _HeartsCanvas.HideHelp();

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
                        #region Animation Speed
                        case Key.OemPlus:
                            {
                                switch (Settings.Instance.AnimationSpeed)
                                {
                                    case HeartAnimationSpeed.Normal:
                                        {
                                            Settings.Instance.AnimationSpeed = HeartAnimationSpeed.Fast;
                                            break;
                                        }
                                    case HeartAnimationSpeed.Slow:
                                        {
                                            Settings.Instance.AnimationSpeed = HeartAnimationSpeed.Normal;
                                            break;
                                        }
                                }
                                break;
                            }
                        case Key.OemMinus:
                            {
                                switch (Settings.Instance.AnimationSpeed)
                                {
                                    case HeartAnimationSpeed.Fast:
                                        {
                                            Settings.Instance.AnimationSpeed = HeartAnimationSpeed.Normal;
                                            break;
                                        }
                                    case HeartAnimationSpeed.Normal:
                                        {
                                            Settings.Instance.AnimationSpeed = HeartAnimationSpeed.Slow;
                                            break;
                                        }
                                }
                                break;
                            }
                        #endregion

                        #region Heart Count
                        case Key.OemPeriod:
                            {
                                EnableStatus(true);
                                Settings.Instance.MaximumHearts += 5;
                                break;
                            }
                        case Key.OemComma:
                            {
                                EnableStatus(true);
                                Settings.Instance.MaximumHearts -= 5;
                                break;
                            }
                        #endregion

                        #region Heart Size
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
                        #endregion

                        #region Options
                        case Key.O:
                            {
                                ShowOptions();
                                break;
                            }
                        #endregion

                        #region Toggle Help
                        case Key.OemQuestion:
                            {
                                if (_HeartsCanvas.IsHelpOpen)
                                {
                                    _HeartsCanvas.HideHelp();
                                }
                                else
                                {
                                    _HeartsCanvas.ShowHelp();
                                }
                                break;
                            }
                        #endregion

                        #region Toggle Status
                        case Key.S:
                            {
                                Settings.Instance.ShowStatus = !Settings.Instance.ShowStatus;
                                break;
                            }
                        #endregion

                        #region * Ignored Keys *
                        case Key.LeftShift:
                        case Key.RightShift:
                            {
                                break;
                            }
                        #endregion

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
