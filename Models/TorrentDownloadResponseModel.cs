using System.Collections.Generic;
using MonoTorrent;
using MonoTorrent.Client;

namespace BetaManager.Classes
{
    public class TorrentDownloadModel
    {
        public string Name { get; set; }
        public long DownloadSpeed { get; set; }
        public TorrentState State { get; set; }
        public long Size { get; set; }
        public long UploadSpeed { get; set; }
        public int Connections { get; set; }
        public bool PartialComplete { get; set; }
        public double Progress { get; set; }
        public double PartialProgress { get; set; }
        public long BytesDownloaded { get; set; }
        public long BytesUploaded { get; set; }
        public int Requests { get; set; }
        public List<FileModel> Files { get; set; }
        public List<PeersModel> Peers { get; set; }
        public TorrentManager Manager { get; set; }
    }
}

public class FileModel
{
    public string Name { get; set; }
    public string SizeString { get; set; }
    public long DownloadPercent { get; set; }
    public long Size { get; set; }
    public ITorrentManagerFile File { get; set; }
}

public class PeersModel
{
    public string URI { get; set; }
    public long DownloadRate { get; set; }
    public bool Seeding { get; set; }
    public long UpRate { get; set; }
}
