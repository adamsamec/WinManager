using System.IO;
using System.Net;
using System.Text.Json;

namespace WinManager
{
    /// <summary>
    /// Update check, download and launch class
    /// </summary>
    public class Updater
    {
        private bool _isDownloading = false;

        public bool IsDownloading
        {
            get { return _isDownloading; }
        }

        public Updater()
        {
        }

        public UpdateData? CheckForUpdate()
        {
            var updateString = new WebClient().DownloadString(Consts.ApiUrl);
            var update = JsonSerializer.Deserialize<UpdateData>(updateString);
            if (update.version == Consts.AppVersion)
            {
                return null;
            }
            return update;
        }

        public delegate void DownloadProgressCallback(int progress);

        public void downloadUpdate(UpdateData updateData, DownloadProgressCallback downloadProgressCallback)
        {
            var url = new System.Uri(updateData.setupUrl);
            Directory.CreateDirectory(Consts.SetupDownloadPath);
            var webClient = new WebClient();
            webClient.DownloadProgressChanged += (sender, e) =>
            {
                downloadProgressCallback(e.ProgressPercentage);
            };
            //webClient.DownloadFileAsync(url, Consts.SetupDownloadPath);
        }
    }
}

