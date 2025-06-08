using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using MongoDB.Driver;
using WordLearningWpfApp.Data;
using WordLearningWpfApp.Models;
using Microsoft.Extensions.Logging;

namespace WordLearningWpfApp.Services
{
    public class WordService : IWordService, IService
    {
        private readonly IMongoCollection<Word> _words;
        private readonly IMongoCollection<UserWordProgress> _userWordProgress;
        private readonly IMongoCollection<WordProgress> _wordProgress;
        private readonly IMongoCollection<WordSample> _samples;
        private bool _disposed;
        private readonly ILogger<WordService> _logger;

        public WordService(MongoDbContext dbContext, ILogger<WordService> logger)
        {
            _words = dbContext.Words ?? throw new ArgumentNullException(nameof(dbContext.Words));
            _userWordProgress = dbContext.GetCollection<UserWordProgress>(MongoDbContext.UserWordProgressCollection) 
                ?? throw new ArgumentNullException(nameof(MongoDbContext.UserWordProgressCollection));
            _wordProgress = dbContext.WordProgress ?? throw new ArgumentNullException(nameof(dbContext.WordProgress));
            _samples = dbContext.GetCollection<WordSample>(MongoDbContext.WordSamplesCollection)
                ?? throw new ArgumentNullException(nameof(MongoDbContext.WordSamplesCollection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Initializing WordService...");

                // Create indexes if they don't exist
                var wordIndexKeys = Builders<Word>.IndexKeys
                    .Ascending(w => w.UserId)
                    .Ascending(w => w.Category)
                    .Ascending(w => w.Difficulty);

                var wordProgressIndexKeys = Builders<WordProgress>.IndexKeys
                    .Ascending(wp => wp.UserId)
                    .Ascending(wp => wp.WordId);

                var userWordProgressIndexKeys = Builders<UserWordProgress>.IndexKeys
                    .Ascending(uwp => uwp.UserId)
                    .Ascending(uwp => uwp.WordId);

                await _words.Indexes.CreateOneAsync(new CreateIndexModel<Word>(wordIndexKeys));
                await _wordProgress.Indexes.CreateOneAsync(new CreateIndexModel<WordProgress>(wordProgressIndexKeys));
                await _userWordProgress.Indexes.CreateOneAsync(new CreateIndexModel<UserWordProgress>(userWordProgressIndexKeys));

                _logger.LogInformation("WordService initialization completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing WordService");
                throw new ServiceInitializationException("Failed to initialize WordService", ex);
            }
        }

        public async Task CleanupAsync()
        {
            try
            {
                _logger.LogInformation("Cleaning up WordService...");
                // Perform any necessary cleanup
                await Task.CompletedTask;
                _logger.LogInformation("WordService cleanup completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during WordService cleanup");
                throw new ServiceException("Failed to cleanup WordService", ex);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    _logger.LogInformation("Disposing WordService resources");
                }
                _disposed = true;
            }
        }

        public async Task<Word> GetWordByIdAsync(string wordId)
        {
            try
            {
                if (string.IsNullOrEmpty(wordId))
                    throw new ArgumentException("Word ID cannot be null or empty", nameof(wordId));

                var filter = Builders<Word>.Filter.Eq(w => w.Id, wordId);
                var word = await _words.Find(filter).FirstOrDefaultAsync();
                
                if (word == null)
                    _logger.LogWarning("Word not found with ID: {WordId}", wordId);
                
                return word;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving word with ID: {WordId}", wordId);
                throw new ServiceException($"Failed to retrieve word with ID: {wordId}", ex);
            }
        }

        public async Task<IEnumerable<Word>> GetAllWordsAsync()
        {
            return await _words.Find(_ => true).ToListAsync();
        }

        public async Task<IEnumerable<Word>> GetWordsByCategoryAsync(string category)
        {
            var filter = Builders<Word>.Filter.Eq(w => w.Category, category);
            return await _words.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Word>> GetWordsByUserIdAsync(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

                var filter = Builders<Word>.Filter.Eq(w => w.UserId, userId);
                return await _words.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving words for user: {UserId}", userId);
                throw new ServiceException($"Failed to retrieve words for user: {userId}", ex);
            }
        }

        public async Task<List<Word>> GetWordsByDifficultyAsync(string userId, WordDifficulty difficulty)
        {
            var filter = Builders<Word>.Filter.And(
                Builders<Word>.Filter.Eq(w => w.UserId, userId),
                Builders<Word>.Filter.Eq(w => w.Difficulty, difficulty)
            );
            return await _words.Find(filter).ToListAsync();
        }

        public async Task<List<Word>> GetWordsByTagsAsync(string userId, List<string> tags)
        {
            var filter = Builders<Word>.Filter.And(
                Builders<Word>.Filter.Eq(w => w.UserId, userId),
                Builders<Word>.Filter.All(w => w.Tags, tags)
            );
            return await _words.Find(filter).ToListAsync();
        }

        public async Task<Word> CreateWordAsync(Word word)
        {
            try
            {
                if (word == null)
                    throw new ArgumentNullException(nameof(word));

                word.CreatedAt = DateTime.UtcNow;
                word.UpdatedAt = DateTime.UtcNow;
                word.IsActive = true;

                await _words.InsertOneAsync(word);
                _logger.LogInformation("Created new word with ID: {WordId}", word.Id);
                return word;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating word");
                throw new ServiceException("Failed to create word", ex);
            }
        }

        public async Task<Word> UpdateWordAsync(Word word)
        {
            try
            {
                if (word == null)
                    throw new ArgumentNullException(nameof(word));

                word.UpdatedAt = DateTime.UtcNow;
                var filter = Builders<Word>.Filter.Eq(w => w.Id, word.Id);
                await _words.ReplaceOneAsync(filter, word);
                _logger.LogInformation("Updated word with ID: {WordId}", word.Id);
                return word;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating word with ID: {WordId}", word?.Id);
                throw new ServiceException($"Failed to update word with ID: {word?.Id}", ex);
            }
        }

        public async Task<bool> DeleteWordAsync(string wordId)
        {
            try
            {
                if (string.IsNullOrEmpty(wordId))
                    throw new ArgumentException("Word ID cannot be null or empty", nameof(wordId));

                var filter = Builders<Word>.Filter.Eq(w => w.Id, wordId);
                var result = await _words.DeleteOneAsync(filter);
                var success = result.DeletedCount > 0;
                
                if (success)
                    _logger.LogInformation("Deleted word with ID: {WordId}", wordId);
                else
                    _logger.LogWarning("Word not found for deletion with ID: {WordId}", wordId);
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting word with ID: {WordId}", wordId);
                throw new ServiceException($"Failed to delete word with ID: {wordId}", ex);
            }
        }

        public async Task<IEnumerable<Word>> GetDailyWordsAsync(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

                var today = DateTime.UtcNow.Date;
                var learnedWords = await _userWordProgress
                    .Find(p => p.UserId == userId && p.SuccessCount >= 3)
                    .Project(p => p.WordId)
                    .ToListAsync();

                var filter = Builders<Word>.Filter.And(
                    Builders<Word>.Filter.Eq(w => w.UserId, userId),
                    Builders<Word>.Filter.Nin(w => w.Id, learnedWords),
                    Builders<Word>.Filter.Eq(w => w.IsActive, true)
                );

                return await _words.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving daily words for user: {UserId}", userId);
                throw new ServiceException($"Failed to retrieve daily words for user: {userId}", ex);
            }
        }

        public async Task<List<Word>> GetWeakWordsAsync(string userId, int count = 10)
        {
            var filter = Builders<Word>.Filter.And(
                Builders<Word>.Filter.Eq(w => w.UserId, userId),
                Builders<Word>.Filter.Eq(w => w.IsActive, true)
            );

            var words = await _words.Find(filter)
                .Sort(Builders<Word>.Sort.Ascending(w => w.SuccessRate))
                .Limit(count)
                .ToListAsync();

            return words;
        }

        public async Task<List<Word>> GetStrongWordsAsync(string userId, int count = 10)
        {
            var filter = Builders<Word>.Filter.And(
                Builders<Word>.Filter.Eq(w => w.UserId, userId),
                Builders<Word>.Filter.Eq(w => w.IsActive, true)
            );

            var words = await _words.Find(filter)
                .Sort(Builders<Word>.Sort.Descending(w => w.SuccessRate))
                .Limit(count)
                .ToListAsync();

            return words;
        }

        public async Task<IEnumerable<Word>> SearchWordsAsync(string userId, string searchTerm)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

                var filter = Builders<Word>.Filter.And(
                    Builders<Word>.Filter.Eq(w => w.UserId, userId),
                    Builders<Word>.Filter.Or(
                        Builders<Word>.Filter.Regex(w => w.English, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                        Builders<Word>.Filter.Regex(w => w.Turkish, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
                    )
                );

                return await _words.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching words for user: {UserId}", userId);
                throw new ServiceException($"Failed to search words for user: {userId}", ex);
            }
        }

        public async Task<Dictionary<string, int>> GetWordStatisticsAsync(string userId)
        {
            var stats = new Dictionary<string, int>();
            var words = await GetWordsByUserIdAsync(userId);
            
            stats["Total"] = words.Count();
            stats["Learned"] = words.Count(w => w.IsLearned);
            stats["NotLearned"] = words.Count(w => !w.IsLearned);
            
            return stats;
        }

        public async Task UpdateWordProgressAsync(string userId, string wordId, bool isCorrect)
        {
            try
            {
                var progress = await _userWordProgress
                    .Find(p => p.UserId == userId && p.WordId == wordId)
                    .FirstOrDefaultAsync();

                if (progress == null)
                {
                    progress = new UserWordProgress
                    {
                        UserId = userId,
                        WordId = wordId,
                        ReviewCount = 0,
                        SuccessCount = 0,
                        LastReviewDate = DateTime.UtcNow
                    };
                }

                progress.ReviewCount++;
                if (isCorrect)
                {
                    progress.SuccessCount++;
                }
                progress.LastReviewDate = DateTime.UtcNow;

                var filter = Builders<UserWordProgress>.Filter.And(
                    Builders<UserWordProgress>.Filter.Eq(p => p.UserId, userId),
                    Builders<UserWordProgress>.Filter.Eq(p => p.WordId, wordId)
                );

                var update = Builders<UserWordProgress>.Update
                    .Set(p => p.ReviewCount, progress.ReviewCount)
                    .Set(p => p.SuccessCount, progress.SuccessCount)
                    .Set(p => p.LastReviewDate, progress.LastReviewDate);

                await _userWordProgress.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating word progress for user: {UserId}, word: {WordId}", userId, wordId);
                throw new ServiceException($"Failed to update word progress for user: {userId}, word: {wordId}", ex);
            }
        }

        public async Task<bool> DeactivateWordAsync(string wordId)
        {
            try
            {
                var filter = Builders<Word>.Filter.Eq(w => w.Id, wordId);
                var update = Builders<Word>.Update.Set(w => w.IsActive, false);
                var result = await _words.UpdateOneAsync(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating word: {WordId}", wordId);
                throw new ServiceException($"Failed to deactivate word: {wordId}", ex);
            }
        }

        public async Task<bool> ActivateWordAsync(string wordId)
        {
            try
            {
                var filter = Builders<Word>.Filter.Eq(w => w.Id, wordId);
                var update = Builders<Word>.Update.Set(w => w.IsActive, true);
                var result = await _words.UpdateOneAsync(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating word: {WordId}", wordId);
                throw new ServiceException($"Failed to activate word: {wordId}", ex);
            }
        }

        public async Task<List<Word>> GetWordsForReviewAsync(string userId)
        {
            var today = DateTime.UtcNow.Date;
            var progress = await _userWordProgress
                .Find(p => p.UserId == userId && p.LastReviewDate < today)
                .ToListAsync();

            var wordIds = progress.Select(p => p.WordId).ToList();
            return await _words.Find(w => wordIds.Contains(w.Id)).ToListAsync();
        }

        public async Task<UserWordProgress> GetLatestUserProgressAsync(string userId)
        {
            return await _userWordProgress
                .Find(p => p.UserId == userId)
                .SortByDescending(p => p.LastReviewDate)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetTodayLearnedWordCountAsync(string userId)
        {
            var today = DateTime.UtcNow.Date;
            return (int)await _userWordProgress
                .CountDocumentsAsync(p => p.UserId == userId && p.LastReviewDate >= today);
        }

        public async Task<bool> MarkWordAsLearnedAsync(string wordId, string userId)
        {
            try
            {
                var filter = Builders<Word>.Filter.And(
                    Builders<Word>.Filter.Eq(w => w.Id, wordId),
                    Builders<Word>.Filter.Eq(w => w.UserId, userId)
                );

                var update = Builders<Word>.Update
                    .Set(w => w.IsLearned, true)
                    .Set(w => w.UpdatedAt, DateTime.UtcNow);

                var result = await _words.UpdateOneAsync(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking word as learned: {WordId}", wordId);
                throw new ServiceException($"Failed to mark word as learned: {wordId}", ex);
            }
        }

        public async Task<bool> MarkWordAsDifficultAsync(string wordId, string userId)
        {
            try
            {
                var filter = Builders<Word>.Filter.And(
                    Builders<Word>.Filter.Eq(w => w.Id, wordId),
                    Builders<Word>.Filter.Eq(w => w.UserId, userId)
                );

                var update = Builders<Word>.Update
                    .Set(w => w.Difficulty, WordDifficulty.Hard)
                    .Set(w => w.UpdatedAt, DateTime.UtcNow);

                var result = await _words.UpdateOneAsync(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking word as difficult: {WordId}", wordId);
                throw new ServiceException($"Failed to mark word as difficult: {wordId}", ex);
            }
        }

        public async Task<IEnumerable<Word>> GetLearnedWordsAsync(string userId)
        {
            var filter = Builders<Word>.Filter.And(
                Builders<Word>.Filter.Eq(w => w.UserId, userId),
                Builders<Word>.Filter.Eq(w => w.IsLearned, true)
            );

            return await _words.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Word>> GetDifficultWordsAsync(string userId)
        {
            var filter = Builders<Word>.Filter.And(
                Builders<Word>.Filter.Eq(w => w.UserId, userId),
                Builders<Word>.Filter.Eq(w => w.Difficulty, WordDifficulty.Hard)
            );

            return await _words.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<string>> GetCategoriesAsync(string userId)
        {
            var filter = Builders<Word>.Filter.Eq(w => w.UserId, userId);
            var words = await _words.Find(filter).ToListAsync();
            return words.Select(w => w.Category).Distinct();
        }

        public async Task<List<UserWordProgress>> GetUserWordProgressAsync(string userId)
        {
            var filter = Builders<UserWordProgress>.Filter.Eq(p => p.UserId, userId);
            return await _userWordProgress.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Word>> GetAllWordsAsync(string userId)
        {
            var filter = Builders<Word>.Filter.Eq(w => w.UserId, userId);
            return await _words.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<string>> GetTagsAsync(string userId)
        {
            var filter = Builders<Word>.Filter.Eq(w => w.UserId, userId);
            var words = await _words.Find(filter).ToListAsync();
            return words.SelectMany(w => w.Tags).Distinct();
        }

        public async Task<IEnumerable<Word>> GetDictionaryWordsAsync(string userId)
        {
            var filter = Builders<Word>.Filter.Eq(w => w.UserId, userId);
            return await _words.Find(filter).ToListAsync();
        }

        public async Task<bool> AddWordToLearningAsync(string userId, string wordId)
        {
            var filter = Builders<Word>.Filter.And(
                Builders<Word>.Filter.Eq(w => w.Id, wordId),
                Builders<Word>.Filter.Eq(w => w.UserId, userId)
            );

            var update = Builders<Word>.Update
                .Set(w => w.IsActive, true)
                .Set(w => w.UpdatedAt, DateTime.UtcNow);

            var result = await _words.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<IEnumerable<Word>> SearchDictionaryWordsAsync(string userId, string searchTerm)
        {
            var filter = Builders<Word>.Filter.And(
                Builders<Word>.Filter.Eq(w => w.UserId, userId),
                Builders<Word>.Filter.Or(
                    Builders<Word>.Filter.Regex(w => w.English, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                    Builders<Word>.Filter.Regex(w => w.Turkish, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
                )
            );

            return await _words.Find(filter).ToListAsync();
        }

        public async Task<bool> AddWordAsync(string userId, Word word)
        {
            try
            {
                word.UserId = userId;
                word.CreatedAt = DateTime.UtcNow;
                word.UpdatedAt = DateTime.UtcNow;
                word.IsActive = true;

                await _words.InsertOneAsync(word);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding word for user: {UserId}", userId);
                throw new ServiceException($"Failed to add word for user: {userId}", ex);
            }
        }

        public async Task<bool> UpdateWordProgressAsync(string userId, string wordId, WordProgress progress)
        {
            var filter = Builders<WordProgress>.Filter.And(
                Builders<WordProgress>.Filter.Eq(p => p.UserId, userId),
                Builders<WordProgress>.Filter.Eq(p => p.WordId, wordId)
            );

            var result = await _wordProgress.ReplaceOneAsync(filter, progress, new ReplaceOptions { IsUpsert = true });
            return result.ModifiedCount > 0 || result.UpsertedId != null;
        }

        public async Task<List<Word>> GetWordsAsync(string userId, string? category = null)
        {
            var filter = Builders<Word>.Filter.Eq(w => w.UserId, userId);
            
            if (!string.IsNullOrEmpty(category))
            {
                filter = Builders<Word>.Filter.And(
                    filter,
                    Builders<Word>.Filter.Eq(w => w.Category, category)
                );
            }

            return await _words.Find(filter).ToListAsync();
        }

        public async Task<bool> AddWordSampleAsync(string wordId, WordSample sample)
        {
            try
            {
                sample.WordId = wordId;
                sample.CreatedAt = DateTime.UtcNow;

                await _samples.InsertOneAsync(sample);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding word sample for word: {WordId}", wordId);
                throw new ServiceException($"Failed to add word sample for word: {wordId}", ex);
            }
        }

        public async Task<bool> DeleteWordSampleAsync(string sampleId)
        {
            try
            {
                var filter = Builders<WordSample>.Filter.Eq(s => s.Id, sampleId);
                var result = await _samples.DeleteOneAsync(filter);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting word sample: {SampleId}", sampleId);
                throw new ServiceException($"Failed to delete word sample: {sampleId}", ex);
            }
        }

        public async Task<List<WordSample>> GetWordSamplesAsync(string wordId)
        {
            var filter = Builders<WordSample>.Filter.Eq(s => s.WordId, wordId);
            return await _samples.Find(filter).ToListAsync();
        }

        public async Task<bool> UpdateWordProgressAsync(string wordId, WordProgress progress)
        {
            var filter = Builders<WordProgress>.Filter.Eq(p => p.WordId, wordId);
            var result = await _wordProgress.ReplaceOneAsync(filter, progress, new ReplaceOptions { IsUpsert = true });
            return result.ModifiedCount > 0 || result.UpsertedId != null;
        }
    }
} 