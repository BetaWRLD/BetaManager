using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BetaManager
{
    internal class ValueBox : Grid
    {
        private int _value;
        private int _maximum;
        private double _step;
        private double _width;
        private Border border_background;
        private Border border_value;
        private TextBox txt_value;


        public ValueBox ()
        {
            _value = 255;
            _maximum = 255;

            this.MouseLeftButtonDown += new MouseButtonEventHandler( ValueBox_MouseLeftButtonDown );
            this.SizeChanged += new SizeChangedEventHandler( ValueBox_SizeChanged );

            border_background = new Border();
            border_background.Margin = new Thickness( 0, 0, 0, 0 );
            border_background.BorderBrush = Brushes.Black;
            border_background.BorderThickness = new Thickness( 1 );
            border_background.HorizontalAlignment = HorizontalAlignment.Stretch;
            border_background.VerticalAlignment = VerticalAlignment.Stretch;
            LinearGradientBrush DarkGrayGradient = new LinearGradientBrush();
            DarkGrayGradient.StartPoint = new Point( 0.5, 0 );
            DarkGrayGradient.EndPoint = new Point( 0.5, 1 );
            DarkGrayGradient.GradientStops.Add( new GradientStop( Color.FromArgb( 255, 51, 51, 51 ), 0.32 ) );
            DarkGrayGradient.GradientStops.Add( new GradientStop( Color.FromArgb( 255, 85, 85, 85 ), 0.169 ) );
            border_background.CornerRadius = new CornerRadius( 3 );
            border_background.Background = DarkGrayGradient;

            border_value = new Border();
            border_value.Margin = new Thickness( 1, 1, 1, 1 );
            border_value.HorizontalAlignment = HorizontalAlignment.Left;
            border_value.VerticalAlignment = VerticalAlignment.Stretch;
            LinearGradientBrush LightGrayGradient = new LinearGradientBrush();
            LightGrayGradient.StartPoint = new Point( 0.5, 0 );
            LightGrayGradient.EndPoint = new Point( 0.5, 1 );
            LightGrayGradient.GradientStops.Add( new GradientStop( Color.FromArgb( 255, 102, 102, 102 ), 0.32 ) );
            LightGrayGradient.GradientStops.Add( new GradientStop( Color.FromArgb( 255, 131, 131, 131 ), 0.169 ) );
            border_value.CornerRadius = new CornerRadius( 3 );
            border_value.Background = LightGrayGradient;

            txt_value = new TextBox();
            txt_value.Margin = new Thickness( 3, 1, 1, 3 );
            txt_value.HorizontalAlignment = HorizontalAlignment.Left;
            txt_value.VerticalAlignment = VerticalAlignment.Center;
            txt_value.Background = Brushes.Transparent;
            txt_value.BorderBrush = Brushes.Transparent;
            txt_value.Foreground = Brushes.White;
            txt_value.Text = "255";
            txt_value.IsReadOnly = true;
            txt_value.TextChanged += new TextChangedEventHandler( txt_value_TextChanged );

            this.Children.Add( border_background );
            this.Children.Add( border_value );
            this.Children.Add( txt_value );
        }

        public int Value
        {
            set
            {
                if ( value > _maximum )
                {
                    _value = 255;
                    txt_value.Text = "255";
                }
                else
                {
                    _value = value;
                    txt_value.Text = value.ToString();
                }
            }
            get { return _value; }
        }

        private void txt_value_TextChanged ( object sender, TextChangedEventArgs e )
        {
            try
            {
                if ( Convert.ToDouble( txt_value.Text ) > _maximum )
                {
                    border_value.Width = _maximum;
                }
                else
                {
                    border_value.Width = ( Convert.ToDouble( txt_value.Text ) ) * _step;
                }
            }
            catch
            {
            }
        }
        void ValueBox_SizeChanged ( object sender, SizeChangedEventArgs e )
        {
            _width = e.NewSize.Width;
            _step = _width / _maximum;
        }
        void ValueBox_MouseLeftButtonDown ( object sender, MouseButtonEventArgs e )
        {
            txt_value.Focus();
            txt_value.SelectAll();
        }
    }
}