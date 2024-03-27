using System.Net;
using System.Text.Json;

namespace WinManager
{
    /// <summary>
    /// Update check, download and launch class
    /// </summary>
    public class Updater
    {
        private const string _apiUrl = "http://api.adamsamec.cz/WinManager/Update.json";
        public const string AppVersion = "0.1.0";

        public Updater()
        {
        }

        public UpdateData? CheckForUpdate()
        {
            var updateString = new WebClient().DownloadString(_apiUrl);
            var update = JsonSerializer.Deserialize<UpdateData>(updateString);
            if (update.version == AppVersion)
            {
                return null;
            }
            {
                return update;
            }
        }
    }
}

