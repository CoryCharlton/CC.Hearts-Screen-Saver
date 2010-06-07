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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CC.Hearts
{
    /// <summary>
    /// Interaction logic for SecondaryScreenSaverWindow.xaml
    /// </summary>
    public partial class SecondaryScreenSaverWindow : Window
    {
        public SecondaryScreenSaverWindow()
        {
            InitializeComponent();
        }

        public void SetParent(ScreenSaverWindow screenSaverWindow)
        {
            _VisualBrush.Visual = screenSaverWindow._CanvasMain;
            _VisualBrush.Viewbox = new Rect(0, 0, screenSaverWindow.ActualWidth, screenSaverWindow.ActualHeight); //TODO: These should be bindings...
        }
    }
}
