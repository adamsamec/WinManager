using System.Windows;

namespace WinManager
{
    /// <summary>
    /// Interaction logic for LaunchUpdateInstallerDialog.xaml
    /// </summary>
    public partial class LaunchUpdateInstallerDialog : Window
    {
        public LaunchUpdateInstallerDialog()
        {
            InitializeComponent();
        }

        private void LaunchUpdateInstallerDialog_Loaded(object sender, RoutedEventArgs e)
        {       
            launchUpdateInstallerButton.Focus();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        private void launchUpdateInstallerButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}