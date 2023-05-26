using MongoDB.Bson; 
using MongoDB.Bson.Serialization.Attributes;

public class Song
{

[BsonId]
[BsonRepresentation(BsonType.ObjectId)]
public string? Id { get; set; }

public string? Title { get; set; }

public string? Artist { get; set; }   

public string? Lyrics { get; set; }

public string? Chords { get; set; }

}
