using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CC.Hearts.Controls
{
    /// <summary>
    /// Interaction logic for HelpCanvas.xaml
    /// </summary>
    public partial class HelpCanvas
    {
        public HelpCanvas()
        {
            InitializeComponent();
            SetupAnimation();
            CacheMode = new BitmapCache(2);
        }

        private int _ShowSeconds = 60;
        private Duration _ShowDuration;
        private DoubleAnimation _OpacityAnimation;

        private void SetupAnimation()
        {
            _ShowDuration = new Duration(TimeSpan.FromSeconds(_ShowSeconds));

            if (_OpacityAnimation == null)
            {
                _OpacityAnimation = new DoubleAnimation(0, _ShowDuration) { AutoReverse = false };
            }
            else
            {
                _OpacityAnimation.Duration = _ShowDuration;
            }
        }

        public void Hide()
        {
            BeginAnimation(OpacityProperty, null);
            Opacity = 0;
            Visibility = Visibility.Collapsed;
        }

        public void Show(int seconds)
        {
            if (_ShowSeconds != seconds)
            {
                _ShowSeconds = seconds;

                SetupAnimation();
            }

            BeginAnimation(OpacityProperty, null);
            Opacity = 0.9;
            Visibility = Visibility.Visible;
            BeginAnimation(OpacityProperty, _OpacityAnimation, HandoffBehavior.SnapshotAndReplace);
        }
    }
}
