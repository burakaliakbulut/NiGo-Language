using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Kelime
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("ingilizce")]
    public string Ingilizce { get; set; }

    [BsonElement("turkce")]
    public string Turkce { get; set; }

    [BsonElement("ornekCumle")]
    public string OrnekCumle { get; set; }

    [BsonElement("resimYolu")]
    public string ResimYolu { get; set; }
    [BsonElement("ogrenildi")]
    public bool Ogrenildi { get; set; } = false;

}
