using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using AngleSharp;

namespace SongbookAPI.Scrapers
{
public class UltimateGuitarScraper
{
    private readonly HttpClient _httpClient;
    private const string SearchUrlFormat = "https://www.ultimate-guitar.com/search.php?search_type=title&value={0}";


    public UltimateGuitarScraper()
    {
        _httpClient = new HttpClient();
    }
    public string GetTabContent(string html)
    {
        // You need to implement this method. It should parse the HTML
        // to extract the content of the tab.
        throw new NotImplementedException();
    }
    public async Task<string> GetFirstTabUrl(string songName)
    {
        // Format the search URL
        string searchUrl = string.Format(SearchUrlFormat, Uri.EscapeDataString(songName));

        // Get the search page
        HttpResponseMessage response = await _httpClient.GetAsync(searchUrl);

        if (!response.IsSuccessStatusCode)
        {
            // TODO: Handle error
            return null;
        }

        string searchPageHtml = await response.Content.ReadAsStringAsync();

        // Load the HTML into HtmlAgilityPack
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(searchPageHtml);

        // Select the first tab URL
        var firstTabNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='js-tp_top']//a[@class='link-primary']");

        if (firstTabNode == null)
        {
            // TODO: Handle error
            return null;
        }

        string firstTabUrl = firstTabNode.GetAttributeValue("href", null);

        return firstTabUrl;
    }


    public async Task<string> DownloadHtml(string url)
    {
        var response = await _httpClient.GetAsync(url);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    public UltimateTab ParseHtml(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // These XPath queries would need to be updated to match the actual layout of the Ultimate Guitar website.
        var titleNode = doc.DocumentNode.SelectSingleNode("//h1");
        var tablatureNode = doc.DocumentNode.SelectSingleNode("//pre");

        var tab = new UltimateTab
        {
            Title = titleNode?.InnerText.Trim(),
            Tablature = tablatureNode?.InnerText.Trim(),
        };

        return tab;
    }

    public async Task<UltimateTabInfo> ParseTab(string tabUrl)
{
    // Download the tab HTML
    string html = await DownloadHtml(tabUrl);

    // Parse the HTML to extract tab info
    var context = BrowsingContext.New(Configuration.Default);
    var document = await context.OpenAsync(req => req.Content(html));

    // Extract tab info
    string songTitle = document.QuerySelector("._1hI5g._1JOMk._1TvtJ ._5yl5u")?.TextContent ?? "UNKNOWN";
    string artistName = document.QuerySelector("._1hI5g._1JOMk._1TvtJ ._5gIQ4")?.TextContent ?? "UNKNOWN";

    string author = "UNKNOWN";
    string difficulty = null;
    string key = null;
    string capo = null;
    string tuning = null;

    var infoHeaders = document.QuerySelectorAll(".T591d ._2Kc82");
    var infoValues = document.QuerySelectorAll(".T591d ._3L0Da");

    for (int i = 0; i < infoHeaders.Length; i++)
    {
        string header = infoHeaders[i].TextContent.ToLower();
        string value = infoValues[i].TextContent.Trim();

        switch (header)
        {
            case "author":
                author = value;
                break;
            case "difficulty":
                difficulty = value;
                break;
            case "key":
                key = value;
                break;
            case "capo":
                capo = value;
                break;
            case "tuning":
                tuning = value;
                break;
        }
    }

    // Extract tab content
    var tabContentElement = document.QuerySelector("pre._1YgOS");
    var formattedTabString = tabContentElement.InnerHtml;

    var tab = new UltimateTab();
    foreach (var tabLine in formattedTabString.Split('\n'))
    {
        if (string.IsNullOrWhiteSpace(tabLine))
        {
            tab.AppendBlankLine();
        }
        else if (tabLine.Contains("<span>"))
        {
            string sanitizedTabLine = tabLine.Replace("<span>", " ").Replace("</span>", " ");
            tab.AppendChordLine(sanitizedTabLine);
        }
        else
        {
            tab.AppendLyricLine(tabLine);
        }
    }

    // Construct tab info object
var tabInfo = new UltimateTabInfo(songTitle, artistName, tab, author, difficulty, key, capo, tuning, tabUrl);

    return tabInfo;
}



    public UltimateTabInfo ParseTabInfo(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // These XPath queries would need to be updated to match the actual layout of the Ultimate Guitar website.
        var titleNode = doc.DocumentNode.SelectSingleNode("//h1");
        var artistNode = doc.DocumentNode.SelectSingleNode("//a[@class='artist']");
        // ... and so on for the other properties of UltimateTabInfo.

        var tabInfo = new UltimateTabInfo
        {
            Title = titleNode?.InnerText.Trim(),
            Artist = artistNode?.InnerText.Trim(),
            // ... and so on for the other properties of UltimateTabInfo.
        };

        return tabInfo;
    }

    public async Task<UltimateTab> GetSongDetails(string songName, string artistName)
    {
        var searchUrl = $"https://www.ultimate-guitar.com/search.php?search_type=title&value={Uri.EscapeDataString(songName)}%20{Uri.EscapeDataString(artistName)}";
        var html = await DownloadHtml(searchUrl);
        return ParseHtml(html);
    }
} 

}