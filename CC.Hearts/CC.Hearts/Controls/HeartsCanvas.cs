using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CC.Utilities;

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
        private readonly DispatcherTimer _Timer;
        #endregion

        #region Private Methods
        private Heart CreateHeart(int minHeight, int maxHeight, int minWidth, int maxWidth)
        {
            Utilities.FixMinMax(ref minHeight, ref maxHeight);
            Utilities.FixMinMax(ref minWidth, ref maxWidth);

            Heart newHeart = new Heart(Utilities.RandomGradientBrush<RadialGradientBrush>(), Utilities.RandomSolidColorBrush())
                                       {
                                           Height = Utilities.RandomNext(minHeight, maxHeight),
                                           Left = Utilities.RandomNext(0, (int) (double) Parent.GetValue(ActualWidthProperty)),
                                           Width = Utilities.RandomNext(minWidth, maxWidth),
                                       };

            newHeart.Top = newHeart.Height*-1;
            newHeart.Start();

            // NOTE: Grasping at straws here ;-)

            /*
            RenderOptions.SetBitmapScalingMode(newHeart, BitmapScalingMode.LowQuality);
            RenderOptions.SetCachingHint(newHeart, CachingHint.Cache);
            RenderOptions.SetEdgeMode(newHeart, EdgeMode.Aliased);
            */

            //RenderOptions.SetBitmapScalingMode(newHeart, BitmapScalingMode.HighQuality);

            return newHeart;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (Parent != null)
            {
                double actualHeight = (double) Parent.GetValue(ActualHeightProperty);
                double actualWidth = (double) Parent.GetValue(ActualWidthProperty);

                for (int i = Children.Count - 1; i >= 0; i--)
                {
                    Heart currentHeart = Children[i] as Heart;

                    if (currentHeart != null)
                    {
                        if (!currentHeart.IsReallyVisible(actualHeight, actualWidth))
                        {
                            //currentHeart.Stop(); // TODO: Implement
                            RenderOptions.SetCachingHint(currentHeart, CachingHint.Unspecified);

                            Children.RemoveAt(i);

                            Settings.DecreaseHeartCount();
                        }
                    }
                    else
                    {
                        Children.RemoveAt(i);
                    }
                }

                int currentCount = Settings.HeartCount;

                if (currentCount < Settings.MaximumHearts)
                {
                    int maxHearts = ((Settings.MaximumHearts - currentCount)/10) + 1;

                    if (maxHearts == 1)
                    {
                        maxHearts = 2;
                    }

                    int minHeight = (int) (actualHeight*(Settings.Scale/150.0));
                    int maxHeight = minHeight*2;

                    int minWidth = (int) (actualWidth*(Settings.Scale/150.0));
                    int maxWidth = minWidth*2;

                    int heartsToCreate = Utilities.RandomNext(1, maxHearts);

                    for (int i = 0; i < heartsToCreate; i++)
                    {
                        Children.Add(CreateHeart(minHeight, maxHeight, minWidth, maxWidth));
                        Settings.IncreaseHeartCount();
                    }
                }
            }
        }
        #endregion

        #region Public Methods
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
