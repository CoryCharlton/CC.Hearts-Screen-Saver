using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CC.Hearts.Controls
{
    public class HeartShape : FrameworkElement
    {
        //TODO: Implement onrendersizechanged ... AdjustScale();

        #region Constructor
        public HeartShape()
        {
            SetDefaultValues();
            CreateTransforms();
        }
        #endregion

        private const int BaseMilliseconds = 50;
        private static readonly TimeSpan BaseTimeSpan = TimeSpan.FromMilliseconds(BaseMilliseconds);
        private static readonly Duration BaseDuration = new Duration(BaseTimeSpan);

        #region Private Constants
        public const double MaximumGravity = 12.0;
        public const double MaximumRandomAngle = 35;
        public const double MinimumGravity = 6.0;
        public const double MinimumRandomAngle = 25;
        #endregion

        #region Private Fields
        private double _Gravity;
        private double _Height = HeartVisual.DefaultHeight;
        private readonly HeartVisual _HeartVisual = new HeartVisual();
        private double _MaximumAngle;
        private double _MinimumAngle;
        private RotateTransform _Rotation;
        private ScaleTransform _Scale;
        private TransformGroup _TransformGroup;
        //private TranslateTransform _Translation;
        private double _Width = HeartVisual.DefaultWidth;
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
            //_Translation = new TranslateTransform(0, 0);

            _TransformGroup.Children.Add(_Rotation);
            _TransformGroup.Children.Add(_Scale);
            //_TransformGroup.Children.Add(_Translation);

            RenderTransform = _TransformGroup; //NOTE: Is this right?
        }

        private void SetDefaultValues()
        {
            CacheMode = new BitmapCache();
            Gravity = Utilities.RandomNext((int)MinimumGravity, (int)(MaximumGravity + 1));

            const double randomAngleDelta = MaximumRandomAngle - MinimumRandomAngle;
            double gravityFactor = Gravity/MaximumGravity;

            MaximumAngle = (randomAngleDelta*gravityFactor) + MinimumRandomAngle;
            NegativeXVelocity = (Utilities.RandomNext(0, 2) == 1);
            XVelocityRatio = Utilities.RandomNext(25, 50)/100.0;
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

        //public bool IsReallyVisible(double height, double width)
        //{
        //    bool returnValue = true;

        //    if (Left < Width * -1)
        //    {
        //        returnValue = false;
        //    }

        //    if (Left > width)
        //    {
        //        returnValue = false;
        //    }

        //    if (Opacity < 0)
        //    {
        //        returnValue = false;
        //    }

        //    if (Top < Height * -1)
        //    {
        //        returnValue = false;
        //    }

        //    if (Top > height)
        //    {
        //        returnValue = false;
        //    }

        //    return returnValue;
        //}

        public void Reset()
        {
            SetDefaultValues();
        }

        public void Start()
        {
            DoubleAnimation rotationAnimation = new DoubleAnimation
            {
                AutoReverse = true,
                Duration = new Duration(TimeSpan.FromMilliseconds((MaximumGravity - Gravity + MinimumGravity) * 250)),
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
                By = NegativeXVelocity ? Gravity * XVelocityRatio * -1 : Gravity * XVelocityRatio,
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
                By = (Gravity / 1000) * -1,
                Duration = BaseDuration,
                From = Opacity,
                IsCumulative = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            BeginAnimation(OpacityProperty, opacityAnimation);
        }

        /*
        public void Start()
        {
            const int baseMilliseconds = 250;
            TimeSpan baseTimeSpan = TimeSpan.FromMilliseconds(baseMilliseconds);
            Duration baseDuration = new Duration(baseTimeSpan);
            
            DoubleAnimation leftAnimation = new DoubleAnimation
                                                {
                                                    AutoReverse = false,
                                                    By = NegativeXVelocity ? Gravity*XVelocityRatio*-1 : Gravity*XVelocityRatio,
                                                    Duration = baseDuration,
                                                    IsAdditive = true,
                                                    IsCumulative = true,
                                                    RepeatBehavior = RepeatBehavior.Forever
                                                };

            _Translation.BeginAnimation(TranslateTransform.XProperty, leftAnimation, HandoffBehavior.SnapshotAndReplace); 
            
            DoubleAnimation topAnimation = new DoubleAnimation
                                               {
                                                   AutoReverse = false,
                                                   By = Gravity,
                                                   Duration = baseDuration,
                                                   IsAdditive = true,
                                                   IsCumulative = true,
                                                   RepeatBehavior = RepeatBehavior.Forever
                                               };

            _Translation.BeginAnimation(TranslateTransform.YProperty, topAnimation, HandoffBehavior.SnapshotAndReplace);

            DoubleAnimation opacityAnimation = new DoubleAnimation
                                                   {
                                                       AutoReverse = false,
                                                       By = (Gravity/1250)*-1,
                                                       Duration = baseDuration,
                                                       From = 0.9,
                                                       IsCumulative = true,
                                                       RepeatBehavior = RepeatBehavior.Forever
                                                   };

            Fill.BeginAnimation(Brush.OpacityProperty, opacityAnimation, HandoffBehavior.SnapshotAndReplace);
            Stroke.BeginAnimation(Brush.OpacityProperty, opacityAnimation, HandoffBehavior.SnapshotAndReplace);

            DoubleAnimation rotationAnimation = new DoubleAnimation
                                                    {
                                                        AutoReverse = true,
                                                        Duration = new Duration(TimeSpan.FromMilliseconds((MaximumGravity - Gravity + MinimumGravity)* baseMilliseconds)),
                                                        From = NegativeXVelocity ? _MaximumAngle : _MinimumAngle,
                                                        RepeatBehavior = RepeatBehavior.Forever,
                                                        To = NegativeXVelocity ? _MinimumAngle : _MaximumAngle,
                                                    };

            _Rotation.BeginAnimation(RotateTransform.AngleProperty, rotationAnimation, HandoffBehavior.SnapshotAndReplace);
        }
        */
        public void Stop()
        {
            //TODO: Clean these up
            _Rotation.BeginAnimation(RotateTransform.AngleProperty, null);
            BeginAnimation(Canvas.TopProperty, null);
            BeginAnimation(Canvas.LeftProperty, null);
            BeginAnimation(OpacityProperty, null);

            //_Translation.BeginAnimation(TranslateTransform.XProperty, null);
            //_Translation.BeginAnimation(TranslateTransform.YProperty, null);
            //_FillBrush.BeginAnimation(Brush.OpacityProperty, null);
            //_OutlineBrush.BeginAnimation(Brush.OpacityProperty, null);
            //_Rotation.BeginAnimation(RotateTransform.AngleProperty, null);
        }
        #endregion



        #region Protected Properties
        protected override int VisualChildrenCount
        {
            get { return 1; }
        }
        #endregion

        protected override Visual GetVisualChild(int index)
        {
            return _HeartVisual;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            AdjustScale();

            base.OnRenderSizeChanged(sizeInfo);
        }
    }
}

