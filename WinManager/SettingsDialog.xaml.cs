using System.Diagnostics;
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
        }

        private void SettingsDialog_Loaded(object sender, RoutedEventArgs e)
        {
            // Retrieve stored settings
            launchOnStartupCheckBox.IsChecked = _manager.Settings.launchOnStartup == Config.TRUE;

            launchOnStartupCheckBox.Focus();
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void launchOnStartupCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _manager.ChangeLaunchOnStartupSetting(true);
        }

        private void launchOnStartupCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _manager.ChangeLaunchOnStartupSetting(false);
                }

    }
}