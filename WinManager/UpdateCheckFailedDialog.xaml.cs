using System.Windows;

namespace WinManager
{
    /// <summary>
    /// Interaction logic for UpdateCheckFailedDialog.xaml
    /// </summary>
    public partial class UpdateCheckFailedDialog : Window
    {
        private Manager _manager;

        public UpdateCheckFailedDialog(Manager manager)
        {
            InitializeComponent();

            _manager = manager;
        }

        private void UpdateCheckFailedDialog_Loaded(object sender, RoutedEventArgs e)
        {
            closeButton.Focus();
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

    }
}