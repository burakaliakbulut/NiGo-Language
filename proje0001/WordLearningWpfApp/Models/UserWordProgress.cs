using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace WordLearningWpfApp.Models
{
    public class UserWordProgress
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [Required]
        [BsonElement("userId")]
        public string UserId { get; set; }

        [Required]
        [BsonElement("wordId")]
        public string WordId { get; set; }

        [Range(0, int.MaxValue)]
        [BsonElement("successCount")]
        public int SuccessCount { get; set; }

        [Range(0, int.MaxValue)]
        [BsonElement("incorrectCount")]
        public int IncorrectCount { get; set; }

        [BsonElement("lastAttemptAt")]
        public DateTime LastAttemptAt { get; set; }

        [BsonElement("nextReview")]
        public DateTime NextReview { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [BsonElement("isLearned")]
        public bool IsLearned { get; set; }

        [BsonElement("learningStage")]
        public LearningStage LearningStage { get; set; }

        [BsonElement("lastReviewScore")]
        public int LastReviewScore { get; set; }

        [BsonElement("learnedWords")]
        public List<string> LearnedWords { get; set; } = new List<string>();

        [BsonElement("learningWords")]
        public List<string> LearningWords { get; set; } = new List<string>();

        [BsonIgnore]
        public double SuccessRate => CalculateSuccessRate();

        [BsonIgnore]
        public int TotalAttempts => SuccessCount + IncorrectCount;

        [BsonIgnore]
        public bool NeedsReview => DateTime.UtcNow >= NextReview;

        [BsonIgnore]
        public bool Learned => IsLearned;

        public UserWordProgress()
        {
            SuccessCount = 0;
            IncorrectCount = 0;
            LastAttemptAt = DateTime.UtcNow;
            NextReview = DateTime.UtcNow.AddDays(1);
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            IsLearned = false;
            LearningStage = LearningStage.New;
            LastReviewScore = 0;
        }

        public UserWordProgress(string userId, string wordId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be empty", nameof(userId));
            if (string.IsNullOrWhiteSpace(wordId))
                throw new ArgumentException("Word ID cannot be empty", nameof(wordId));

            UserId = userId;
            WordId = wordId;
            SuccessCount = 0;
            IncorrectCount = 0;
            LastAttemptAt = DateTime.UtcNow;
            NextReview = DateTime.UtcNow.AddDays(1);
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            IsLearned = false;
            LearningStage = LearningStage.New;
            LastReviewScore = 0;
        }

        private double CalculateSuccessRate()
        {
            return TotalAttempts > 0 ? (double)SuccessCount / TotalAttempts * 100 : 0;
        }

        public void UpdateProgress(bool isCorrect, int score = 0)
        {
            if (isCorrect)
            {
                SuccessCount++;
                LastReviewScore = score;
            }
            else
            {
                IncorrectCount++;
                LastReviewScore = 0;
            }

            UpdateLearningStage();
            LastAttemptAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            UpdateNextReview();
        }

        private void UpdateLearningStage()
        {
            LearningStage = SuccessCount switch
            {
                0 => LearningStage.New,
                1 => LearningStage.Learning,
                2 => LearningStage.Practicing,
                3 => LearningStage.Mastering,
                _ => LearningStage.Mastered
            };

            IsLearned = LearningStage == LearningStage.Mastered;
        }

        public void UpdateNextReview()
        {
            var days = LearningStage switch
            {
                LearningStage.New => 1,
                LearningStage.Learning => 2,
                LearningStage.Practicing => 4,
                LearningStage.Mastering => 7,
                LearningStage.Mastered => 14,
                _ => 1
            };

            NextReview = DateTime.UtcNow.AddDays(days);
            UpdatedAt = DateTime.UtcNow;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(UserId))
                throw new ValidationException("User ID is required");
            if (string.IsNullOrWhiteSpace(WordId))
                throw new ValidationException("Word ID is required");
            if (SuccessCount < 0)
                throw new ValidationException("Success count cannot be negative");
            if (IncorrectCount < 0)
                throw new ValidationException("Incorrect count cannot be negative");
            if (LastReviewScore < 0 || LastReviewScore > 100)
                throw new ValidationException("Last review score must be between 0 and 100");
        }
    }
} 