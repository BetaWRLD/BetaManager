using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using BetaManager.Classes;
using BetaManager.Views.SettingsTabs;
using static BetaManager.Views.MainView;

namespace BetaManager.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : System.Windows.Controls.UserControl
    {
        public SettingsView()
        {
            this.Opacity = 0;
            InitializeComponent();
            CurrentView.Content = new GeneralSettingsTab();
            Instances.SettingsViewInstance = this;
        }

        private void ColorPicker_ColorSelected(ColorInfo info) { }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            new Functions().FadeIn(this, 250);
        }

        private void DefaultButton_Click(object sender, RoutedEventArgs e)
        {
            new Functions().DefaultColor();
        }

        private async void UploadPFPText_MouseLeftButtonDown(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e
        )
        {
            using (var FileDialog = new OpenFileDialog())
            {
                FileDialog.Filter = "JPG & PNG|*.jpg;*.png";
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
                        FileStream fs;
                        Byte[] bindata;
                        fs = new FileStream(FileDialog.FileName, FileMode.Open, FileAccess.Read);
                        bindata = new byte[Convert.ToInt32(fs.Length)];
                        fs.Read(bindata, 0, Convert.ToInt32(fs.Length));
                        switch (
                            await Auth.Update(
                                "Users",
                                "ProfilePicture",
                                Convert.ToBase64String(bindata),
                                "Username",
                                Saved.User.Username
                            )
                        )
                        {
                            case true:
                                Saved.User.ProfilePicture = new Functions().Bitmap2BitmapImage(
                                    new Bitmap(fs)
                                );
                                //ImageDisplay.ImageSource = Saved.User.ProfilePicture;
                                MainViewMediator.MainViewInstance.UpdateProfilePicture(
                                    Saved.User.ProfilePicture
                                );
                                new Functions().SendNotification(
                                    "BetaManager",
                                    "Successfully Updated Your Profile Picture",
                                    1
                                );
                                break;
                            case false:
                                new Functions().SendNotification(
                                    "BetaManager",
                                    "An error occuored",
                                    3
                                );
                                break;
                        }
                    }
                }
            }
        }

        private async void RemovePFPText_MouseLeftButtonDown(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e
        )
        {
            if (Saved.User.ProfilePicture == null)
            {
                new Functions().SendNotification(
                    "BetaManager",
                    "You Haven't Set a Profile Picture Yet",
                    3
                );
            }
            else
            {
                switch (
                    await Auth.Update(
                        "Users",
                        "ProfilePicture",
                        null,
                        "Username",
                        Saved.User.Username
                    )
                )
                {
                    case true:
                        Saved.User.ProfilePicture = null;
                        //ImageDisplay.ImageSource = new Functions().Bitmap2BitmapImage( new Bitmap( BetaManager.Properties.Resources.profile ) );
                        MainViewMediator.MainViewInstance.UpdateProfilePicture(
                            new Functions().Bitmap2BitmapImage(
                                new Bitmap(BetaManager.Properties.Resources.profile)
                            )
                        );
                        new Functions().SendNotification(
                            "BetaManager",
                            "Successfully Removed Your Profile Picture",
                            1
                        );
                        break;
                    case false:
                        new Functions().SendNotification("BetaManager", "An error occuored", 3);
                        break;
                }
            }
        }

        private async Task HideAll()
        {
            if (CurrentView.Content is GeneralSettingsTab)
            {
                if (Instances.GeneralSettingsTabInstance != null)
                {
                    await new Functions().FadeOut(Instances.GeneralSettingsTabInstance, 150);
                }
            }
            if (CurrentView.Content is ProfileSettingsTab)
            {
                if (Instances.ProfileSettingsTabInstance != null)
                {
                    await new Functions().FadeOut(Instances.ProfileSettingsTabInstance, 150);
                }
            }
        }

        private async void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(CurrentView.Content is ProfileSettingsTab))
                await HideAll();
            CurrentView.Content = Instances.ProfileSettingsTabInstance ?? new ProfileSettingsTab();
        }

        private async void GeneralButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(CurrentView.Content is GeneralSettingsTab))
                await HideAll();
            CurrentView.Content = Instances.GeneralSettingsTabInstance ?? new GeneralSettingsTab();
        }

        private async void SupportButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(CurrentView.Content is SupportMeSettingsTab))
                await HideAll();
            CurrentView.Content = new SupportMeSettingsTab();
        }
    }
}
