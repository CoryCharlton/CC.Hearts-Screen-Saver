using System.Windows;
using System.Windows.Media;

namespace CC.Hearts.Controls
{
    public class HeartVisual: DrawingVisual
    {
        #region Constructor
        public HeartVisual()
        {
            SetDefaultValues();
            CreateVisual();
        }
        #endregion

        #region Public Constants
        public const int Height = 352;
        public const int Width = 367;
        #endregion

        #region Private Fields
        private Brush _FillBrush;
        private Brush _StrokeBrush;
        private Pen _StrokePen;

        private static StreamGeometry _StreamGeometry;
        #endregion

        #region Public Fields
        public static int DefaultHeight = Height;
        public static int DefaultWidth = Width;
        #endregion

        #region Public Properties
        public Brush Fill
        {
            get { return _FillBrush; }
            set
            {
                _FillBrush = value;

                CreateVisual();
            }
        }

        public Brush Stroke
        {
            get { return _StrokeBrush; }
            set
            {
                _StrokeBrush = value;
                _StrokePen.Brush = _StrokeBrush;

                CreateVisual();
            }
        }
        #endregion

        #region Private Methods
        private void CreateVisual()
        {
            if (_StreamGeometry == null)
            {
                LoadStreamGeometry();
            }

            using (DrawingContext drawingContext = RenderOpen())
            {
                drawingContext.DrawGeometry(_FillBrush, new Pen(_StrokeBrush, 10), _StreamGeometry);
            }
        }

        private static void LoadStreamGeometry()
        {
            _StreamGeometry = (StreamGeometry)Application.Current.FindResource("HeartGeometry");
        }

        private void SetDefaultValues()
        {
            _FillBrush = Brushes.Red;
            _StrokeBrush = Brushes.Black;
            _StrokePen = new Pen(_StrokeBrush, 10);
        }
        #endregion
    }
}
