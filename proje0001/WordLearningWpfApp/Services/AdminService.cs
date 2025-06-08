using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using WordLearningWpfApp.Data;
using WordLearningWpfApp.Models;

namespace WordLearningWpfApp.Services
{
    public class AdminService : IAdminService
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Word> _words;
        private readonly IMongoCollection<UserWordProgress> _userWordProgress;
        private readonly IMongoCollection<ExamResult> _examResults;

        public AdminService(MongoDbContext dbContext)
        {
            _users = dbContext.GetCollection<User>(MongoDbContext.UsersCollection);
            _words = dbContext.GetCollection<Word>(MongoDbContext.WordsCollection);
            _userWordProgress = dbContext.GetCollection<UserWordProgress>(MongoDbContext.UserWordProgressCollection);
            _examResults = dbContext.GetCollection<ExamResult>(MongoDbContext.ExamResultsCollection);
        }

        public async Task<bool> ActivateUserAsync(string userId)
        {
            var update = Builders<User>.Update.Set(u => u.IsActive, true);
            var result = await _users.UpdateOneAsync(u => u.Id == userId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeactivateUserAsync(string userId)
        {
            var update = Builders<User>.Update.Set(u => u.IsActive, false);
            var result = await _users.UpdateOneAsync(u => u.Id == userId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<Statistics> GetSystemStatisticsAsync()
        {
            var stats = new Statistics
            {
                TotalUsers = (int)await _users.CountDocumentsAsync(_ => true),
                TotalWords = (int)await _words.CountDocumentsAsync(_ => true),
                TotalExams = (int)await _examResults.CountDocumentsAsync(_ => true),
                ActiveUsers = (int)await _users.CountDocumentsAsync(u => u.IsActive),
                CreatedAt = DateTime.UtcNow
            };
            return stats;
        }

        public async Task<Dictionary<string, int>> GetUserActivityStatsAsync()
        {
            var stats = new Dictionary<string, int>();
            var today = DateTime.UtcNow.Date;
            var lastWeek = today.AddDays(-7);

            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("lastLoginAt", new BsonDocument("$gte", lastWeek))),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", new BsonDocument("$dateToString", new BsonDocument { { "format", "%Y-%m-%d" }, { "date", "$lastLoginAt" } }) },
                    { "count", new BsonDocument("$sum", 1) }
                })
            };

            var results = await _users.Aggregate<BsonDocument>(pipeline).ToListAsync();
            foreach (var result in results)
            {
                stats[result["_id"].AsString] = result["count"].AsInt32;
            }

            return stats;
        }

        public async Task<Dictionary<string, int>> GetWordUsageStatsAsync()
        {
            var stats = new Dictionary<string, int>();
            var pipeline = new[]
            {
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$wordId" },
                    { "count", new BsonDocument("$sum", 1) }
                }),
                new BsonDocument("$sort", new BsonDocument("count", -1)),
                new BsonDocument("$limit", 100)
            };

            var results = await _userWordProgress.Aggregate<BsonDocument>(pipeline).ToListAsync();
            foreach (var result in results)
            {
                stats[result["_id"].AsString] = result["count"].AsInt32;
            }

            return stats;
        }

        public async Task<Dictionary<string, double>> GetCategorySuccessRatesAsync()
        {
            var stats = new Dictionary<string, double>();
            var pipeline = new[]
            {
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$category" },
                    { "total", new BsonDocument("$sum", 1) },
                    { "correct", new BsonDocument("$sum", new BsonDocument("$cond", new BsonArray { "$isCorrect", 1, 0 })) }
                })
            };

            var results = await _examResults.Aggregate<BsonDocument>(pipeline).ToListAsync();
            foreach (var result in results)
            {
                var total = result["total"].AsInt32;
                var correct = result["correct"].AsInt32;
                stats[result["_id"].AsString] = total > 0 ? (double)correct / total * 100 : 0;
            }

            return stats;
        }

        public async Task<Dictionary<string, int>> GetUserRegistrationStatsAsync(DateTime startDate, DateTime endDate)
        {
            var stats = new Dictionary<string, int>();
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("createdAt", new BsonDocument
                {
                    { "$gte", startDate },
                    { "$lte", endDate }
                })),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", new BsonDocument("$dateToString", new BsonDocument { { "format", "%Y-%m-%d" }, { "date", "$createdAt" } }) },
                    { "count", new BsonDocument("$sum", 1) }
                })
            };

            var results = await _users.Aggregate<BsonDocument>(pipeline).ToListAsync();
            foreach (var result in results)
            {
                stats[result["_id"].AsString] = result["count"].AsInt32;
            }

            return stats;
        }

        public async Task<Dictionary<string, int>> GetExamCompletionStatsAsync()
        {
            var stats = new Dictionary<string, int>();
            var pipeline = new[]
            {
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", new BsonDocument("$dateToString", new BsonDocument { { "format", "%Y-%m-%d" }, { "date", "$createdAt" } }) },
                    { "count", new BsonDocument("$sum", 1) }
                })
            };

            var results = await _examResults.Aggregate<BsonDocument>(pipeline).ToListAsync();
            foreach (var result in results)
            {
                stats[result["_id"].AsString] = result["count"].AsInt32;
            }

            return stats;
        }

        public async Task<Dictionary<string, double>> GetAverageScoresByCategoryAsync()
        {
            var stats = new Dictionary<string, double>();
            var pipeline = new[]
            {
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$category" },
                    { "averageScore", new BsonDocument("$avg", "$score") }
                })
            };

            var results = await _examResults.Aggregate<BsonDocument>(pipeline).ToListAsync();
            foreach (var result in results)
            {
                stats[result["_id"].AsString] = result["averageScore"].AsDouble;
            }

            return stats;
        }

        public async Task<bool> BackupDatabaseAsync(string backupPath)
        {
            try
            {
                // Implementation would depend on your database backup strategy
                // This is a placeholder for the actual implementation
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RestoreDatabaseAsync(string backupPath)
        {
            try
            {
                // Implementation would depend on your database restore strategy
                // This is a placeholder for the actual implementation
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ExportDataAsync(string exportPath, ExportFormat format)
        {
            try
            {
                // Implementation would depend on your export strategy
                // This is a placeholder for the actual implementation
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ImportDataAsync(string importPath, ImportFormat format)
        {
            try
            {
                // Implementation would depend on your import strategy
                // This is a placeholder for the actual implementation
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
} 