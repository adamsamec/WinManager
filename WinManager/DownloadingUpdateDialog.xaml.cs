using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace WinManager
{
    /// <summary>
    /// Interaction logic for DownloadingUpdateDialog.xaml
    /// </summary>
    public partial class DownloadingUpdateDialog : Window
    {
        private Manager _manager;
        private UpdateData? _updateData;
        private UpdateDownloadInProgressDialog? _updateDownloadInProgressDialog;

        public DownloadingUpdateDialog(Manager manager, UpdateData? updateData)
        {
            InitializeComponent();

            _manager = manager;
            _updateData = updateData;

            _manager.AppUpdater.DownloadingDialog = this;
            KeyDown += DownloadingUpdateDialog_KeyDown;
        }

        public void DownloadUpdate()
        {
            Updater.DownloadProgressHandler downloadProgressHandler = (progress) =>
            {
                if (_manager.AppUpdater.DownloadingDialog != null)
                {
                _manager.AppUpdater.DownloadingDialog.updateDownloadProgressBar.Value = progress;
                }
            };
            Updater.DownloadCompleteHandler downloadCompleteHandler = () =>
            {
                if (_updateDownloadInProgressDialog != null && _updateDownloadInProgressDialog.IsVisible)
                {
                    _updateDownloadInProgressDialog.DialogResult = false;
                }
                ShowLaunchUpdateInstallerDialog();
                if (_manager.AppUpdater.DownloadingDialog != null) { 
                _manager.AppUpdater.DownloadingDialog.DialogResult = true;
                }
            };
            Updater.InstallerRunningHandler installerRunningHandler = () =>
            {
                var updateInstallRunningDialog = new UpdateInstallRunningDialog();
                updateInstallRunningDialog.Owner = this;
                updateInstallRunningDialog.ShowDialog();
            };
            Updater.DownloadErrorHandler downloadErrorHandler = () =>
            {
                var updateDownloadFailedDialog = new UpdateDownloadFailedDialog();
                if (IsVisible)
                {
                    updateDownloadFailedDialog.Owner = this;
                }
                updateDownloadFailedDialog.ShowDialog();
            };
            if (_updateData != null)
            {
            var downloadTask = _manager.AppUpdater.DownloadAsync(_updateData, downloadProgressHandler, downloadCompleteHandler, installerRunningHandler, downloadErrorHandler);
            }
        }

        public void ShowLaunchUpdateInstallerDialog()
        {
            if (_manager.AppUpdater.State != Updater.UpdateState.Downloaded)
            {
                return;
            }
            var launchUpdateInstallerDialog = new LaunchUpdateInstallerDialog();
            _manager.AppUpdater.LaunchInstallerDialog = launchUpdateInstallerDialog;
            if (IsVisible)
            {
                launchUpdateInstallerDialog.Owner = this;
            }
            var doLaunchInstaller = launchUpdateInstallerDialog.ShowDialog() == true;
            _manager.AppUpdater.State = Updater.UpdateState.Initial;
            if (doLaunchInstaller)
            {
                _manager.AppUpdater.LaunchInstaller();
            }
        }

        private void DownloadingUpdateDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (_updateData != null) { 
            downloadingUpdateMessage.Text = String.Format(WinManager.Resources.downloadingUpdateMessage, _updateData.version, Consts.AppVersion);
            }
            updateDownloadProgressBar.Focus();
        }

        private void DownloadingUpdateDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape || (e.Key == Key.System && e.SystemKey == Key.F4))
            {
                if (CancelUpdateDownloadAndClose())
                {
                    DialogResult = true;
                }
            }
        }

        private bool CancelUpdateDownloadAndClose()
        {
            if (_manager.AppUpdater.State != Updater.UpdateState.Downloading)
            {
                return true;
            }
            _updateDownloadInProgressDialog = new UpdateDownloadInProgressDialog();
            _updateDownloadInProgressDialog.Owner = this;
            var doCancelAndClose = _updateDownloadInProgressDialog.ShowDialog() == true;
            if (doCancelAndClose)
            {
                _manager.AppUpdater.CancelDownload();
            }
            return doCancelAndClose;
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            if (CancelUpdateDownloadAndClose())
            {
                DialogResult = true;
            }
        }

    }
}