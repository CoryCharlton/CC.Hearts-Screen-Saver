using System;
using System.Windows.Media;

namespace CC.Hearts.Utilities
{
    public static class CommonFunctions
    {
        #region Private Fields
        private static readonly Random _Random = new Random(Environment.TickCount);
        #endregion

        #region Public Methods
        public static T CreateGradientBrush<T>(Color color1, Color color2, Color color3) where T : GradientBrush, new()
        {
            return new T
                       {
                           GradientStops = new GradientStopCollection(3)
                                               {
                                                   new GradientStop(color1, 0),
                                                   new GradientStop(color2, 0.531),
                                                   new GradientStop(color3, 1),
                                               }
                       };

        }

        public static void FixMinMax(ref int minValue, ref int maxValue)
        {
            if (minValue > maxValue)
            {
                int x = minValue;
                minValue = maxValue;
                maxValue = x;
            }
        }

        public static Color InterpolateColors(Color color1, Color color2, float percentage)
        {
            double a1 = color1.A / 255.0, r1 = color1.R / 255.0, g1 = color1.G / 255.0, b1 = color1.B / 255.0;
            double a2 = color2.A / 255.0, r2 = color2.R / 255.0, g2 = color2.G / 255.0, b2 = color2.B / 255.0;

            byte a3 = Convert.ToByte((a1 + (a2 - a1) * percentage) * 255);
            byte r3 = Convert.ToByte((r1 + (r2 - r1) * percentage) * 255);
            byte g3 = Convert.ToByte((g1 + (g2 - g1) * percentage) * 255);
            byte b3 = Convert.ToByte((b1 + (b2 - b1) * percentage) * 255);
            return Color.FromArgb(a3, r3, g3, b3);
        }

        public static T RandomGradientBrush<T>() where T: GradientBrush, new()
        {
            const int firstPass = 25;
            const int secondPass = 50;

            int blue = RandomNext(secondPass, 256);
            int green = RandomNext(secondPass, 256);
            int red = RandomNext(secondPass, 256);

            Color firstColor = Color.FromRgb((byte) red, (byte) green, (byte) blue);
            Color secondColor = Color.FromRgb((byte) (red - firstPass), (byte) (green - firstPass), (byte) (blue - firstPass));
            Color thirdColor = Color.FromRgb((byte) (red - secondPass), (byte) (green - secondPass), (byte) (blue - secondPass));

            return CreateGradientBrush<T>(firstColor, secondColor, thirdColor);
        }

        public static int RandomNext(int minValue, int maxValue)
        {
            return _Random.Next(minValue, maxValue);
        }

        public static SolidColorBrush RandomSolidColorBrush()
        {
            int blue = RandomNext(0, 256);
            int green = RandomNext(0, 256);
            int red = RandomNext(0, 256);

            return new SolidColorBrush(Color.FromRgb((byte)red, (byte)green, (byte)blue));
        }
        #endregion
    }
}
