using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using BetaManager.ViewModels;
using BetaManager.Views;
using BetaManager.Views.SettingsTabs;

namespace BetaManager.Classes
{
    internal class Instances
    {
        public static MainView MainViewInstance { get; set; }
        public static MainViewModel MainViewModel { get; set; }
        public static GamesViewModel GamesViewModel { get; set; }
        public static LibraryViewModel LibraryViewModel { get; set; }
        public static GeneralSettingsTabModel GeneralSettingsTabModel { get; set; }
        public static EditLibraryGameViewModel EditLibraryGameViewModel { get; set; }
        public static SettingsView SettingsViewInstance { get; set; }
        public static Views.NewLibraryGame NewLibraryGame { get; set; }
        public static ManagerView ManagerViewInstance { get; set; }
        public static LibraryView LibraryViewInstance { get; set; }
        public static ProfileSettingsTab ProfileSettingsTabInstance { get; set; }
        public static GeneralSettingsTab GeneralSettingsTabInstance { get; set; }
        public static EventsHandler EventsHandler { get; set; }
        public static NetworkInterfacesHandler NetworkInterfacesHandler { get; set; }
        public static GamesView GamesViewInstance { get; set; }
        public static GameView GameViewInstance { get; set; }
        public static HomeView HomeViewInstance { get; set; }
        public static Dispatcher AppDispatcher { get; set; }
        public static DiscordRPCServer DiscordClient { get; set; } = new DiscordRPCServer();
        private static CancellationTokenSource cancellationTokenSource;
        public static Thread GameMonitorThread;

        public static async Task StartHeartBeatAsync()
        {
            if (cancellationTokenSource == null)
            {
                cancellationTokenSource = new CancellationTokenSource();
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    bool? isAccountValid = await Auth.HeartBeat();

                    if (isAccountValid == true) { }
                    else { }

                    await Task.Delay(TimeSpan.FromMinutes(1), cancellationTokenSource.Token);
                }
            }
        }

        public static void StopHeartBeat()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = null;
        }
    }
}
