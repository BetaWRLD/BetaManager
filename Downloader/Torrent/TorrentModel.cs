using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BetaManager.Classes;
using MonoTorrent;
using MonoTorrent.Client;

namespace BetaManager.Downloader.Torrent
{
    public class TorrentDownloader
    {
        public Task? Task { get; set; }
        public TorrentManager? Manager { get; private set; }

        public TorrentManager AddTorrentDownload(
            string magnet,
            string dir,
            TorrentSettings settings = null
        )
        {
            if (settings == null)
                settings = new TorrentSettingsBuilder()
                {
                    CreateContainingDirectory = false
                }.ToSettings();
            Directory.CreateDirectory(dir);
            Saved.Engine = new(new EngineSettingsBuilder() { }.ToSettings());
            Task.Run(async () =>
                {
                    Manager = await Saved.Engine.AddAsync(MagnetLink.Parse(magnet), dir, settings);
                })
                .Wait();
            return Manager;
        }

        private FileModel MapFile(ITorrentManagerFile file)
        {
            return new FileModel
            {
                Name = file.Path,
                DownloadPercent = file.BytesDownloaded(),
                SizeString = DownloadManager.FormatSizeString(file.Length),
                Size = file.Length,
                File = file
            };
        }

        private PeersModel MapPeers(PeerId Peer)
        {
            return new PeersModel
            {
                URI = Peer.Uri.ToString(),
                Seeding = Peer.IsSeeder,
                DownloadRate = Peer.Monitor.DownloadRate,
                UpRate = Peer.Monitor.UploadRate
            };
        }

        public async Task<TorrentDownloadModel> GetCurrentInfo()
        {
            List<PeerId> peers = await Manager.GetPeersAsync();
            return new TorrentDownloadModel
            {
                Name = (Manager.Torrent == null ? "MetaDataMode" : Manager.Torrent.Name),
                DownloadSpeed = Manager.Monitor.DownloadRate,
                UploadSpeed = Manager.Monitor.UploadRate,
                Connections = Manager.OpenConnections,
                State = Manager.State,
                Size = (Manager.Torrent == null ? 0 : Manager.Torrent.Size),
                Progress = Manager.Progress,
                PartialProgress = Manager.PartialProgress,
                PartialComplete = Manager.PartialProgress == 100.0,
                BytesDownloaded = Manager.Monitor.DataBytesReceived,
                BytesUploaded = Manager.Monitor.DataBytesSent,
                Requests = await Manager.PieceManager.CurrentRequestCountAsync(),
                Files = Manager.Files.Select(file => MapFile(file)).ToList(),
                Peers = peers.Select(peer => MapPeers(peer)).ToList(),
                Manager = Manager
            };
        }
    }
}
