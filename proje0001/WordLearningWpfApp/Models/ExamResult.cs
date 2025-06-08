using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WordLearningWpfApp.Models
{
    public class ExamResult
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("userId")]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("correctAnswers")]
        public int CorrectAnswers { get; set; }

        [BsonElement("totalQuestions")]
        public int TotalQuestions { get; set; }

        [BsonElement("score")]
        public double Score { get; set; }

        [BsonElement("completedAt")]
        public DateTime? CompletedAt { get; set; }

        [BsonElement("duration")]
        public TimeSpan Duration { get; set; }

        [BsonElement("incorrectAnswers")]
        public int IncorrectAnswers { get; set; }

        [BsonElement("questionResults")]
        public Dictionary<int, bool> QuestionResults { get; set; } = new Dictionary<int, bool>();

        [BsonElement("studyTime")]
        public int StudyTime { get; set; }

        [BsonElement("correctWordIds")]
        public List<string> CorrectWordIds { get; set; } = new List<string>();

        [BsonElement("incorrectWordIds")]
        public List<string> IncorrectWordIds { get; set; } = new List<string>();

        [BsonElement("status")]
        public ExamStatus Status { get; set; }

        [BsonElement("startTime")]
        public DateTime StartTime { get; set; }

        [BsonElement("endTime")]
        public DateTime EndTime { get; set; }

        [BsonElement("category")]
        public string Category { get; set; } = string.Empty;

        [BsonElement("questions")]
        public List<Question> Questions { get; set; } = new List<Question>();

        [BsonElement("examDate")]
        public DateTime ExamDate { get; set; }

        [BsonElement("examId")]
        public string ExamId { get; set; } = string.Empty;

        [BsonElement("answers")]
        public List<string> Answers { get; set; } = new List<string>();

        [BsonElement("startedAt")]
        public DateTime StartedAt { get; set; }

        [BsonElement("difficulty")]
        public DifficultyLevel Difficulty { get; set; }
    }
} 