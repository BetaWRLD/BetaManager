using System.Windows.Media;

namespace BetaManager
{
    public static class ColorExtensions
    {
        public static System.Drawing.Color ToDrawingColor ( this System.Windows.Media.Color mediaColor )
        {
            return System.Drawing.Color.FromArgb( mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B );
        }

        public static System.Windows.Media.Color ToMediaColor ( this System.Drawing.Color drawingColor )
        {
            return System.Windows.Media.Color.FromArgb( drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B );
        }
        public static Color ConvertToDrawingColor ( SolidColorBrush solidColorBrush )
        {
            Color mediaColor = solidColorBrush.Color;
            return Color.FromArgb( mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B );
        }
    }
}