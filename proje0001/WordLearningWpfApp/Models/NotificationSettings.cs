using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WordLearningWpfApp.Models
{
    public class NotificationSettings
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("userId")]
        public string UserId { get; set; }

        [BsonElement("emailEnabled")]
        public bool EmailEnabled { get; set; }

        [BsonElement("pushEnabled")]
        public bool PushEnabled { get; set; }

        [BsonElement("dailyReminderEnabled")]
        public bool DailyReminderEnabled { get; set; }

        [BsonElement("weeklyReportEnabled")]
        public bool WeeklyReportEnabled { get; set; }
    }
} 