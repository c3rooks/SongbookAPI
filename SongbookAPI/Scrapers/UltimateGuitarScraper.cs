using PuppeteerSharp;
using System.Threading.Tasks;

namespace SongbookAPI.Scrapers
{
public class UltimateGuitarScraper
{
    private const string SearchUrlFormat = "https://www.ultimate-guitar.com/search.php?search_type=title&value={0}";

    public async Task<string> GetFirstTabUrl(string songName)
    {
        // Launch the browser
        await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
        using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true
        });

        // Open a new page
        using var page = await browser.NewPageAsync();

        // Navigate to the search page
        string searchUrl = string.Format(SearchUrlFormat, Uri.EscapeDataString(songName));
        await page.GoToAsync(searchUrl);

        // Select the first tab URL
        string firstTabUrl = await page.EvaluateFunctionAsync<string>(@"() => {
            let firstTabNode = document.querySelector('div.js-tp_top a.link-primary');
            return firstTabNode ? firstTabNode.href : null;
        }");

        return firstTabUrl;
    }
}
}