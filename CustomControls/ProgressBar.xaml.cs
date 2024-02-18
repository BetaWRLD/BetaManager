using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace BetaManager.Views
{
    /// <summary>
    /// Interaction logic for ProgressBar.xaml
    /// </summary>
    public partial class CustomProgressBar : UserControl
    {
        public CustomProgressBar ()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register( "Value", typeof( double ), typeof( CustomProgressBar ), new PropertyMetadata( 0.0, OnValueChanged ) );
        public static new readonly DependencyProperty HeightProperty =
            DependencyProperty.Register( "Height", typeof( double ), typeof( CustomProgressBar ), new PropertyMetadata( 5.0, OnHeightChanged ) );
        public static new readonly DependencyProperty WidthProperty =
    DependencyProperty.Register( "Width", typeof( double ), typeof( CustomProgressBar ), new PropertyMetadata( 0.0, OnWidthChanged ) );

        public new double Width
        {
            get { return ( double )GetValue( WidthProperty ); }
            set { SetValue( WidthProperty, value ); }
        }

        private static void OnWidthChanged ( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            CustomProgressBar customProgressBar = ( CustomProgressBar )d;
            customProgressBar.UpdateProgressBarWidth();
        }

        private void UpdateProgressBarWidth ()
        {
            double newWidth = Width;
            if ( newWidth < 0 ) newWidth = 0;

            ProgressBarTrack.Width = newWidth;
            // You might need to update the ProgressBarFill as well if needed.
        }
        public async Task AnimateToValue ( double targetValue )
        {
            double currentValue = ProgressBarFill.Width;
            double atargetValue = ( targetValue / 100 ) * ProgressBarTrack.Width;

            if ( double.IsNaN( currentValue ) || double.IsNaN( atargetValue ) )
                return;

            if ( currentValue == atargetValue )
                return;

            DoubleAnimation animation = new DoubleAnimation( currentValue, atargetValue, TimeSpan.FromSeconds( 1 ) );

            animation.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut };

            var tcs = new TaskCompletionSource<bool>();
            animation.Completed += ( sender, e ) =>
            {
                tcs.SetResult( true );
            };

            ProgressBarFill.BeginAnimation( Rectangle.WidthProperty, animation );

            Value = targetValue;
            await tcs.Task;

        }
        public new double Height
        {
            get { return ( double )GetValue( HeightProperty ); }
            set { SetValue( HeightProperty, value ); }
        }

        private static void OnHeightChanged ( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            CustomProgressBar customProgressBar = ( CustomProgressBar )d;
            customProgressBar.UpdateProgressBarHeight();
        }

        private void UpdateProgressBarHeight ()
        {
            double newHeight = Height;
            if ( newHeight < 0 ) newHeight = 0;

            ProgressBarTrack.Height = newHeight;
            ProgressBarFill.Height = newHeight;
        }
        public double Value
        {
            get { return ( double )GetValue( ValueProperty ); }
            set { SetValue( ValueProperty, value ); }
        }

        private static void OnValueChanged ( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            CustomProgressBar customProgressBar = ( CustomProgressBar )d;
            customProgressBar.UpdateProgressBarFill();
        }
        private void UpdateProgressBarFill ()
        {
            double progress = Value;
            if ( progress < 0 ) progress = 0;
            if ( progress > 100 ) progress = 100;

            ProgressBarFill.Width = ( progress / 100 ) * ProgressBarTrack.Width;
        }
    }
}
