//#define USETRANSLATION //NOTE: Comment/uncomment this line to play with animation on a TranslateTransform VS Canvas.Left/Top. So far the canvas animation seems to use less resources

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CC.Hearts.Utilities;

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

        public HeartShape(HeartAnimationSpeed animationSpeed) : this()
        {
            AnimationSpeed = animationSpeed;
        }
        #endregion

        #region Private Constants

        public const int FastMilliseconds = 50;
        public const int FastOpacityModifier = 1000;
        public static readonly Duration FastDuration = new Duration(TimeSpan.FromMilliseconds(FastMilliseconds));

        public const int NormalMilliseconds = 100;
        public const int NormalOpacityModifier = 950;
        public static readonly Duration NormalDuration = new Duration(TimeSpan.FromMilliseconds(NormalMilliseconds));

        public const int SlowMilliseconds = 150;
        public const int SlowOpacityModifier = 900;
        public static readonly Duration SlowDuration = new Duration(TimeSpan.FromMilliseconds(SlowMilliseconds));
        
        public const double MaximumGravity = 12.0;
        public const double MaximumRandomAngle = 35;
        public const double MinimumGravity = 6.0;
        public const double MinimumRandomAngle = 25;
        #endregion

        #region Private Fields
        private HeartAnimationSpeed _AnimationSpeed;
        private Duration _Duration = NormalDuration;
        private double _Gravity;
        private readonly HeartVisual _HeartVisual = new HeartVisual();
        private double _MaximumAngle;
        private double _MinimumAngle;
        private double _OpacityModifier = NormalOpacityModifier;
        private RotateTransform _Rotation;
        private ScaleTransform _Scale;
        private TransformGroup _TransformGroup;
        private readonly VisualCollection _VisualCollection;

#if USETRANSLATION
        private TranslateTransform _Translation;
#endif
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

        public HeartAnimationSpeed AnimationSpeed
        {
            get { return _AnimationSpeed; }
            set
            {
                _AnimationSpeed = value;

                switch (_AnimationSpeed)
                {
                    case HeartAnimationSpeed.Fast:
                        {
                            _Duration = FastDuration;
                            _OpacityModifier = FastOpacityModifier;
                            break;
                        }
                    case HeartAnimationSpeed.Slow:
                        {
                            _Duration = SlowDuration;
                            _OpacityModifier = SlowOpacityModifier;
                            break;
                        }
                    default:
                        {
                            _Duration = NormalDuration;
                            _OpacityModifier = NormalOpacityModifier;
                            break;
                        }
                }

            }
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
#if USETRANSLATION
            _Translation = new TranslateTransform(0, 0);
#endif
            _TransformGroup.Children.Add(_Rotation);
            _TransformGroup.Children.Add(_Scale);
#if USETRANSLATION
            _TransformGroup.Children.Add(_Translation);
#endif
            RenderTransform = _TransformGroup;
        }

        private void SetDefaultValues()
        {
            CacheMode = new BitmapCache(0.25); // {EnableClearType = true, SnapsToDevicePixels = true}; // NOTE: Most of the time this gets scaled down so rendering the cache smaller seems to produce crisper images
            Gravity = CommonFunctions.RandomNext((int)MinimumGravity, (int)(MaximumGravity + 1));
            Height = HeartVisual.DefaultHeight;
            Width = HeartVisual.DefaultWidth;

            const double randomAngleDelta = MaximumRandomAngle - MinimumRandomAngle;
            double gravityFactor = Gravity/MaximumGravity;

            MaximumAngle = (randomAngleDelta*gravityFactor) + MinimumRandomAngle;
            NegativeXVelocity = (CommonFunctions.RandomNext(0, 2) == 1);
            XVelocityRatio = CommonFunctions.RandomNext(25, 50) / 100.0;
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
                                                   Duration = _Duration,
                                                   From = Opacity,
                                                   IsAdditive = true,
                                                   IsCumulative = true,
                                                   RepeatBehavior = RepeatBehavior.Forever
                                               };

#if USETRANSLATION
            _Translation.BeginAnimation(TranslateTransform.YProperty, topAnimation);
#else
            BeginAnimation(Canvas.TopProperty, topAnimation);
#endif

            DoubleAnimation leftAnimation = new DoubleAnimation
                                                {
                                                    AutoReverse = false,
                                                    By = NegativeXVelocity ? Gravity*XVelocityRatio*-1 : Gravity*XVelocityRatio,
                                                    Duration = _Duration,
                                                    From = Left,
                                                    IsAdditive = true,
                                                    IsCumulative = true,
                                                    RepeatBehavior = RepeatBehavior.Forever
                                                };

#if USETRANSLATION
            _Translation.BeginAnimation(TranslateTransform.XProperty, leftAnimation);
#else
            BeginAnimation(Canvas.LeftProperty, leftAnimation);
#endif

            DoubleAnimation opacityAnimation = new DoubleAnimation
                                                   {
                                                       AutoReverse = false,
                                                       By = (Gravity/_OpacityModifier)*-1,
                                                       Duration = _Duration,
                                                       From = Opacity,
                                                       IsCumulative = true,
                                                       RepeatBehavior = RepeatBehavior.Forever
                                                   };

            BeginAnimation(OpacityProperty, opacityAnimation);
        }

        public void Stop()
        {
            _Rotation.BeginAnimation(RotateTransform.AngleProperty, null);
#if USETRANSLATION
            _Translation.BeginAnimation(TranslateTransform.XProperty, null);
            _Translation.BeginAnimation(TranslateTransform.YProperty, null);
#else
            BeginAnimation(Canvas.LeftProperty, null);
            BeginAnimation(Canvas.TopProperty, null);
#endif
            BeginAnimation(OpacityProperty, null);
        }
        #endregion
    }
}

