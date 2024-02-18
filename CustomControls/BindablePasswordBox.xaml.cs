using System;
using System.Diagnostics;
using System.Drawing;
using System.Security;
using System.Windows;
using System.Windows.Controls;

namespace BetaManager.CustomControls
{
    /// <summary>
    /// Interaction logic for BindablePasswordBox.xaml
    /// </summary>
    public partial class BindablePasswordBox : UserControl
    {
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register( "Password", typeof( SecureString ), typeof( BindablePasswordBox ) );

        private int _visible = 0;

        public SecureString Password
        {
            get { return ( SecureString )GetValue( PasswordProperty ); }
            set { SetValue( PasswordProperty, value ); }
        }

        public BindablePasswordBox ()
        {
            InitializeComponent();
            PasswordTextbox.PasswordChanged += OnPasswordChanged;
            PasswordNormal.TextChanged += OnPasswordChangedVisible;

        }

        private void OnPasswordChanged ( object sender, RoutedEventArgs e )
        {
            Password = PasswordTextbox.SecurePassword;
        }
        private void OnPasswordChangedVisible ( object sender, RoutedEventArgs e )
        {
            Password = Functions.StringToSecureString( PasswordNormal.Text );
        }

        private void VisibleChange_Click ( object sender, RoutedEventArgs e )
        {
            try
            {
                if ( _visible == 0 )
                {
                    _visible = 1;
                    VisibleChangeImage.ImageSource = new Functions().Bitmap2BitmapImage( new Bitmap( Properties.Resources.visible ) );
                    PasswordNormal.Visibility = Visibility.Visible;
                    PasswordTextbox.Visibility = Visibility.Hidden;
                    PasswordNormal.Text = Functions.SecureStringToString( PasswordTextbox.SecurePassword );
                }
                else
                {
                    _visible = 0;
                    VisibleChangeImage.ImageSource = new Functions().Bitmap2BitmapImage( new Bitmap( Properties.Resources.hidden ) );
                    PasswordNormal.Visibility = Visibility.Hidden;
                    PasswordTextbox.Visibility = Visibility.Visible;
                    PasswordTextbox.Password = PasswordNormal.Text;
                }
            }
            catch ( Exception x ) { Debug.WriteLine( x.ToString() ); }
        }
    }
}
