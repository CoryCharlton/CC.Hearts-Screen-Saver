using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CC.Hearts.Controls;

namespace CC.Hearts
{
    // NOTE: A quick test to try and improve render performance. If I could bypass the bitmap I think this might be a better option...
    public class Class1 : Canvas
    {
        public Class1()
        {
            _HeartsHost = new HeartsVisualHost();
            Children.Add(_HeartsHost);
            _HeartsHost.Start();
        }

        private HeartsVisualHost _HeartsHost;

        protected override void OnRender(System.Windows.Media.DrawingContext dc)
        {
            foreach (Visual visualChild in _HeartsHost.VisualChildren)
            {
                HeartVisual heartVisual = visualChild as HeartVisual;

                if (heartVisual != null)
                {
                    RenderTargetBitmap bmp = new RenderTargetBitmap((int)heartVisual.Width, (int)heartVisual.Height, 120, 96, PixelFormats.Pbgra32);
                    bmp.Render(heartVisual);
                    dc.DrawImage(bmp, new Rect(0, 0, heartVisual.Width, heartVisual.Height));
                    bmp.Clear();
                }
            }

        }
    }
}
