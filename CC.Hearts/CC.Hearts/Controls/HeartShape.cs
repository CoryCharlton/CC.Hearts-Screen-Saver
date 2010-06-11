using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CC.Hearts.Controls
{
    public class HeartShape : FrameworkElement
    {
        #region Constructor
        public HeartShape()
        {
            _VisualCollection = new VisualCollection(this) {_HeartVisual};

            SetDefaultValues();
            CreateTransforms();
        }
        #endregion

        #region Private Constants
        public const int BaseMilliseconds = 50;
        public static readonly TimeSpan BaseTimeSpan = TimeSpan.FromMilliseconds(BaseMilliseconds);
        public static readonly Duration BaseDuration = new Duration(BaseTimeSpan);
        public const double MaximumGravity = 12.0;
        public const double MaximumRandomAngle = 35;
        public const double MinimumGravity = 6.0;
        public const double MinimumRandomAngle = 25;
        #endregion

        #region Private Fields
        private double _Gravity;
        private readonly HeartVisual _HeartVisual = new HeartVisual();
        private double _MaximumAngle;
        private double _MinimumAngle;
        private RotateTransform _Rotation;
        private ScaleTransform _Scale;
        private TransformGroup _TransformGroup;
        private readonly VisualCollection _VisualCollection;
        #endregion

        #region Protected Properties
        protected override int VisualChildrenCount
        {
            get { return _VisualCollection.Count; }
        }
        #endregion

        #region Public Properties
        public double Angle
        {
            get { return _Rotation.Angle; }
            set { _Rotation.Angle = value; }
        }

        public Brush Fill
        {
            get { return _HeartVisual.Fill; }
            set { _HeartVisual.Fill = value; }
        }

        public double Gravity
        {
            get { return _Gravity; }
            set
            {
                if (value > MaximumGravity)
                {
                    _Gravity = MaximumGravity;
                }
                else if (value < MinimumGravity)
                {
                    _Gravity = MinimumGravity;
                }
                else
                {
                    _Gravity = value;
                }
            }
        }

        public double Left
        {
            get { return (double)(GetValue(Canvas.LeftProperty)); }
            set { SetValue(Canvas.LeftProperty, value); }
        }

        public double MaximumAngle
        {
            get { return _MaximumAngle; }
            set
            {
                _MaximumAngle = (value < 0) ? value*-1 : value;
                _MinimumAngle = (value < 0) ? value : value*-1;
            }
        }

        public bool NegativeXVelocity { get; set; }

        public Brush Stroke
        {
            get { return _HeartVisual.Stroke; }
            set { _HeartVisual.Stroke = value; }
        }

        public double Top
        {
            get { return (double)(GetValue(Canvas.TopProperty)); }
            set { SetValue(Canvas.TopProperty, value); }
        }

        public double XVelocityRatio { get; set; }
        #endregion

        #region Private Methods
        private void AdjustScale()
        {
            _Scale.ScaleX = Width / HeartVisual.DefaultWidth;
            _Scale.ScaleY = Height / HeartVisual.DefaultHeight;
        }

        private void CreateTransforms()
        {
            _TransformGroup = new TransformGroup();
            _Rotation = new RotateTransform(0, HeartVisual.DefaultWidth / 2.0, HeartVisual.DefaultHeight / 2.0);
            _Scale = new ScaleTransform(Width / HeartVisual.DefaultWidth, Height / HeartVisual.DefaultHeight);

            _TransformGroup.Children.Add(_Rotation);
            _TransformGroup.Children.Add(_Scale);

            RenderTransform = _TransformGroup;
        }

        private void SetDefaultValues()
        {
            CacheMode = new BitmapCache() {EnableClearType = true, SnapsToDevicePixels = true};
            Gravity = Utilities.RandomNext((int)MinimumGravity, (int)(MaximumGravity + 1));
            Height = HeartVisual.DefaultHeight;
            Width = HeartVisual.DefaultWidth;

            const double randomAngleDelta = MaximumRandomAngle - MinimumRandomAngle;
            double gravityFactor = Gravity/MaximumGravity;

            MaximumAngle = (randomAngleDelta*gravityFactor) + MinimumRandomAngle;
            NegativeXVelocity = (Utilities.RandomNext(0, 2) == 1);
            XVelocityRatio = Utilities.RandomNext(25, 50)/100.0;
        }
        #endregion

        #region Protected Methods
        protected override Visual GetVisualChild(int index)
        {
            return _VisualCollection[index];
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            AdjustScale();

            base.OnRenderSizeChanged(sizeInfo);
        }
        #endregion

        #region Public Methods
        public bool IsReallyVisible(double height, double width)
        {
            if (Left < Width * -1)
            {
                return false;
            }

            if (Left > width)
            {
                return false;
            }

            if (Opacity < 0)
            {
                return false;
            }

            if (Top < Height * -1)
            {
                return false;
            }

            if (Top > height)
            {
                return false;
            }

            return true;
        }

        public void Reset()
        {
            SetDefaultValues();
        }

        public void Start()
        {
            DoubleAnimation rotationAnimation = new DoubleAnimation
                                                    {
                                                        AutoReverse = true,
                                                        Duration = new Duration(TimeSpan.FromMilliseconds((MaximumGravity - Gravity + MinimumGravity)*250)),
                                                        From = NegativeXVelocity ? _MaximumAngle : _MinimumAngle,
                                                        RepeatBehavior = RepeatBehavior.Forever,
                                                        To = NegativeXVelocity ? _MinimumAngle : _MaximumAngle,
                                                    };

            _Rotation.BeginAnimation(RotateTransform.AngleProperty, rotationAnimation);


            DoubleAnimation topAnimation = new DoubleAnimation
                                               {
                                                   AutoReverse = false,
                                                   By = Gravity,
                                                   Duration = BaseDuration,
                                                   From = Opacity,
                                                   IsAdditive = true,
                                                   IsCumulative = true,
                                                   RepeatBehavior = RepeatBehavior.Forever
                                               };

            BeginAnimation(Canvas.TopProperty, topAnimation);

            DoubleAnimation leftAnimation = new DoubleAnimation
                                                {
                                                    AutoReverse = false,
                                                    By = NegativeXVelocity ? Gravity*XVelocityRatio*-1 : Gravity*XVelocityRatio,
                                                    Duration = BaseDuration,
                                                    From = Left,
                                                    IsAdditive = true,
                                                    IsCumulative = true,
                                                    RepeatBehavior = RepeatBehavior.Forever
                                                };

            BeginAnimation(Canvas.LeftProperty, leftAnimation);

            DoubleAnimation opacityAnimation = new DoubleAnimation
                                                   {
                                                       AutoReverse = false,
                                                       By = (Gravity/1000)*-1,
                                                       Duration = BaseDuration,
                                                       From = Opacity,
                                                       IsCumulative = true,
                                                       RepeatBehavior = RepeatBehavior.Forever
                                                   };

            BeginAnimation(OpacityProperty, opacityAnimation);
        }

        public void Stop()
        {
            _Rotation.BeginAnimation(RotateTransform.AngleProperty, null);
            BeginAnimation(Canvas.TopProperty, null);
            BeginAnimation(Canvas.LeftProperty, null);
            BeginAnimation(OpacityProperty, null);
        }
        #endregion
    }
}

