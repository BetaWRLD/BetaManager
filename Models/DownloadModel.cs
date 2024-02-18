using System.Windows.Media.Imaging;

namespace BetaManager.Models
{
    public class DownloadModel
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Requirements { get; set; }
        public string Developer { get; set; }
        public string Date { get; set; }
        public BitmapSource Picture { get; set; }
        public bool Available { get; set; }
        public string Data { get; set; }
        public string Version { get; set; }
        public string Size { get; set; }
        public int Percent { get; set; }
        public string DownloadSpeed { get; set; }
        public string RepackSize { get; set; }
        public string Genre { get; set; }
        public string Languages { get; set; }
    }
}
