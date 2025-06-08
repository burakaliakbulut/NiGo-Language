using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WordLearningWpfApp.Models
{
    public class Statistics
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("userId")]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("totalWords")]
        public int TotalWords { get; set; }

        [BsonElement("learnedWords")]
        public int LearnedWords { get; set; }

        [BsonElement("learningWords")]
        public int LearningWords { get; set; }

        [BsonElement("difficultWords")]
        public int DifficultWords { get; set; }

        [BsonElement("totalExams")]
        public int TotalExams { get; set; }

        [BsonElement("passedExams")]
        public int PassedExams { get; set; }

        [BsonElement("failedExams")]
        public int FailedExams { get; set; }

        [BsonElement("averageScore")]
        public double AverageScore { get; set; }

        [BsonElement("currentStreak")]
        public int CurrentStreak { get; set; }

        [BsonElement("longestStreak")]
        public int LongestStreak { get; set; }

        [BsonElement("lastStudyDate")]
        public DateTime LastStudyDate { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("categoryProgress")]
        public Dictionary<string, double> CategoryProgress { get; set; } = new();

        [BsonElement("difficultyProgress")]
        public Dictionary<string, double> DifficultyProgress { get; set; } = new();

        [BsonElement("tagProgress")]
        public Dictionary<string, double> TagProgress { get; set; } = new();

        [BsonElement("dailyActivity")]
        public Dictionary<DateTime, int> DailyActivity { get; set; } = new();

        [BsonElement("wordStatusDistribution")]
        public Dictionary<string, int> WordStatusDistribution { get; set; } = new();

        [BsonElement("successRatesByCategory")]
        public Dictionary<string, double> SuccessRatesByCategory { get; set; } = new();

        [BsonElement("totalStudyTime")]
        public TimeSpan TotalStudyTime { get; set; }

        [BsonElement("categoryStats")]
        public Dictionary<string, int> CategoryStats { get; set; } = new();

        [BsonElement("date")]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [BsonElement("totalCorrectAnswers")]
        public int TotalCorrectAnswers { get; set; }

        [BsonElement("totalQuestions")]
        public int TotalQuestions { get; set; }

        [BsonElement("totalUsers")]
        public int TotalUsers { get; set; }

        [BsonElement("activeUsers")]
        public int ActiveUsers { get; set; }

        [BsonElement("wordsLearned")]
        public int WordsLearned { get; set; }

        public Statistics()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            LastStudyDate = DateTime.UtcNow;
            CategoryProgress = new Dictionary<string, double>();
            DifficultyProgress = new Dictionary<string, double>();
            TagProgress = new Dictionary<string, double>();
            DailyActivity = new Dictionary<DateTime, int>();
            WordStatusDistribution = new Dictionary<string, int>();
            SuccessRatesByCategory = new Dictionary<string, double>();
            TotalStudyTime = TimeSpan.Zero;
            CategoryStats = new Dictionary<string, int>();
        }

        public void UpdateProgress(int totalWords, int learnedWords, int learningWords, int difficultWords)
        {
            TotalWords = totalWords;
            LearnedWords = learnedWords;
            LearningWords = learningWords;
            DifficultWords = difficultWords;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateExamResults(int totalExams, int passedExams, int failedExams, double averageScore)
        {
            TotalExams = totalExams;
            PassedExams = passedExams;
            FailedExams = failedExams;
            AverageScore = averageScore;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateStreak(int currentStreak, int longestStreak)
        {
            CurrentStreak = currentStreak;
            LongestStreak = longestStreak;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateLastStudyDate()
        {
            LastStudyDate = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public class DailyStatistics
    {
        [BsonElement("correctAnswers")]
        public int CorrectAnswers { get; set; }

        [BsonElement("incorrectAnswers")]
        public int IncorrectAnswers { get; set; }

        [BsonElement("studyTime")]
        public int StudyTime { get; set; } // in minutes

        [BsonElement("examCount")]
        public int ExamCount { get; set; }

        [BsonElement("averageScore")]
        public double AverageScore { get; set; }

        [BsonElement("wordsLearned")]
        public int WordsLearned { get; set; }

        [BsonElement("wordsReviewed")]
        public int WordsReviewed { get; set; }

        [BsonElement("streak")]
        public int Streak { get; set; }

        public DailyStatistics()
        {
            CorrectAnswers = 0;
            IncorrectAnswers = 0;
            StudyTime = 0;
            ExamCount = 0;
            AverageScore = 0;
            WordsLearned = 0;
            WordsReviewed = 0;
            Streak = 0;
        }
    }

    public class WeeklyStatistics
    {
        [BsonElement("totalExams")]
        public int TotalExams { get; set; }

        [BsonElement("totalStudyTime")]
        public int TotalStudyTime { get; set; } // in minutes

        [BsonElement("averageScore")]
        public double AverageScore { get; set; }

        [BsonElement("wordsLearned")]
        public int WordsLearned { get; set; }

        [BsonElement("wordsReviewed")]
        public int WordsReviewed { get; set; }

        [BsonElement("streak")]
        public int Streak { get; set; }

        [BsonElement("mostActiveDay")]
        public DateTime MostActiveDay { get; set; }

        [BsonElement("mostActiveTime")]
        public int MostActiveTime { get; set; } // hour of day (0-23)

        public WeeklyStatistics()
        {
            TotalExams = 0;
            TotalStudyTime = 0;
            AverageScore = 0;
            WordsLearned = 0;
            WordsReviewed = 0;
            Streak = 0;
            MostActiveDay = DateTime.UtcNow;
            MostActiveTime = 0;
        }
    }

    public class MonthlyStatistics
    {
        [BsonElement("totalExams")]
        public int TotalExams { get; set; }

        [BsonElement("totalStudyTime")]
        public int TotalStudyTime { get; set; } // in minutes

        [BsonElement("averageScore")]
        public double AverageScore { get; set; }

        [BsonElement("wordsLearned")]
        public int WordsLearned { get; set; }

        [BsonElement("wordsReviewed")]
        public int WordsReviewed { get; set; }

        [BsonElement("streak")]
        public int Streak { get; set; }

        [BsonElement("mostActiveDay")]
        public DateTime MostActiveDay { get; set; }

        [BsonElement("mostActiveTime")]
        public int MostActiveTime { get; set; } // hour of day (0-23)

        [BsonElement("categoryProgress")]
        public Dictionary<string, double> CategoryProgress { get; set; }

        [BsonElement("difficultyProgress")]
        public Dictionary<string, double> DifficultyProgress { get; set; }

        public MonthlyStatistics()
        {
            TotalExams = 0;
            TotalStudyTime = 0;
            AverageScore = 0;
            WordsLearned = 0;
            WordsReviewed = 0;
            Streak = 0;
            MostActiveDay = DateTime.UtcNow;
            MostActiveTime = 0;
            CategoryProgress = new Dictionary<string, double>();
            DifficultyProgress = new Dictionary<string, double>();
        }
    }

    public class OverallStatistics
    {
        [BsonElement("totalExams")]
        public int TotalExams { get; set; }

        [BsonElement("totalStudyTime")]
        public int TotalStudyTime { get; set; } // in minutes

        [BsonElement("averageScore")]
        public double AverageScore { get; set; }

        [BsonElement("learnedWords")]
        public int LearnedWords { get; set; }

        [BsonElement("totalWords")]
        public int TotalWords { get; set; }

        [BsonElement("currentStreak")]
        public int CurrentStreak { get; set; }

        [BsonElement("longestStreak")]
        public int LongestStreak { get; set; }

        [BsonElement("categoryProgress")]
        public Dictionary<string, double> CategoryProgress { get; set; }

        [BsonElement("difficultyProgress")]
        public Dictionary<string, double> DifficultyProgress { get; set; }

        [BsonElement("tagProgress")]
        public Dictionary<string, double> TagProgress { get; set; }

        [BsonElement("lastStudyDate")]
        public DateTime LastStudyDate { get; set; }

        [BsonElement("mostActiveDay")]
        public DateTime MostActiveDay { get; set; }

        [BsonElement("mostActiveTime")]
        public int MostActiveTime { get; set; } // hour of day (0-23)

        public OverallStatistics()
        {
            TotalExams = 0;
            TotalStudyTime = 0;
            AverageScore = 0;
            LearnedWords = 0;
            TotalWords = 0;
            CurrentStreak = 0;
            LongestStreak = 0;
            CategoryProgress = new Dictionary<string, double>();
            DifficultyProgress = new Dictionary<string, double>();
            TagProgress = new Dictionary<string, double>();
            LastStudyDate = DateTime.UtcNow;
            MostActiveDay = DateTime.UtcNow;
            MostActiveTime = 0;
        }
    }
} 