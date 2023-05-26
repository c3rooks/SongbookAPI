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
        _database = _mongoClient.GetDatabase("YourDatabaseName");
        _songs = _database.GetCollection<Song>("YourCollectionName");
    }

    // GET: api/Songs
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Song>>> GetSongs()
    {
        return Ok(await _songs.Find(song => true).ToListAsync());
    }

    // GET: api/Songs/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Song>> GetSong(string id)
    {
        var song = await _songs.Find<Song>(song => song.Id == id).FirstOrDefaultAsync();

        if (song == null)
        {
            return NotFound();
        }

        return Ok(song);
    }

    // POST: api/Songs
    [HttpPost]
    public async Task<ActionResult<Song>> PostSong([FromBody] Song song)
    {
        await _songs.InsertOneAsync(song);
        return CreatedAtAction(nameof(GetSong), new { id = song.Id }, song);
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
}
