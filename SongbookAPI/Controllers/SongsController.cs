using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SongbookAPI.Models;
using SongbookAPI.Scrapers;
using System.Net.Http;
using System.Net.Http.Headers;

[ApiController]
[Route("[controller]")]
public class SongsController : ControllerBase
{
    private readonly IMongoClient _mongoClient;
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<Song> _songs;
    private readonly UltimateGuitarScraper _scraper;

    public SongsController(IMongoClient mongoClient, UltimateGuitarScraper scraper)
    {
        _mongoClient = mongoClient;
        _scraper = scraper;
        _database = _mongoClient.GetDatabase("SongbookDB");
        _songs = _database.GetCollection<Song>("Songs");
    }

    [HttpGet("tab/{id}")]
    public async Task<IActionResult> GetTab(string id)
    {
        var url = $"https://www.ultimate-guitar.com/tab/{id}";
        var html = await _scraper.DownloadHtml(url);
        var tab = _scraper.ParseHtml(html);
        return Ok(tab);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Song>>> GetSongs()
    {
        return Ok(await _songs.Find(song => true).ToListAsync());
    }

    private async Task<string> GetSpotifyTrackId(string songName, string artistName)
    {
        string accessToken = "BQBJRkezVUnsZMiUZZhaCzUAIqViky0-IBpKFq428t8aOw9MbhxgyR344DnqKMnBhwFdwSrqf_WSLbImBRwyVxgLmCvVwriQNEkFPucezMjWetjDqKw";

        using HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync($"https://api.spotify.com/v1/search?q=track:{songName}%20artist:{artistName}&type=track&limit=1");
    
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var spotifyObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(content);
            var trackId = spotifyObject.tracks.items[0].id;
            
            return trackId;
        }
        else
        {
            throw new Exception("Failed to retrieve track from Spotify.");
        }  
    }

    [HttpGet("tabs/{songName}/{artistName}")]
public async Task<ActionResult<string>> GetTab(string songName, string artistName)
{
        // Check if the song is already in the database
       
        string song = null;

        string tabUrl = "";
    // If the song is not in the database, or if it is in the database but the tab has not been fetched yet
    if (song == null)
    {
        // Fetch the tab
        UltimateGuitarScraper scraper = new UltimateGuitarScraper();
      //  string highestRatedTabUrl = await scraper.GetHighestRatedTabUrl(songName, artistName);
         tabUrl = await scraper.GetFirstTabUrl(songName, artistName);
        string tabContent = await scraper.GetTabContent(tabUrl); // This method doesn't exist yet, you'll need to implement it

       
    }

    // Construct UltimateTabInfo object
    //var tabInfo = new UltimateTabInfo(
    //    //song.Artist,
    //    //"", // Provide the appropriate author value here
    //    //new UltimateTab(),
    //    //"", // Provide the appropriate difficulty value here
    //    //"", // Provide the appropriate key value here
    //    //"", // Provide the appropriate capo value here
    //    //"" // Provide the appropriate tuning value here
    //);

    return tabUrl;
}
}