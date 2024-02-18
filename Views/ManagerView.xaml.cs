using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using BetaManager.Classes;
using BetaManager.Downloader;
using BetaManager.Models;
using File = System.IO.File;

namespace BetaManager.Views
{
    public partial class ManagerView : System.Windows.Controls.UserControl
    {
        byte AddPercent;
        private DownloadClient download;

        public ManagerView()
        {
            this.Opacity = 0;
            InitializeComponent();
            Instances.ManagerViewInstance = this;
            ListViewProducts.ItemsSource = DownloadManager.Instance.DownloadsList;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            new Functions().FadeIn(this, 250);
        }

        private void UpdateProgressBarWidth(string gameId)
        {
            foreach (var item in ListViewProducts.Items)
            {
                if (item is FrameworkElement container)
                {
                    var game = (dynamic)container.DataContext;
                    if (game.ID == gameId)
                    {
                        game.percent = 200;
                    }
                }
            }
        }

        private void Border_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        private void Border_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.Cursor = null;
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Button button && button.Content is Grid grid)
            {
                foreach (UIElement child in grid.Children)
                {
                    if (child is System.Windows.Controls.Image image)
                    {
                        image.RenderTransform = new System.Windows.Media.ScaleTransform(
                            1.05,
                            1.05,
                            0.5,
                            0.5
                        );
                        break;
                    }
                }
            }
        }

        private void UpdateItem(string ID, string property, string value) { }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Button button && button.Content is Grid grid)
            {
                foreach (UIElement child in grid.Children)
                {
                    if (child is System.Windows.Controls.Image image)
                    {
                        image.RenderTransform = null;
                        break;
                    }
                }
            }
        }

        private async void InstallGame(string Target, DownloadClient DC)
        {
            if (File.Exists(Directory.GetFiles(Target, "*.exe").FirstOrDefault()))
            {
                Functions.StartProccess(Directory.GetFiles(Target, "*.exe").FirstOrDefault());
                FitGirlGameModel fg = await Auth.GetGame(DC.ID);
                Functions.AddGameToLibrary(
                    new Models.LibraryGameModel
                    {
                        ID = DC.ID,
                        Name = DC.Name,
                        Description = fg.Description,
                        Developer = fg.Developer,
                        IsReady = false,
                        Folder = null,
                        FolderAvailable = false,
                        InstallDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                        InstallDateString = Functions
                            .UnixTimeStampToDateTime(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                            .ToString("MM/dd/yyyy"),
                        LastPlayDate = 0,
                        LastPlayDateString = Functions.TimeSince(0),
                        Picture =
                            Saved.SaveLocation + "Games//Images//" + Path.GetFileName(fg.Picture),
                        PictureURL = Saved.Site + fg.Picture,
                        Version = fg.Version,
                        GameExe = null,
                        SizeOnDisk = 0,
                        Credits = fg.Credits,
                    }
                );
                if (Instances.LibraryViewInstance != null)
                {
                    if (!Instances.LibraryViewInstance.IsLoaded)
                        Instances.LibraryViewInstance = new LibraryView();
                    Instances.LibraryViewInstance.ListViewProducts.ItemsSource = null;
                    Instances.LibraryViewInstance.ListViewProducts.ItemsSource = Instances
                        .LibraryViewModel
                        .Games;
                }
                DownloadManager.Instance.DownloadsList.Remove(download);
                ListViewProducts.Items.Refresh();
                Functions.SaveLibrary();
            }
        }

        public void DownloadCompletedHandler(object sender, EventArgs e)
        {
            download = (DownloadClient)sender;
            new Functions().SendNotification(
                "BetaManager",
                $"{download.Name} has been downloaded!",
                1
            );
            download.UpdateDownloadDisplay();
            if (download.OpenInstaller == true)
                InstallGame(download.TorrentModel.Manager.SavePath, download);
        }

        private void UpdatePropertiesList(object sender, PropertyChangedEventArgs e)
        {
            var download = (DownloadClient)sender;
        }

        private void aa_Click(object sender, RoutedEventArgs e)
        {
            UpdateProgressBarWidth("0");
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            UpdateProgressBarWidth("0");
        }

        private void PauseResumeButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            DownloadClient Downloader = (DownloadClient)button.DataContext;

            if (Downloader.Completed == true)
            {
                new Functions().SaveDownloadsToXml();
                InstallGame(Downloader.TorrentModel.Manager.SavePath, Downloader);
                return;
            }
            if (Downloader != null && Downloader.Status != DownloadStatus.Paused)
                Downloader.Pause();
            else if (Downloader != null && Downloader.Status == DownloadStatus.Paused)
                Downloader.Resume();
            new Functions().SaveDownloadsToXml();
        }

        public void BlurFunc(int mode = 0)
        {
            BlurEffect blurEffect = new BlurEffect();

            DoubleAnimation blurAnimation;

            if (mode == 0)
            {
                blurAnimation = new DoubleAnimation(10, TimeSpan.FromSeconds(0.5));
                blurAnimation.Completed += (s, e) => {
                    //TopBorder.Visibility = Visibility.Visible;
                };
            }
            else
            {
                blurAnimation = new DoubleAnimation(0, TimeSpan.FromSeconds(0.5));
                blurAnimation.Completed += (s, e) => {
                    //TopBorder.Visibility = Visibility.Hidden;
                };
            }

            blurEffect.BeginAnimation(BlurEffect.RadiusProperty, blurAnimation);
            //TopBorder.Effect = blurEffect;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            DownloadClient download = (DownloadClient)button.DataContext;
            new ManagerTimers().BlurStuff(download);
        }

        private void FolderButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            DownloadClient download = (DownloadClient)button.DataContext;
            if (Directory.Exists(download.Path))
            {
                Process.Start(download.Path);
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Instances.MainViewInstance.BlurFunc(0);
            Instances.MainViewInstance.AdditionalView.Content = new DownloadSettings();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            DownloadClient download = (DownloadClient)button.DataContext;
            new ManagerTimers().UnBlurStuff(download);
        }

        private async void YesButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            DownloadClient download = (DownloadClient)button.DataContext;
            if (await download.Cancel())
            {
                DownloadManager.Instance.DownloadsList.Remove(download);
                new Functions().SendNotification(
                    "BetaManager",
                    $"Successfully cancelled {download.Name}!",
                    1
                );
                new Functions().SaveDownloadsToXml();
            }
            else
                new Functions().SendNotification(
                    "BetaManager",
                    $"An error occurred while cancelling {download.Name}!",
                    3
                );
        }
    }
}
