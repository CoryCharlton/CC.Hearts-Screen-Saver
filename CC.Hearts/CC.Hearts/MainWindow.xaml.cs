using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using CC.Hearts.Controls;

namespace CC.Hearts
{
    public partial class MainWindow
    {
        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = Settings.FramesPerSecond });

            _Timer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 100), DispatcherPriority.Background, TimerTick, Dispatcher);
            _Timer.Start();

            if (Settings.IsDebug)
            {
                Topmost = false;
            }
        }

        public MainWindow(bool primaryScreen): this()
        {
            if (primaryScreen && Settings.IsDebug)
            {
                CompositionTarget.Rendering += CompositionTargetRendering;
                _RowDebug.Height = new GridLength(22);                
            }
        }

        public MainWindow(IntPtr previewHandle) : this()
        {
            if (previewHandle != IntPtr.Zero)
            {
                // TODO: Need to handle preview handle, should be fun :P   
            }
        }
        #endregion

        #region Private Fields
        private Point _FirstMousePosition;
        private int _FrameCount;
        private DateTime _FrameReset;
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
        #endregion

        #region Protected Methods
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!Settings.IsDebug || e.Key == Key.Escape)
            {
                Close();
            }
            else if (e.KeyboardDevice.Modifiers == ModifierKeys.Alt && e.KeyboardDevice.IsKeyDown(Key.O))
            {
                OptionsWindow windowOptions = new OptionsWindow {Opacity = 0.85};
                windowOptions.ShowDialog();
            }

            base.OnKeyDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            Point currentPosition = e.GetPosition(this);
        
            if (_FirstMousePosition.X <= 0 && _FirstMousePosition.Y <= 0)
            {
                _FirstMousePosition = currentPosition;
            }

            if (Point.Subtract(_FirstMousePosition, currentPosition).Length > 20 && !Settings.IsDebug)
            {
                Close();
            }

            base.OnMouseMove(e);
        }
        #endregion
    }
}
