using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
namespace WinManager
{
    /// <summary>
    /// Update check, download and launch class
    /// </summary>
    public class Updater
    {
        private UpdateState _state = UpdateState.Initial;
        private CancellationTokenSource? _cancellationTokenSource;
        private string _installerDownloadPath = "";

        private const int FileDeletionTimeLimit = 30000; // 30 seconds   
        private const int FileDeletionCheckInterval = 500; // 0.5 seconds

        public UpdateState State
        {
            get { return _state; }
        }

        public enum UpdateState
        {
            Initial,
            Downloading,
            FilesExist,
            Downloaded,
            Deleting
        }

        public async Task<UpdateData?> CheckForUpdate()
        {
            var updateString = await new HttpClient().GetStringAsync(Consts.ApiUrl);
            var update = JsonSerializer.Deserialize<UpdateData>(updateString);
            if (update != null && update.version == Consts.AppVersion)
            {
                return null;
            }
            return update;
        }

        public delegate void DownloadProgressHandler(int progress);
        public delegate void DownloadCompleteHandler();
        public delegate void InstallerRunningHandler();
        public delegate void DownloadErrorHandler();

        public async Task<bool> DownloadAsync(UpdateData updateData, DownloadProgressHandler downloadProgressHandler, DownloadCompleteHandler downloadCompleteHandler, InstallerRunningHandler installerRunningHandler, DownloadErrorHandler downloadErrorHandler)
        {
            if (_state == UpdateState.Downloading || _state == UpdateState.Deleting)
            {
                Debug.WriteLine("Returning because update is being downloaded or deleted");
                return false;
            }
            var installerUrlString = updateData.installerUrl;
            var installerUri = new System.Uri(installerUrlString);
            var installerFilename = Path.GetFileName(installerUri.LocalPath);
            _installerDownloadPath = Path.Combine(Consts.InstallerDownloadFolder, installerFilename);

            // Check if installer is not running
            if (_state == UpdateState.Downloaded)
            {
                Debug.WriteLine("Update is already downloaded");
                if (Utils.IsFileInUse(_installerDownloadPath))
                {
                    Debug.WriteLine("Returning because update is running");
                    installerRunningHandler();
                    return false;
                }
            }

            // Make sure empty installer folder is prepared for download
            Directory.CreateDirectory(Consts.InstallerDownloadFolder);
            if (_state == UpdateState.FilesExist || _state == UpdateState.Downloaded)
            {
                _state = UpdateState.Deleting;
                DeleteInstallerFiles();
            }
            _state = UpdateState.Downloading;

            using (var client = new HttpClientDownloadWithProgress(installerUrlString, _installerDownloadPath))
            {
                client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
                {
                    downloadProgressHandler((int)progressPercentage);
                };
                _cancellationTokenSource = new CancellationTokenSource();

                // Start download
                try
                {
                    Debug.WriteLine("Starting download");
                    await client.StartDownload(_cancellationTokenSource);
                    Debug.WriteLine("Downloadd completed successfully");
                    _state = UpdateState.Downloaded;
                    downloadCompleteHandler();
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
                        downloadErrorHandler();
                    }
                    throw;
                }
            }
            Debug.WriteLine("End of download function");
            return true;
        }

        public void LaunchInstaller()
        {
            Process.Start(_installerDownloadPath);
        }

        public void DeleteInstallerFiles()
        {
            int elapsedTime = 0;
            Debug.WriteLine("Starting files deletion");
            while (true)
            {
                try
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(Consts.InstallerDownloadFolder);
                    var files = dirInfo.GetFiles();
                    if (files.Count() == 0)
                    {
                        Debug.WriteLine("All files have been deleted");
                        return;
                    }
                    if (elapsedTime > FileDeletionTimeLimit)
                    {
                        Debug.WriteLine("File deletion time limit reached");
                        return;
                    }
                    foreach (var file in files)
                    {
                        file.Delete();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception during files deletion" + ex.ToString());
                }
                Task.Delay(FileDeletionCheckInterval).Wait();
                elapsedTime += FileDeletionCheckInterval;
            }
        }

        public void CancelDownload()
        {
            if (_cancellationTokenSource != null)
            {
                Debug.WriteLine("Canceling download by user");
                _cancellationTokenSource.CancelAsync();
            }
        }
    }
}
