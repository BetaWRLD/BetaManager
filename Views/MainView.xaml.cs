using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using BetaManager.Classes;
using BetaManager.Models;
using Newtonsoft.Json;

namespace BetaManager.Views
{
    internal enum AccentState
    {
        ACCENT_DISABLED = 1,
        ACCENT_ENABLE_GRADIENT = 0,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_INVALID_STATE = 4
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    internal enum WindowCompositionAttribute
    {
        // ...
        WCA_ACCENT_POLICY = 19
        // ...
    }

    /// <summary>
    /// Interaction logic for MainWindowView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public NotifyIcon notifyIcon;
        StartupEventArgs StartArgs;

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(
            IntPtr hwnd,
            ref WindowCompositionAttributeData data
        );

        public static class MainViewMediator
        {
            public static MainView MainViewInstance { get; set; }
        }

        public MainView(StartupEventArgs Args)
        {
            StartArgs = Args;
            this.Opacity = 0;
            InitializeComponent();
            Instances.MainViewInstance = this;
            MainViewMediator.MainViewInstance = this;
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight - 30;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private void pnlControlBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SendMessage(helper.Handle, 161, 2, 0);
        }

        private void pnlControlBar_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }

        public async Task MinimizeToTray()
        {
            await new Functions().FadeOut(this, 150);
            Hide();
            InitializeNotifyIcon();
            new Functions().SendNotification(
                "BetaManager",
                $"BetaManager has been minimized to the tray",
                1
            );
        }

        private async void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsModel.CloseBehaviour == 1)
            {
                MinimizeToTray();
            }
            else if (SettingsModel.CloseBehaviour == 0)
                Process.GetCurrentProcess().Kill();
        }

        private async void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            await new Functions().FadeOut(this, 100);
            WindowState =
                WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            new Functions().FadeIn(this, 100);
        }

        private void InitializeNotifyIcon()
        {
            notifyIcon = new NotifyIcon
            {
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(
                    Assembly.GetExecutingAssembly().Location
                ),
                Visible = true,
                Text = "BetaManager"
            };
            var contextMenu = new System.Windows.Forms.ContextMenu();

            // Add menu items
            var closeMenuItem = new System.Windows.Forms.MenuItem("Exit");
            closeMenuItem.Click += (sender, e) =>
            {
                Process.GetCurrentProcess().Kill();
            };

            var showMenuItem = new System.Windows.Forms.MenuItem("Show");
            showMenuItem.Click += async (sender, e) =>
            {
                Show();
                await new Functions().FadeIn(this, 150);
                WindowState = WindowState.Normal;
                notifyIcon.Dispose();
            };
            ;

            contextMenu.MenuItems.Add(showMenuItem);

            contextMenu.MenuItems.Add(closeMenuItem);

            notifyIcon.ContextMenu = contextMenu;
            notifyIcon.MouseClick += NotifyIcon_MouseClick;
        }

        private async void NotifyIcon_MouseClick(
            object sender,
            System.Windows.Forms.MouseEventArgs e
        )
        {
            if (e.Button == MouseButtons.Left)
            {
                Show();
                await new Functions().FadeIn(this, 150);
                WindowState = WindowState.Normal;
                notifyIcon.Dispose();
            }
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Ellipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { }

        public async Task BlurFunc(int mode = 0)
        {
            BlurEffect blurEffect = new BlurEffect();
            DoubleAnimation blurAnimation;

            var tcs = new TaskCompletionSource<bool>();

            if (mode == 0)
            {
                TopBorder.Visibility = Visibility.Visible;
                blurEffect.Radius = 0.01; // Start with a small non-zero value
                blurAnimation = new DoubleAnimation(5, TimeSpan.FromSeconds(0.25));
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

        internal void EnableBlur()
        {
            var windowHelper = new WindowInteropHelper(this);

            var accent = new AccentPolicy();
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        private async Task ChangeButtons(int buttons, bool enable, bool Nobutton = true)
        {
            if (buttons == 0)
            {
                if (enable)
                {
                    new Functions().FadeIn(UpdateButton);
                    DontUpdateButton.IsHitTestVisible = Nobutton;
                    UpdateButton.IsHitTestVisible = true;
                    await new Functions().FadeIn(DontUpdateButton);
                }
                else
                {
                    new Functions().FadeOut(UpdateButton);
                    DontUpdateButton.IsHitTestVisible = Nobutton;
                    UpdateButton.IsHitTestVisible = false;
                    await new Functions().FadeOut(DontUpdateButton);
                }
            }
            else
            {
                if (enable)
                {
                    new Functions().FadeIn(GuestButton);
                    await new Functions().FadeIn(LoginButton);
                    GuestButton.IsHitTestVisible = true;
                    LoginButton.IsHitTestVisible = true;
                }
                else
                {
                    new Functions().FadeOut(GuestButton);
                    await new Functions().FadeOut(LoginButton);
                    GuestButton.IsHitTestVisible = false;
                    LoginButton.IsHitTestVisible = false;
                }
            }
        }

        private int? v = 0;
        private bool fromSettings = false;

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (StartArgs.Args.Length > 0)
            {
                string url = StartArgs.Args[0];
                if (url == "--minimized")
                {
                    await MinimizeToTray();
                    CheckUpdates(false);
                }
                else
                {
                    using (var n = new StartupHandler(url))
                    {
                        if (n.Function == "opengame")
                        {
                            List<LibraryGameModel> Games = new List<LibraryGameModel>();
                            if (File.Exists(Saved.SaveLocation + "User\\Games.json"))
                                Games = JsonConvert.DeserializeObject<
                                    List<Models.LibraryGameModel>
                                >(
                                    Functions.Decrypt(
                                        File.ReadAllText(Saved.SaveLocation + "User\\Games.json")
                                    )
                                );
                            else
                                System.Windows.Forms.MessageBox.Show(
                                    "Couldn't find the library file.",
                                    "BetaManager",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error
                                );
                            if (Games.Count > 0)
                            {
                                LibraryGameModel game = Games.Find(g => g.ID == n.SecondArgument);
                                if (game != null)
                                {
                                    game.LastPlayDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                                    game.LastPlayDateString = Functions.TimeSince(
                                        DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                                    );
                                    int index = Games.FindIndex(x => x.ID == game.ID);

                                    if (index != -1)
                                    {
                                        Games[index] = game;
                                    }
                                    Functions.SaveLibrary(Games);
                                    if (game.GameExe != null)
                                    {
                                        Functions.StartProccess(game.GameExe);
                                        if (SettingsModel.MinimizeOnGameLaunch)
                                            Instances.MainViewInstance.MinimizeToTray();
                                    }
                                    else
                                        System.Windows.Forms.MessageBox.Show(
                                            "This game has no executable file.",
                                            "BetaManager",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Error
                                        );
                                }
                                else
                                    System.Windows.Forms.MessageBox.Show(
                                        "Couldn't find the provided game in the library.",
                                        "BetaManager",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error
                                    );
                            }
                            else
                                System.Windows.Forms.MessageBox.Show(
                                    "There are no games in the library.",
                                    "BetaManager",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error
                                );
                        }
                    }
                    BlurFunc();
                    new Functions().FadeIn(this, 250);
                    CheckUpdates(false);
                }
            }
            else
            {
                BlurFunc();
                new Functions().FadeIn(this, 250);
                CheckUpdates(false);
            }
        }

        public async void CheckUpdates(bool settings = false)
        {
            fromSettings = settings;
            if (SettingsModel.AutoUpdateChecking || fromSettings)
            {
                await new Functions().UpdateText(state, "Checking for updates 🧐");
                v = await Auth.Version();
                if (v == 2)
                {
                    await new Functions().UpdateText(
                        state,
                        "This version can no longer work! You need to update the app to be able to use it."
                    );
                    ChangeButtons(0, true, false);
                    return;
                }
                else if (v == 3)
                {
                    await new Functions().UpdateText(
                        state,
                        "There's a new update! Do you want to update the app?"
                    );
                    ChangeButtons(0, true, true);
                }
                else if (v == 1)
                {
                    if (fromSettings)
                    {
                        new Functions().FadeOut(state, 350);
                        BlurFunc(1);
                    }
                    else
                        ContinueLoading();
                }
            }
            else
            {
                ContinueLoading();
                if (!fromSettings)
                {
                    new Thread(async () =>
                    {
                        v = await Auth.Version();
                        if (v == 2)
                        {
                            await Instances.AppDispatcher.InvokeAsync(async () =>
                            {
                                await new Functions().UpdateText(
                                    state,
                                    "This version can no longer work! You need to update the app to be able to use it."
                                );
                                ChangeButtons(0, true, false);
                            });
                        }
                    }).Start();
                }
            }
        }

        private async void ContinueLoading()
        {
            ChangeButtons(0, false, false);
            if (SettingsModel.GuestUser)
            {
                ChangeButtons(1, false);
                Instances.MainViewModel.LoginButtonVisibility = Visibility.Visible;
                Instances.MainViewModel.LoginButtonEnabled = true;
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
                Saved.Logger.Log("931021");
                ProfilePicture.ImageSource = new Functions().Bitmap2BitmapImage(
                    new Bitmap(Properties.Resources.profile)
                );
                Saved.Logger.Log("732650");
                Instances.MainViewModel.CurrentUserAccount = Saved.User;
                new Functions().FadeOut(state, 350);
                BlurFunc(1);
                return;
            }
            await new Functions().UpdateText(state, "Checking for any saved accounts here 🧐");
            if (SettingsModel.Username?.Length > 0 && SettingsModel.Password?.Length > 0)
            {
                await new Functions().UpdateText(
                    state,
                    "Trying to log into your account... Juuust a second 😐"
                );
                bool logged_in = await Auth.Login(
                    new NetworkCredential(SettingsModel.Username, SettingsModel.Password),
                    this.Dispatcher
                );
                if (logged_in)
                {
                    if (!Saved.User.Verified)
                    {
                        await new Functions().UpdateText(
                            state,
                            $"Account not verified. You may check your email or contact the support team"
                        );
                        return;
                    }
                    Instances.DiscordClient.Client.UpdateState("Sailing");
                    Instances.MainViewModel.LoginButtonVisibility = Visibility.Hidden;
                    Instances.MainViewModel.LoginButtonEnabled = false;
                    await new Functions().UpdateText(state, "Enjoy 😘");
                    await Functions.SleepAsync(1000);
                    await new Functions().FadeOut(state);
                    if (Saved.User.ProfilePicture != null)
                    {
                        ProfilePicture.ImageSource = Saved.User.ProfilePicture;
                    }
                    else
                    {
                        ProfilePicture.ImageSource = new Functions().Bitmap2BitmapImage(
                            new Bitmap(Properties.Resources.profile)
                        );
                    }
                    BlurFunc(1);
                }
                else
                {
                    await Functions.SleepAsync(2000);
                    await new Functions().UpdateText(state, "Log in again or make one 🙄");
                    await Functions.SleepAsync(2000);
                    ChangeButtons(1, true);
                }
            }
            else
            {
                await Functions.SleepAsync(2000);
                new Functions().UpdateText(state, "Log in again or make one 🙄");
                await Functions.SleepAsync(2000);
                ChangeButtons(1, true);
            }
            Instances.DiscordClient.LoggedIn();
        }

        private void ema_Click(object sender, RoutedEventArgs e) { }

        private void Button_Click(object sender, RoutedEventArgs e) { }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SendMessage(helper.Handle, 161, 2, 0);
        }

        public void UpdateProfilePicture(BitmapSource newProfilePicture)
        {
            if (newProfilePicture != null)
            {
                ProfilePicture.ImageSource = newProfilePicture;
            }
        }

        private async void GuestButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeButtons(1, false);
            await new Functions().UpdateText(state, $"Well, fair enough.");
            SettingsModel.GuestUser = true;
            await Functions.SleepAsync(2000);
            Saved.Logger.Log("932746");
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
                Date = "idk. Today?",
            };
            Saved.Logger.Log("931021");
            ProfilePicture.ImageSource = new Functions().Bitmap2BitmapImage(
                new Bitmap(Properties.Resources.profile)
            );
            Saved.Logger.Log("732650");
            Instances.MainViewModel.CurrentUserAccount = Saved.User;
            new Functions().FadeOut(state, 350);
            BlurFunc(1);
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            new LoginView().ShowDialog();
            if (Saved.User.Username != "guest")
            {
                await ChangeButtons(1, false);
                await new Functions().UpdateText(
                    state,
                    $"You are good to go, {Saved.User.DisplayName} 🥲"
                );
                Instances.MainViewModel.LoginButtonVisibility = Visibility.Hidden;
                Instances.MainViewModel.LoginButtonEnabled = false;
                await Functions.SleepAsync(2000);
                await new Functions().FadeOut(state, 350);
                if (Saved.User.ProfilePicture != null)
                {
                    ProfilePicture.ImageSource = Saved.User.ProfilePicture;
                }
                else
                {
                    ProfilePicture.ImageSource = new Functions().Bitmap2BitmapImage(
                        new Bitmap(Properties.Resources.profile)
                    );
                }
                BlurFunc(1);
            }
            else
            {
                ChangeButtons(1, false);
                await new Functions().UpdateText(state, $"Bye bye.");
                await Functions.SleepAsync(4000);
                Process.GetCurrentProcess().Kill();
            }
        }

        private void NoButton_Click(object sender, RoutedEventArgs e) { }

        private void YesButton_Click(object sender, RoutedEventArgs e) { }

        private void DiscordButton_Click(object sender, RoutedEventArgs e)
        {
            Functions.StartProccess("https://discord.gg/sbxMVMzGsF");
        }

        private void RedditButton_Click(object sender, RoutedEventArgs e)
        {
            Functions.StartProccess("https://reddit.com/r/BetaManager");
        }

        private void DontUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (fromSettings)
            {
                ChangeButtons(0, false, false);
                new Functions().FadeOut(state, 350);
                BlurFunc(1);
            }
            else
                ContinueLoading();
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (
                File.Exists(
                    Path.Combine(Directory.GetCurrentDirectory(), "BetaManager Updater.exe")
                )
            )
                Process.Start(
                    Path.Combine(Directory.GetCurrentDirectory(), "BetaManager Updater.exe")
                );
            else
            {
                System.Windows.Forms.MessageBox.Show(
                    "Couldn't find the updater file. Please run the installer again. For now I will redirect you to the app's website"
                );
                Process.Start(Saved.Site);
            }
            Process.GetCurrentProcess().Kill();
        }

        private async void GuestLoginButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            BlurFunc();
            new LoginView().ShowDialog();
            if (Saved.User.Username != "guest")
            {
                Instances.GeneralSettingsTabModel?.Refresh();
                await ChangeButtons(1, false);
                Instances.MainViewModel.LoginButtonVisibility = Visibility.Hidden;
                Instances.MainViewModel.LoginButtonEnabled = false;
                await new Functions().FadeOut(state, 350);
                if (Saved.User.ProfilePicture != null)
                {
                    ProfilePicture.ImageSource = Saved.User.ProfilePicture;
                }
                else
                {
                    ProfilePicture.ImageSource = new Functions().Bitmap2BitmapImage(
                        new Bitmap(Properties.Resources.profile)
                    );
                }
                BlurFunc(1);
            }
            else
            {
                await ChangeButtons(1, false);
                BlurFunc(1);
            }
        }

        private async void Window_Closed(object sender, EventArgs e) { }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            await new Functions().FadeOut(this, 150);
            Hide();
            InitializeNotifyIcon();
            new Functions().SendNotification(
                "BetaManager",
                $"BetaManager has been minimized to the tray",
                1
            );
        }

        private TaskCompletionSource<bool> NotifierTaskCompletion;

        public async Task<bool> Notify(string message)
        {
            NotifierTaskCompletion = new TaskCompletionSource<bool>();
            state.Text = message;
            BlurFunc();
            new Functions().FadeIn(state);
            new Functions().FadeIn(NotifierOK);
            NotifierOK.IsHitTestVisible = true;
            var s = await NotifierTaskCompletion.Task;

            if (s)
            {
                new Functions().FadeOut(state);
                new Functions().FadeOut(NotifierOK);
                await BlurFunc(1);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void btnClose_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void NotifierOK_Click(object sender, RoutedEventArgs e)
        {
            NotifierTaskCompletion.SetResult(true);
        }

        private void ProfileBorder_Click(object sender, RoutedEventArgs e)
        {
            ProfileButton.IsChecked = true;
            Instances.MainViewModel.ShowProfileViewCommand.Execute(null);
            e.Handled = true;
        }

        private void PCFocusButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://twitter.com/pc_focus_/");
        }
    }
}
