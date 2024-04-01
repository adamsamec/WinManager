using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Text.Json;

namespace WinManager
{
    /// <summary>
    /// Update check, download and launch class
    /// </summary>
    public class Updater
    {
        private UpdateState _state= UpdateState.Ready;
        private WebClient _webClient;

        public UpdateState State
        {
            get { return _state; }
        }
        public enum UpdateState
        {
            Ready,
            Downloading,
            FilesExist,
            Deleting
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

        public async Task DownloadAsync(UpdateData updateData, DownloadProgressCallback downloadProgressCallback, DownloadCompleteCallback downloadCompleteCallback, DownloadErrorCallback downloadErrorCallback)
        {
            if (_state == UpdateState.Downloading || _state == UpdateState.Deleting)
            {
                return;
            }
            var setupUrl = new System.Uri("https://files.adamsamec.cz/apps/test.zip");
            //var setupUrl = new System.Uri(updateData.setupUrl);
            var setupFilename = Path.GetFileName(setupUrl.LocalPath);
            var setupDownloadPath = Path.Combine(Consts.SetupDownloadFolder, setupFilename);

            // Make sure empty folder is prepared for download
            Directory.CreateDirectory(Consts.SetupDownloadFolder);
            if (_state == UpdateState.FilesExist)
            {
                _state = UpdateState.Deleting;
            DeleteSetupFiles();
            }
                _state = UpdateState.Downloading;
            //WaitUntilDownloadFileDeleted(setupDownloadPath);

            _webClient = new WebClient();
            _webClient.DownloadProgressChanged += (sender, e) =>
            {
                downloadProgressCallback(e.ProgressPercentage);
            };
            _webClient.DownloadFileCompleted += (sender, e) =>
            {
                if (e.Cancelled)
                {
                    //DeleteSetupFiles();
                }
                else
                {
                    downloadCompleteCallback();
                }
                _state = UpdateState.FilesExist;
                _webClient.Dispose();
            };

            // Initiate download
            try
            {
                await _webClient.DownloadFileTaskAsync(setupUrl, setupDownloadPath);
            }
            catch (Exception ex)
            {
                //if (_isDownloading)
                //{
                _state = UpdateState.FilesExist;
                    downloadErrorCallback();
                    _webClient.Dispose();
                //}
            }
        }

        public void WaitUntilDownloadFileDeleted(string filePath)
        {
            while (File.Exists(filePath))
            {
                Task.Delay(500).Wait();
            }
        }

        public void DeleteSetupFiles()
        {
            var keepTrying = true;
            while (keepTrying)
            {
                Task.Delay(500).Wait();
                try
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(Consts.SetupDownloadFolder);
                    var files = dirInfo.GetFiles();
                    if (files.Count() == 0)
                    {
                        return;
                    }
                    foreach (var file in files)
                    {
                        file.Delete();
                    }
                    keepTrying = false;
                } catch (Exception ex) { }
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
