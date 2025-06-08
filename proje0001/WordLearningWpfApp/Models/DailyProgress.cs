using System;

namespace WordLearningWpfApp.Models
{
    public class DailyProgress
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int WordsLearned { get; set; }
        public int WordsReviewed { get; set; }
        public int ExamsTaken { get; set; }
        public double AverageScore { get; set; }
        public TimeSpan StudyTime { get; set; }
    }
} 