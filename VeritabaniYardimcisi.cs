using MongoDB.Driver;

public static class VeritabaniYardimcisi
{
    private static readonly string _baglantiMetni = "mongodb://localhost:27017";
    private static readonly string _veritabaniAdi = "NiGoLanguageDB";
    private static IMongoDatabase _veritabani;

    private static void VeritabaniyiBaslat()
    {
        if (_veritabani == null)
        {
            var istemci = new MongoClient(_baglantiMetni);
            _veritabani = istemci.GetDatabase(_veritabaniAdi);
        }
    }

    public static IMongoCollection<Kelime> Kelimeler
    {
        get
        {
            VeritabaniyiBaslat();
            return _veritabani.GetCollection<Kelime>("Kelimeler");
        }
    }

    public static IMongoCollection<KelimeCozum> KelimeCozumler
    {
        get
        {
            VeritabaniyiBaslat();
            return _veritabani.GetCollection<KelimeCozum>("KelimeCozumler");
        }
    }

    public static IMongoCollection<Kullanici> Kullanicilar
    {
        get
        {
            VeritabaniyiBaslat();
            return _veritabani.GetCollection<Kullanici>("Kullanicilar");
        }
    }

    public static IMongoCollection<KullaniciAyar> Ayarlar
    {
        get
        {
            VeritabaniyiBaslat();
            return _veritabani.GetCollection<KullaniciAyar>("Ayarlar");
        }
    }
}
