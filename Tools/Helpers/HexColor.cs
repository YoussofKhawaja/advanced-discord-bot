using System.Windows.Media;

namespace Tools.Helpers
{
    public static class HexColor
    {
        public static Brush GetColorFromHex(string hex)
        {
            return (Brush)new BrushConverter().ConvertFrom(hex);
        }
    }
}