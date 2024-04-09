using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WinManager
{
    /// <summary>
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : Window
    {
        private Manager _manager;
        private UpdateDownloadInProgressDialog? _updateDownloadInProgressDialog;

        public SettingsDialog(Manager manager)
        {
            InitializeComponent();

            _manager = manager;

            KeyDown += SettingsDialog_KeyDown;
        }

        private void SettingsDialog_Loaded(object sender, RoutedEventArgs e)
        {
            // Retrieve stored settings
            launchOnStartupCheckBox.IsChecked = _manager.AppSettings.launchOnStartup == Config.True;

            // Create apps and windows shortcuts chekcboxes
            var checkBox = new CheckBox { Content = "Windows + F12" };
            var appsStackPanel = new StackPanel();
            appsStackPanel.Children.Add(checkBox);
            appsShortcutsGroup.Content = appsStackPanel;

            // Set state of chekc for updates button and progress bar
            if (_manager.AppUpdater.State == Updater.UpdateState.Downloading || _manager.AppUpdater.State == Updater.UpdateState.Deleting)
            {
                checkForUpdatesButton.IsEnabled = false;
            }
            else
            {
                updateDownloadProgressBar.IsEnabled = false;
            }

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
            _updateDownloadInProgressDialog = new UpdateDownloadInProgressDialog();
            _updateDownloadInProgressDialog.Owner = this;
            var doCancelAndClose = _updateDownloadInProgressDialog.ShowDialog() == true;
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

        private async void checkForUpdatesButton_Click(object sender, RoutedEventArgs e)
        {
            var doUpdate = false;
            UpdateData? updateData = null;
            try
            {
                updateData = await _manager.AppUpdater.CheckForUpdate();
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
                Updater.DownloadProgressHandler downloadProgressHandler = (progress) =>
                {
                    updateDownloadProgressBar.Value = progress;
                };
                Updater.DownloadCompleteHandler downloadCompleteHandler = () =>
                {
                    if (_updateDownloadInProgressDialog != null && _updateDownloadInProgressDialog.IsVisible)
                    {
                        _updateDownloadInProgressDialog.DialogResult = false;
                    }
                    var launchUpdateInstallerDialog = new LaunchUpdateInstallerDialog();
                    launchUpdateInstallerDialog.Owner = this;
                    var doLaunchInstaller = launchUpdateInstallerDialog.ShowDialog() == true;
                    if (doLaunchInstaller)
                    {
                        _manager.AppUpdater.LaunchInstaller();
                    }
                    MakeCheckForUpdateAvailable();
                };
                Updater.InstallerRunningHandler installerRunningHandler = () =>
                {
                    MakeCheckForUpdateAvailable();
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
                    MakeCheckForUpdateAvailable();
                };
                MakeDownloadInProgress();
                var downloadTask = _manager.AppUpdater.DownloadAsync(updateData, downloadProgressHandler, downloadCompleteHandler, installerRunningHandler, downloadErrorHandler);
            }
        }

        private void MakeCheckForUpdateAvailable()
        {
            checkForUpdatesButton.IsEnabled = true;
            checkForUpdatesButton.Focus();
            updateDownloadProgressBar.IsEnabled = false;
        }

        private void MakeDownloadInProgress()
        {
            updateDownloadProgressBar.IsEnabled = true;
            updateDownloadProgressBar.Focus();
            checkForUpdatesButton.IsEnabled = false;
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