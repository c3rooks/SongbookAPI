using MongoDB.Driver;
using SongbookAPI.Models;

public class SongRepository
{
    private readonly IMongoClient _mongoClient;
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<Song> _songs;

    public SongRepository(IMongoClient mongoClient)
    {
        _mongoClient = mongoClient;
        _database = _mongoClient.GetDatabase("SongbookDB");
        _songs = _database.GetCollection<Song>("Songs");
    }

    public async Task<Song> GetSongAsync(string artistName, string songName)
    {
        return await _songs.Find(s => s.ArtistName == artistName && s.SongName == songName).FirstOrDefaultAsync();
    }

    public async Task SaveSongAsync(Song song)
    {
        var options = new ReplaceOptions { IsUpsert = true };
        await _songs.ReplaceOneAsync(s => s.ArtistName == song.ArtistName && s.SongName == song.SongName, song, options);
    }
}
