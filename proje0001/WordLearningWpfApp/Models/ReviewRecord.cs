using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WordLearningWpfApp.Models
{
    public class ReviewRecord
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("wordId")]
        public string WordId { get; set; }

        [BsonElement("userId")]
        public string UserId { get; set; }

        [BsonElement("reviewDate")]
        public DateTime ReviewDate { get; set; }

        [BsonElement("isCorrect")]
        public bool IsCorrect { get; set; }

        [BsonElement("responseTime")]
        public TimeSpan ResponseTime { get; set; }

        [BsonElement("reviewType")]
        public ReviewType ReviewType { get; set; }

        [BsonElement("notes")]
        public string Notes { get; set; }

        public ReviewRecord()
        {
            Id = ObjectId.GenerateNewId().ToString();
            ReviewDate = DateTime.UtcNow;
        }
    }

    public enum ReviewType
    {
        MultipleChoice,
        Typing,
        Listening,
        Speaking,
        Flashcard
    }
} 