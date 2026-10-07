using System;
using System.Drawing;

namespace Volcano.Interface
{
    public class ColorTransition
    {
        public static Color GetColorFromRedToGreen(double t, int alpha)
        {
            t = Math.Clamp(t, 0f, 1f);

            // Red is at hue 0°, Yellow is at 60°, Green is at 120°
            // As t goes from 0 to 1, hue goes from 0 to 120
            float hue = (float)t * 120f;
            float saturation = 1.0f; // Full saturation to avoid muddiness/brown
            float lightness = 0.5f;  // Balanced lightness

            return HslToRgb(hue, saturation, lightness, alpha);
        }

        private static Color HslToRgb(float hue, float saturation, float lightness, int alpha)
        {
            // Hue: 0 to 360, Saturation: 0 to 1, Lightness: 0 to 1
            float c = (1f - Math.Abs(2f * lightness - 1f)) * saturation;
            float x = c * (1f - Math.Abs((hue / 60f) % 2f - 1f));
            float m = lightness - c / 2f;

            float r1 = 0, g1 = 0, b1 = 0;

            if (hue >= 0 && hue < 60)
            {
                r1 = c; g1 = x; b1 = 0;
            }
            else if (hue >= 60 && hue < 120)
            {
                r1 = x; g1 = c; b1 = 0;
            }
            else if (hue >= 120 && hue <= 180)
            {
                r1 = 0; g1 = c; b1 = x;
            }

            int r = (int)((r1 + m) * 255);
            int g = (int)((g1 + m) * 255);
            int b = (int)((b1 + m) * 255);

            return Color.FromArgb(alpha, r, g, b);
        }
    }
}