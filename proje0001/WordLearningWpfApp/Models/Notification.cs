using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace WordLearningWpfApp.Models
{
    public class Notification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("userId")]
        public string UserId { get; set; }

        [BsonElement("message")]
        public string Message { get; set; }

        [BsonElement("isRead")]
        public bool IsRead { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        public Notification()
        {
            CreatedAt = DateTime.UtcNow;
            IsRead = false;
        }
    }
}

