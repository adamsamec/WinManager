using Markdig;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Windows.Controls;
using HtmlAgilityPack;
using System.Security.Policy;

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
            var url = String.Format(Consts.ChangeLogUrl, WinManager.Resources.Culture.Name);
            string mdString;
            try
            {
                mdString = await new HttpClient().GetStringAsync(url);
            }
            catch (Exception)
            {
                throw new PageRetrieveFailedException("Unable to retrieve the What's new page from the Internet");
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

        public async static Task<string> GetTranslationPageContent(string keyword)
        {
            var htmlString = ""; 
            var encodedKeyword = HttpUtility.UrlEncode(keyword);
            var url = Consts.translatorUrl + encodedKeyword;
            try {
                var web = new HtmlWeb();
                var document = web.Load(url);

                // Replace all the link href attributes to "#"
                foreach (var link in document.DocumentNode.SelectNodes("//a[@href]"))
                {
                    var attribute = link.Attributes["href"];
                    attribute.Value = "#";
                }

                // Select the first <article>
                var articles = document.DocumentNode.SelectNodes("//article");
                if (articles != null && articles.Count() >= 1)
                {
                    foreach (var article in articles)
                    {
                    htmlString += article.OuterHtml;
                    }
                }
                else
                {
                    htmlString = WinManager.Resources.translationNotFoundMessage;
                }
            }
            catch (Exception)
            {
                throw new PageRetrieveFailedException("Unable to retrieve the translation page from the Internet");
            }
            return htmlString;
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

        public static void SetupWebBrowser(WebBrowser webBrowser, string pageContent, string? languageCode = null, bool makePageTabable = true, bool doFocus = true)
        {
            if (languageCode == null)
            {
                languageCode = WinManager.Resources.Culture.TwoLetterISOLanguageName;
            }
            var pageTabIndex = makePageTabable ? "0" : "-1";
            var html = @"<html lang='" + languageCode + @"'>
<head>
<meta charset='utf-8'>
</head>
 <body>
<div id='page' tabindex='" + pageTabIndex + @"'>
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
                if (doFocus)
                { 
                webBrowser.InvokeScript("focusBegining");
                }
            };
            webBrowser.NavigateToString(html);
            if (doFocus)
            {
                webBrowser.Focus();
            }
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

