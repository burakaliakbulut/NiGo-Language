using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using WordLearningWpfApp.Data;
using WordLearningWpfApp.Models;

namespace WordLearningWpfApp.Repository
{
    public class UserWordProgressRepository
    {
        private readonly MongoDbContext _dbContext;

        public UserWordProgressRepository(MongoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<UserWordProgress>> GetUserProgressAsync(string userId)
        {
            return await _dbContext.UserWordProgress
                .Find(Builders<UserWordProgress>.Filter.Eq(p => p.UserId, userId))
                .ToListAsync();
        }

        public async Task<UserWordProgress?> GetProgressAsync(string userId, string wordId)
        {
            return await _dbContext.UserWordProgress
                .Find(Builders<UserWordProgress>.Filter.And(
                    Builders<UserWordProgress>.Filter.Eq(p => p.UserId, userId),
                    Builders<UserWordProgress>.Filter.Eq(p => p.WordId, wordId)))
                .FirstOrDefaultAsync();
        }

        public async Task<bool> AddOrUpdateAsync(UserWordProgress progress)
        {
            try
            {
                var existingProgress = await GetProgressAsync(progress.UserId, progress.WordId);
                if (existingProgress == null)
                {
                    await _dbContext.UserWordProgress.InsertOneAsync(progress);
                }
                else
                {
                    await _dbContext.UserWordProgress.ReplaceOneAsync(p => p.Id == existingProgress.Id, progress);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string userId, string wordId)
        {
            try
            {
                var result = await _dbContext.UserWordProgress
                    .DeleteOneAsync(p => p.UserId == userId && p.WordId == wordId);
                return result.DeletedCount > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<UserWordProgress>> GetDueWordsAsync(string userId, DateTime now, int limit)
        {
            return await _dbContext.UserWordProgress
                .Find(Builders<UserWordProgress>.Filter.And(
                    Builders<UserWordProgress>.Filter.Eq(p => p.UserId, userId),
                    Builders<UserWordProgress>.Filter.Lte(p => p.NextReview, now)))
                .Limit(limit)
                .ToListAsync();
        }

        public async Task<double> GetSuccessRateAsync(string userId)
        {
            var progresses = await _dbContext.UserWordProgress.Find(Builders<UserWordProgress>.Filter.Eq(p => p.UserId, userId)).ToListAsync();
            var totalAnswers = progresses.Sum(p => p.CorrectCount);
            if (totalAnswers == 0) return 0;
            return (double)progresses.Sum(p => p.CorrectCount) / totalAnswers * 100;
        }
    }
} 