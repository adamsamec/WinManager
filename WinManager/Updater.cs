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
        private WebClient _webClient;

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

        public async void DownloadAsync(UpdateData updateData, DownloadProgressCallback downloadProgressCallback, DownloadCompleteCallback downloadCompleteCallback, DownloadErrorCallback downloadErrorCallback)
        {
            var setupUrl = new System.Uri("https://files.adamsamec.cz/apps/test.zip");
            //var setupUrl = new System.Uri(updateData.setupUrl);
            var setupFilename = Path.GetFileName(setupUrl.LocalPath);
            var setupDownloadPath = Path.Combine(Consts.SetupDownloadFolder, setupFilename);

            // Make sure empty download folder is created
            Directory.CreateDirectory(Consts.SetupDownloadFolder);
            DeleteSetupFiles();

            _webClient = new WebClient();
            _webClient.DownloadProgressChanged += (sender, e) =>
            {
                downloadProgressCallback(e.ProgressPercentage);
            };
            _webClient.DownloadFileCompleted += (sender, e) =>
            {
                _isDownloading = false;
                if (e.Cancelled)
                {
                    DeleteSetupFiles();
                }
                else
                {
                    downloadCompleteCallback();
                }
            };

            // Initiate download
            _isDownloading = true;
            try
            {
                await _webClient.DownloadFileTaskAsync(setupUrl, setupDownloadPath);
            }
            catch (Exception ex)
            {
                if (_isDownloading) { 
                downloadErrorCallback();
                _isDownloading = false;
                }
            }
            _webClient.Dispose();
        }

        public void DeleteSetupFiles()
        {
            DirectoryInfo dirInfo = new DirectoryInfo(Consts.SetupDownloadFolder);
            foreach (var file in dirInfo.GetFiles())
            {
                file.Delete();
            }
        }

        public void CancelDownload()
        {
            if (_webClient != null)
            {
                _webClient.CancelAsync();
            }
        }
    }
}
