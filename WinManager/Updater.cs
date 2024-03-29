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
        public delegate void DownloadCompleteCallback();
        public delegate void DownloadErrorCallback();

        public async void DownloadUpdateAsync(UpdateData updateData, DownloadProgressCallback downloadProgressCallback, DownloadCompleteCallback downloadCompleteCallback, DownloadErrorCallback downloadErrorCallback)
        {
            var setupUrl = new System.Uri(updateData.setupUrl);
            var setupFilename = Path.GetFileName(setupUrl.LocalPath);
            var setupDownloadPath = Path.Combine(Consts.SetupDownloadFolder, setupFilename);
            Directory.CreateDirectory(Consts.SetupDownloadFolder);
            var webClient = new WebClient();
            webClient.DownloadProgressChanged += (sender, e) =>
            {
                downloadProgressCallback(e.ProgressPercentage);
            };
            try
            {
                await webClient.DownloadFileTaskAsync(setupUrl, setupDownloadPath);
                downloadCompleteCallback();
            }
            catch (Exception ex)
            {
                if (ex is WebException || ex is InvalidOperationException)
                {
                    downloadErrorCallback();
                }
            }
        }
    }
}
