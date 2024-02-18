using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using BetaManager.Classes;
using BetaManager.Models;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace BetaManager.Views
{
    public partial class NewLibraryGame : System.Windows.Controls.UserControl
    {
        private LibraryGameModel Game = new LibraryGameModel();

        public NewLibraryGame()
        {
            this.Opacity = 0;
            InitializeComponent();
        }

        private async void MainViewInstance_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseOutside(e, this) && this.Opacity == 1)
            {
                await new Functions().FadeOut(this);
                this.IsHitTestVisible = false;
                Instances.LibraryViewInstance.BlurFunc(1);
                Instances.LibraryViewInstance.ListViewProducts.Items.Refresh();
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await new Functions().FadeIn(this);
            this.IsHitTestVisible = true;
            Instances.MainViewInstance.PreviewMouseDown += MainViewInstance_PreviewMouseDown;
            Game.ID = Functions.GenerateRandomString(32);
        }

        private async void GameImage_Click(object sender, RoutedEventArgs e)
        {
            Instances.MainViewInstance.BlurFunc();
            using (var FileDialog = new OpenFileDialog())
            {
                FileDialog.Title = "Update profile picture";
                FileDialog.Filter = "JPG, JPEG, & PNG|*.jpg;*.jpeg;*.png";
                FileDialog.RestoreDirectory = true;

                if (FileDialog.ShowDialog() == DialogResult.OK)
                {
                    NoImageText.Opacity = 0;
                    Directory.CreateDirectory(
                        Saved.SaveLocation + "Games\\Custom Library\\" + Game.ID
                    );
                    if (File.Exists(Game.Picture))
                    {
                        GamePic.ImageSource = null;
                        if (Functions.UnlockFile(Game.Picture))
                            File.Delete(Game.Picture);
                    }
                    Functions.CopyFile(
                        FileDialog.FileName,
                        Saved.SaveLocation
                            + "Games\\Custom Library\\"
                            + Game.ID
                            + "\\image"
                            + Path.GetExtension(FileDialog.FileName)
                    );
                    Game.Picture =
                        Saved.SaveLocation
                        + "Games\\Custom Library\\"
                        + Game.ID
                        + "\\image"
                        + Path.GetExtension(FileDialog.FileName);
                    byte[] bytes = Functions.ReadFileAsBytes(Game.Picture);
                    GamePic.ImageSource = Functions.LoadBitmapFromBytes(bytes);
                }
                await Instances.MainViewInstance.BlurFunc(1);
            }
        }

        private void SelectEXE_Click(object sender, RoutedEventArgs e)
        {
            Instances.MainViewInstance.BlurFunc();
            using (var FileDialog = new OpenFileDialog())
            {
                FileDialog.Title = "Select the executable for the game";
                FileDialog.Filter = "EXE, CMD, & BAT|*.exe;*.bat;*.cmd";
                FileDialog.RestoreDirectory = true;

                if (FileDialog.ShowDialog() == DialogResult.OK)
                {
                    Game.IsReady = true;
                    Game.GameExe = FileDialog.FileName;
                    GameEXE.Text = FileDialog.FileName;
                }
            }
            Instances.MainViewInstance.BlurFunc(1);
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (GameNameText.Text.Length < 1 || GameEXE.Text.Length < 1)
            {
                System.Windows.Forms.MessageBox.Show(
                    "Make sure to provide a valid Game Name/EXE file",
                    "BetaManager | Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }
            if (!File.Exists(GameEXE.Text))
            {
                System.Windows.Forms.MessageBox.Show(
                    "I couldn't find the EXE file, you sure you selected the right now?",
                    "BetaManager | Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }
            Game.IsCustom = true;
            Game.Name = GameNameText.Text;
            Game.InstallDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Functions.AddGameToLibrary(Game);
            Functions.SaveLibrary();
            await new Functions().FadeOut(this);
            this.IsHitTestVisible = false;
            Instances.LibraryViewInstance.BlurFunc(1);
            Instances.LibraryViewInstance.ListViewProducts.Items.Refresh();
        }

        private bool IsMouseOutside(MouseButtonEventArgs e, UIElement i)
        {
            Point relativePosition = e.GetPosition(i);

            return relativePosition.X < 0
                || relativePosition.Y < 0
                || relativePosition.X >= this.ActualWidth
                || relativePosition.Y >= this.ActualHeight;
        }

        private async void RefreshImageLink_Click(object sender, RoutedEventArgs e)
        {
            if (GameImageURL.Text.Length == 0)
                return;
            Directory.CreateDirectory(Saved.SaveLocation + "Games\\Custom Library\\" + Game.ID);
            await Functions.DownloadFileAsync(
                GameImageURL.Text,
                Saved.SaveLocation
                    + "Games\\Custom Library\\"
                    + Game.ID
                    + "\\image"
                    + Path.GetExtension(new Uri(GameImageURL.Text).GetLeftPart(UriPartial.Path))
            );
            Game.Picture =
                Saved.SaveLocation
                + "Games\\Custom Library\\"
                + Game.ID
                + "\\image"
                + Path.GetExtension(new Uri(GameImageURL.Text).GetLeftPart(UriPartial.Path));
            byte[] bytes = Functions.ReadFileAsBytes(Game.Picture);
            GamePic.ImageSource = Functions.LoadBitmapFromBytes(bytes);
            NoImageText.Opacity = 0;
        }

        private void SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var FileDialog = new CommonOpenFileDialog())
            {
                FileDialog.Title = "Select the folder containing the game";
                FileDialog.IsFolderPicker = true;
                FileDialog.RestoreDirectory = true;

                if (FileDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    Game.FolderAvailable = true;
                    Game.Folder = FileDialog.FileName;
                    Game.SizeOnDisk = Functions.DirectorySize(FileDialog.FileName);
                    Game.SizeOnDiskString = Functions.FormatSize(Game.SizeOnDisk);
                    GameFolder.Text = FileDialog.FileName;
                }
            }
        }
    }
}
