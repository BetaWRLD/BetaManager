using System.Windows;
using BetaManager.Classes;
using BetaManager.Models;

namespace BetaManager.ViewModels
{
    public class GeneralSettingsTabModel : ViewModelBase
    {
        public UserModel User
        {
            get { return Saved.User; }
        }
        public bool AutoUpdatingChecked
        {
            get { return SettingsModel.AutoUpdateChecking; }
            set
            {
                SettingsModel.AutoUpdateChecking = value;
                OnPropertyChanged(nameof(AutoUpdatingChecked));
            }
        }
        public bool StartSeedingOnCompleteChecked
        {
            get { return SettingsModel.StartSeedingOnComplete; }
            set
            {
                SettingsModel.StartSeedingOnComplete = value;
                OnPropertyChanged(nameof(StartSeedingOnCompleteChecked));
            }
        }
        public bool MinimizeOnGameLaunchChecked
        {
            get { return SettingsModel.MinimizeOnGameLaunch; }
            set
            {
                SettingsModel.MinimizeOnGameLaunch = value;
                OnPropertyChanged(nameof(MinimizeOnGameLaunchChecked));
            }
        }
        public bool OpenOnStartupChecked
        {
            get { return SettingsModel.RunOnStartup; }
            set
            {
                SettingsModel.RunOnStartup = value;
                OnPropertyChanged(nameof(OpenOnStartupChecked));
            }
        }
        public bool LogoutButtonEnabled
        {
            get { return !User.Guest; }
        }
        public Visibility LogoutButtonVisibility
        {
            get { return User.Guest != true ? Visibility.Visible : Visibility.Hidden; }
        }
        public string TotalGamesToLoad
        {
            get { return SettingsModel.TotalGamesToLoad.ToString(); }
            set
            {
                SettingsModel.TotalGamesToLoad = Functions.ToInt(value);
                OnPropertyChanged(nameof(TotalGamesToLoad));
            }
        }
        public string DefaultDownloadLocation
        {
            get { return SettingsModel.DefaultDownloadLocation.ToString(); }
            set
            {
                SettingsModel.DefaultDownloadLocation = value;
                OnPropertyChanged(nameof(DefaultDownloadLocation));
            }
        }
        public string SelectedCloseBehaviour
        {
            get { return SettingsModel.CloseBehaviour.ToString(); }
            set
            {
                SettingsModel.CloseBehaviour = Functions.ToInt(value);
                OnPropertyChanged(nameof(SelectedCloseBehaviour));
            }
        }

        public void Refresh()
        {
            OnPropertyChanged(nameof(User));
            OnPropertyChanged(nameof(LogoutButtonVisibility));
            OnPropertyChanged(nameof(LogoutButtonEnabled));
        }

        public GeneralSettingsTabModel()
        {
            Instances.GeneralSettingsTabModel = this;
        }
    }
}
