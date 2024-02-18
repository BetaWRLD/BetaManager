using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BetaManager
{
    public class UIFunctions
    {
        public static bool IsMouseOutside(MouseButtonEventArgs e, UserControl uc)
        {
            Point relativePosition = e.GetPosition(uc);

            return relativePosition.X < 0
                || relativePosition.Y < 0
                || relativePosition.X >= uc.ActualWidth
                || relativePosition.Y >= uc.ActualHeight;
        }
    }
}
