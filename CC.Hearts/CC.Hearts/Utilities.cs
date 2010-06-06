using System;
using System.Windows.Media;

namespace CC.Hearts
{
    public static class Utilities
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

        public static T RandomGradientBrush<T>() where T: GradientBrush, new()
        {
            const int firstPass = 25;
            const int secondPass = 60;

            int blue = RandomNext(secondPass, 256);
            int green = RandomNext(secondPass, 256);
            int red = RandomNext(secondPass, 256);

            return CreateGradientBrush<T>(Color.FromRgb((byte)red, (byte)green, (byte)blue), Color.FromRgb((byte)(red - firstPass), (byte)(green - firstPass), (byte)(blue - firstPass)), Color.FromRgb((byte)(red - secondPass), (byte)(green - secondPass), (byte)(blue - secondPass)));
        }

        public static int RandomNext(int minValue, int maxValue)
        {
            return _Random.Next(minValue, maxValue);
        }
        #endregion
    }
}
