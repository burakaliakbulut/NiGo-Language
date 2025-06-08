using System;
using System.Collections.Generic;

namespace WordLearningWpfApp.Models
{
    public class SystemStatistics
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalWords { get; set; }
        public int TotalExams { get; set; }
        public double AverageUserScore { get; set; }
        public Dictionary<string, int> CategoryDistribution { get; set; } = new();
        public Dictionary<string, int> DifficultyDistribution { get; set; } = new();
        public Dictionary<DateTime, int> DailyActiveUsers { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        public SystemStatistics()
        {
            CategoryDistribution = new Dictionary<string, int>();
            DifficultyDistribution = new Dictionary<string, int>();
            DailyActiveUsers = new Dictionary<DateTime, int>();
        }

        public void UpdateUserStats(int totalUsers, int activeUsers)
        {
            TotalUsers = totalUsers;
            ActiveUsers = activeUsers;
            LastUpdated = DateTime.UtcNow;
        }

        public void UpdateWordStats(int totalWords)
        {
            TotalWords = totalWords;
            LastUpdated = DateTime.UtcNow;
        }

        public void UpdateExamStats(int totalExams, double averageScore)
        {
            TotalExams = totalExams;
            AverageUserScore = averageScore;
            LastUpdated = DateTime.UtcNow;
        }

        public void UpdateCategoryDistribution(Dictionary<string, int> distribution)
        {
            CategoryDistribution = distribution;
            LastUpdated = DateTime.UtcNow;
        }

        public void UpdateDifficultyDistribution(Dictionary<string, int> distribution)
        {
            DifficultyDistribution = distribution;
            LastUpdated = DateTime.UtcNow;
        }

        public void UpdateDailyActiveUsers(Dictionary<DateTime, int> dailyUsers)
        {
            DailyActiveUsers = dailyUsers;
            LastUpdated = DateTime.UtcNow;
        }
    }
} 