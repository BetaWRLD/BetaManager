using MonoTorrent;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BetaManager.Views
{
    /// <summary>
    /// Interaction logic for TorrentFiles.xaml
    /// </summary>
    public partial class TorrentFiles : Window
    {
        public TorrentFiles ()
        {
            this.Opacity = 0;
            InitializeComponent();
            new Functions().FadeIn( this );
        }

        private void Window_Loaded ( object sender, RoutedEventArgs e )
        {
            FilesList.ItemsSource = Saved.SelectedTorrent.Files;
        }

        private void TopPanel_MouseDown ( object sender, MouseButtonEventArgs e )
        {
            this.DragMove();
        }

        private async void btnClose_Click ( object sender, RoutedEventArgs e )
        {
            await new Functions().FadeOut( this );
            this.Close();
        }

        private void btnMinimize_Click ( object sender, RoutedEventArgs e )
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CheckBox_Checked ( object sender, RoutedEventArgs e )
        {
        }

        private void CheckBox_Unchecked ( object sender, RoutedEventArgs e )
        {
        }

        private void CheckBox_Loaded ( object sender, RoutedEventArgs e )
        {
            CheckBox checkBox = ( CheckBox )sender;
            ITorrentManagerFile file = ( ITorrentManagerFile )checkBox.DataContext;

            checkBox.IsChecked = ( file.Priority != Priority.DoNotDownload );
        }

        private void CheckBox_Click ( object sender, RoutedEventArgs e )
        {
            CheckBox button = ( CheckBox )sender;
            ITorrentManagerFile file = ( ITorrentManagerFile )button.DataContext;

            Saved.SelectedTorrent.SetFilePriorityAsync( file, button.IsChecked == true ? Priority.Normal : Priority.DoNotDownload );
        }
    }
}
