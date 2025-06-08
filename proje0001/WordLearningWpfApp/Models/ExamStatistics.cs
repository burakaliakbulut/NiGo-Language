using System;

namespace WordLearningWpfApp.Models
{
    public class ExamStatistics
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public int TotalExams { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public double AverageScore => TotalQuestions > 0 ? (double)CorrectAnswers / TotalQuestions * 100 : 0;
        public TimeSpan AverageDuration { get; set; }
        public DateTime LastExamDate { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
    }
} 