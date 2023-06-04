using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using PuppeteerSharp;
using System.Text.RegularExpressions;

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



        public async Task<string> GetFirstTabUrl(string songName, string artistName)
        {
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();

            // URL encode the song name for use in the URL
            songName = Uri.EscapeDataString(songName);

            string url = $"https://www.ultimate-guitar.com/search.php?search_type=title&value={artistName}%20{songName}";

            // Start the browser and open a new page
            using (var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true }))
            {
                using (var page = await browser.NewPageAsync())
                {
                    await page.SetRequestInterceptionAsync(true);
                    page.Request += async (sender, e) =>
                    {
                        if (e.Request.ResourceType == ResourceType.Image ||
                            e.Request.ResourceType == ResourceType.StyleSheet)
                            await e.Request.AbortAsync();
                        else
                            await e.Request.ContinueAsync();
                    };

                    await page.GoToAsync(url, WaitUntilNavigation.DOMContentLoaded);

                    var resultNodes = await page.QuerySelectorAllAsync(".LQUZJ");
                    double highestRating = 0.0;
                    IElementHandle bestResultNode = null;
                    string highestRatingUrl = null;

                    foreach (var resultNode in resultNodes)
                    {
                        string outerHTML = await resultNode.EvaluateFunctionAsync<string>("el => el.outerHTML");

                        // Extract tab type
                        var matchType = Regex.Match(outerHTML, "<div class=\"lIKMM PdXKy\">([^<]+)</div>");

                        if (matchType.Success)
                        {
                            string type = matchType.Groups[1].Value.ToLower();

                            // Skip 'official' and 'pro' tabs
                            if (type == "official" || type == "pro")
                            {
                                continue;
                            }

                            if (type == "chords" || type == "tab")
                            {
                                // Extract rating
                                var matchRating = Regex.Match(outerHTML, "<div class=\"djFV9\">(\\d+)</div>");
                                if (matchRating.Success && double.TryParse(matchRating.Groups[1].Value, out double rating))
                                {
                                    if (rating > highestRating)
                                    {
                                        highestRating = rating;
                                        bestResultNode = resultNode;

                                        // Extract url
                                        var matchUrl = Regex.Match(outerHTML, "<a href=\"(https[^\"]+)\"");
                                        if (matchUrl.Success)
                                        {
                                            highestRatingUrl = matchUrl.Groups[1].Value;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return highestRatingUrl;
                }
            }
        }

        public async Task<string> GetTabContent(string url)
        {
            // Add a check here to ensure the URL is valid before using it
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri validatedUri))
            {
                var html = await DownloadHtml(url);
                var doc = new HtmlParser().ParseDocument(html);
                var tabContentNode = doc.QuerySelector("pre.js-tab-content");
                if (tabContentNode != null)
                {
                    return tabContentNode.InnerHtml;
                }
            }

            return null;
        }

    }
}

