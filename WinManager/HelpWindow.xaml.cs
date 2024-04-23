using System.Windows;

namespace WinManager
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        public HelpWindow()
        {
            InitializeComponent();
        }

        private void HelpWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var pageContent = "<h1>WinManager " + Consts.AppVersion + @"</h1>
" + Page.GetHelpPageContent();
            Page.SetupWebBrowser(webBrowser, pageContent);
        }
    }
}