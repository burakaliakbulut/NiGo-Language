using System;

namespace WordLearningWpfApp.Models
{
    public class CategorySuccessRate
    {
        public string Category { get; set; } = string.Empty;
        public int TotalWords { get; set; }
        public int LearnedWords { get; set; }
        public double SuccessRate => TotalWords > 0 ? (double)LearnedWords / TotalWords : 0;

        public CategorySuccessRate()
        {
        }

        public CategorySuccessRate(string category)
        {
            Category = category;
        }

        public void UpdateProgress(int totalWords, int learnedWords)
        {
            TotalWords = totalWords;
            LearnedWords = learnedWords;
        }

        public void AddWord(bool isLearned)
        {
            TotalWords++;
            if (isLearned)
            {
                LearnedWords++;
            }
        }
    }
} 