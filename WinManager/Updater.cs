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
        private UpdateState _state = UpdateState.Initial;
        private CancellationTokenSource? _cancellationToken;

        public UpdateState State
        {
            get { return _state; }
        }

        public enum UpdateState
        {
            Initial,
            Downloading,
            FilesExist,
            Deleting
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
            var setupUrlString = "https://files.adamsamec.cz/apps/test.zip";
            //var setupUrlString = updateData.setupUrl;
            var setupUri = new System.Uri(setupUrlString);
            var setupFilename = Path.GetFileName(setupUri.LocalPath);
            var setupDownloadPath = Path.Combine(Consts.SetupDownloadFolder, setupFilename);

            // Make sure empty setup folder is prepared for download
            Directory.CreateDirectory(Consts.SetupDownloadFolder);
            if (_state == UpdateState.FilesExist)
            {
                _state = UpdateState.Deleting;
                DeleteSetupFiles();
            }
            _state = UpdateState.Downloading;

            using (var client = new HttpClientDownloadWithProgress(setupUrlString, setupDownloadPath))
            {
                client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
                {
                    downloadProgressCallback((int)progressPercentage);
                };
                _cancellationToken = new CancellationTokenSource();

                // Start download
                try
                {
                    Debug.WriteLine("Starting download");
                    await client.StartDownload(_cancellationToken);
                    Debug.WriteLine("Downloadd completed successfully");
                    _state = UpdateState.FilesExist;
                    downloadCompleteCallback();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception during download: " + ex.ToString());
                    _state = UpdateState.FilesExist;
                    if (ex is TaskCanceledException)
                    {
                        Debug.WriteLine("Downloadd cancelled by user");
                    }
                    else
                    {
                        downloadErrorCallback();
                    }
                    throw;
                }
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
            if (_cancellationToken != null)
            {
                Debug.WriteLine("Canceling download by user");
                _cancellationToken.CancelAsync();
            }
        }
    }
}
