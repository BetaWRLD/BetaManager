// The downloader needs fixes and improving.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using BetaManager.Classes;
using BetaManager.Downloader.Torrent;
using BetaManager.Models;
using MonoTorrent.Client;

namespace BetaManager.Downloader
{
    public class DownloadClient : INotifyPropertyChanged
    {
        TorrentManager Manager { get; set; }
        public TorrentDownloadModel TorrentModel { get; set; }
        TorrentDownloader TorrentDownloader { get; set; }
        public List<BitSwarmTorrentFile> BitSwarmFiles { get; set; }
        NumberFormatInfo numberFormat = NumberFormatInfo.InvariantInfo;
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler StatusChanged;
        public event EventHandler DownloadProgressChanged;
        public event EventHandler DownloadCompleted;

        private Thread ProgressThread { get; set; }
        public string ID { get; set; }
        public string Magnet { get; private set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Picture { get; set; }
        public FitGirlGameModel Game { get; set; }
        public long DownloadSize { get; set; }
        public bool Completed { get; set; }
        public DateTime LastUpdateTime { get; set; }

        public bool OpenInstaller { get; set; } = false;
        public bool StartImmediatly { get; set; } = true;
        public bool DontSeed { get; set; }
        public long addBytesDownloaded;
        public long addBytesUploaded;

        public DownloadClient(
            string Magnet,
            string Path,
            string Name,
            FitGirlGameModel Game,
            long DownloadSize,
            bool restart = false,
            TorrentDownloader TM = null
        )
        {
            this.Game = Game;
            this.Magnet = Magnet;
            this.Name = Name;
            this.Path = Path;
            this.DownloadSize = DownloadSize;

            if (TM != null)
                TorrentDownloader = TM;
            else
                TorrentDownloader = new();

            Manager = TorrentDownloader.AddTorrentDownload(Magnet, Path);

            ProgressThread = new Thread(async () =>
            {
                while (!Completed)
                {
                    TorrentModel = await TorrentDownloader.GetCurrentInfo();
                    UpdateDownloadDisplay();
                    if (Manager.Complete || Manager.PartialProgress == 100.0)
                    {
                        TorrentModel = await TorrentDownloader.GetCurrentInfo();
                        UpdateDownloadDisplay();
                        RaiseDownloadCompleted();
                        return;
                    }
                    Thread.Sleep(1000);
                }
            });
        }

        private string downloadSpeed { get; set; } = "0 B/s";
        public string DownloadSpeed
        {
            get { return downloadSpeed; }
            set
            {
                if (downloadSpeed != value)
                {
                    downloadSpeed = value;
                    UpdateDownloadDisplay();
                }
            }
        }

        private string upSpeed { get; set; } = "0 B/s";
        public string UpSpeed
        {
            get { return upSpeed; }
            set
            {
                if (upSpeed != value)
                {
                    upSpeed = value;
                    UpdateDownloadDisplay();
                }
            }
        }

        private string _blurAmount { get; set; } = "0";
        public string BlurAmount
        {
            get { return _blurAmount; }
            set
            {
                if (_blurAmount != value)
                {
                    _blurAmount = value;
                    UpdateDownloadDisplay();
                }
            }
        }

        Visibility _cancelConfirmation = Visibility.Hidden;
        public Visibility CancelConfirmation
        {
            get { return _cancelConfirmation; }
            set
            {
                if (_cancelConfirmation != value)
                {
                    _cancelConfirmation = value;
                    UpdateDownloadDisplay();
                }
            }
        }

        private string _pauseResumeButtonIcon { get; set; } = "Resources/Icons/resume.svg";
        public string PauseResumeButtonIcon
        {
            get { return _pauseResumeButtonIcon; }
            set
            {
                if (_pauseResumeButtonIcon != value)
                {
                    _pauseResumeButtonIcon = value;
                    UpdateDownloadDisplay();
                }
            }
        }

        public long DownloadedSize { get; set; }
        public string DownloadUnit
        {
            get { return DownloadManager.GetSizeUnit(DownloadSize); }
        }
        public string DownloadedString
        {
            get
            {
                return DownloadManager.FormatSizeString(TorrentModel.BytesDownloaded, true, true);
            }
        }
        public string TotalString
        {
            get { return DownloadManager.FormatSizeString(DownloadSize, true, true); }
        }
        public int SeedersCount
        {
            get { return TorrentModel.Peers.FindAll(a => a.Seeding == true).Count; }
        }
        public double _PercentageToWidth = 0;
        public double _PercentageToWidthExtract = 0;
        public double PercentageToWidth
        {
            get
            {
                _PercentageToWidth =
                    (Manager.PartialProgress / 100)
                    * (double)((Instances.MainViewInstance.Width - 242));

                return _PercentageToWidth;
            }
            set { _PercentageToWidth = value; }
        }

        public float Percent
        {
            get
            {
                if (DownloadSize <= 0)
                    DownloadSize = TorrentModel.Size;
                return (((float)(TorrentModel.BytesDownloaded + addBytesDownloaded)) / DownloadSize)
                    * 100F;
            }
        }

        public string PercentString
        {
            get
            {
                if (Percent < 0 || float.IsNaN(Percent))
                    return "0.0%";
                else if (Percent > 100)
                    return "100.0%";
                else
                    return string.Format(numberFormat, "{0:0.0}%", Percent);
            }
        }

        public string TimeLeft
        {
            get
            {
                if (TorrentModel.DownloadSpeed > 0)
                {
                    double remainingSeconds =
                        (DownloadSize - TorrentModel.BytesDownloaded) / TorrentModel.DownloadSpeed;

                    if (remainingSeconds > 0)
                    {
                        var ts = TimeSpan.FromSeconds(remainingSeconds);
                        var result = "";

                        if (ts.Days > 1)
                            result += $"{ts.Days}d ";
                        else if (ts.Days == 1)
                            result += "1d ";

                        if (ts.Hours > 1)
                            result += $"{ts.Hours}h ";
                        else if (ts.Hours == 1)
                            result += "1h ";

                        if (ts.Minutes > 1)
                            result += $"{ts.Minutes}m ";
                        else if (ts.Minutes == 1)
                            result += "1m ";

                        if (ts.Seconds > 1)
                            result += $"{ts.Seconds}s";
                        else if (ts.Seconds == 1)
                            result += "1s";

                        if (ts.Days > 30)
                            result = ">30d";
                        return result;
                    }
                    else
                    {
                        return (Completed ? "Download Completed" : "uhhh, idk?");
                    }
                }
                else
                    return "";
            }
        }

        DownloadStatus status;
        public DownloadStatus Status
        {
            get { return status; }
            set
            {
                status = value;

                if (status != DownloadStatus.Deleting)
                    RaiseStatusChanged();
            }
        }
        public string StatusString
        {
            get { return TorrentModel.State.ToString(); }
        }

        protected void RaisePropertyChanged(string name)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new(name));
        }

        protected virtual void RaiseStatusChanged()
        {
            if (StatusChanged != null)
            {
                StatusChanged(this, EventArgs.Empty);
            }
        }

        protected virtual void RaiseDownloadProgressChanged()
        {
            if (DownloadProgressChanged != null)
            {
                DownloadProgressChanged(this, EventArgs.Empty);
            }
        }

        protected virtual void RaiseDownloadCompleted()
        {
            if (DownloadCompleted != null)
            {
                Completed = true;
                if (DontSeed == true || SettingsModel.StartSeedingOnComplete == false)
                    this.Cancel();
                _pauseResumeButtonIcon = "Resources/Icons/bolt.svg";
                UpdateDownloadDisplay();
                DownloadCompleted(this, EventArgs.Empty);
            }
        }

        string FormatSpeed(long speedInBytes)
        {
            const long Kilobyte = 1024;
            const long Megabyte = Kilobyte * 1024;
            const long Gigabyte = Megabyte * 1024;

            if (speedInBytes < Kilobyte)
            {
                return $"{speedInBytes} B/s";
            }
            else if (speedInBytes < Megabyte)
            {
                return $"{speedInBytes / (double)Kilobyte:F2} KB/s";
            }
            else if (speedInBytes < Gigabyte)
            {
                return $"{speedInBytes / (double)Megabyte:F2} MB/s";
            }
            else
            {
                return $"{speedInBytes / (double)Gigabyte:F2} GB/s";
            }
        }

        public void DownloadProgressChangedHandler(object sender, EventArgs e)
        {
            if (DateTime.UtcNow > LastUpdateTime.AddSeconds(1))
            {
                UpdateDownloadDisplay();
                LastUpdateTime = DateTime.UtcNow;
            }
        }

        public void DownloadCompletedHandler(object sender, EventArgs e)
        {
            Status = DownloadStatus.Completed;
            UpdateDownloadDisplay();
        }

        int n = 0;

        // Update download display (on downloadsGrid and propertiesGrid controls)
        public async void UpdateDownloadDisplay()
        {
            if (TorrentModel != null)
            {
                downloadSpeed = FormatSpeed(TorrentModel.DownloadSpeed);
                upSpeed = FormatSpeed(TorrentModel.UploadSpeed);
            }

            RaisePropertyChanged("DownloadedSizeString");
            RaisePropertyChanged("PercentString");
            RaisePropertyChanged("BlurAmount");
            RaisePropertyChanged("CancelConfirmation");
            RaisePropertyChanged("PercentageToWidth");
            RaisePropertyChanged("PauseResumeButtonIcon");
            RaisePropertyChanged("Progress");
            RaisePropertyChanged("SeedersCount");
            RaisePropertyChanged("DownloadSpeed");
            RaisePropertyChanged("UpSpeed");
            RaisePropertyChanged("TotalString");
            RaisePropertyChanged("DownloadedString");
            RaisePropertyChanged("TimeLeft");
            RaisePropertyChanged("StatusString");
            RaisePropertyChanged("CompletedOnString");
            if (n < 5)
                n++;
            else
                new Functions().SaveDownloadsToXml();
        }

        // Start or continue download
        public async void Start()
        {
            Status = DownloadStatus.Waiting;

            if (Manager == null || Manager.State == MonoTorrent.Client.TorrentState.Stopped)
                await Manager.StartAsync();
            TorrentModel = await TorrentDownloader.GetCurrentInfo();

            _pauseResumeButtonIcon = "Resources/Icons/pause.svg";
            new Thread(async () =>
            {
                while (!Manager.HasMetadata)
                {
                    Thread.Sleep(1000);
                }
                if (BitSwarmFiles != null)
                    foreach (BitSwarmTorrentFile file in BitSwarmFiles)
                    {
                        await Manager.SetFilePriorityAsync(
                            Manager.Files.FirstOrDefault(f => f.Path == file.FilePath),
                            file.Priority
                        );
                    }
                DownloadSize = Manager
                    .Files.ToList()
                    .FindAll(e => e.Priority != MonoTorrent.Priority.DoNotDownload)
                    .Sum(e => e.Length);
            }).Start();
            ProgressThread.Start();
        }

        public async void Pause()
        {
            UpdateDownloadDisplay();

            if (Manager.State != TorrentState.Paused && Manager.State != TorrentState.Stopped)
            {
                this.Status = DownloadStatus.Paused;
                await Manager.PauseAsync();
                new Functions().SaveDownloadsToXml();
                _pauseResumeButtonIcon = "Resources/Icons/resume.svg";
                UpdateDownloadDisplay();
            }
            else if (Manager.State == TorrentState.Stopped)
            {
                Start();
            }
            else
            {
                await Manager.StartAsync();
                new Functions().SaveDownloadsToXml();
                _pauseResumeButtonIcon = "Resources/Icons/pause.svg";
                UpdateDownloadDisplay();
            }
        }

        public async Task<bool> Cancel()
        {
            await Manager.StopAsync();
            await Manager.Engine.RemoveAsync(Manager);
            return true;
        }

        public async void Resume()
        {
            UpdateDownloadDisplay();

            if (Manager.State == TorrentState.Stopped)
            {
                Start();
            }
            else
                await Manager.StartAsync();
            this.Status = DownloadStatus.Downloading;
            _pauseResumeButtonIcon = "Resources/Icons/pause.svg";
            UpdateDownloadDisplay();
        }

        public void Restart() { }
    }
}
