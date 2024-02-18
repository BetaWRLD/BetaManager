using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using BetaManager.Classes;
using BetaManager.Models;
using static BetaManager.Views.MainView;

namespace BetaManager.Views.SettingsTabs
{
    /// <summary>
    /// Interaction logic for ProfileSettingsTab.xaml
    /// </summary>
    public partial class ProfileSettingsTab : System.Windows.Controls.UserControl
    {
        public ProfileSettingsTab()
        {
            this.Opacity = 0;
            InitializeComponent();
            Instances.ProfileSettingsTabInstance = this;
            DisplayNameText.Text = Saved.User.DisplayName;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await new Functions().FadeIn(this, 150);
            if (Saved.User.Guest)
            {
                UploadButton.IsEnabled = false;
                PFPBorder.IsEnabled = false;
                PFPBorder.Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        private async void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Saved.User.Guest)
                await Auth.Update("Users", "Name", DisplayNameText.Text, "ID", Saved.User.ID);
            new Functions().SendNotification(
                "BetaManager",
                "Successfully Updated Your Display Name",
                1
            );
            Saved.User.DisplayName = DisplayNameText.Text;

            await new Functions().FadeOut(Instances.MainViewInstance.DisplayName);
            Instances.MainViewInstance.DisplayName.Text = Saved.User.DisplayName;
            await new Functions().FadeIn(Instances.MainViewInstance.DisplayName);

            new Functions().FadeOut(SettingsButton);
            SettingsButton.IsEnabled = false;
        }

        private async void DisplayNameText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DisplayNameText.Text != Saved.User.DisplayName)
            {
                new Functions().FadeIn(SettingsButton);
                SettingsButton.IsEnabled = true;
            }
            else
            {
                new Functions().FadeOut(SettingsButton);
                SettingsButton.IsEnabled = false;
            }
        }

        private async void DisplayNameText_LostFocus(object sender, RoutedEventArgs e)
        {
            if (DisplayNameText.Text.Length < 1)
            {
                await new Functions().FadeOut(DisplayNameText);
                DisplayNameText.Text = Saved.User.DisplayName;
                await new Functions().FadeIn(DisplayNameText);
            }
        }

        private async void UserControl_MouseDown(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e
        )
        {
            System.Windows.Point mousePosition = e.GetPosition(DisplayNameText);

            if (
                mousePosition.X < 0
                || mousePosition.Y < 0
                || mousePosition.X >= DisplayNameText.ActualWidth
                || mousePosition.Y >= DisplayNameText.ActualHeight
            )
            {
                Keyboard.ClearFocus();
                if (DisplayNameText.Text.Length < 1)
                {
                    await new Functions().FadeOut(DisplayNameText);
                    DisplayNameText.Text = Saved.User.DisplayName;
                    await new Functions().FadeIn(DisplayNameText);
                }
            }
        }

        private async void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            Instances.MainViewInstance.BlurFunc();
            using (var FileDialog = new OpenFileDialog())
            {
                FileDialog.Title = "Update profile picture";
                FileDialog.Filter = "JPG, JPEG, & PNG|*.jpg;*.jpeg;*.png";
                FileDialog.RestoreDirectory = true;

                if (FileDialog.ShowDialog() == DialogResult.OK)
                {
                    if ((new FileInfo(FileDialog.FileName).Length / 1024) / 1024 > 15)
                    {
                        new Functions().SendNotification(
                            "BetaManager",
                            "Profile Picture File Cannot Exceed 15MB",
                            3
                        );
                    }
                    else
                    {
                        bool? result = await Auth.UpdatePFP(
                            Convert.ToBase64String(File.ReadAllBytes(FileDialog.FileName)),
                            Path.GetExtension(FileDialog.FileName)
                        );
                        if (result == true)
                        {
                            if (
                                File.Exists(
                                    Saved.SaveLocation
                                        + "User\\PFP"
                                        + Path.GetExtension(FileDialog.FileName)
                                )
                            )
                                File.Delete(
                                    Saved.SaveLocation
                                        + "User\\PFP"
                                        + Path.GetExtension(FileDialog.FileName)
                                );
                            File.Copy(
                                FileDialog.FileName,
                                Saved.SaveLocation
                                    + "User\\PFP"
                                    + Path.GetExtension(FileDialog.FileName)
                            );
                            Saved.User.ProfilePicture = Functions.LoadBitmapFromBytes(
                                Functions.ReadFileAsBytes(
                                    Saved.SaveLocation
                                        + "User\\PFP"
                                        + Path.GetExtension(FileDialog.FileName)
                                )
                            );
                            ProfilePicture.ImageSource = Saved.User.ProfilePicture;
                            MainViewMediator.MainViewInstance.UpdateProfilePicture(
                                Saved.User.ProfilePicture
                            );
                            new Functions().SendNotification(
                                "BetaManager",
                                "Successfully Updated Your Profile Picture",
                                1
                            );
                        }
                        else
                        {
                            await Instances.MainViewInstance.BlurFunc(1);
                            new Functions().SendNotification("BetaManager", "An error occuored", 3);
                        }
                    }
                }
                await Instances.MainViewInstance.BlurFunc(1);
            }
        }

        private async void UsernameText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Clipboard.SetText(Saved.User.Username);
            await new Functions().UpdateText(UsernameText, "Copied!");
            await Functions.SleepAsync(2000);
            await new Functions().UpdateText(UsernameText, Saved.User.Username);
        }

        private void GuestLoginButton_Click(object sender, RoutedEventArgs e)
        {
            Saved.User = new Models.UserModel
            {
                DisplayName = "guest",
                Username = "guest",
                Rank = "0",
                ID = "∞",
                Email = "guest",
                Downloads = 0,
                Ratings = 0,
                Guest = true,
                ProfilePicture = new Functions().Bitmap2BitmapImage(
                    new Bitmap(Properties.Resources.profile)
                ),
                Date = "idk, today?",
            };
            Instances.MainViewModel.LoadCurrentUserData();
            Instances.MainViewInstance.ProfilePicture.ImageSource =
                new Functions().Bitmap2BitmapImage(new Bitmap(Properties.Resources.profile));
            Instances.MainViewModel.LoginButtonVisibility = Visibility.Visible;
            Instances.MainViewModel.LoginButtonEnabled = true;
            Instances.GeneralSettingsTabModel.Refresh();
            SettingsModel.Username = null;
            SettingsModel.Password = null;
        }
    }
}
