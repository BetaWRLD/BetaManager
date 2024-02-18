using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using BetaManager.Classes;
using BetaManager.Models;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace BetaManager.Views
{
    public partial class EditLibraryGameView : System.Windows.Controls.UserControl
    {
        public EditLibraryGameView(LibraryGameModel game)
        {
            this.Opacity = 0;
            InitializeComponent();
            Instances.EditLibraryGameViewModel.Game = game;
        }

        private async void MainViewInstance_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (UIFunctions.IsMouseOutside(e, this) && this.Opacity == 1)
            {
                await new Functions().FadeOut(this);
                this.IsHitTestVisible = false;
                Instances.LibraryViewInstance.BlurFunc(1);
                Instances.LibraryViewInstance.ListViewProducts.Items.Refresh();
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Instances.EditLibraryGameViewModel.Game.Picture != null)
            {
                GamePic.ImageSource = new BitmapImage(
                    new Uri(Instances.EditLibraryGameViewModel.Game.Picture)
                );
                NoImageText.Opacity = 0;
            }
            GameNameText.Text = Instances.EditLibraryGameViewModel.Game.Name;
            GameFolder.Text = Instances.EditLibraryGameViewModel.Game.Folder;
            GameEXE.Text = Instances.EditLibraryGameViewModel.Game.GameExe;
            await new Functions().FadeIn(this);
            this.IsHitTestVisible = true;
            Instances.MainViewInstance.PreviewMouseDown += MainViewInstance_PreviewMouseDown;
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
                    Instances.EditLibraryGameViewModel.Game.Picture = FileDialog.FileName;
                    GamePic.ImageSource = new BitmapImage(
                        new Uri(Instances.EditLibraryGameViewModel.Game.Picture)
                    );
                    NoImageText.Opacity = 0;
                    Directory.CreateDirectory(
                        Saved.SaveLocation
                            + "Games\\Custom Library\\"
                            + Instances.EditLibraryGameViewModel.Game.ID
                    );
                    if (
                        File.Exists(
                            Saved.SaveLocation
                                + "Games\\Custom Library\\"
                                + Instances.EditLibraryGameViewModel.Game.ID
                                + "\\image"
                                + Path.GetExtension(FileDialog.FileName)
                        )
                    )
                        File.Delete(
                            Saved.SaveLocation
                                + "Games\\Custom Library\\"
                                + Instances.EditLibraryGameViewModel.Game.ID
                                + "\\image"
                                + Path.GetExtension(FileDialog.FileName)
                        );
                    File.Copy(
                        FileDialog.FileName,
                        Saved.SaveLocation
                            + "Games\\Custom Library\\"
                            + Instances.EditLibraryGameViewModel.Game.ID
                            + "\\image"
                            + Path.GetExtension(FileDialog.FileName)
                    );
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
                    Instances.EditLibraryGameViewModel.Game.IsReady = true;
                    Instances.EditLibraryGameViewModel.Game.GameExe = FileDialog.FileName;
                    GameEXE.Text = FileDialog.FileName;
                }
            }
            Instances.MainViewInstance.BlurFunc(1);
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
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
            Instances.EditLibraryGameViewModel.Game.GameExe = GameEXE.Text;
            Instances.EditLibraryGameViewModel.Game.Name = GameNameText.Text;
            if (GameFolder.Text.Length > 3)
            {
                Instances.EditLibraryGameViewModel.Game.FolderAvailable = true;
                Instances.EditLibraryGameViewModel.Game.Folder = GameFolder.Text;
                Instances.EditLibraryGameViewModel.Game.SizeOnDisk = Functions.DirectorySize(
                    GameFolder.Text
                );
            }
            else
            {
                Instances.EditLibraryGameViewModel.Game.Folder = null;
                Instances.EditLibraryGameViewModel.Game.FolderAvailable = false;
                Instances.EditLibraryGameViewModel.Game.SizeOnDisk = 0;
            }
            int index = Saved.LibraryGames.FindIndex(
                x => x.ID == Instances.EditLibraryGameViewModel.Game.ID
            );

            if (index != -1)
            {
                Saved.LibraryGames[index] = Instances.EditLibraryGameViewModel.Game;
            }
            Functions.SaveLibrary();
            Instances.LibraryViewModel.Refresh();
            await new Functions().FadeOut(this);
            this.IsHitTestVisible = false;
            Instances.LibraryViewInstance.BlurFunc(1);
            Instances.LibraryViewInstance.ListViewProducts.Items.Refresh();
            new Functions().SendNotification(
                "BetaManager",
                $"Successfully updated {Instances.EditLibraryGameViewModel.Game.Name}",
                1
            );
        }

        private void GameNameText_TextChanged(
            object sender,
            System.Windows.Controls.TextChangedEventArgs e
        ) { }

        private async void Reset_Click(object sender, RoutedEventArgs e)
        {
            await new Functions().FadeOut(this);
            this.IsHitTestVisible = false;
            Instances.LibraryViewInstance.BlurFunc(1);
            Instances.LibraryViewInstance.ListViewProducts.Items.Refresh();
        }

        private void SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var FileDialog = new CommonOpenFileDialog())
            {
                FileDialog.Title =
                    "Select the folder containing " + Instances.EditLibraryGameViewModel.Game.Name;
                FileDialog.IsFolderPicker = true;
                FileDialog.RestoreDirectory = true;

                if (FileDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    LibraryGameModel game = Saved.LibraryGames.Find(
                        x => x.ID == Instances.EditLibraryGameViewModel.Game.ID
                    );
                    game.FolderAvailable = true;
                    game.Folder = FileDialog.FileName;
                    game.SizeOnDisk = Functions.DirectorySize(FileDialog.FileName);
                    game.SizeOnDiskString = Functions.FormatSize(game.SizeOnDisk);
                    GameFolder.Text = FileDialog.FileName;
                }
            }
        }
    }
}
