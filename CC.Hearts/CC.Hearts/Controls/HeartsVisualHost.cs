using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

        public VisualCollection VisualChildren
        {
            get { return _VisualChildren; }
        }

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

            // NOTE: Grasping at straws here ;-)
            RenderOptions.SetBitmapScalingMode(newHeart, BitmapScalingMode.LowQuality);
            RenderOptions.SetCachingHint(newHeart, CachingHint.Cache);
            RenderOptions.SetEdgeMode(newHeart, EdgeMode.Aliased);

            return newHeart;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (Parent != null)
            {
                double actualHeight = (double) Parent.GetValue(ActualHeightProperty);
                double actualWidth = (double) Parent.GetValue(ActualWidthProperty);

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
                        _VisualChildren.Add(CreateHeart(minHeight, maxHeight, minWidth, maxWidth));

                        Settings.IncreaseHeartCount();
                    }
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

        protected override void OnRender(DrawingContext drawingContext)
        {
            foreach (Visual visualChild in _VisualChildren)
            {
                HeartVisual heartVisual = visualChild as HeartVisual;

                if (heartVisual != null)
                {
                    RenderTargetBitmap bmp = new RenderTargetBitmap((int)heartVisual.Width, (int)heartVisual.Height, 120, 96, PixelFormats.Pbgra32);
                    bmp.Render(heartVisual);
                    drawingContext.DrawImage(bmp, new Rect(0,0, heartVisual.Width, heartVisual.Height));
                }
            }
        }
        #endregion

        #region Public Methods
        public void Start()
        {
            _Timer.Start();
        }
        #endregion
    }
}
