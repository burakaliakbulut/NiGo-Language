using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Proje0001.Services
{
    public class WordService
    {
        private readonly IMongoDatabase _context;

        public WordService(IMongoDatabase context)
        {
            _context = context;
        }

        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            try
            {
                var collection = _context.GetCollection<Word>("Words");
                var filter = Builders<Word>.Filter.Empty;
                var categories = await collection.Distinct(x => x.Category, filter).ToListAsync();
                return categories;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get categories", ex);
            }
        }

        public async Task<IEnumerable<Word>> GetWordsByDifficultyAsync(string userId, WordDifficulty difficulty)
        {
            try
            {
                var collection = _context.GetCollection<Word>("Words");
                var filter = Builders<Word>.Filter.Eq(x => x.DifficultyLevel, difficulty);
                var words = await collection.Find(filter).ToListAsync();
                return words;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get words by difficulty", ex);
            }
        }
    }
} 