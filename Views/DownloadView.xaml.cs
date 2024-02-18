using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using BetaManager.Classes;
using BetaManager.Downloader;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace BetaManager.Views
{
    public partial class DownloadView : Window
    {
        private NumberFormatInfo numberFormat = NumberFormatInfo.InvariantInfo;

        public DownloadView()
        {
            InitializeComponent();
            InstallDir.Text = "D:\\BetaManger Games\\" + CleanDir(Saved.SelectedGame.Name);

            if (System.Windows.Clipboard.ContainsText())
            {
                string clipboardText = System.Windows.Clipboard.GetText();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GameSize.Text = Saved.SelectedGame.RequiredSpace;
            FreeSpace.Text = GetFreeDiskSpace("D:\\");
        }

        private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        // Return the amount of free disk space on a given partition
        private string GetFreeDiskSpace(string driveName)
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name == driveName)
                {
                    long freeSpace = drive.AvailableFreeSpace;
                    double mbFreeSpace = (double)freeSpace / Math.Pow(1024, 2);
                    double gbFreeSpace = mbFreeSpace / 1024D;

                    if (freeSpace < Math.Pow(1024, 3))
                    {
                        return mbFreeSpace.ToString("#.00", numberFormat) + " MB";
                    }
                    return gbFreeSpace.ToString("#.00", numberFormat) + " GB";
                }
            }
            return String.Empty;
        }

        private bool CanDownload()
        {
            string drive2 = String.Empty;
            if (InstallDir.Text.Length > 3)
                drive2 = InstallDir.Text.Remove(3);
            else
                drive2 = InstallDir.Text;
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name == drive2)
                {
                    long freeSpace = drive.AvailableFreeSpace;
                    if (freeSpace <= Saved.SelectedGame.RequiredSpaceRaw)
                    {
                        return false;
                    }
                    else
                        return true;
                }
            }

            return false;
        }

        private string CleanDir(string dir)
        {
            string regSearch =
                new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex rg = new Regex(string.Format("[{0}]", Regex.Escape(regSearch)));
            return rg.Replace(dir, "");
        }

        private void InstallDir_TextChanged(
            object sender,
            System.Windows.Controls.TextChangedEventArgs e
        )
        {
            string drive = String.Empty;
            if (InstallDir.Text.Length > 3)
                drive = InstallDir.Text.Remove(3);
            else
                drive = InstallDir.Text;
            string[] parts = InstallDir.Text.Split('\\');
            for (int i = 1; i < parts.Length; i++)
            {
                parts[i] = CleanDir(parts[i]);
            }
            InstallDir.Text = string.Join("\\", parts);
            FreeSpace.Text = GetFreeDiskSpace(drive);
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.Title = "Select The Game Folder";
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string path = dialog.FileName;
                if (path.EndsWith("\\") == false)
                    path += "\\";
                string[] parts = (path + Saved.SelectedGame.Name).Split('\\');
                for (int i = 1; i < parts.Length; i++)
                {
                    parts[i] = CleanDir(parts[i]);
                }
                InstallDir.Text = string.Join("\\", parts);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        static string getFileName(string url)
        {
            string[] arr = url.Split('/');
            return arr[arr.Length - 1];
        }

        public string GetFileNameAndExtensionFromUrl(string url)
        {
            // Use Uri to extract the file name and query
            Uri uri = new Uri(url);

            // Get the filename from the URL
            string fileName = Path.GetFileName(uri.LocalPath);

            // Get the file extension from the filename
            string fileExtension = Path.GetExtension(fileName);

            // Remove the leading dot from the file extension if present
            if (!string.IsNullOrEmpty(fileExtension) && fileExtension.StartsWith("."))
            {
                fileExtension = fileExtension.Substring(1);
            }

            // Concatenate the filename and extension
            string fileNameAndExtension = $"{fileName}.{fileExtension}";

            return fileNameAndExtension;
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanDownload())
                {
                    MessageBox.Show(
                        "No Enough Space in The Selected Drive.\nThe Install Needs The Repack File Size + The Game Size in order to be able to install the Game Correctly",
                        "BetaManager",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
                else
                {
                    Saved.Logger.Log("53821");
                    if (InstallDir.Text.EndsWith("\\") == false)
                        InstallDir.Text += "\\";
                    DownloadClient download = new DownloadClient(
                        Saved.SelectedGame.URL.First(),
                        Saved.SelectedGame.Torrent,
                        InstallDir.Text
                    );
                    Saved.Logger.Log("89491");

                    download.ID = Saved.SelectedGame.ID;

                    download.DownloadProgressChanged += download.DownloadProgressChangedHandler;
                    download.DownloadCompleted += download.DownloadCompletedHandler;
                    download.DownloadCompleted += Instances
                        .ManagerViewInstance
                        .DownloadCompletedHandler;

                    if (!Directory.Exists(InstallDir.Text))
                    {
                        Directory.CreateDirectory(InstallDir.Text);
                    }
                    if (download.HasError)
                        return;
                    Saved.Logger.Log("91613");

                    download.Path = InstallDir.Text;

                    download.AddedOn = DateTime.UtcNow;
                    download.CompletedOn = DateTime.MinValue;

                    Saved.Logger.Log("21849");
                    DownloadManager.Instance.DownloadsList.Add(download);
                    download.ID = Saved.SelectedGame.ID;
                    download.Name = Saved.SelectedGame.Name;
                    download.Picture = Saved.SelectedGame.Picture;
                    download.Size = Saved.SelectedGame.Size;
                    download.RepackSize = Saved.SelectedGame.RepackSize;
                    download.RequiredSpace =
                        "Final Game Size: ("
                        + Saved.SelectedGame.Size
                        + ") Required For Installation: ("
                        + Saved.SelectedGame.RequiredSpace
                        + ")";
                    download.Start();
                    Saved.Logger.Log("88495");
                    Auth.AddDownload(Saved.SelectedGame.ID);
                    this.Close();
                }
            }
            catch (Exception ea)
            {
                Saved.Logger.Log("12452", ea.ToString());
            }
        }

        private void btnClose_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
