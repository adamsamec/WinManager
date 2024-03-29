using System.Windows;

namespace WinManager
{
    /// <summary>
    /// Interaction logic for UpdateAvailableDialog.xaml
    /// </summary>
    public partial class UpdateAvailableDialog : Window
    {
        private Manager _manager;
        private UpdateData _updateData;

        public UpdateAvailableDialog(Manager manager, UpdateData updateData)
        {
            InitializeComponent();

            _manager = manager;
            _updateData = updateData;
        }

        private void UpdateAvailableDialog_Loaded(object sender, RoutedEventArgs e)
        {
            updateAvailableMessage.Text = String.Format(WinManager.Resources.updateAvailableMessage, _updateData.version, Consts.AppVersion);
            yesButton.Focus();
        }

        private void yesButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void notNowButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

    }
}