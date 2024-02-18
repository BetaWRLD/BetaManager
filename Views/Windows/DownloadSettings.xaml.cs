using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BetaManager.Classes;

namespace BetaManager.Views
{
    /// <summary>
    /// Interaction logic for DownloadSettings.xaml
    /// </summary>
    public partial class DownloadSettings : UserControl
    {
        public DownloadSettings()
        {
            this.Opacity = 0;
            InitializeComponent();
            Instances.MainViewInstance.PreviewMouseDown += MainViewInstance_PreviewMouseDown;
        }

        private async void MainViewInstance_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (UIFunctions.IsMouseOutside(e, this) && this.Opacity == 1)
            {
                //return;
                await new Functions().FadeOut(this);
                this.IsHitTestVisible = false;
                Instances.MainViewInstance.BlurFunc(1);
                Instances.MainViewInstance.AdditionalView.Content = null;
            }
        }

        private void DownloadSettings_Loaded(object sender, RoutedEventArgs e)
        {
            new Functions().FadeIn(this);
        }

        private void AfterDownloading_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e
        ) { }
    }
}
