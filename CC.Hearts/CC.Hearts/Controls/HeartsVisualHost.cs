using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace CC.Hearts.Controls
{
    public class HeartsVisualHost : FrameworkElement
    {
        #region Constructor
        public HeartsVisualHost()
        {
            _Timer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 250), DispatcherPriority.Background, TimerTick, Dispatcher);
            _VisualChildren = new VisualCollection(this);
            _Timer.Start(); // TODO: Move this so you have to explicitly start the process...
        }
        #endregion

        #region Private Fields
        private readonly DispatcherTimer _Timer;
        private readonly VisualCollection _VisualChildren;
        #endregion

        #region Protected Properties
        protected override int VisualChildrenCount
        {
            get { return _VisualChildren.Count; }
        }
        #endregion

        #region Private Methods
        private HeartVisual CreateHeart(int minHeight, int maxHeight, int minWidth, int maxWidth)
        {
            Utilities.FixMinMax(ref minHeight, ref maxHeight);
            Utilities.FixMinMax(ref minWidth, ref maxWidth);

            HeartVisual newHeart = new HeartVisual(Utilities.RandomGradientBrush<RadialGradientBrush>(), Utilities.RandomGradientBrush<LinearGradientBrush>())
                                       {
                                           Height = Utilities.RandomNext(minHeight, maxHeight),
                                           Left = Utilities.RandomNext(0, (int)(double)Parent.GetValue(ActualWidthProperty)),
                                           Width = Utilities.RandomNext(minWidth, maxWidth),
                                       };

            newHeart.Top = newHeart.Height*-1;
            newHeart.Start();

            return newHeart;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            //double actualHeight = ActualHeight;
            //double actualWidth = ActualWidth;

            double actualHeight = (double)Parent.GetValue(ActualHeightProperty);
            double actualWidth = (double)Parent.GetValue(ActualWidthProperty);

            for (int i = _VisualChildren.Count - 1; i >= 0; i--)
            {
                HeartVisual currentHeart = _VisualChildren[i] as HeartVisual;
                
                if (currentHeart != null)
                {
                    if (!currentHeart.IsReallyVisible(actualHeight, actualWidth))
                    {
                        _VisualChildren.RemoveAt(i);

                        Settings.DecreaseHeartCount();
                    }
                }
                else
                {
                    _VisualChildren.RemoveAt(i);
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
                    _VisualChildren.Add(CreateHeart(minHeight, maxHeight, minWidth, maxWidth));

                    Settings.IncreaseHeartCount();
                }
            }
        }
        #endregion

        #region Protected Methods
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _VisualChildren.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return _VisualChildren[index];
        }
        #endregion

        #region Public Methods
        #endregion
    }
}
