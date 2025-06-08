using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using WordLearningWpfApp.Data;
using WordLearningWpfApp.Models;

namespace WordLearningWpfApp.Repository
{
    public class WordRepository
    {
        private readonly IMongoCollection<Word> _words;

        public WordRepository(MongoDbContext dbContext)
        {
            _words = dbContext.Words;
        }

        public async Task<List<Word>> GetAllWordsAsync()
        {
            return await _words.Find(_ => true).ToListAsync();
        }

        public async Task<Word> GetWordByIdAsync(string id)
        {
            return await _words.Find(w => w.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Word>> GetWordsByCategoryAsync(string category)
        {
            return await _words.Find(w => w.Category == category).ToListAsync();
        }

        public async Task<List<Word>> GetWordsByDifficultyAsync(WordDifficultyLevel difficulty)
        {
            return await _words.Find(w => w.DifficultyLevel == difficulty).ToListAsync();
        }

        public async Task<List<Word>> GetWordsByTagAsync(string tag)
        {
            return await _words.Find(w => w.Tags.Contains(tag)).ToListAsync();
        }

        public async Task<bool> AddWordAsync(Word word)
        {
            try
            {
                await _words.InsertOneAsync(word);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateWordAsync(Word word)
        {
            try
            {
                var result = await _words.ReplaceOneAsync(w => w.Id == word.Id, word);
                return result.ModifiedCount > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteWordAsync(string id)
        {
            try
            {
                var result = await _words.DeleteOneAsync(w => w.Id == id);
                return result.DeletedCount > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Word>> SearchWordsAsync(FilterDefinition<Word> filter)
        {
            return await _words.Find(filter).ToListAsync();
        }
    }
} 