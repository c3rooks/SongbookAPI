using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
namespace SongbookAPI.Models
{
public class Song
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfDefault]
    public string Id { get; set; }

    [BsonElement("Name")]
    public string Name { get; set; }

    public string Artist { get; set; }

    public string TabContent { get; set; }

    public string Genre { get; set; }

    public List<string> Chords { get; set; }

    public string SpotifyTrackId { get; set; }

    public string UltimateGuitarTabUrl { get; set; }
}
}