using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WordLearningWpfApp.Models
{
    public class WordDifficultyLevel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        [BsonRequired]
        public string Name { get; set; }

        [BsonElement("reviewInterval")]
        public int ReviewInterval { get; set; }

        [BsonElement("description")]
        [BsonRequired]
        public string Description { get; set; }

        [BsonElement("level")]
        public int Level { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        public WordDifficultyLevel()
        {
            Id = ObjectId.GenerateNewId().ToString();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            IsActive = true;
        }

        public WordDifficultyLevel(string name, string description, int level)
        {
            Id = ObjectId.GenerateNewId().ToString();
            Name = name;
            Description = description;
            Level = level;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            IsActive = true;
        }

        public void Update(string name, string description, int level)
        {
            Name = name;
            Description = description;
            Level = level;
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

        public static WordDifficultyLevel[] GetDefaultLevels()
        {
            return new[]
            {
                new WordDifficultyLevel
                {
                    Level = 1,
                    Name = "Kolay",
                    Description = "Temel kelimeler",
                    ReviewInterval = 7
                },
                new WordDifficultyLevel
                {
                    Level = 2,
                    Name = "Orta",
                    Description = "Orta seviye kelimeler",
                    ReviewInterval = 5
                },
                new WordDifficultyLevel
                {
                    Level = 3,
                    Name = "Zor",
                    Description = "İleri seviye kelimeler",
                    ReviewInterval = 3
                }
            };
        }
    }
} 