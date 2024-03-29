using System.Windows;

namespace WinManager
{
    /// <summary>
    /// Interaction logic for NoUpdateAvailableDialog.xaml
    /// </summary>
    public partial class NoUpdateAvailableDialog : Window
    {
        public NoUpdateAvailableDialog()
        {
            InitializeComponent();
        }

        private void NoUpdateAvailableDialog_Loaded(object sender, RoutedEventArgs e)
        {
            noUpdateAvailableMessage.Text = String.Format(WinManager.Resources.noUpdateAvailableMessage, Consts.AppVersion);
            closeButton.Focus();
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

    }
}