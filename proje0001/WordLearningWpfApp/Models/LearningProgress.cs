using System;
using MongoDB.Bson.Serialization.Attributes;

namespace WordLearningWpfApp.Models
{
    public class LearningProgress
    {
        [BsonElement("currentLevel")]
        public int CurrentLevel { get; set; }

        [BsonElement("totalReviews")]
        public int TotalReviews { get; set; }

        [BsonElement("consecutiveCorrect")]
        public int ConsecutiveCorrect { get; set; }

        [BsonElement("consecutiveIncorrect")]
        public int ConsecutiveIncorrect { get; set; }

        [BsonElement("lastReviewDate")]
        public DateTime? LastReviewDate { get; set; }

        [BsonElement("nextReviewDate")]
        public DateTime? NextReviewDate { get; set; }

        [BsonElement("masteryScore")]
        public double MasteryScore { get; set; }

        [BsonElement("learningStage")]
        public LearningStage LearningStage { get; set; }

        [BsonElement("timeSpent")]
        public TimeSpan TimeSpent { get; set; }

        public LearningProgress()
        {
            CurrentLevel = 1;
            TotalReviews = 0;
            ConsecutiveCorrect = 0;
            ConsecutiveIncorrect = 0;
            MasteryScore = 0.0;
            LearningStage = LearningStage.New;
            TimeSpent = TimeSpan.Zero;
        }

        public void UpdateProgress(bool isCorrect, TimeSpan responseTime)
        {
            TotalReviews++;
            TimeSpent += responseTime;
            LastReviewDate = DateTime.UtcNow;

            if (isCorrect)
            {
                ConsecutiveCorrect++;
                ConsecutiveIncorrect = 0;
                MasteryScore = Math.Min(1.0, MasteryScore + 0.1);
            }
            else
            {
                ConsecutiveIncorrect++;
                ConsecutiveCorrect = 0;
                MasteryScore = Math.Max(0.0, MasteryScore - 0.1);
            }

            UpdateLearningStage();
            CalculateNextReviewDate();
        }

        private void UpdateLearningStage()
        {
            LearningStage = ConsecutiveCorrect switch
            {
                0 => LearningStage.New,
                1 => LearningStage.Learning,
                2 => LearningStage.Practicing,
                3 => LearningStage.Mastering,
                _ => LearningStage.Mastered
            };

            if (ConsecutiveIncorrect >= 3)
            {
                LearningStage = LearningStage.Difficult;
            }
        }

        private void CalculateNextReviewDate()
        {
            if (LastReviewDate == null) return;

            var interval = LearningStage switch
            {
                LearningStage.Mastered => TimeSpan.FromDays(14),
                LearningStage.Mastering => TimeSpan.FromDays(7),
                LearningStage.Practicing => TimeSpan.FromDays(4),
                LearningStage.Learning => TimeSpan.FromDays(2),
                LearningStage.Difficult => TimeSpan.FromDays(1),
                _ => TimeSpan.FromDays(1)
            };

            NextReviewDate = LastReviewDate.Value.Add(interval);
        }
    }
} 