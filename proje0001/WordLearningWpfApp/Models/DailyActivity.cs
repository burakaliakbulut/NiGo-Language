using System;

namespace WordLearningWpfApp.Models
{
    public class DailyActivity
    {
        public DateTime Date { get; set; }
        public int WordsLearned { get; set; }
        public int CorrectAnswers { get; set; }
        public int WrongAnswers { get; set; }

        public DailyActivity()
        {
            Date = DateTime.UtcNow;
            WordsLearned = 0;
            CorrectAnswers = 0;
            WrongAnswers = 0;
        }

        public DailyActivity(DateTime date, int wordsLearned, int correctAnswers, int wrongAnswers)
        {
            Date = date;
            WordsLearned = wordsLearned;
            CorrectAnswers = correctAnswers;
            WrongAnswers = wrongAnswers;
        }
    }
} 