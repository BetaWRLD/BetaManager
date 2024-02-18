using System;
using System.Net;
using System.Security;
using System.Security.Principal;
using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;
using BetaManager.Classes;
using BetaManager.Models;
using BetaManager.Repositories;

namespace BetaManager.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        //Fields
        public static bool _savePassword = false;
        private string _username;
        private SecureString _password;
        private string _errorMessage;
        private string _LoginText = "LOG IN";
        private bool _isViewVisible = true;
        private bool _isLoginEnabled = true;

        private IUserRepository userRepository;

        //Properties
        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public SecureString Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public string LoginText
        {
            get { return _LoginText; }
            set
            {
                _LoginText = value;
                OnPropertyChanged(nameof(LoginText));
            }
        }

        public bool IsViewVisible
        {
            get { return _isViewVisible; }
            set
            {
                _isViewVisible = value;
                OnPropertyChanged(nameof(IsViewVisible));
            }
        }

        public bool LoginEnabled
        {
            get { return _isLoginEnabled; }
            set
            {
                _isLoginEnabled = value;
                OnPropertyChanged(nameof(LoginEnabled));
            }
        }

        //-> Commands
        public ICommand LoginCommand { get; }
        public ICommand RecoverPasswordCommand { get; }
        public ICommand ShowPasswordCommand { get; }
        public ICommand RememberPasswordCommand { get; }

        //Constructor
        public LoginViewModel()
        {
            userRepository = new UserRepository();
            LoginCommand = new ViewModelCommand(ExecuteLoginCommand, CanExecuteLoginCommand);
            RecoverPasswordCommand = new ViewModelCommand(p => ExecuteRecoverPassCommand("", ""));
        }

        private bool CanExecuteLoginCommand(object obj)
        {
            bool validData;
            if (
                string.IsNullOrWhiteSpace(Username)
                || Username.Length < 3
                || Password == null
                || Password.Length < 3
            )
                validData = false;
            else
                validData = true;
            return validData;
        }

        private void ExecuteLoginCommand(object obj)
        {
            Dispatcher uiDispatcher = Dispatcher.CurrentDispatcher;

            new Thread(async () =>
            {
                uiDispatcher.Invoke(() =>
                {
                    LoginEnabled = false;
                    LoginText = "Please Wait";
                });

                bool isValidUser = await Auth.Login(
                    new NetworkCredential(Username, Password),
                    uiDispatcher
                );
                if (isValidUser == null)
                {
                    LoginEnabled = true;
                    LoginText = "LOGIN";
                }

                uiDispatcher.Invoke(() =>
                {
                    if (isValidUser)
                    {
                        if (!Saved.User.Verified)
                        {
                            LoginEnabled = true;
                            LoginText = "LOG IN";
                            ErrorMessage =
                                "Account not verified. You may check your email or contact the support team";
                        }
                        else
                        {
                            Instances.DiscordClient.LoggedIn();
                            if (_savePassword)
                            {
                                try
                                {
                                    SettingsModel.GuestUser = false;
                                    Functions.SaveSetting("Username", Username);
                                    Functions.SaveSetting(
                                        "Password",
                                        Functions.SecureStringToString(Password)
                                    );
                                }
                                catch
                                {
                                    new Functions().SendNotification(
                                        "BetaManager",
                                        "Couldn't save the login info into the registry!",
                                        3
                                    );
                                }
                            }
                            Thread.CurrentPrincipal = new GenericPrincipal(
                                new GenericIdentity(Username),
                                null
                            );
                            IsViewVisible = false;
                        }
                    }
                    else
                    {
                        LoginEnabled = true;
                        LoginText = "LOG IN";
                        ErrorMessage = "* Invalid username or password";
                    }
                });
            }).Start();
        }

        private void ExecuteRecoverPassCommand(string username, string email)
        {
            throw new NotImplementedException();
        }
    }
}
