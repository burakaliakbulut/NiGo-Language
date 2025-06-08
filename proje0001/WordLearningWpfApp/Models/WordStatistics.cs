using System;

namespace WordLearningWpfApp.Models
{
    public class WordStatistics
    {
        public int Id { get; set; }
        public int WordId { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalAttempts { get; set; }
        public double SuccessRate { get; set; }
        public DateTime LastReviewed { get; set; }
        public TimeSpan AverageResponseTime { get; set; }
    }
} 