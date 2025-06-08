using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WordLearningWpfApp.Models;

namespace WordLearningWpfApp.Services
{
    public interface IStatisticsService
    {
        Task<Statistics> GetUserStatisticsAsync(string userId);
        Task<List<DailyProgress>> GetDailyProgressAsync(string userId, int days);
        Task<List<WordProgress>> GetWordProgressAsync(string userId);
        Task<ExamStatistics> GetExamStatisticsAsync(string userId);
        Task<bool> UpdateStatisticsAsync(string userId, Statistics statistics);
        Task<Statistics> GetUserStatisticsByDateRangeAsync(string userId, DateTime startDate, DateTime endDate);
        Task<Dictionary<string, int>> GetCategoryStatisticsAsync(string userId);
        Task<Dictionary<DifficultyLevel, int>> GetDifficultyStatisticsAsync(string userId);
        Task<Dictionary<string, int>> GetTagStatisticsAsync(string userId);
        Task<int> GetLearningStreakAsync(string userId);
        Task<Dictionary<DateTime, int>> GetDailyActivityAsync(string userId, int days);
        Task<Dictionary<string, double>> GetSuccessRatesByCategoryAsync(string userId);
        Task<Dictionary<string, int>> GetWordStatusDistributionAsync(string userId);
        Task<Statistics> GetSystemStatisticsAsync();
        Task GetProgressDataAsync(string userId);
        Task<List<WordStatistics>> GetWordStatisticsAsync();
        Task<bool> AddExamResultAsync(string userId, ExamResult result);
    }
} 