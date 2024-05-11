using Markdig;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Windows.Controls;

namespace WinManager
{
    /// <summary>
    /// Class for HTML page related features
    /// </summary>
    public static class Page
    {
        public static string GetHelpPageContent()
        {
            return GetHTML(Consts.HelpFileRelativePath);
        }

        public async static Task<string> GetChangeLogPageContent()
        {
            string mdString;
            try
            {
            var url = String.Format(Consts.ChangeLogUrl, WinManager.Resources.Culture.Name);
            mdString = await new HttpClient().GetStringAsync(url);
            } catch (Exception)
            {
                throw new PageRetrieveFailedException("Unable to retrieve the What's new page from Internet");
            }
            try
            {
                var html = Markdown.ToHtml(mdString);
                return html;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception during Markdown conversion: " + ex.ToString());
            }
            return "Error";
    }

        private static string GetHTML(string fileRelativePath)
        {
            var mdRelativePath = String.Format(fileRelativePath, WinManager.Resources.Culture.Name);
            var mdPath = Path.Combine(Consts.InstallFolder, mdRelativePath);
            try
            {
                var mdString = File.ReadAllText(mdPath, Encoding.UTF8);
                var html = Markdown.ToHtml(mdString);
                return html;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception during Markdown file load: " + ex.ToString());
            }
            return "Error";
        }

public static void SetupWebBrowser(WebBrowser webBrowser, string pageContent)
        {
            var html = @"<html lang='" + WinManager.Resources.Culture.TwoLetterISOLanguageName + @"'>
<head>
<meta charset='utf-8'>
</head>
 <body>
<div id='page' tabindex='0'>
" + pageContent + @"
</div>
<script>
function focusBegining() {
var page = document.getElementById('page');
page.focus();
}
</script>
</body>
</html>";
            webBrowser.LoadCompleted += (sendr, e) =>
            {
                webBrowser.InvokeScript("focusBegining");
            };
            webBrowser.NavigateToString(html);
            webBrowser.Focus();
        }
    }

[Serializable]
public class PageRetrieveFailedException : Exception
{
    public PageRetrieveFailedException() { }

    public PageRetrieveFailedException(string message)
        : base(message) { }

    public PageRetrieveFailedException(string message, Exception inner)
        : base(message, inner) { }
}
}

