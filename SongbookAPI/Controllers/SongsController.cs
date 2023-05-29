using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SongbookAPI.Models;
using SongbookAPI.Scrapers;
using System.Net.Http.Headers;
using SongbookAPI.Scrapers;


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
        UltimateGuitarScraper scraper = new UltimateGuitarScraper();
        string tabUrl = await scraper.GetFirstTabUrl(songIn.Name);

        var song = new Song
        {
            Name = songIn.Name,
            Artist = songIn.Artist,
            Genre = songIn.Genre,
            Chords = songIn.Chords,
            SpotifyTrackId = await GetSpotifyTrackId(songIn.Name, songIn.Artist),
            UltimateGuitarTabUrl = tabUrl // Updated here
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

[HttpGet("tabs/{songName}/{artistName}")]
public async Task<ActionResult<Song>> GetTab(string songName, string artistName)
{
    // Check if the song is already in the database
    var song = await _songs.Find<Song>(s => s.Name == songName && s.Artist == artistName).FirstOrDefaultAsync();
    
    // If the song is not in the database, or if it is in the database but the tab has not been fetched yet
    if (song == null || string.IsNullOrEmpty(song.TabContent))
    {
        // Fetch the tab
        UltimateGuitarScraper scraper = new UltimateGuitarScraper();
        string tabUrl = await scraper.GetFirstTabUrl(songName);
        string tabContent = await scraper.GetTabContent(tabUrl); // This method doesn't exist yet, you'll need to implement it

        // If the song is not in the database, create a new song
        if (song == null)
        {
            song = new Song
            {
                Name = songName,
                Artist = artistName,
                // Genre and Chords aren't provided, so they'll need to be set later
                Genre = null,
                Chords = null,
                SpotifyTrackId = await GetSpotifyTrackId(songName, artistName),
                UltimateGuitarTabUrl = tabUrl,
                TabContent = tabContent
            };

            _songs.InsertOne(song);
        }
        // If the song is in the database, update it with the fetched tab
        else
        {
            song.UltimateGuitarTabUrl = tabUrl;
            song.TabContent = tabContent;
            
            _songs.ReplaceOne(s => s.Id == song.Id, song);
        }
    }

    return song;
}



}
