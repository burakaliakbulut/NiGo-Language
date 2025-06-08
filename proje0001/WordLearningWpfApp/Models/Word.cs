using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WordLearningWpfApp.Models
{
    public class Word
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("text")]
        public string Text { get; set; } = string.Empty;

        [BsonElement("translation")]
        public string Translation { get; set; } = string.Empty;

        [BsonElement("language")]
        public string Language { get; set; } = "en";

        [BsonElement("examples")]
        public List<string> Examples { get; set; } = new();

        [BsonElement("synonyms")]
        public List<string> Synonyms { get; set; } = new();

        [BsonElement("antonyms")]
        public List<string> Antonyms { get; set; } = new();

        [BsonElement("difficulty")]
        public int Difficulty { get; set; } = 1;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("lastReviewed")]
        public DateTime? LastReviewed { get; set; }

        [BsonElement("reviewCount")]
        public int ReviewCount { get; set; }

        [BsonElement("successRate")]
        public double SuccessRate { get; set; }

        [BsonElement("userId")]
        public string UserId { get; set; }

        [BsonElement("category")]
        public string Category { get; set; }

        [BsonElement("difficultyLevel")]
        public int DifficultyLevel { get; set; }

        [BsonElement("examples")]
        public List<WordSample> ExamplesList { get; set; } = new List<WordSample>();

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [BsonElement("lastReviewedAt")]
        public DateTime? LastReviewedAt { get; set; }

        [BsonElement("successCount")]
        public int SuccessCount { get; set; }

        [BsonElement("failureCount")]
        public int FailureCount { get; set; }

        [BsonElement("isLearned")]
        public bool IsLearned { get; set; }

        [BsonElement("isFavorite")]
        public bool IsFavorite { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("notes")]
        public string Notes { get; set; }

        [BsonElement("pronunciation")]
        public string Pronunciation { get; set; }

        [BsonElement("partOfSpeech")]
        public string PartOfSpeech { get; set; }

        [BsonElement("etymology")]
        public string Etymology { get; set; }

        [BsonElement("usageNotes")]
        public string UsageNotes { get; set; }

        [BsonElement("relatedWords")]
        public List<string> RelatedWords { get; set; } = new List<string>();

        [BsonElement("mnemonics")]
        public string Mnemonics { get; set; }

        [BsonElement("imageUrl")]
        public string ImageUrl { get; set; }

        [BsonElement("audioUrl")]
        public string AudioUrl { get; set; }

        [BsonElement("context")]
        public string Context { get; set; }

        [BsonElement("difficultyScore")]
        public double DifficultyScore { get; set; }

        [BsonElement("masteryLevel")]
        public int MasteryLevel { get; set; }

        [BsonElement("nextReviewDate")]
        public DateTime? NextReviewDate { get; set; }

        [BsonElement("reviewHistory")]
        public List<ReviewRecord> ReviewHistory { get; set; } = new List<ReviewRecord>();

        [BsonElement("learningProgress")]
        public LearningProgress LearningProgress { get; set; }

        public Word()
        {
            Id = ObjectId.GenerateNewId().ToString();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            Difficulty = 1;
            DifficultyLevel = 1;
            LearningProgress = new LearningProgress();
        }

        public void UpdateDifficulty()
        {
            if (ReviewCount == 0) return;

            double successRate = (double)SuccessCount / ReviewCount;
            Difficulty = successRate switch
            {
                >= 0.8 => 2,
                >= 0.6 => 3,
                _ => 4
            };
        }

        public void UpdateMasteryLevel()
        {
            if (ReviewCount == 0) return;

            double successRate = (double)SuccessCount / ReviewCount;
            MasteryLevel = successRate switch
            {
                >= 0.9 => 5,
                >= 0.8 => 4,
                >= 0.7 => 3,
                >= 0.6 => 2,
                _ => 1
            };
        }

        public void AddSample(string sentence, string translation)
        {
            var sample = new WordSample
            {
                Id = ObjectId.GenerateNewId().ToString(),
                WordId = Id,
                Sentence = sentence,
                Translation = translation,
                CreatedAt = DateTime.UtcNow
            };
            ExamplesList.Add(sample);
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddSynonym(string synonym)
        {
            if (!Synonyms.Contains(synonym))
            {
                Synonyms.Add(synonym);
                UpdatedAt = DateTime.UtcNow;
            }
        }

        public void AddAntonym(string antonym)
        {
            if (!Antonyms.Contains(antonym))
            {
                Antonyms.Add(antonym);
                UpdatedAt = DateTime.UtcNow;
            }
        }

        public void Update(string text, string translation, string category, int difficultyLevel)
        {
            Text = text;
            Translation = translation;
            Category = category;
            DifficultyLevel = difficultyLevel;
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

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Text))
                throw new ValidationException("Text is required");
            if (string.IsNullOrWhiteSpace(Translation))
                throw new ValidationException("Translation is required");
            if (string.IsNullOrWhiteSpace(Category))
                throw new ValidationException("Category is required");
            if (string.IsNullOrWhiteSpace(UserId))
                throw new ValidationException("User ID is required");
            if (DifficultyLevel < 1 || DifficultyLevel > 5)
                throw new ValidationException("Difficulty level must be between 1 and 5");
        }
    }

    public enum WordDifficulty
    {
        Easy,
        Medium,
        Hard
    }
} 