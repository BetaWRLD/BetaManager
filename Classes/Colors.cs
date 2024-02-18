using System.Windows.Media;

namespace BetaManager
{
    public static class UIColors
    {
        public static Color MainColor = ( Color )ColorConverter.ConvertFromString( "#FAF9F6" );
        public static SolidColorBrush ActiveColor = new SolidColorBrush( ( Color )ColorConverter.ConvertFromString( "#426BE8" ) );
    }
}
