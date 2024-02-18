using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BetaManager.Classes;
using BetaManager.Downloader;
using BetaManager.Downloader.Torrent;
using BetaManager.Models;
using Microsoft.WindowsAPICodePack.Dialogs;
using MonoTorrent;
using SuRGeoNix.BitSwarmLib;

namespace BetaManager.Views
{
    public partial class DownloadView : UserControl
    {
        private NumberFormatInfo numberFormat = NumberFormatInfo.InvariantInfo;
        private TorrentDownloader torrentDownloader;
        private BitSwarm BS = new();
        private List<BitSwarmTorrentFile> BitSwarmFiles = new();

        public DownloadView()
        {
            this.Opacity = 0;
            InitializeComponent();

            if (System.Windows.Clipboard.ContainsText())
            {
                string clipboardText = System.Windows.Clipboard.GetText();
            }
        }

        private async void MainViewInstance_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseOutside(e, this) && this.Opacity == 1)
            {
                BS?.Dispose();
                await new Functions().FadeOut(this);
                this.IsHitTestVisible = false;
                Instances.MainViewInstance.BlurFunc(1);
                Instances.MainViewInstance.AdditionalView.Content = null;
            }
        }

        private bool IsMouseOutside(MouseButtonEventArgs e, UIElement i)
        {
            Point relativePosition = e.GetPosition(i);

            return relativePosition.X < 0
                || relativePosition.Y < 0
                || relativePosition.X >= this.ActualWidth
                || relativePosition.Y >= this.ActualHeight;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (SettingsModel.DefaultDownloadLocation != null)
                DownloadFolder.Text = SettingsModel.DefaultDownloadLocation;
            else
                DownloadFolder.Text =
                    "D:\\BetaManger Games\\"
                    + Functions.RemoveInvalidCharDir(Saved.SelectedGame.Name);
            FreeSpace.Text = Functions.GetFreeDiskSpace(Path.GetPathRoot(DownloadFolder.Text));
            GameName.Text = Saved.SelectedGame.Name;
            GameNameTitle.Text = Saved.SelectedGame.Name;
            await new Functions().FadeIn(this);
            this.IsHitTestVisible = true;
            Instances.MainViewInstance.PreviewMouseDown += MainViewInstance_PreviewMouseDown;

            BS.Open(Saved.SelectedGame.URL.First());
            BS.MetadataReceived += BS_MetadataReceived;
            BS.Start();
        }

        private void BS_MetadataReceived(object source, BitSwarm.MetadataReceivedArgs e)
        {
            SuRGeoNix.BitSwarmLib.BEP.Torrent torrent = e.Torrent;

            for (int i = 0; i < torrent.file.paths.Count; i++)
            {
                BitSwarmFiles.Add(
                    new BitSwarmTorrentFile
                    {
                        FilePath = torrent.file.paths[i],
                        FileSize = torrent.file.GetLength(torrent.file.paths[i]),
                        FileSizeString = Functions.FormatSize(
                            torrent.file.GetLength(torrent.file.paths[i])
                        ),
                        Priority = Priority.Normal,
                    }
                );
            }
            Instances.AppDispatcher.Invoke(() =>
            {
                GameSize.Text = DownloadManager.FormatSizeString(GetSize());
                FilesList.ItemsSource = BitSwarmFiles;
            });
            BS.Dispose();
        }

        private long GetSize()
        {
            long totalSize = 0;

            foreach (var file in BitSwarmFiles)
            {
                if (file.Priority != Priority.DoNotDownload)
                    totalSize += file.FileSize;
            }
            return totalSize;
        }

        private void Border_MouseDown(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e
        ) { }

        private bool CanDownload()
        {
            string drive2 = String.Empty;
            if (DownloadFolder.Text.Length > 3)
                drive2 = DownloadFolder.Text.Remove(3);
            else
                drive2 = DownloadFolder.Text;
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name == drive2)
                {
                    long freeSpace = drive.AvailableFreeSpace;
                    if (freeSpace <= GetSize())
                    {
                        return false;
                    }
                    else
                        return true;
                }
            }

            return false;
        }

        private void DownloadFolder_TextChanged(
            object sender,
            System.Windows.Controls.TextChangedEventArgs e
        )
        {
            FreeSpace.Text = Functions.GetFreeDiskSpace(Path.GetPathRoot(DownloadFolder.Text));
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.Title = "Select The Game Folder";
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                DownloadFolder.Text =
                    dialog.FileName + "\\" + Functions.RemoveInvalidCharDir(GameName.Text);
                FreeSpace.Text = Functions.GetFreeDiskSpace(Path.GetPathRoot(DownloadFolder.Text));
                SaveLocation.IsChecked = false;
            }
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanDownload())
                {
                    Instances.MainViewInstance.Notify(
                        "No Enough Space in The Selected Drive.\nThe Install Needs The Repack File Size + The Game Size in order to be able to install the Game Correctly"
                    );
                }
                else
                {
                    Saved.Logger.Log("53821");
                    if (DownloadFolder.Text.EndsWith("\\") == false)
                        DownloadFolder.Text += "\\";
                    DownloadClient download = new DownloadClient(
                        Saved.SelectedGame.URL.First(),
                        DownloadFolder.Text,
                        GameName.Text,
                        Saved.SelectedGame,
                        GetSize()
                    );
                    Saved.Logger.Log("89491");

                    download.ID = Saved.SelectedGame.ID;

                    download.BitSwarmFiles = BitSwarmFiles;
                    download.DownloadProgressChanged += download.DownloadProgressChangedHandler;
                    download.DownloadCompleted += download.DownloadCompletedHandler;
                    download.DownloadCompleted += Instances
                        .ManagerViewInstance
                        .DownloadCompletedHandler;

                    if (!Directory.Exists(DownloadFolder.Text))
                    {
                        Directory.CreateDirectory(DownloadFolder.Text);
                    }
                    Saved.Logger.Log("91613");

                    download.Path = DownloadFolder.Text;
                    download.StartImmediatly = (bool)StartImmediately.IsChecked;
                    download.OpenInstaller = (bool)StartInstaller.IsChecked;
                    //download.MaximumUploadRate =
                    //    (int)(
                    //        Seeding.IsChecked == true
                    //            ? -1
                    //            : (
                    //                (UploadSpeed.Text != null && UploadSpeed.Text.Length > 0)
                    //                    ? Functions.ToInt(UploadSpeed.Text)
                    //                    : 0
                    //            )
                    //    ) * 1024;

                    Saved.Logger.Log("21849");
                    download.ID = Saved.SelectedGame.ID;
                    download.DontSeed = (bool)Seeding.IsChecked;
                    download.Picture = Saved.SelectedGame.Picture;
                    if ((bool)StartImmediately.IsChecked)
                        download.Start();
                    if (SaveLocation.IsChecked == true)
                        SettingsModel.DefaultDownloadLocation = DownloadFolder.Text;
                    DownloadManager.Instance.DownloadsList.Add(download);
                    Saved.Logger.Log("88495");
                    Auth.AddDownload(Saved.SelectedGame.ID);
                    await new Functions().FadeOut(this);
                    this.IsHitTestVisible = false;
                    BS?.Dispose();
                    Instances.MainViewInstance.BlurFunc(1);
                }
            }
            catch (Exception ea)
            {
                Saved.Logger.Log("12452", ea.ToString());
            }
        }

        private void btnClose_Click_1(object sender, RoutedEventArgs e) { }

        private void CheckBox_Loaded(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            BitSwarmTorrentFile file = (BitSwarmTorrentFile)checkBox.DataContext;

            checkBox.IsChecked = (file.Priority != Priority.DoNotDownload);
        }

        private async void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            BitSwarmTorrentFile file = (BitSwarmTorrentFile)checkBox.DataContext;
            file.Priority = checkBox.IsChecked == true ? Priority.Normal : Priority.DoNotDownload;
            BitSwarmFiles[BitSwarmFiles.FindIndex(f => f.Equals(file))] = file;
            GameSize.Text = DownloadManager.FormatSizeString(GetSize());
        }
    }
}
