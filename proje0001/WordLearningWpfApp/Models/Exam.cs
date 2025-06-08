using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WordLearningWpfApp.Models
{
    public class Exam
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("userId")]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("type")]
        public ExamType Type { get; set; }

        [BsonElement("status")]
        public ExamStatus Status { get; set; }

        [BsonElement("difficulty")]
        public DifficultyLevel Difficulty { get; set; }

        [BsonElement("category")]
        public string Category { get; set; } = string.Empty;

        [BsonElement("startedAt")]
        public DateTime StartedAt { get; set; }

        [BsonElement("completedAt")]
        public DateTime? CompletedAt { get; set; }

        [BsonElement("duration")]
        public TimeSpan Duration { get; set; }

        [BsonElement("totalQuestions")]
        public int TotalQuestions { get; set; }

        [BsonElement("score")]
        public double Score { get; set; }

        [BsonElement("notes")]
        public string? Notes { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        public List<Question> Questions { get; set; } = new List<Question>();
        public bool IsCompleted => CompletedAt.HasValue;
    }

    public enum ExamType
    {
        Practice,
        Quiz,
        Test,
        Review,
        Daily
    }

    public enum ExamStatus
    {
        NotStarted,
        InProgress,
        Completed,
        Cancelled
    }
} 