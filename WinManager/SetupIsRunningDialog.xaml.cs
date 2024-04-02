using System.Windows;

namespace WinManager
{
    /// <summary>
    /// Interaction logic for SetupIsRunningDialog.xaml
    /// </summary>
    public partial class SetupIsRunningDialog : Window
    {
        public SetupIsRunningDialog()
        {
            InitializeComponent();
        }

        private void SetupIsRunningDialog_Loaded(object sender, RoutedEventArgs e)
        {
            closeButton.Focus();
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

    }
}