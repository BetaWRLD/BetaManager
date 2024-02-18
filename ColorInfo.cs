using System.Windows.Media;

namespace BetaManager
{
    public class ColorInfo
    {
        public Color? Color;
        public bool WasSpecifiedWithAlpha;

        public static ColorInfo White = new ColorInfo() { Color = Colors.White, WasSpecifiedWithAlpha = true };
    }
}
