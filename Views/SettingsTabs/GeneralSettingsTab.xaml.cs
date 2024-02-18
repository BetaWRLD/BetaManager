using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using BetaManager.Classes;
using BetaManager.Models;

namespace BetaManager.Views.SettingsTabs
{
    /// <summary>
    /// Interaction logic for GeneralSettingsTab.xaml
    /// </summary>
    public partial class GeneralSettingsTab : UserControl
    {
        public GeneralSettingsTab()
        {
            this.Opacity = 0;
            InitializeComponent();
            Instances.GeneralSettingsTabInstance = this;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (NetworkInterfaces.Items == null)
            {
                NetworkInterfaces.Items.Clear();
                NetworkInterfaces.Items.Add(new { Name = "Disabled" });
                foreach (VPNInterfaceModel VPNM in Functions.GetNetworkInterfaces())
                {
                    NetworkInterfaces.Items.Add(VPNM);
                }
            }
            new Functions().FadeIn(this);
        }

        private void DownloadSpeedLimit_TextChanged(object sender, TextChangedEventArgs e) { }

        private void AutoUpdateChecking_Unchecked(object sender, RoutedEventArgs e)
        {
            Instances.GeneralSettingsTabModel.AutoUpdatingChecked = false;
        }

        private void AutoUpdateChecking_Checked(object sender, RoutedEventArgs e)
        {
            Instances.GeneralSettingsTabModel.AutoUpdatingChecked = true;
        }

        private async void CheckForUpdatesButton_Click(object sender, RoutedEventArgs e)
        {
            await Instances.MainViewInstance.BlurFunc();
            Instances.MainViewInstance.CheckUpdates(true);
        }

        private void TotalGames_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SaveTotalGames == null)
                return;
            int? num = Functions.ToInt(Regex.Replace(TotalGames.Text, "[^0-9]", ""));
            if (num > 500)
                num = 500;
            if (num != null && num < 1)
                num = 1;
            TotalGames.Text = num.ToString();
            TotalGames.CaretIndex = TotalGames.Text.Length;
            if (TotalGames.Text != SettingsModel.TotalGamesToLoad.ToString())
            {
                new Functions().FadeIn(SaveTotalGames);
                SaveTotalGames.IsEnabled = true;
            }
            else
            {
                new Functions().FadeOut(SaveTotalGames);
                SaveTotalGames.IsEnabled = false;
            }
        }

        private void SaveTotalGames_Click(object sender, RoutedEventArgs e)
        {
            SettingsModel.TotalGamesToLoad = Functions.ToInt(
                Regex.Replace(TotalGames.Text, "[^0-9]", "")
            );
            new Functions().FadeOut(SaveTotalGames);
            SaveTotalGames.IsEnabled = false;
        }

        private void StartSeedingOnComplete_Click(object sender, RoutedEventArgs e)
        {
            SettingsModel.StartSeedingOnComplete = (bool)StartSeedingOnComplete.IsChecked;
        }

        private void DefaultDownloadLocation_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SaveDefaultDownloadLocation == null)
                return;
            if (DefaultDownloadLocation.Text != SettingsModel.DefaultDownloadLocation)
            {
                new Functions().FadeIn(SaveDefaultDownloadLocation);
                SaveDefaultDownloadLocation.IsEnabled = true;
            }
            else
            {
                new Functions().FadeOut(SaveDefaultDownloadLocation);
                SaveDefaultDownloadLocation.IsEnabled = false;
            }
        }

        private void SaveDefaultDownloadLocation_Click(object sender, RoutedEventArgs e)
        {
            SettingsModel.TotalGamesToLoad = Functions.ToInt(
                Regex.Replace(TotalGames.Text, "[^0-9]", "")
            );
            new Functions().FadeOut(SaveTotalGames);
            SaveTotalGames.IsEnabled = false;
        }

        private void CloseBehaviour_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Instances.GeneralSettingsTabModel.SelectedCloseBehaviour =
                CloseBehaviour.SelectedIndex.ToString();
        }

        private void NetworkInterfaces_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Saved.KillSwitchTrigger = Instances
                    .NetworkInterfacesHandler
                    .InterfacesList[NetworkInterfaces.SelectedIndex - 1]
                    .Name;
            }
            catch
            {
                Saved.KillSwitchTrigger = null;
            }
        }

        public void NetworkInterfacesChanged(object sender, EventArgs e) { }

        private void NetworkInterfaceKillSwitchEnabled_Click(object sender, RoutedEventArgs e)
        {
            Saved.KillSwitchON = (bool)NetworkInterfaceKillSwitchEnabled.IsChecked;
            Saved.KillSwitchMode = 0;
        }

        private void RefreshNetworkInterfaces_Click(object sender, RoutedEventArgs e)
        {
            NetworkInterfaces.SelectedIndex = 0;
            NetworkInterfaces.Items.Clear();
            NetworkInterfaces.Items.Add(new { Name = "Disabled" });
            foreach (VPNInterfaceModel VPNM in Functions.GetNetworkInterfaces())
            {
                NetworkInterfaces.Items.Add(VPNM);
            }
        }

        private void MinimizeOnGameLaunch_Click(object sender, RoutedEventArgs e)
        {
            SettingsModel.MinimizeOnGameLaunch = (bool)MinimizeOnGameLaunch.IsChecked;
        }

        private void OpenOnStartup_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)OpenOnStartup.IsChecked == true)
            {
                Functions.AddAppToStartup(
                    "BetaManager",
                    System.Reflection.Assembly.GetExecutingAssembly().Location
                );
            }
            else
            {
                Functions.RemoveAppToStartup("BetaManager");
            }
            SettingsModel.RunOnStartup = (bool)OpenOnStartup.IsChecked;
        }
    }
}
