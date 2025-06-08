using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WordLearningWpfApp.Models
{
    public class Question
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("wordId")]
        public string WordId { get; set; } = string.Empty;

        [BsonElement("english")]
        public string English { get; set; } = string.Empty;

        [BsonElement("correctAnswer")]
        public string CorrectAnswer { get; set; } = string.Empty;

        [BsonElement("userAnswer")]
        public string? UserAnswer { get; set; }

        [BsonElement("isCorrect")]
        public bool IsCorrect { get; set; }

        [BsonElement("options")]
        public List<string> Options { get; set; } = new List<string>();

        [BsonElement("answeredAt")]
        public DateTime? AnsweredAt { get; set; }

        [BsonElement("examId")]
        public string ExamId { get; set; } = string.Empty;

        [BsonElement("questionNumber")]
        public int QuestionNumber { get; set; }

        [BsonElement("difficulty")]
        public DifficultyLevel Difficulty { get; set; }

        [BsonElement("category")]
        public string Category { get; set; } = string.Empty;

        [BsonElement("isAnswered")]
        public bool IsAnswered { get; set; }

        [BsonElement("selectedAnswer")]
        public string? SelectedAnswer { get; set; }
    }
} 