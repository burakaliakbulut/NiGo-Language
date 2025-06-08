using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WordLearningWpfApp.Models
{
    public class WordProgress
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("userId")]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("wordId")]
        public string WordId { get; set; } = string.Empty;

        [BsonElement("status")]
        public WordStatus Status { get; set; }

        [BsonElement("correctAnswers")]
        public int CorrectAnswers { get; set; }

        [BsonElement("incorrectAnswers")]
        public int IncorrectAnswers { get; set; }

        [BsonElement("lastAttempted")]
        public DateTime? LastAttempted { get; set; }

        [BsonElement("lastCorrect")]
        public DateTime? LastCorrect { get; set; }

        [BsonElement("nextReview")]
        public DateTime? NextReview { get; set; }

        [BsonElement("difficulty")]
        public DifficultyLevel Difficulty { get; set; }

        [BsonElement("category")]
        public string Category { get; set; } = string.Empty;

        [BsonElement("notes")]
        public string? Notes { get; set; }

        [BsonElement("successRate")]
        public double SuccessRate => CorrectAnswers + IncorrectAnswers == 0 ? 0 : (double)CorrectAnswers / (CorrectAnswers + IncorrectAnswers);

        [BsonElement("difficultyLevel")]
        public DifficultyLevel DifficultyLevel { get; set; }

        [BsonElement("tags")]
        public List<string> Tags { get; set; } = new List<string>();

        [BsonElement("lastReviewDate")]
        public DateTime? LastReviewDate { get; set; }

        [BsonElement("nextReviewDate")]
        public DateTime? NextReviewDate { get; set; }

        [BsonElement("reviewCount")]
        public int ReviewCount { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
} 