using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using CC.Hearts.Utilities;

namespace CC.Hearts.Controls
{
    public class HeartsCanvas : Canvas
    {
        #region Constructor
        public HeartsCanvas()
        {
            _Timer = new DispatcherTimer(TimeSpan.FromMilliseconds(NormalsMilliseconds), DispatcherPriority.Background, TimerTick, Dispatcher) {IsEnabled = false};
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty AnimationSpeedProperty = DependencyProperty.Register("AnimationSpeed", typeof(HeartAnimationSpeed), typeof(HeartsCanvas), new PropertyMetadata(HeartAnimationSpeed.Normal, AnimationSpeedChanged));
        #endregion

        #region Private Constants
        private const int FastMilliseconds = 250;
        private const int NormalsMilliseconds = 400;
        private const int SlowMilliseconds = 640;
        #endregion

        #region Private Fields
        private readonly Queue<HeartShape> _Hearts = new Queue<HeartShape>();
        private HelpPopup _HelpPopup;
        private readonly DispatcherTimer _Timer;
        #endregion

        #region Public Properties
        public HeartAnimationSpeed AnimationSpeed
        {
            get { return (HeartAnimationSpeed)GetValue(AnimationSpeedProperty); }
            set { SetValue(AnimationSpeedProperty, value); }
        }

        public bool IsHelpOpen
        {
            get
            {
                bool returnValue = false;

                if (_HelpPopup != null)
                {
                    returnValue = _HelpPopup.IsOpen;
                }

                return returnValue;
            }
        }
        #endregion

        #region Private Methods
        private static void AnimationSpeedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HeartAnimationSpeed animationSpeed = (HeartAnimationSpeed) e.NewValue;
            HeartsCanvas heartsCanvas = d as HeartsCanvas;

            if (heartsCanvas != null)
            {
                switch (animationSpeed)
                {
                    case HeartAnimationSpeed.Fast:
                        {
                            heartsCanvas._Timer.Interval = TimeSpan.FromMilliseconds(FastMilliseconds); 
                            break;
                        }
                    case HeartAnimationSpeed.Normal:
                        {
                            heartsCanvas._Timer.Interval = TimeSpan.FromMilliseconds(NormalsMilliseconds);
                            break;
                        }
                    case HeartAnimationSpeed.Slow:
                        {
                            heartsCanvas._Timer.Interval = TimeSpan.FromMilliseconds(SlowMilliseconds); 
                            break;
                        }
                }
            }
        }
        
        private HeartShape CreateHeart(int minHeight, int maxHeight, int minWidth, int maxWidth)
        {
            CommonFunctions.FixMinMax(ref minHeight, ref maxHeight);
            CommonFunctions.FixMinMax(ref minWidth, ref maxWidth);

            HeartShape newHeart = new HeartShape
                                      {
                                          Fill = CommonFunctions.RandomGradientBrush<RadialGradientBrush>(),
                                          Stroke = CommonFunctions.RandomSolidColorBrush()
                                      };

            ResetHeart(newHeart, minHeight, maxHeight, minWidth, maxWidth);

            return newHeart;
        }

        private void CreatePopup()
        {
            _HelpPopup = new HelpPopup {PlacementTarget = this};
        }

        private void ResetHeart(HeartShape heart, int minHeight, int maxHeight, int minWidth, int maxWidth)
        {
            heart.AnimationSpeed = AnimationSpeed;
            heart.Height = CommonFunctions.RandomNext(minHeight, maxHeight);
            heart.Left = CommonFunctions.RandomNext(0, (int)ActualWidth);
            heart.Width = CommonFunctions.RandomNext(minWidth, maxWidth);
            heart.Top = heart.Height*-1;
            heart.Start();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (Parent != null)
            {
                double actualHeight = (double) Parent.GetValue(ActualHeightProperty);
                double actualWidth = (double) Parent.GetValue(ActualWidthProperty);

                for (int i = Children.Count - 1; i >= 0; i--)
                {
                    HeartShape currentHeart = Children[i] as HeartShape;

                    if (currentHeart != null)
                    {
                        if (!currentHeart.IsReallyVisible(actualHeight, actualWidth))
                        {
                            currentHeart.Stop();

                            _Hearts.Enqueue(currentHeart);

                            Children.RemoveAt(i);
                            Settings.Instance.DecreaseHeartCount();
                        }
                    }
                }

                int currentCount = Settings.Instance.HeartCount;

                if (currentCount < Settings.Instance.MaximumHearts)
                {
                    int maxHearts = ((Settings.Instance.MaximumHearts - currentCount)/10) + 1;

                    if (maxHearts == 1)
                    {
                        maxHearts = 2;
                    }

                    int minHeight = (int) (actualHeight*(Settings.Instance.Scale/150.0));
                    int maxHeight = minHeight*2;

                    int minWidth = (int) (actualWidth*(Settings.Instance.Scale/150.0));
                    int maxWidth = minWidth*2;

                    int heartsToCreate = CommonFunctions.RandomNext(1, maxHearts);

                    for (int i = 0; i < heartsToCreate; i++)
                    {
                        HeartShape newHeart;

                        if (_Hearts.Count > 0)
                        {
                            newHeart = _Hearts.Dequeue();
                            newHeart.Reset();
                            ResetHeart(newHeart, minHeight, maxHeight, minWidth, maxWidth);
                        }
                        else
                        {
                            newHeart = CreateHeart(minHeight, maxHeight, minWidth, maxWidth);
                        }

                        Children.Add(newHeart);
                        Settings.Instance.IncreaseHeartCount();
                    }
                }
            }
        }
        #endregion

        #region Public Methods
        public void HideHelp()
        {
            if (_HelpPopup != null)
            {
                _HelpPopup.Hide();
            }
        }

        public void ShowHelp()
        {
            ShowHelp(0);
        }
            
        public void ShowHelp(int closeSeconds)
        {
            if (_HelpPopup == null)
            {
                CreatePopup();
            }

            if (_HelpPopup != null)
            {
                _HelpPopup.Show(closeSeconds);
            }
        }

        public void Start()
        {
            _Timer.Start();
        }

        public void Stop()
        {
            _Timer.Stop();
        }
        #endregion
    }
}
