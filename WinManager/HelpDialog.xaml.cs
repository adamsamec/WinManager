using System.Windows;

namespace WinManager
{
    /// <summary>
    /// Interaction logic for HelpDialog.xaml
    /// </summary>
    public partial class HelpDialog : Window
    {
        public HelpDialog()
        {
            InitializeComponent();
        }

        private void HelpDialog_Loaded(object sender, RoutedEventArgs e)
        {
            Title = WinManager.Resources.helpDialogTitle + Consts.WindowTitleSeparator + Consts.AppName;

            var pageContent = "<h1>Win" +
                "Manager " + Consts.AppVersion + @"</h1>
" + Page.GetHelpPageContent();
            Page.SetupWebBrowser(webBrowser, pageContent);
        }
    }
}