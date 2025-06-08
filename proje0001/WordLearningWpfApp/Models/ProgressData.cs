using System;

namespace WordLearningWpfApp.Models
{
    public class ProgressData
    {
        public DateTime Date { get; set; }
        public int LearnedWords { get; set; }
        public int TotalWords { get; set; }
        public double SuccessRate { get; set; }
        public TimeSpan StudyTime { get; set; }
    }
} 