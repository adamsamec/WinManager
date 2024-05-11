using System.Windows;

namespace WinManager
{
    /// <summary>
    /// Interaction logic for ChangeLogWindow.xaml
    /// </summary>
    public partial class ChangeLogWindow : Window
    {
        private string _pageContent;

        public ChangeLogWindow(string pageContent)
        {
            InitializeComponent();

            _pageContent = pageContent;
        }

        private void ChangeLogWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Page.SetupWebBrowser(webBrowser, _pageContent);
        }
    }
}