using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BetaManager.Classes;
using BetaManager.Models;
using BetaManager.Repositories;
using BetaManager.Views;
using BetaManager.Views.SettingsTabs;
using FontAwesome.Sharp;

namespace BetaManager.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        //Fields

        private UserModel _currentUserAccount;
        private UserControl _currentChildView;
        private string _caption;
        private Visibility _loginButtonVisibility = Visibility.Visible;
        private bool _loginButtonEnabled = true;
        private IconChar _icon;
        private static ImageSource _pfp;
        private IUserRepository userRepository;

        //Properties
        public UserModel CurrentUserAccount
        {
            get { return _currentUserAccount; }
            set
            {
                _currentUserAccount = value;
                OnPropertyChanged(nameof(CurrentUserAccount));
            }
        }
        public UserControl CurrentChildView
        {
            get { return _currentChildView; }
            set
            {
                _currentChildView = value;
                OnPropertyChanged(nameof(CurrentChildView));
            }
        }
        public string Caption
        {
            get { return _caption; }
            set
            {
                _caption = value;
                OnPropertyChanged(nameof(Caption));
            }
        }

        public IconChar Icon
        {
            get { return _icon; }
            set
            {
                _icon = value;
                OnPropertyChanged(nameof(Icon));
            }
        }

        public Visibility LoginButtonVisibility
        {
            get { return _loginButtonVisibility; }
            set
            {
                _loginButtonVisibility = value;
                OnPropertyChanged(nameof(LoginButtonVisibility));
            }
        }

        public bool LoginButtonEnabled
        {
            get { return _loginButtonEnabled; }
            set
            {
                _loginButtonEnabled = value;
                OnPropertyChanged(nameof(LoginButtonEnabled));
            }
        }

        public ImageSource ProfilePicture
        {
            get { return _pfp; }
            set
            {
                if (_pfp != value)
                {
                    _pfp = value;
                    OnPropertyChanged(nameof(ProfilePicture)); // Error occurs here
                }
            }
        }

        //--> Commands
        public ICommand ShowGamesViewCommand { get; }
        public ICommand ShowHomeViewCommand { get; }
        public ICommand ShowGameViewCommand { get; }
        public ICommand ShowLibraryViewCommand { get; }
        public ICommand ShowSettingsViewCommand { get; }
        public ICommand ShowManagerViewCommand { get; }
        public ICommand ShowProfileViewCommand { get; }

        public MainViewModel()
        {
            Instances.MainViewModel = this;
            userRepository = new UserRepository();
            CurrentUserAccount = new UserModel();
            //Initialize commands
            ShowGamesViewCommand = new ViewModelCommand(ExecuteShowGamesViewCommand);
            ShowHomeViewCommand = new ViewModelCommand(ExecuteShowHomeViewCommand);
            ShowLibraryViewCommand = new ViewModelCommand(ExecuteShowLibraryViewCommand);
            ShowManagerViewCommand = new ViewModelCommand(ExecuteShowManagerViewCommand);
            ShowSettingsViewCommand = new ViewModelCommand(ExecuteShowSettingsViewCommand);
            ShowProfileViewCommand = new ViewModelCommand(ExecuteShowProfileViewCommand);
            //Default view
            ExecuteShowHomeViewCommand(null);
            LoadCurrentUserData();
        }

        private async Task HideAll()
        {
            if (CurrentChildView is GamesView)
            {
                if (Instances.GamesViewInstance != null)
                {
                    await new Functions().FadeOut(Instances.GamesViewInstance, 150);
                }
            }

            if (CurrentChildView is HomeView)
            {
                if (Instances.HomeViewInstance != null)
                {
                    await new Functions().FadeOut(Instances.HomeViewInstance, 150);
                }
            }

            if (CurrentChildView is ManagerView)
            {
                if (Instances.ManagerViewInstance != null)
                {
                    await new Functions().FadeOut(Instances.ManagerViewInstance, 150);
                }
            }

            if (CurrentChildView is SettingsView)
            {
                if (Instances.SettingsViewInstance != null)
                {
                    await new Functions().FadeOut(Instances.SettingsViewInstance, 150);
                }
            }

            if (CurrentChildView is GameView)
            {
                if (Instances.GameViewInstance != null)
                {
                    await new Functions().FadeOut(Instances.GameViewInstance, 150);
                }
            }

            if (CurrentChildView is LibraryView)
            {
                if (Instances.LibraryViewInstance != null)
                {
                    await new Functions().FadeOut(Instances.LibraryViewInstance, 150);
                }
            }
        }

        public async void ExecuteShowGameViewCommand()
        {
            if (!(CurrentChildView is GameView))
                await HideAll();
            CurrentChildView = new GameView();
            Caption = Saved.SelectedGame.Name;
            Icon = IconChar.Gamepad;
        }

        private async void ExecuteShowSettingsViewCommand(object obj)
        {
            if (!(CurrentChildView is SettingsView))
                await HideAll();
            CurrentChildView = Instances.SettingsViewInstance ?? new SettingsView();
            Caption = "Settings";
            Icon = IconChar.Gears;
        }

        private async void ExecuteShowProfileViewCommand(object obj)
        {
            if (!(CurrentChildView is ProfileSettingsTab))
                await HideAll();
            CurrentChildView = Instances.ProfileSettingsTabInstance ?? new ProfileSettingsTab();
            Caption = "Settings";
            Icon = IconChar.Gears;
        }

        private async void ExecuteShowManagerViewCommand(object obj)
        {
            if (!(CurrentChildView is ManagerView))
                await HideAll();
            CurrentChildView = Instances.ManagerViewInstance ?? new ManagerView();
            Caption = "Manager";
            Icon = IconChar.Gears;
        }

        private async void ExecuteShowGamesViewCommand(object obj)
        {
            if (!(CurrentChildView is GamesView))
                await HideAll();
            CurrentChildView = Instances.GamesViewInstance ?? new GamesView();
        }

        private async void ExecuteShowHomeViewCommand(object obj)
        {
            if (!(CurrentChildView is HomeView))
                await HideAll();
            CurrentChildView = Instances.HomeViewInstance ?? new HomeView();
            Caption = "Home";
            Icon = IconChar.Home;
        }

        private async void ExecuteShowLibraryViewCommand(object obj)
        {
            if (!(CurrentChildView is LibraryView))
                await HideAll();
            CurrentChildView = new LibraryView();
            Caption = "Library";
            Icon = IconChar.Home;
        }

        public void LoadCurrentUserData()
        {
            CurrentUserAccount = Saved.User;
        }
    }
}
