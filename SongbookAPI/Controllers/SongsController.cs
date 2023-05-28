using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

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
    public ActionResult<Song> Create(SongDTO songIn)
    {
        var song = new Song
        {
            Name = songIn.Name,
            Artist = songIn.Artist,
            Genre = songIn.Genre,
            Chords = songIn.Chords
        };
        
        _songs.InsertOne(song);  // Assuming _songs is your MongoDB collection
        return CreatedAtRoute("GetSong", new { id = song.Id.ToString() }, song);
    }

    // GET: api/Songs
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Song>>> GetSongs()
    {
        return Ok(await _songs.Find(song => true).ToListAsync());
    }

    // GET: api/Songs/{id}
    [HttpGet("{id}", Name = "GetSong")]
    public async Task<ActionResult<Song>> GetSong(string id)
    {
        var song = await _songs.Find<Song>(song => song.Id == id).FirstOrDefaultAsync();

        if (song == null)
        {
            return NotFound();
        }

        return Ok(song);
    }

    // PUT: api/Songs/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> PutSong(string id, [FromBody] Song songIn)
    {
        var song = await _songs.Find<Song>(song => song.Id == id).FirstOrDefaultAsync();

        if (song == null)
        {
            return NotFound();
        }

        await _songs.ReplaceOneAsync(song => song.Id == id, songIn);

        return NoContent();
    }

    // DELETE: api/Songs/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSong(string id)
    {
        var song = await _songs.Find<Song>(song => song.Id == id).FirstOrDefaultAsync();

        if (song == null)
        {
            return NotFound();
        }

        await _songs.DeleteOneAsync(song => song.Id == id);

        return NoContent();
    }

    // GET: api/Songs/search/{query}
    [HttpGet("search/{query}")]
    public async Task<ActionResult<IEnumerable<Song>>> SearchSongs(string query)
    {
        var songs = await _songs.Find(song => 
            song.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
            song.Artist.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
            song.Genre.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
            .ToListAsync();

        return Ok(songs);
    }

    // POST: api/Songs/{id}/chords
    [HttpPost("{id}/chords")]
    public async Task<ActionResult<Song>> AddChord(string id, [FromBody] string chord)
    {
        var song = await _songs.Find<Song>(song => song.Id == id).FirstOrDefaultAsync();

        if (song == null)
        {
            return NotFound();
        }

        song.Chords.Add(chord);

        await _songs.ReplaceOneAsync(song => song.Id == id, song);

        return Ok(song);
    }
}
