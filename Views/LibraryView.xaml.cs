using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using BetaManager.Classes;
using BetaManager.Models;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace BetaManager.Views
{
    /// <summary>
    /// Interaction logic for ManagerView.xaml
    /// </summary>
    public partial class LibraryView : System.Windows.Controls.UserControl
    {
        public LibraryView()
        {
            this.Opacity = 0;
            InitializeComponent();
            Instances.LibraryViewInstance = this;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            new Functions().FadeIn(this);
        }

        private void GameButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Windows.Controls.Button button = (System.Windows.Controls.Button)sender;
            LibraryGameModel ClickedGame = (LibraryGameModel)button.DataContext;
            if (!ClickedGame.IsReady)
            {
                Instances.MainViewInstance.BlurFunc();
                using (var FileDialog = new OpenFileDialog())
                {
                    FileDialog.Title = "Select the executable for " + ClickedGame.Name;
                    FileDialog.Filter = "EXE, CMD, & BAT|*.exe;*.bat;*.cmd";
                    FileDialog.RestoreDirectory = true;

                    if (FileDialog.ShowDialog() == DialogResult.OK)
                    {
                        LibraryGameModel game = Saved.LibraryGames.Find(
                            x => x.ID == ClickedGame.ID
                        );
                        game.IsReady = true;
                        game.GameExe = FileDialog.FileName;
                        Instances.LibraryViewModel.Refresh();
                        ListViewProducts.Items.Refresh();
                        Functions.SaveLibrary();
                    }
                }
                Instances.MainViewInstance.BlurFunc(1);
            }
            else
            {
                ClickedGame.LastPlayDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                ClickedGame.LastPlayDateString = Functions.TimeSince(
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                );
                int index = Saved.LibraryGames.FindIndex(x => x.ID == ClickedGame.ID);

                if (index != -1)
                {
                    Saved.LibraryGames[index] = ClickedGame;
                }
                Instances.LibraryViewModel.Refresh();
                ListViewProducts.Items.Refresh();
                Functions.SaveLibrary();
                Functions.StartProccess(ClickedGame.GameExe);
                if (SettingsModel.MinimizeOnGameLaunch)
                    Instances.MainViewInstance.MinimizeToTray();
            }
        }

        private async void SearchTextBox_TextChanged(
            object sender,
            System.Windows.Controls.TextChangedEventArgs e
        )
        {
            await new Functions().FadeOut(ListViewProducts, 100);
            if (SearchTextBox.Text.Length > 0)
                ListViewProducts.ItemsSource = Saved.LibraryGames.FindAll(
                    match =>
                        match.Name.ToLower().Contains(SearchTextBox.Text.ToLower())
                        || SearchTextBox.Text.ToLower().Contains(match.Name.ToLower())
                );
            else
                ListViewProducts.ItemsSource = Saved.LibraryGames;
            await new Functions().FadeIn(ListViewProducts, 100);
        }

        private void UninstallGame_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem block = (System.Windows.Controls.MenuItem)sender;
            LibraryGameModel ClickedGame = (LibraryGameModel)block.DataContext;
            Saved.LibraryGames.Remove(Saved.LibraryGames.Find(item => item.ID == ClickedGame.ID));
            try
            {
                if (
                    ClickedGame.IsCustom
                    && Directory.Exists(
                        Saved.SaveLocation + "Games\\Custom Library\\" + ClickedGame.ID
                    )
                )
                    Directory.Delete(
                        Saved.SaveLocation + "Games\\Custom Library\\" + ClickedGame.ID,
                        true
                    );
            }
            catch { }
            Functions.SearchShortcutsOnDesktop("betamanager://opengame/" + ClickedGame.ID);
            Functions.SaveLibrary(Saved.LibraryGames);
            ListViewProducts.Items.Refresh();
            new Functions().SendNotification(
                "BetaManager",
                $"Successfully removed {ClickedGame.Name}!",
                1
            );
        }

        private void SizeText_MouseLeftButtonDown(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e
        )
        {
            System.Windows.Controls.TextBlock block = (System.Windows.Controls.TextBlock)sender;
            LibraryGameModel ClickedGame = (LibraryGameModel)block.DataContext;
            if (!ClickedGame.FolderAvailable)
            {
                Instances.MainViewInstance.BlurFunc();
                using (var FileDialog = new CommonOpenFileDialog())
                {
                    FileDialog.Title = "Select the folder containing " + ClickedGame.Name;
                    FileDialog.IsFolderPicker = true;
                    FileDialog.RestoreDirectory = true;

                    if (FileDialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        LibraryGameModel game = Saved.LibraryGames.Find(
                            x => x.ID == ClickedGame.ID
                        );
                        game.FolderAvailable = true;
                        game.Folder = FileDialog.FileName;
                        game.SizeOnDisk = Functions.DirectorySize(FileDialog.FileName);
                        game.SizeOnDiskString = Functions.FormatSize(game.SizeOnDisk);
                        Instances.LibraryViewModel.Refresh();
                        ListViewProducts.Items.Refresh();
                        Functions.SaveLibrary();
                    }
                }
                Instances.MainViewInstance.BlurFunc(1);
            }
        }

        public async Task BlurFunc(int mode = 0)
        {
            BlurEffect blurEffect = new BlurEffect();
            DoubleAnimation blurAnimation;

            var tcs = new TaskCompletionSource<bool>();

            if (mode == 0)
            {
                TopBorder.Visibility = Visibility.Visible;
                blurEffect.Radius = 0.01; // Start with a small non-zero value
                blurAnimation = new DoubleAnimation(20, TimeSpan.FromSeconds(0.25));
                blurAnimation.Completed += (s, e) =>
                {
                    tcs.SetResult(true);
                };
            }
            else
            {
                blurAnimation = new DoubleAnimation(0, TimeSpan.FromSeconds(0.25));
                blurAnimation.Completed += (s, e) =>
                {
                    TopBorder.Visibility = Visibility.Hidden;
                    tcs.SetResult(true);
                };
            }

            blurEffect.BeginAnimation(BlurEffect.RadiusProperty, blurAnimation);
            MainGrid.Effect = blurEffect;

            await tcs.Task;
        }

        private async void AddGame_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            BlurFunc();
            CurrentView.Content = new NewLibraryGame();
        }

        private void EditGame_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem block = (System.Windows.Controls.MenuItem)sender;
            LibraryGameModel ClickedGame = (LibraryGameModel)block.DataContext;
            BlurFunc();
            CurrentView.Content = new EditLibraryGameView(ClickedGame);
        }

        private void GameOptions_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button gameOptionsButton =
                (System.Windows.Controls.Button)sender;
            System.Windows.Controls.ContextMenu contextMenu =
                new System.Windows.Controls.ContextMenu();
            contextMenu.Style = (Style)FindResource("ModernContextMenuStyle");
            contextMenu.Background = new SolidColorBrush(Color.FromArgb(34, 255, 255, 255));

            System.Windows.Controls.MenuItem createShortcutMenuItem =
                new System.Windows.Controls.MenuItem
                {
                    Name = "CreateShortcut",
                    Header = "Create Desktop Shortcut",
                    FontFamily = (FontFamily)FindResource("Medium"),
                    FontSize = 12,
                    Foreground = (Brush)FindResource("ActiveText"),
                    Style = (Style)FindResource("ModernContextMenuItemStyle"),
                };
            createShortcutMenuItem.Click += CreateShortcut_Click;

            System.Windows.Controls.MenuItem editGame = new System.Windows.Controls.MenuItem
            {
                Name = "EditGame",
                Header = "Edit Game",
                FontFamily = (FontFamily)FindResource("Medium"),
                FontSize = 12,
                Foreground = (Brush)FindResource("ActiveText"),
                Style = (Style)FindResource("ModernContextMenuItemStyle"),
            };
            editGame.Click += EditGame_Click;

            System.Windows.Controls.MenuItem uninstallGameMenuItem =
                new System.Windows.Controls.MenuItem
                {
                    Name = "UninstallGame",
                    Header = "Remove Game",
                    FontFamily = (FontFamily)FindResource("Medium"),
                    Foreground = (Brush)FindResource("RedColor"),
                    Style = (Style)FindResource("ModernContextMenuItemStyle"),
                };
            uninstallGameMenuItem.Click += UninstallGame_Click;

            contextMenu.Items.Add(editGame);
            contextMenu.Items.Add(createShortcutMenuItem);
            contextMenu.Items.Add(uninstallGameMenuItem);

            SetContextMenuDataContext(contextMenu, gameOptionsButton);

            contextMenu.IsOpen = true;
        }

        private void CreateShortcut_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem block = (System.Windows.Controls.MenuItem)sender;
            LibraryGameModel ClickedGame = (LibraryGameModel)block.DataContext;
            //string icon = null;
            //if (File.Exists(ClickedGame.Picture))
            //{
            //    Functions.ConvertImageToIcon(
            //        ClickedGame.Picture,
            //        Path.GetDirectoryName(ClickedGame.Picture) + "icon.ico"
            //    );
            //    icon = Path.GetDirectoryName(ClickedGame.Picture) + "icon.ico";
            //}
            Functions.SearchShortcutsOnDesktop(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "betamanager://opengame/" + ClickedGame.ID
            );
            Functions.CreateDesktopShortcut(ClickedGame.Name, ClickedGame.ID, ClickedGame.GameExe);
            new Functions().SendNotification(
                "BetaManager",
                $"Added {ClickedGame.Name} to the desktop",
                1
            );
        }

        private void SetContextMenuDataContext(
            System.Windows.Controls.ContextMenu contextMenu,
            FrameworkElement owner
        )
        {
            if (owner != null)
            {
                contextMenu.DataContext = owner.DataContext;
            }
        }
    }
}
