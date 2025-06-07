using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Kullanici
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("kullanici_adi")]
    public string KullaniciAdi { get; set; }

    [BsonElement("sifre")]
    public string Sifre { get; set; }
}

