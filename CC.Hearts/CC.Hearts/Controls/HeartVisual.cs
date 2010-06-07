using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CC.Hearts.Controls
{
    public class HeartVisual: DrawingVisual
    {
        #region Constructor
        public HeartVisual()
        {
            SetDefaultValues();
            CreateTransforms();
            CreateVisual();
        }

        public HeartVisual(Brush fillBrush): this()
        {
            FillBrush = fillBrush;
        }

        public HeartVisual(Brush fillBrush, Brush outlineBrush): this()
        {
            FillBrush = fillBrush;
            OutlineBrush = outlineBrush;
        }
        #endregion

        #region Private Constants
        private const int DefaultHeight = 352;
        private const int DefaultWidth = 367;
        public const double MaximumGravity = 8.0;
        public const double MaximumRandomAngle = 35;
        public const double MinimumGravity = 4.0;
        public const double MinimumRandomAngle = 25;
        #endregion

        #region Private Fields
        private Brush _FillBrush;
        private double _Gravity;
        private double _Height = DefaultHeight;
        private double _MaximumAngle;
        private double _MinimumAngle;
        private Brush _OutlineBrush;
        private RotateTransform _Rotation;
        private ScaleTransform _Scale;
        private static StreamGeometry _StreamGeometry;
        private TransformGroup _TransformGroup;
        private TranslateTransform _Translation;
        private double _Width = DefaultWidth;
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

                CreateVisual();
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

        public double Height
        {
            get { return _Height; }
            set
            {
                _Height = value;

                AdjustScale();
            }
        }

        public double Left
        {
            get { return _Translation.X; }
            set { _Translation.X = value; }
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

                CreateVisual();
            }
        }

        public double Top
        {
            get { return _Translation.Y; }
            set { _Translation.Y = value; }
        }

        public double Width
        {
            get { return _Width; }
            set
            {
                _Width = value;

                AdjustScale();
            }
        }
        
        public double XVelocityRatio { get; set; }
        #endregion

        #region Private Methods
        private void AdjustScale()
        {
            _Scale.ScaleX = Width / DefaultWidth;
            _Scale.ScaleY = Height / DefaultHeight;
        }

        private void CreateTransforms()
        {
            _TransformGroup = new TransformGroup();
            _Rotation = new RotateTransform(0, DefaultWidth / 2, DefaultHeight / 2);
            _Scale = new ScaleTransform(Width/DefaultWidth, Height/DefaultHeight);
            _Translation = new TranslateTransform(0, 0);

            _TransformGroup.Children.Add(_Rotation);
            _TransformGroup.Children.Add(_Scale);
            _TransformGroup.Children.Add(_Translation);

            Transform = _TransformGroup;
        }

        private void CreateVisual()
        {
            if (_StreamGeometry == null)
            {
                LoadStreamGeometry();
            }

            DrawingContext drawingContext = RenderOpen();
            drawingContext.DrawGeometry(_FillBrush, new Pen(_OutlineBrush, 10), _StreamGeometry);
            drawingContext.Close();
        }

        private static void LoadStreamGeometry()
        {
            _StreamGeometry = (StreamGeometry)Application.Current.FindResource("HeartGeometry");
        }

        private void SetDefaultValues()
        {
            _FillBrush = Brushes.Red;
            _OutlineBrush = Brushes.Black;

            Gravity = Utilities.RandomNext((int) MinimumGravity, (int) (MaximumGravity + 1));

            const double randomAngleDelta = MaximumRandomAngle - MinimumRandomAngle;
            double gravityFactor = Gravity/MaximumGravity;

            MaximumAngle = (randomAngleDelta*gravityFactor) + MinimumRandomAngle;
            NegativeXVelocity = (Utilities.RandomNext(0, 2) == 1);
            Opacity = 0.9;
            XVelocityRatio = Utilities.RandomNext(25, 75)/100.0;
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
                                                   Duration = new Duration(TimeSpan.FromMilliseconds(50)), 
                                                   From = Opacity, 
                                                   IsAdditive = true, 
                                                   IsCumulative = true,
                                                   RepeatBehavior = RepeatBehavior.Forever
                                               };

            _Translation.BeginAnimation(TranslateTransform.YProperty, topAnimation);

            DoubleAnimation leftAnimation = new DoubleAnimation
                                               {
                                                   AutoReverse = false,
                                                   By = NegativeXVelocity ? Gravity * XVelocityRatio * -1 : Gravity * XVelocityRatio,
                                                   Duration = new Duration(TimeSpan.FromMilliseconds(50)),
                                                   From = Left,
                                                   IsAdditive = true,
                                                   IsCumulative = true,
                                                   RepeatBehavior = RepeatBehavior.Forever
                                               };

            _Translation.BeginAnimation(TranslateTransform.XProperty, leftAnimation);

            DoubleAnimation opacityAnimation = new DoubleAnimation
                                                {
                                                    AutoReverse = false,
                                                    By = (Gravity/1000) * -1,
                                                    Duration = new Duration(TimeSpan.FromMilliseconds(50)),
                                                    From = Opacity,
                                                    IsCumulative = true,
                                                    RepeatBehavior = RepeatBehavior.Forever
                                                };

            _FillBrush.BeginAnimation(Brush.OpacityProperty, opacityAnimation);
            _OutlineBrush.BeginAnimation(Brush.OpacityProperty, opacityAnimation);
        }
        #endregion

    }
}
