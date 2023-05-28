using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

public class Song
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfDefault]    //This attribute tells MongoDB driver to ignore if default (null for reference types)
    public string Id { get; set; }

    [BsonElement("Name")]
    public string Name { get; set; }

    public string Artist { get; set; }

    public string Genre { get; set; }

    public List<string> Chords { get; set; }
}
