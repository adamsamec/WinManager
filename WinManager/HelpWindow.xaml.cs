using System.Windows;

namespace WinManager
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        private Manager _manager;

        public HelpWindow(Manager manager)
        {
            InitializeComponent();

            _manager = manager;
        }

        private void HelpWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var html = _manager.GetHelpHTML();
            webBrowser.NavigateToString(html);

            webBrowser.Focus();
        }
    }
}