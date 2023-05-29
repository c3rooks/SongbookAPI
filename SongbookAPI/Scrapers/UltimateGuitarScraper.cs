using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SongbookAPI.Scrapers
{
    public class UltimateGuitarScraper
    {
        private readonly HttpClient _httpClient = new HttpClient();

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
            var tabInfo = new UltimateTabInfo(songTitle, artistName, author, new UltimateTab(), difficulty, key, capo, tuning);

            return tabInfo;
        }

        public async Task<string> GetFirstTabUrl(string songName)
        {
            // URL encode the song name for use in the URL
            songName = Uri.EscapeDataString(songName);

            // Construct the search URL
            string url = $"https://www.ultimate-guitar.com/search.php?search_type=title&order=&value={songName}";

            // Download the search results HTML
            string html = await DownloadHtml(url);

            // Parse the HTML to find the first tab URL
            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(html));

            // Find the search result items
            var searchResults = document.QuerySelectorAll("._1Hd7J");

            var tabLinks = new List<(string Url, int ReviewCount, double TotalStars)>();

            foreach (var searchResult in searchResults)
            {
                var tabLinkElement = searchResult.QuerySelector("._2KJtL._1mes3.kWOod");
                var reviewCountElement = searchResult.QuerySelector("._3KcHJ");
                var totalStarsElement = searchResult.QuerySelector("._3KcHJ");

                if (tabLinkElement == null || reviewCountElement == null || totalStarsElement == null) continue;

                string tabUrl = tabLinkElement.GetAttribute("href");
                int reviewCount = int.Parse(reviewCountElement.TextContent);
                double totalStars = double.Parse(totalStarsElement.TextContent.Split(' ')[0]);

                tabLinks.Add((tabUrl, reviewCount, totalStars));
            }

            // Sort by review count and total stars, and then pick the first tab
            var bestTab = tabLinks.OrderByDescending(t => t.ReviewCount)
                .ThenByDescending(t => t.TotalStars)
                .FirstOrDefault();

            return bestTab.Url;
        }

        public async Task<string> GetTabContent(string url)
        {
            var html = await DownloadHtml(url);
            var doc = new HtmlParser().ParseDocument(html);
            var tabContentNode = doc.QuerySelector("pre.js-tab-content");
            if  (tabContentNode != null)
            {
                return tabContentNode.InnerHtml;
            }
            else
            {
                return null;
            }
        }
    }
}

