using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BetaManager
{
    internal class CopyBox : Border
    {
        #region Variables
        TextBox txt_value;
        #endregion

        #region Constructor
        public CopyBox ()
        {
            this.Background = new SolidColorBrush( Color.FromRgb( 51, 51, 51 ) );
            this.CornerRadius = new System.Windows.CornerRadius( 3 );
            this.BorderBrush = Brushes.Black;
            this.BorderThickness = new System.Windows.Thickness( 1 );
            txt_value = new TextBox();
            txt_value.Background = Brushes.Transparent;
            txt_value.BorderBrush = Brushes.Transparent;
            txt_value.Margin = new Thickness( 3, 1, 3, 1 );
            txt_value.HorizontalAlignment = HorizontalAlignment.Stretch;
            txt_value.VerticalAlignment = VerticalAlignment.Center;
            txt_value.Foreground = Brushes.White;
            txt_value.IsReadOnly = true;
            this.Child = txt_value;
        }
        #endregion

        #region Propertys
        /// <summary>
        /// Sets the Text of the CopyBox.
        /// </summary>        
        public string Text
        {
            set { txt_value.Text = value; }
        }
        #endregion
    }
}
