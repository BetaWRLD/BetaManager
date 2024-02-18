using System.Collections.Generic;
using MonoTorrent.Client;

namespace BetaManager
{
    internal class Saved
    {
        public static string LogFile;
        public static string CurrentVersion;
        public static Logger Logger;
        public static int GamesPage = 1;
        public static string Site = "https://beta-manager.com";
        public static string SaveLocation = "C:\\BetaManager\\";
        public static Models.UserModel User;
        public static List<Models.FitGirlGameModel> Games;
        private static List<Models.LibraryGameModel> _libraryGames;
        public static List<Models.LibraryGameModel> LibraryGames
        {
            get
            {
                if (_libraryGames == null)
                    _libraryGames = Functions.GetLibraryGames();
                return _libraryGames;
            }
            set { _libraryGames = value; }
        }
        public static List<Models.VPNInterfaceModel> VPNInterfaces;
        public static int SelectedSort = 1;
        public static int SelectedSortType = 1;
        public static List<Models.DownloadModel> Downloads;
        public static Models.FitGirlGameModel SelectedGame;
        public static TorrentManager SelectedTorrent;
        public static bool KillSwitchON;
        public static int KillSwitchMode;
        public static string KillSwitchTrigger;
        public static ClientEngine Engine = new();
    }
}
