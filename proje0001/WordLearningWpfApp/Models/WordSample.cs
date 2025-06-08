using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WordLearningWpfApp.Models
{
    public class WordSample
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("wordId")]
        [BsonRequired]
        public string WordId { get; set; }

        [BsonElement("sentence")]
        [BsonRequired]
        public string Sentence { get; set; }

        [BsonElement("translation")]
        [BsonRequired]
        public string Translation { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        public WordSample()
        {
            Id = ObjectId.GenerateNewId().ToString();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            IsActive = true;
        }

        public WordSample(string wordId, string sentence, string translation)
        {
            Id = ObjectId.GenerateNewId().ToString();
            WordId = wordId;
            Sentence = sentence;
            Translation = translation;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            IsActive = true;
        }

        public void Update(string sentence, string translation)
        {
            Sentence = sentence;
            Translation = translation;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }
} 