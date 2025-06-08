using MongoDB.Driver;
using System;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("MongoDB bağlantı testi başlıyor...");
            
            // MongoDB bağlantı bilgileri
            const string connectionString = "mongodb://localhost:27017";
            const string databaseName = "WordLearningDB";

            // MongoDB bağlantısını başlat
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);

            // Bağlantıyı test et
            var collections = database.ListCollections().ToList();
            
            Console.WriteLine("MongoDB bağlantısı başarılı!");
            Console.WriteLine($"Veritabanı: {databaseName}");
            Console.WriteLine($"Koleksiyon sayısı: {collections.Count}");
            
            // Koleksiyonları listele
            foreach (var collection in collections)
            {
                Console.WriteLine($"- {collection["name"]}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Hata oluştu!");
            Console.WriteLine($"Hata mesajı: {ex.Message}");
            Console.WriteLine($"Hata detayı: {ex}");
        }

        Console.WriteLine("\nÇıkmak için bir tuşa basın...");
        Console.ReadKey();
    }
} 