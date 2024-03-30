using System.Windows;

namespace WinManager
{
    /// <summary>
    /// Interaction logic for UpdateDownloadInProgressDialog.xaml
    /// </summary>
    public partial class UpdateDownloadInProgressDialog : Window
    {
        public UpdateDownloadInProgressDialog()
        {
            InitializeComponent();
        }

        private void UpdateDownloadInProgressDialog_Loaded(object sender, RoutedEventArgs e)
        {   
            continueUpdateDownloadButton.Focus();
        }

        private void cancelUpdateDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
        private void continueUpdateDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}