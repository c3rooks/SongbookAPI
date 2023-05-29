using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Parser; 



namespace SongbookAPI.Scrapers{
public class UltimateGuitarScraper
{
    private HttpClient _httpClient = new HttpClient();

    public async Task<string> DownloadHtml(string url)
    {
        return await _httpClient.GetStringAsync(url);
    }

    public UltimateTab ParseHtml(string html)
    {
        var parser = new HtmlParser();
        var document = parser.ParseDocument(html);
        var tab = new UltimateTab();
        tab.Title = document.QuerySelector("h1").TextContent.Trim();
        var tabContent = document.QuerySelector(".js-store");
        var preContent = tabContent.PreviousElementSibling;
        tab.Tablature = preContent.TextContent;
        return tab;
    }

    public async Task<string> GetFirstTabUrl(string songName)
    {
        throw new NotImplementedException();
    }

    public async Task<string> GetTabContent(string html)
    {
        // You need to implement this method. It should parse the HTML
        // to extract the content of the tab.
        throw new NotImplementedException();
    }
}
}