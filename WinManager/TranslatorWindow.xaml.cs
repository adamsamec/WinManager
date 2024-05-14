using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace WinManager
{
    /// <summary>
    /// Interaction logic for TranslatorWindow.xaml
    /// </summary>
    public partial class TranslatorWindow : Window
    {
        private string? _keyword;

        public TranslatorWindow(string? keyword)
        {
            InitializeComponent();

            _keyword = keyword;
        }

        private void TranslatorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            keywordTextBox.Focus();
        }

        private void translateButton_Click(object sender, RoutedEventArgs e)
        {
            TranslateKeyword();
        }

        private async Task TranslateKeyword()
        {
            if (!String.IsNullOrEmpty(keywordTextBox.Text))
            {
                string pageContent;
                try
                {
                    pageContent = await Page.GetTranslationPageContent(keywordTextBox.Text);
                }
                catch (PageRetrieveFailedException)
                {
                    pageContent = WinManager.Resources.translationRetrievalFailedMessage;
                }
                Page.SetupWebBrowser(webBrowser, pageContent, "cs", false, false);
            }
        }

        public void Display()
        {
            Show();
            Activate();
            keywordTextBox.Focus();
        }

    }
}