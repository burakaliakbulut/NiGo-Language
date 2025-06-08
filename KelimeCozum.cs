using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

public class KelimeCozum
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("kelimeId")]
    public string KelimeId { get; set; }

    [BsonElement("kullaniciAdi")]
    public string KullaniciAdi { get; set; }

    [BsonElement("dogruCozumTarihleri")]
    public List<DateTime> DogruCozumTarihleri { get; set; } = new List<DateTime>();

    [BsonElement("YanlisCozumTarihleri")]
    public List<DateTime> YanlisCozumTarihleri { get; set; } = new List<DateTime>();

}
