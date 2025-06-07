using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class KullaniciAyar
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("kullaniciAdi")]
    public string KullaniciAdi { get; set; }

    [BsonElement("gunlukKelimeSayisi")]
    public int GunlukKelimeSayisi { get; set; } = 10; // Varsayılan 10
    public int XP { get; set; }

}
