using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace AdvancedMouse
{

    /// <summary>
    ///  Provides methodes to clip the mouse. 
    /// </summary>    
    internal class CLPcursor
    {
        #region API - Methodes        
        [DllImport("user32.dll")]
        static extern bool ClipCursor(ref RECT lpRect);
        #endregion

        #region Variables
        private static RECT myrect = new RECT();
        private static UIElement uIEelement;
        
        #endregion

        #region Structs
        private struct RECT
        {
            #region Variables.
            /// <summary>
            /// Left position of the rectangle.
            /// </summary>
            public int Left;
            /// <summary>
            /// Top position of the rectangle.
            /// </summary>
            public int Top;
            /// <summary>
            /// Right position of the rectangle.
            /// </summary>
            public int Right;
            /// <summary>
            /// Bottom position of the rectangle.
            /// </summary>
            public int Bottom;
            #endregion

            #region Constructor.
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="left">Horizontal position.</param>
            /// <param name="top">Vertical position.</param>
            /// <param name="right">Right most side.</param>
            /// <param name="bottom">Bottom most side.</param>
            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }
            #endregion

            #region Operators
            /// <summary>
            /// Operator to convert a RECT to Drawing.Rectangle.
            /// </summary>
            /// <param name="rect">Rectangle to convert.</param>
            /// <returns>A Drawing.Rectangle</returns>
            public static implicit operator System.Drawing.Rectangle(RECT rect)
            {
                return System.Drawing.Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
            }

            /// <summary>
            /// Operator to convert Drawing.Rectangle to a RECT.
            /// </summary>
            /// <param name="rect">Rectangle to convert.</param>
            /// <returns>RECT rectangle.</returns>
            public static implicit operator RECT(System.Drawing.Rectangle rect)
            {
                return new RECT(rect.Left, rect.Top, rect.Right, rect.Bottom);
            }
            #endregion
        }
        #endregion

        #region Propertys
        
        /// <summary>
        /// Gets the Position of the Cliping.
        /// </summary>
        public static Point Position
        {
            get { return new Point(myrect.Left, myrect.Top); }
        }       
        #endregion

        #region Methodes
        /// <summary>
        /// Clips the cursor to an UIElement.
        /// </summary>
        /// <param name="element"></param>
        public static void OnUIElement(UIElement element)
        {
            uIEelement = element;
            myrect.Left = (int)(element.PointFromScreen(new Point(0, 0)).X * -1);
            myrect.Top = (int)(element.PointFromScreen(new Point(0, 0)).Y * -1);
            myrect.Right = (int)((element.PointFromScreen(new Point(0, 0)).X * -1) + element.RenderSize.Width);
            myrect.Bottom = (int)((element.PointFromScreen(new Point(0, 0)).Y * -1) + element.RenderSize.Height);
            ClipCursor(ref myrect);            
        }

        /// <summary>
        /// Release the cliping of the cursor.
        /// </summary>
        public static void Release()
        {
            myrect.Left = 0;
            myrect.Top = 0;
            myrect.Right = SystemInformation.PrimaryMonitorSize.Width;
            myrect.Bottom = SystemInformation.PrimaryMonitorSize.Height;
            ClipCursor(ref myrect);            
        }
        #endregion
    }
}
