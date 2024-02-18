using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace AdvancedMouse
{
    /// <summary>
    /// Provides methodes for controling the mouse position by code. 
    /// </summary>
    internal class MouseControling
    {
        #region API - Methodes
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll", EntryPoint = "SetCursorPos")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int X, int Y);
        [DllImport("user32.dll")]
        private static extern int GetCursorPos(ref POINT lpPoint);
        [DllImport("gdi32.dll")]
        private static extern int GetPixel(int hdc, int nXPos, int nYPos);
        [DllImport("user32.dll")]
        private static extern int GetWindowDC(int hWnd);
        [DllImport("user32.dll")]
        private static extern int ReleaseDC(int hWnd, int hDC);
        #endregion

        #region Structs
        /// <summary>
        /// 
        /// </summary>
        private struct POINT
        {
            public uint x;
            public uint y;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the absolute mouse position.
        /// </summary>
        public static Point GetPosition
        {
            get
            {
                POINT point = new POINT();
                GetCursorPos(ref point);
                Point pos = new Point();
                pos.X = point.x;
                pos.Y = point.y;
                return pos;
            }
        }
        #endregion

        #region Methodes
        /// <summary>
        ///  Sets the Mouse position to an point on the assigned element.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="element"></param>
        public static void SetOnUIElement(Point point, UIElement element)
        {
            int x = (int)element.PointFromScreen(new Point(0, 0)).X * -1;
            int y = (int)element.PointFromScreen(new Point(0, 0)).Y * -1;
            SetCursorPos((int)(x + point.X), (int)(y + point.Y));
        }     
        
        /// <summary>
        /// Returns the color of the pixel under the mouse.
        /// </summary>
        /// <returns></returns>
        public static Color PixelUnderMouse()
        {
            POINT point = new POINT();
            GetCursorPos(ref point);
            Point pos = new Point();
            pos.X = point.x;
            pos.Y = point.y;
            int lDC = GetWindowDC(0);
            int intColor = GetPixel(lDC, (int)pos.X, (int)pos.Y);
            ReleaseDC(0, lDC);
            byte[] argb = new byte[4];
            //A
            //argb[0] = (byte)((intColor >> 0x18) & 0xffL);
            //B
            argb[1] = (byte)(intColor & 0xffL);
            //G
            argb[2] = (byte)((intColor >> 8) & 0xffL);
            //B
            argb[3] = (byte)((intColor >> 0x10) & 0xffL);

            return Color.FromRgb(argb[1], argb[2], argb[3]);
        }
        /// <summary>
        /// Returns the color of the specified pixel on Window.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Color PixelColor(Point point)
        {
            int lDC = GetWindowDC(0);
            int intColor = GetPixel(lDC, (int)point.X, (int)point.Y);
            ReleaseDC(0, lDC);
            byte[] argb = new byte[4];
            //A
            //argb[0] = (byte)((intColor >> 0x18) & 0xffL);
            //B
            argb[1] = (byte)(intColor & 0xffL);
            //G
            argb[2] = (byte)((intColor >> 8) & 0xffL);
            //B
            argb[3] = (byte)((intColor >> 0x10) & 0xffL);

            return Color.FromRgb(argb[1], argb[2], argb[3]);
        }
        #endregion
    }
}
