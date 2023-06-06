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

    [HttpGet("tabs/{songName}/{artistName}")]
    public async Task<ActionResult<string>> GetTab(string songName, string artistName)
    {
        // Check if the song is already in the database
        var song = await _songs.Find(s => s.ArtistName == artistName && s.SongName == songName).FirstOrDefaultAsync();

        // If the song is not in the database, or if it is in the database but the tab has not been fetched yet
        if (song == null || string.IsNullOrEmpty(song.TabUrl))
        {
            // Fetch the tab
            var tabUrl = await _scraper.GetFirstTabUrl(songName, artistName);
            var tabContent = await _scraper.GetTabContent(tabUrl);

            // Create a new song if it does not exist in the database
            if (song == null)
            {
                song = new Song { ArtistName = artistName, SongName = songName };
            }

            // Update song with fetched tab
            song.TabUrl = tabUrl;
            song.TabContent = tabContent;

            // Upsert song in the database
            var options = new ReplaceOptions { IsUpsert = true };
            await _songs.ReplaceOneAsync(s => s.ArtistName == artistName && s.SongName == songName, song, options);
        }

        // At this point, song contains the fetched tab
        return song.TabUrl;
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

}