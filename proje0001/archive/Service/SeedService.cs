using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WordLearningWpfApp.Data;
using WordLearningWpfApp.Models;

namespace WordLearningWpfApp.Service
{
    public class SeedService
    {
        private readonly MongoDbContext _dbContext;

        public SeedService(MongoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SeedDataAsync()
        {
            try
            {
                // Veritabanı bağlantısını test et
                var database = _dbContext.Database;
                var collections = await database.ListCollectionNamesAsync();
                
                // Koleksiyonların varlığını kontrol et ve gerekirse oluştur
                var collectionNames = await collections.ToListAsync();
                
                if (!collectionNames.Contains("Users"))
                {
                    await database.CreateCollectionAsync("Users");
                }
                
                if (!collectionNames.Contains("Words"))
                {
                    await database.CreateCollectionAsync("Words");
                }
                
                if (!collectionNames.Contains("UserWordProgress"))
                {
                    await database.CreateCollectionAsync("UserWordProgress");
                }

                Console.WriteLine("Veritabanı başarıyla yapılandırıldı.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Veritabanı yapılandırması sırasında hata oluştu: {ex.Message}");
                throw;
            }
        }
    }
} 