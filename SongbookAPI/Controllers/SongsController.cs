using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SongbookAPI.Models;
using SongbookAPI.Scrapers;
using System.Net.Http.Headers;


[ApiController]
[Route("[controller]")]
public class SongsController : ControllerBase
{
    private readonly IMongoClient _mongoClient;
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<Song> _songs;

    public SongsController(IMongoClient mongoClient)
    {
        _mongoClient = mongoClient;
        _database = _mongoClient.GetDatabase("SongbookDB");
        _songs = _database.GetCollection<Song>("Songs");
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

}
