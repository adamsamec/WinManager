using System.ComponentModel;
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
        private CancellationTokenSource? _cancellationTokenSource;
        private string _installerDownloadPath = "";

        private const int FileDeletionTimeLimit = 30000; // 30 seconds   
        private const int FileDeletionCheckInterval = 500; // 0.5 seconds

        public UpdateState State { get; set; }
        public DownloadingUpdateDialog? DownloadingDialog { get; set; }
        public LaunchUpdateInstallerDialog? LaunchInstallerDialog { get; set; }

        public enum UpdateState
        {
            Initial,
            Downloading,
            FilesExist,
            Downloaded,
            Deleting
        }

        public Updater()
        {
            State = UpdateState.Initial;
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
            if (State == UpdateState.Downloading || State == UpdateState.Deleting)
            {
                Debug.WriteLine("Returning because update is being downloaded or deleted");
                return false;
            }
            //var installerUrlString = "https://files.adamsamec.cz/apps/test.zip"; // URL for testing
            var installerUrlString = updateData.installerUrl;
            var installerUri = new System.Uri(installerUrlString);
            var installerFilename = Path.GetFileName(installerUri.LocalPath);
            _installerDownloadPath = Path.Combine(Consts.InstallerDownloadFolder, installerFilename);

            // Check if installer is not running
            if (State == UpdateState.Downloaded || State == UpdateState.Initial)
            {
                if (File.Exists(_installerDownloadPath) && Utils.IsFileInUse(_installerDownloadPath))
                {
                    Debug.WriteLine("Returning because update is running");
                    installerRunningHandler();
                    return false;
                }
            }

            // Make sure empty installer folder is prepared for download
            Directory.CreateDirectory(Consts.InstallerDownloadFolder);
            if (!Utils.IsDirectoryEmpty(Consts.InstallerDownloadFolder))
            {
                State = UpdateState.FilesExist;
            }
            if (State == UpdateState.FilesExist || State == UpdateState.Downloaded)
            {
                State = UpdateState.Deleting;
                DeleteInstallerFiles();
            }
            State = UpdateState.Downloading;

            using (var client = new HttpClientDownloadWithProgress(installerUrlString, _installerDownloadPath))
            {
                client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
                {
                    if (progressPercentage != null)
                    {
                        downloadProgressHandler((int)progressPercentage);
                    }
                };
                _cancellationTokenSource = new CancellationTokenSource();

                // Start download
                try
                {
                    Debug.WriteLine("Starting download");
                    await client.StartDownload(_cancellationTokenSource);
                    Debug.WriteLine("Downloadd completed successfully");
                    State = UpdateState.Downloaded;
                    downloadCompleteHandler();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception during download: " + ex.ToString());
                    State = UpdateState.FilesExist;
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
            try
            {
                Process.Start(_installerDownloadPath);
            }
            catch (Win32Exception ex)
            {
                Debug.WriteLine("Exception during update installer launch: " + ex.ToString());
            }
        }

        public void DeleteInstallerFiles()
        {
            int elapsedTime = 0;
            Debug.WriteLine("Starting files deletion");
            while (true)
            {
                try
                {
                    var dirInfo = new DirectoryInfo(Consts.InstallerDownloadFolder);
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
