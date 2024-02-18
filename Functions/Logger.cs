using System;
using System.IO;

namespace BetaManager
{
    public class Logger
    {
        private DateTime _date;
        private string _filePath;

        public Logger()
        {
            _date = DateTime.Now;
            _filePath =
                Saved.SaveLocation
                + "Logs\\BetaManager_Logs_"
                + _date.ToString("d_M_yyyy__HH_mm_ss")
                + ".bmlog";
            foreach (FileInfo file in new DirectoryInfo(Saved.SaveLocation + "Logs").GetFiles())
            {
                if (file.Name.EndsWith(".bmlog"))
                    file.Delete();
            }
            File.WriteAllText(_filePath, $"BetaManager {Functions.Decrypt(Saved.CurrentVersion)}");
        }

        public void Log(string functionID, string error = null)
        {
            File.WriteAllText(
                _filePath,
                $"{File.ReadAllText(_filePath)}\n{DateTime.Now.ToString("d/M/yyyy HH:mm:ss")} {(error != null ? "(ERROR) " : "")}➜ {functionID}{(error != null ? $"\n{error}" : "")}"
            );
        }
    }
}
