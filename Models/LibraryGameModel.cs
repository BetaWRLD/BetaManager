namespace BetaManager.Models
{
    public class LibraryGameModel
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Developer { get; set; }
        public bool FolderAvailable { get; set; }
        public string Folder { get; set; }
        public bool IsReady { get; set; }
        public bool IsCustom { get; set; }
        public long InstallDate { get; set; }
        public string InstallDateString { get; set; }
        public long LastPlayDate { get; set; }
        public string LastPlayDateString { get; set; }
        public string Picture { get; set; }
        public string PictureURL { get; set; }
        public string Version { get; set; }
        public string GameExe { get; set; }
        public long SizeOnDisk { get; set; }
        public long PlayedTime { get; set; }
        public string PlayedTimeString { get; set; }
        public string SizeOnDiskString { get; set; }
        public string Credits { get; set; }

        public void AddPlayTime(long time)
        {
            PlayedTime += time;
        }
    }
}
