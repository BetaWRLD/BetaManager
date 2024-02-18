using System;
using BetaManager.Downloader;

namespace BetaManager.Classes
{
    internal class EventsHandler
    {
        public static void NetworkInterfacesHandlerEvent(object sender, EventArgs e)
        {
            if (Saved.KillSwitchON)
            {
                switch (Saved.KillSwitchMode)
                {
                    case 0:
                        if (
                            Saved.KillSwitchTrigger != null
                            && Saved.KillSwitchTrigger != "Disabled"
                            && Saved.KillSwitchTrigger.Length > 1
                            && Instances.NetworkInterfacesHandler.InterfacesList.Find(
                                a => a.Name == Saved.KillSwitchTrigger
                            ) == null
                        )
                        {
                            if (DownloadManager.Instance.DownloadsList.Count > 0)
                            {
                                foreach (
                                    DownloadClient dC in DownloadManager.Instance.DownloadsList
                                )
                                {
                                    if (
                                        dC.Status != DownloadStatus.Paused
                                        && dC.Status != DownloadStatus.Queued
                                        && dC.Status != DownloadStatus.Pausing
                                    )
                                        dC.Pause();
                                }
                            }
                        }
                        break;
                    case 1:
                        string IP = Functions.getPublicIP();
                        if (Saved.KillSwitchTrigger != IP)
                        {
                            if (DownloadManager.Instance.DownloadsList.Count > 0)
                            {
                                foreach (
                                    DownloadClient dC in DownloadManager.Instance.DownloadsList
                                )
                                {
                                    dC.Pause();
                                }
                            }
                        }
                        break;
                }
            }
        }
    }
}
