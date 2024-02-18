using MonoTorrent;

namespace BetaManager.Models
{
    public class BitSwarmTorrentFile
    {
        public string FilePath { get; set; }
        public long FileSize { get; set; }
        public string FileSizeString { get; set; }
        public Priority Priority { get; set; }
        public ITorrentManagerFile File { get; set; }
    }
}
