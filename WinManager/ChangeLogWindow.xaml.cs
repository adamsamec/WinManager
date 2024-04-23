using System.Windows;

namespace WinManager
{
    /// <summary>
    /// Interaction logic for ChangeLogWindow.xaml
    /// </summary>
    public partial class ChangeLogWindow : Window
    {
        public ChangeLogWindow()
        {
            InitializeComponent();
        }

        private void ChangeLogWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var pageContent = Page.GetChangeLogPageContent(); ;
            Page.SetupWebBrowser(webBrowser, pageContent);
        }
    }
}