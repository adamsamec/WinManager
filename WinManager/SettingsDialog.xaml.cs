using System.Windows;
using System.Windows.Controls;

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

            CreateShortcutsCheckBoxes();

        }

        private void SettingsDialog_Loaded(object sender, RoutedEventArgs e)
        {
            // Set checkboxes state from settings
            launchOnStartupCheckBox.IsChecked = Config.StringToBool(_manager.AppSettings.launchOnStartup);
            checkUpdateOnFirstShowCheckBox.IsChecked = Config.StringToBool(_manager.AppSettings.checkUpdateOnFirstShow);

            // Set initial focus
            launchOnStartupCheckBox.Focus();
        }

        private void CreateShortcutsCheckBoxes()
        {
            var appsStackPanel = new StackPanel();
            var windowsStackPanel = new StackPanel();
            var translatorStackPanel = new StackPanel();
            foreach (var shortcut in _manager.TriggerShortcuts)
            {
                var checkBox = new CheckBox
                {
                    Content = shortcut.Text,
                    IsChecked = shortcut.IsEnabled
                };
                checkBox.Checked += (sender, e) =>
                {
                    checkBox.IsChecked = _manager.ChangeEnabledStateForTriggerShortcut(shortcut, true);
                };
                checkBox.Unchecked += (sender, e) =>
                {
                    checkBox.IsChecked = _manager.ChangeEnabledStateForTriggerShortcut(shortcut, false);
                };
                switch (shortcut.Action)
                {
                    case TriggerShortcut.TriggerAction.ShowApps:
                        appsStackPanel.Children.Add(checkBox);
                        break;
                    case TriggerShortcut.TriggerAction.ShowWindows:
                        windowsStackPanel.Children.Add(checkBox);
                        break;
                    case TriggerShortcut.TriggerAction.ShowTranslator:
                        translatorStackPanel.Children.Add(checkBox);
                        break;
                }
            }
            appsShortcutsGroup.Content = appsStackPanel;
            windowsShortcutsGroup.Content = windowsStackPanel;
            translatorShortcutsGroup.Content = translatorStackPanel;
        }

        private void launchOnStartupCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _manager.ChangeLaunchOnStartupSetting(true);
        }

        private void launchOnStartupCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _manager.ChangeLaunchOnStartupSetting(false);
        }

        private void checkUpdateOnFirstShowCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _manager.ChangeCheckUpdateOnFirstShowSetting(true);
        }

        private void checkUpdateOnFirstShowCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _manager.ChangeCheckUpdateOnFirstShowSetting(false);
        }

        private async void checkForUpdateButton_Click(object sender, RoutedEventArgs e)
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
                _manager.AppUpdateData = updateData;
                var downloadingUpdateDialog = new DownloadingUpdateDialog(_manager, updateData);
                downloadingUpdateDialog.Owner = this;
                downloadingUpdateDialog.DownloadUpdate();
                downloadingUpdateDialog.ShowDialog();
            }
        }
    }
}