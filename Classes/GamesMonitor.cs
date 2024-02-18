using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using BetaManager.Models;

namespace BetaManager.Classes
{
    internal class GamesMonitor
    {
        public GamesMonitor() { }

        public void StartTracking()
        {
            Instances.GameMonitorThread = new Thread(TrackGames);
            Instances.GameMonitorThread.Start();
        }

        private void TrackGames()
        {
            while (true)
            {
                foreach (var game in Saved.LibraryGames)
                {
                    LibraryGameModel GAME = Saved.LibraryGames.Find(a => a.Name == game.Name);
                    if (IsProcessRunning(Path.GetFileNameWithoutExtension(GAME.GameExe)))
                    {
                        GAME.PlayedTime += 10000;
                        GAME.PlayedTimeString =
                            Functions.ConvertMillisecondsToReadableTimeMonitorMode(GAME.PlayedTime);
                        GAME.LastPlayDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        GAME.LastPlayDateString = Functions.TimeSince(
                            DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                        );
                        Functions.SaveLibrary(Saved.LibraryGames);
                    }
                    else
                    {
                        GAME.LastPlayDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        GAME.LastPlayDateString = Functions.TimeSince(
                            DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                        );
                    }
                }

                Thread.Sleep(10000);
            }
        }

        private bool IsProcessRunning(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            return processes.Length > 0;
        }
    }
}
