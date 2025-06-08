using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using WordLearningWpfApp.Data;
using WordLearningWpfApp.Models;

namespace WordLearningWpfApp.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IMongoCollection<Statistics> _statistics;
        private readonly IMongoCollection<ExamResult> _examResults;
        private readonly IMongoCollection<WordProgress> _wordProgress;
        private readonly IMongoCollection<DailyProgress> _dailyProgress;
        private readonly IMongoCollection<ExamStatistics> _examStatistics;

        public StatisticsService(MongoDbContext dbContext)
        {
            _statistics = dbContext.Statistics;
            _examResults = dbContext.GetCollection<ExamResult>(MongoDbContext.ExamResultsCollection);
            _wordProgress = dbContext.WordProgress;
            _dailyProgress = dbContext.DailyProgress;
            _examStatistics = dbContext.GetCollection<ExamStatistics>(MongoDbContext.ExamStatisticsCollection);
        }

        public async Task<Statistics> GetUserStatisticsAsync(string userId)
        {
            var filter = Builders<Statistics>.Filter.Eq(s => s.UserId, userId);
            return await _statistics.Find(filter).FirstOrDefaultAsync() ?? new Statistics { UserId = userId };
        }

        public async Task<List<ExamResult>> GetUserExamResultsAsync(string userId)
        {
            return await _examResults.Find(r => r.UserId == userId)
                .SortByDescending(r => r.ExamDate)
                .ToListAsync();
        }

        public async Task<List<DailyProgress>> GetDailyProgressAsync(string userId, int days = 7)
        {
            var startDate = DateTime.UtcNow.AddDays(-days);
            return await _dailyProgress.Find(p => p.UserId == userId && p.Date >= startDate)
                .SortBy(p => p.Date)
                .ToListAsync();
        }

        public async Task<List<WordProgress>> GetWordProgressAsync(string userId)
        {
            return await _wordProgress.Find(p => p.UserId == userId)
                .SortByDescending(p => p.LastAttemptDate)
                .ToListAsync();
        }

        public async Task<Statistics> GetUserStatisticsByDateRangeAsync(string userId, DateTime startDate, DateTime endDate)
        {
            var filter = Builders<Statistics>.Filter.And(
                Builders<Statistics>.Filter.Eq(s => s.UserId, userId),
                Builders<Statistics>.Filter.Gte(s => s.Date, startDate),
                Builders<Statistics>.Filter.Lte(s => s.Date, endDate)
            );

            var statistics = await _statistics.Find(filter).ToListAsync();
            return statistics.FirstOrDefault() ?? new Statistics { UserId = userId, Date = DateTime.Now };
        }

        public async Task<Dictionary<string, int>> GetCategoryStatisticsAsync(string userId)
        {
            var wordProgress = await _wordProgress.Find(p => p.UserId == userId).ToListAsync();
            return wordProgress.GroupBy(p => p.Category)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<DifficultyLevel, int>> GetDifficultyStatisticsAsync(string userId)
        {
            var wordProgress = await _wordProgress.Find(p => p.UserId == userId).ToListAsync();
            return wordProgress.GroupBy(p => p.DifficultyLevel)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<string, int>> GetTagStatisticsAsync(string userId)
        {
            var wordProgress = await _wordProgress.Find(p => p.UserId == userId).ToListAsync();
            var tagStats = new Dictionary<string, int>();
            foreach (var progress in wordProgress)
            {
                foreach (var tag in progress.Tags)
                {
                    if (!tagStats.ContainsKey(tag))
                        tagStats[tag] = 0;
                    tagStats[tag]++;
                }
            }
            return tagStats;
        }

        public async Task<int> GetLearningStreakAsync(string userId)
        {
            var dailyProgress = await _dailyProgress.Find(p => p.UserId == userId)
                .SortByDescending(p => p.Date)
                .ToListAsync();

            int streak = 0;
            var currentDate = DateTime.UtcNow.Date;

            foreach (var progress in dailyProgress)
            {
                if (progress.Date.Date == currentDate)
                {
                    streak++;
                    currentDate = currentDate.AddDays(-1);
                }
                else
                {
                    break;
                }
            }

            return streak;
        }

        public async Task<Dictionary<DateTime, int>> GetDailyActivityAsync(string userId, int days)
        {
            var startDate = DateTime.Now.AddDays(-days);
            var filter = Builders<Statistics>.Filter.And(
                Builders<Statistics>.Filter.Eq(s => s.UserId, userId),
                Builders<Statistics>.Filter.Gte(s => s.Date, startDate)
            );

            var statistics = await _statistics.Find(filter).ToListAsync();
            return statistics.ToDictionary(s => s.Date.Date, s => s.WordsLearned);
        }

        public async Task<Dictionary<string, double>> GetSuccessRatesByCategoryAsync(string userId)
        {
            var wordProgress = await _wordProgress.Find(p => p.UserId == userId).ToListAsync();
            return wordProgress.GroupBy(p => p.Category)
                .ToDictionary(
                    g => g.Key,
                    g => g.Average(p => p.SuccessRate)
                );
        }

        public async Task<Dictionary<string, int>> GetWordStatusDistributionAsync(string userId)
        {
            var filter = Builders<WordProgress>.Filter.Eq(wp => wp.UserId, userId);
            var progress = await _wordProgress.Find(filter).ToListAsync();

            return progress
                .GroupBy(wp => wp.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<Statistics> GetSystemStatisticsAsync()
        {
            var totalUsers = await _statistics.CountDocumentsAsync(_ => true);
            var totalWords = await _wordProgress.CountDocumentsAsync(_ => true);
            var totalExams = await _statistics.Aggregate()
                .Group(_ => 1, g => new { TotalExams = g.Sum(s => s.TotalExams) })
                .FirstOrDefaultAsync();

            return new Statistics
            {
                TotalUsers = (int)totalUsers,
                TotalWords = (int)totalWords,
                TotalExams = totalExams?.TotalExams ?? 0,
                CreatedAt = DateTime.UtcNow
            };
        }

        public async Task GetProgressDataAsync(string userId)
        {
            var filter = Builders<WordProgress>.Filter.Eq(wp => wp.UserId, userId);
            var progress = await _wordProgress.Find(filter).ToListAsync();
            // Process progress data as needed
        }

        public async Task<ExamStatistics> GetExamStatisticsAsync(string userId)
        {
            return await _examStatistics.Find(s => s.UserId == userId).FirstOrDefaultAsync();
        }

        public async Task<bool> AddExamResultAsync(string userId, ExamResult result)
        {
            // Dummy implementation
            return await Task.FromResult(true);
        }

        public async Task UpdateWordProgressAsync(WordProgress progress)
        {
            await _wordProgress.ReplaceOneAsync(
                p => p.UserId == progress.UserId && p.WordId == progress.WordId,
                progress,
                new ReplaceOptions { IsUpsert = true }
            );
        }

        public async Task AddDailyProgressAsync(DailyProgress progress)
        {
            await _dailyProgress.InsertOneAsync(progress);
        }

        public async Task UpdateExamStatisticsAsync(ExamStatistics statistics)
        {
            await _examStatistics.ReplaceOneAsync(
                s => s.UserId == statistics.UserId,
                statistics,
                new ReplaceOptions { IsUpsert = true }
            );
        }

        public async Task<List<WordProgress>> GetUserWordProgressAsync(string userId)
        {
            return await _wordProgress.Find(p => p.UserId == userId).ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetWordDifficultyDistributionAsync(string userId)
        {
            var progress = await _wordProgress.Find(p => p.UserId == userId).ToListAsync();
            return progress.GroupBy(p => p.DifficultyLevel)
                .ToDictionary(g => g.Key.ToString(), g => g.Count());
        }

        public async Task<Dictionary<string, double>> GetCategoryPerformanceAsync(string userId)
        {
            var results = await _examResults.Find(r => r.UserId == userId).ToListAsync();
            return results.GroupBy(r => r.Category)
                .ToDictionary(g => g.Key, g => g.Average(r => r.Score));
        }

        public async Task<bool> UpdateStatisticsAsync(string userId, Statistics statistics)
        {
            var filter = Builders<Statistics>.Filter.Eq(s => s.UserId, userId);
            var result = await _statistics.ReplaceOneAsync(filter, statistics, new ReplaceOptions { IsUpsert = true });
            return result.ModifiedCount > 0 || result.UpsertedId != null;
        }

        public async Task<List<WordStatistics>> GetWordStatisticsAsync()
        {
            // Dummy implementation, replace with actual logic as needed
            return await Task.FromResult(new List<WordStatistics>());
        }
    }
} 