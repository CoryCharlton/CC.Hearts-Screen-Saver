using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CC.Hearts.Controls
{
    public partial class Heart
    {
        #region Constructor
        public Heart()
        {
            InitializeComponent();
            SetDefaultValues();
        }

        public Heart(Brush fillBrush): this()
        {
            FillBrush = fillBrush;
        }

        public Heart(Brush fillBrush, Brush outlineBrush): this()
        {
            FillBrush = fillBrush;
            OutlineBrush = outlineBrush;
        }
        #endregion

        #region Private Constants
        private const int BaseMilliseconds = 50;
        private const int DefaultHeight = 352;
        private const int DefaultWidth = 367;
        public const double MaximumGravity = 12.0;
        public const double MaximumRandomAngle = 35;
        public const double MinimumGravity = 6.0;
        public const double MinimumRandomAngle = 25;
        #endregion

        #region Private Fields
        public static readonly TimeSpan BaseTimeSpan = TimeSpan.FromMilliseconds(BaseMilliseconds);
        public static readonly Duration BaseDuration = new Duration(BaseTimeSpan);

        private Brush _FillBrush;
        private double _Gravity;
        private double _MaximumAngle;
        private double _MinimumAngle;
        private Brush _OutlineBrush;
        #endregion

        #region Public Properties
        public double Angle
        {
            get { return _Rotation.Angle; }
            set { _Rotation.Angle = value; }
        }

        public Brush FillBrush
        {
            get { return _FillBrush; }
            set
            {
                _FillBrush = value;
                _PathMain.Fill = _FillBrush;
            }
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

        public Brush OutlineBrush
        {
            get { return _OutlineBrush; }
            set
            {
                _OutlineBrush = value;
                _PathMain.Stroke = _OutlineBrush;
            }
        }

        public double Top
        {
            get { return (double)(GetValue(Canvas.TopProperty)); }
            set { SetValue(Canvas.TopProperty, value); }
        }

        public double XVelocityRatio
        {
            get; set;
        }
        #endregion

        #region Private Methods
        private void SetDefaultValues()
        {
            _Scale.ScaleX = Width / DefaultWidth;
            _Scale.ScaleY = Height / DefaultHeight;

            Gravity = Utilities.RandomNext((int) MinimumGravity, (int) (MaximumGravity + 1));

            const double randomAngleDelta = MaximumRandomAngle - MinimumRandomAngle;
            double gravityFactor = Gravity/MaximumGravity;

            MaximumAngle = (randomAngleDelta*gravityFactor) + MinimumRandomAngle;
            NegativeXVelocity = (Utilities.RandomNext(0, 2) == 1);
            Opacity = 0.9;
            XVelocityRatio = Utilities.RandomNext(25, 75)/100.0;
        }
        #endregion

        #region Protected Methods
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            _Scale.ScaleX = ActualWidth / DefaultWidth;
            _Scale.ScaleY = ActualHeight / DefaultHeight;

            base.OnRenderSizeChanged(sizeInfo);
        }
        #endregion

        #region Public Methods
        public bool IsReallyVisible(double height, double width)
        {
            bool returnValue = true;

            if (Left < Width * -1)
            {
                returnValue = false;    
            }

            if (Left > width)
            {
                returnValue = false;
            }

            if (Opacity < 0)
            {
                returnValue = false;
            }

            if (Top < Height * -1)
            {
                returnValue = false;
            }

            if (Top > height)
            {
                returnValue = false;
            }

            return returnValue;
        }

        public void Reset()
        {
            SetDefaultValues();
        }

        public void Start()
        {
            //TODO: Optimize these... I hear setting Opacity on the Brush is better than the Path but I'm not sure about with Cached Composition
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
                                                    By = (Gravity/1000) * -1,
                                                    Duration = BaseDuration,
                                                    From = Opacity,
                                                    IsCumulative = true,
                                                    RepeatBehavior = RepeatBehavior.Forever
                                                };

            BeginAnimation(OpacityProperty, opacityAnimation);
        }

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
    }
}