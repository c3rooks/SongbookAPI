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

    [HttpPost] 
public async Task<ActionResult<Song>> Create(SongDTO songIn)
{
    string tabUrl = await _scraper.GetFirstTabUrl(songIn.Name);

    var tabInfo = await _scraper.ParseTab(tabUrl);

    var song = new Song
    {
        Name = songIn.Name,
        Artist = songIn.Artist,
        Genre = songIn.Genre,
        Chords = songIn.Chords,
        SpotifyTrackId = await GetSpotifyTrackId(songIn.Name, songIn.Artist),
        UltimateGuitarTabUrl = tabUrl,
        //TabContent = tabInfo.TabContent != null ? tabInfo.TabContent : string.Empty, // Commented out this line
        Author = tabInfo.Author,
        Difficulty = tabInfo.Difficulty,
        Key = tabInfo.Key,
        Capo = tabInfo.Capo,
        Tuning = tabInfo.Tuning,
        TabUrl = tabInfo.TabUrl
    };

    _songs.InsertOne(song);
    return CreatedAtRoute("GetSong", new { id = song.Id.ToString() }, song);
}



    [HttpGet]
    public async Task<ActionResult<IEnumerable<Song>>> GetSongs()
    {
        return Ok(await _songs.Find(song => true).ToListAsync());
    }

    [HttpGet("{id}", Name = "GetSong")]
    public async Task<ActionResult<Song>> GetSong(string id)
    {
        var song = await _songs.Find<Song>(song => song.Id == id).FirstOrDefaultAsync();

        if (song == null)
        {
            return NotFound();
        }

        return song;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, Song songIn)
    {
        var song = await _songs.Find<Song>(s => s.Id == id).FirstOrDefaultAsync();

        if (song == null)
        {
            return NotFound();
        }

        _songs.ReplaceOne(s => s.Id == id, songIn);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var song = await _songs.Find<Song>(song => song.Id == id).FirstOrDefaultAsync();

        if (song == null)
        {
            return NotFound();
        }

        _songs.DeleteOne(song => song.Id == id);

        return NoContent();
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

    [HttpGet("song/{songName}/{artistName}")]
public async Task<ActionResult<UltimateTabInfo>> GetSongInfo(string songName, string artistName)
{
    // Check MongoDB collection for existing song
    var song = await _songs.Find<Song>(s => s.Name == songName && s.Artist == artistName).FirstOrDefaultAsync();

    if (song != null)
    {
        // If song exists in DB, return it
        return Ok(song);
    }
    else
    {
        // If song does not exist in DB, scrape info from Ultimate-Guitar.com
        var tabInfo = await _scraper.ParseTab($"https://www.ultimate-guitar.com/search.php?search_type=title&value={Uri.EscapeDataString(songName)}%20{Uri.EscapeDataString(artistName)}");

        // Add new song to DB
        var newSong = new Song
        {
            Name = tabInfo.SongName,
            Artist = tabInfo.ArtistName,
            TabUrl = tabInfo.TabUrl
            // ...set other properties from tabInfo
        };
        await _songs.InsertOneAsync(newSong);

        // Return scraped song info
        return Ok(tabInfo);
    }
}


[HttpGet("tabs/{songName}/{artistName}")]
public async Task<ActionResult<UltimateTabInfo>> GetTab(string songName, string artistName)
{
    var song = await _songs.Find<Song>(s => s.Name == songName && s.Artist == artistName).FirstOrDefaultAsync();

    if (song == null || string.IsNullOrEmpty(song.TabUrl)) // Changed from TabContent to TabUrl
    {
        UltimateGuitarScraper scraper = new UltimateGuitarScraper();
        string tabUrl = await scraper.GetFirstTabUrl(songName);
        //string tabContent = await scraper.GetTabContent(tabUrl); // Commented out this line

        if (song == null)
        {
            song = new Song
            {
                Name = songName,
                Artist = artistName,
                Genre = null,
                Chords = null,
                SpotifyTrackId = await GetSpotifyTrackId(songName, artistName),
                UltimateGuitarTabUrl = tabUrl,
                //TabContent = tabContent // Commented out this line
            };

            _songs.InsertOne(song);
        }
        else
        {
            song.UltimateGuitarTabUrl = tabUrl;
            //_songs.ReplaceOne(s => s.Id == song.Id, song); // Commented out this line
        }
    }

    var tabInfo = new UltimateTabInfo(
        song.Name,
        song.Artist,
        song.Genre,
        song.Chords,
        song.SpotifyTrackId,
        song.UltimateGuitarTabUrl,
        //song.TabContent, // Commented out this line
        new UltimateTab(), 
        "", 
        "", 
        "", 
        "", 
        song.TabUrl // Changed from tabUrl to TabUrl
    );

    return Ok(tabInfo);
}

}