using System.Windows;
using System.Windows.Input;

namespace WinManager
{
    /// <summary>
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : Window
    {
        private Manager _manager;

        public SettingsDialog(Manager manager)
        {
            InitializeComponent();

            _manager = manager;

            KeyDown += SettingsDialog_KeyDown;
        }

        private void SettingsDialog_Loaded(object sender, RoutedEventArgs e)
        {
            // Retrieve stored settings
            launchOnStartupCheckBox.IsChecked = _manager.AppSettings.launchOnStartup == Config.TRUE;

            // Set initial focus
            launchOnStartupCheckBox.Focus();
        }

        private void SettingsDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape || (e.Key == Key.System && e.SystemKey == Key.F4))
            {
                if (CancelUpdateDownloadAndClose())
                {
                    DialogResult = true;
                }
                else
                {
                    e.Handled = true;
                }
            }
        }

        private bool CancelUpdateDownloadAndClose()
        {
            if (_manager.AppUpdater.State != Updater.UpdateState.Downloading)
            {
                return true;
            }
            var dialog = new UpdateDownloadInProgressDialog();
            dialog.Owner = this;
            var doCancelAndClose = dialog.ShowDialog() == true;
            if (doCancelAndClose)
            {
                _manager.AppUpdater.CancelDownload();
            }
            return doCancelAndClose;
        }

        private void launchOnStartupCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _manager.ChangeLaunchOnStartupSetting(true);
        }

        private void launchOnStartupCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _manager.ChangeLaunchOnStartupSetting(false);
        }

        private void checkForUpdatesButton_Click(object sender, RoutedEventArgs e)
        {
            var doUpdate = false;
            UpdateData? updateData = null;
            try
            {
                updateData = _manager.AppUpdater.CheckForUpdate();
                if (updateData == null)
                {
                    var noUpdateAvailableDialog = new NoUpdateAvailableDialog();
                    noUpdateAvailableDialog.Owner = this;
                    noUpdateAvailableDialog.ShowDialog();
                }
                else
                {
                    var updateAvailableDialog = new UpdateAvailableDialog(_manager, updateData);
                    updateAvailableDialog.Owner = this;
                    doUpdate = updateAvailableDialog.ShowDialog() == true;
                }
            }
            catch (Exception)
            {
                var updateCheckFailedDialog = new UpdateCheckFailedDialog();
                updateCheckFailedDialog.Owner = this;
                updateCheckFailedDialog.ShowDialog();
            }
            if (doUpdate && updateData != null)
            {
                Updater.DownloadProgressCallback downloadProgressCallback = (progress) =>
                {
                    updateDownloadProgressBar.Value = progress;
                };
                Updater.DownloadCompleteCallback downloadCompleteCallback = () =>
                {
                };
                Updater.DownloadErrorCallback downloadErrorCallback = () =>
                {
                    var updateDownloadFailedDialog = new UpdateDownloadFailedDialog();
                    if (this.IsVisible)
                    {
                        updateDownloadFailedDialog.Owner = this;
                    }
                    updateDownloadFailedDialog.ShowDialog();
                };
                var downloadResult = _manager.AppUpdater.DownloadAsync(updateData, downloadProgressCallback, downloadCompleteCallback, downloadErrorCallback);
            }
        }

        private void ShowUpdateDownloadProgress()
        {

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