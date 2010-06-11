using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace CC.Hearts.Controls
{
    public class HeartsCanvas : Canvas
    {
        #region Constructor
        public HeartsCanvas()
        {
            _Timer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 250), DispatcherPriority.Background, TimerTick, Dispatcher) {IsEnabled = false};
        }
        #endregion

        #region Private Fields
        private readonly Queue<HeartShape> _Hearts = new Queue<HeartShape>();
        private HelpPopup _HelpPopup;
        private readonly DispatcherTimer _Timer;
        #endregion

        #region Private Methods
        private HeartShape CreateHeart(int minHeight, int maxHeight, int minWidth, int maxWidth)
        {
            Utilities.FixMinMax(ref minHeight, ref maxHeight);
            Utilities.FixMinMax(ref minWidth, ref maxWidth);

            HeartShape newHeart = new HeartShape
                                      {
                                          Fill = Utilities.RandomGradientBrush<RadialGradientBrush>(), 
                                          Stroke = Utilities.RandomSolidColorBrush()
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
            heart.Height = Utilities.RandomNext(minHeight, maxHeight);
            heart.Left = Utilities.RandomNext(0, (int) ActualWidth);
            heart.Width = Utilities.RandomNext(minWidth, maxWidth);
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

                    int heartsToCreate = Utilities.RandomNext(1, maxHearts);

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
        public void ShowHelp(bool? showHelp)
        {
            //if (!Children.Contains(_HelpPopup))
            //{
            //    Children.Add(_HelpPopup);
            //}

            if (_HelpPopup == null)
            {
                CreatePopup();
            }

            if (_HelpPopup != null)
            {
                if (showHelp == null)
                {
                    _HelpPopup.IsOpen = !_HelpPopup.IsOpen;
                }
                else
                {
                    _HelpPopup.IsOpen = showHelp.GetValueOrDefault(false);
                }
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
