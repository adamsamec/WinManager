using System.Diagnostics;
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
        private UpdateState _state = UpdateState.Ready;
        private WebClient? _webClient;

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
            if (update != null && update.version == Consts.AppVersion)
            {
                return null;
            }
            return update;
        }

        public delegate void DownloadProgressCallback(int progress);
        public delegate void DownloadCompleteCallback();
        public delegate void DownloadErrorCallback();

        public async Task<bool> DownloadAsync(UpdateData updateData, DownloadProgressCallback downloadProgressCallback, DownloadCompleteCallback downloadCompleteCallback, DownloadErrorCallback downloadErrorCallback)
        {
            if (_state == UpdateState.Downloading || _state == UpdateState.Deleting)
            {
                return false;
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

            _webClient = new WebClient();
            _webClient.DownloadProgressChanged += (sender, e) =>
            {
                downloadProgressCallback(e.ProgressPercentage);
            };
            _webClient.DownloadFileCompleted += (sender, e) =>
            {
                if (e.Cancelled)
                {
                    Debug.WriteLine("Downloadd cancelled");
                    //DeleteSetupFiles();
                }
                else
                {
                    Debug.WriteLine("Downloadd successful");
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
                Debug.WriteLine("Downloading exception " + ex.ToString());
                if (_state == UpdateState.Downloading)
                {
                    downloadErrorCallback();
                }
                _state = UpdateState.FilesExist;
                _webClient.Dispose();
            }
            Debug.WriteLine("End of download function");
            return true;
        }

        public void DeleteSetupFiles()
        {
            Debug.WriteLine("Starting deleting files");
            while (true)
            {
                Task.Delay(500).Wait();
                try
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(Consts.SetupDownloadFolder);
                    var files = dirInfo.GetFiles();
                    if (files.Count() == 0)
                    {
                        Debug.WriteLine("All files have been deleted");
                        return;
                    }
                    foreach (var file in files)
                    {
                        file.Delete();
                    }
                    //keepTrying = false;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Deleting files exception " + ex.ToString());
                }
            }
        }

        public void CancelDownload()
        {
            if (_webClient != null)
            {
                Debug.WriteLine("Canceling download");
                _webClient.CancelAsync();
            }
        }
    }
}
