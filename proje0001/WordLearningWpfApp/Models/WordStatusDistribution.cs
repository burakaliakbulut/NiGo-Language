using System;
using System.Collections.Generic;

namespace WordLearningWpfApp.Models
{
    public class WordStatusDistribution
    {
        public int TotalWords { get; set; }
        public int LearnedWords { get; set; }
        public int LearningWords { get; set; }
        public int DifficultWords { get; set; }
        public Dictionary<string, int> StatusCounts { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        public WordStatusDistribution()
        {
            StatusCounts = new Dictionary<string, int>();
        }

        public void UpdateDistribution(int total, int learned, int learning, int difficult)
        {
            TotalWords = total;
            LearnedWords = learned;
            LearningWords = learning;
            DifficultWords = difficult;
            LastUpdated = DateTime.UtcNow;

            StatusCounts["Learned"] = learned;
            StatusCounts["Learning"] = learning;
            StatusCounts["Difficult"] = difficult;
        }

        public void AddWord(string status)
        {
            TotalWords++;
            if (StatusCounts.ContainsKey(status))
            {
                StatusCounts[status]++;
            }
            else
            {
                StatusCounts[status] = 1;
            }

            switch (status)
            {
                case "Learned":
                    LearnedWords++;
                    break;
                case "Learning":
                    LearningWords++;
                    break;
                case "Difficult":
                    DifficultWords++;
                    break;
            }

            LastUpdated = DateTime.UtcNow;
        }

        public void RemoveWord(string status)
        {
            if (TotalWords > 0)
            {
                TotalWords--;
                if (StatusCounts.ContainsKey(status) && StatusCounts[status] > 0)
                {
                    StatusCounts[status]--;
                }

                switch (status)
                {
                    case "Learned":
                        if (LearnedWords > 0) LearnedWords--;
                        break;
                    case "Learning":
                        if (LearningWords > 0) LearningWords--;
                        break;
                    case "Difficult":
                        if (DifficultWords > 0) DifficultWords--;
                        break;
                }

                LastUpdated = DateTime.UtcNow;
            }
        }
    }
} 